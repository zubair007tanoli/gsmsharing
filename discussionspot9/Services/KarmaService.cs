using discussionspot9.Data.DbContext;
using discussionspot9.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace discussionspot9.Services
{
    public interface IKarmaService
    {
        Task<int> CalculateUserKarmaAsync(string userId);
        Task UpdatePostKarmaAsync(int postId, int voteType);
        Task UpdateCommentKarmaAsync(int commentId, int voteType);
        Task AwardKarmaAsync(string userId, int karma, string reason);
        Task<Dictionary<string, int>> GetKarmaBreakdownAsync(string userId);
        Task<List<UserKarmaViewModel>> GetKarmaLeaderboardAsync(string period, int limit);
    }

    public class KarmaService : IKarmaService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly ILogger<KarmaService> _logger;

        // Karma calculation rules
        private const int POST_UPVOTE_KARMA = 1;
        private const int POST_DOWNVOTE_KARMA = -1;
        private const int COMMENT_UPVOTE_KARMA = 1;
        private const int COMMENT_DOWNVOTE_KARMA = -1;
        private const int AWARD_RECEIVED_KARMA = 10;
        private const int POST_CREATED_KARMA = 1;

        public KarmaService(
            IDbContextFactory<ApplicationDbContext> contextFactory,
            ILogger<KarmaService> logger)
        {
            _contextFactory = contextFactory;
            _logger = logger;
        }

        /// <summary>
        /// Calculate total karma for a user from all sources
        /// </summary>
        public async Task<int> CalculateUserKarmaAsync(string userId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                // Get user's posts
                var userPosts = await context.Posts
                    .Where(p => p.UserId == userId && p.Status == "published")
                    .ToListAsync();

                // Calculate post karma (upvotes - downvotes)
                var postKarma = userPosts.Sum(p =>
                    (p.UpvoteCount * POST_UPVOTE_KARMA) +
                    (p.DownvoteCount * POST_DOWNVOTE_KARMA));

                // Get user's comments
                var userComments = await context.Comments
                    .Where(c => c.UserId == userId)
                    .ToListAsync();

                // Calculate comment karma
                var commentKarma = userComments.Sum(c =>
                    (c.UpvoteCount * COMMENT_UPVOTE_KARMA) +
                    (c.DownvoteCount * COMMENT_DOWNVOTE_KARMA));

                // Get awards received
                var postAwardKarma = await context.PostAwards
                    .Where(pa => userPosts.Select(p => p.PostId).Contains(pa.PostId))
                    .CountAsync() * AWARD_RECEIVED_KARMA;

                var commentAwardKarma = await context.CommentAwards
                    .Where(ca => userComments.Select(c => c.CommentId).Contains(ca.CommentId))
                    .CountAsync() * AWARD_RECEIVED_KARMA;

                // Post creation bonus (1 karma per post)
                var postCreationKarma = userPosts.Count * POST_CREATED_KARMA;

                var totalKarma = postKarma + commentKarma + postAwardKarma +
                                commentAwardKarma + postCreationKarma;

                _logger.LogInformation(
                    "Karma calculated for user {UserId}: Post={PostKarma}, Comment={CommentKarma}, " +
                    "PostAwards={PostAwardKarma}, CommentAwards={CommentAwardKarma}, Creation={PostCreationKarma}, Total={TotalKarma}",
                    userId, postKarma, commentKarma, postAwardKarma, commentAwardKarma, postCreationKarma, totalKarma);

                return totalKarma;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating karma for user {UserId}", userId);
                return 0;
            }
        }

        /// <summary>
        /// Update author's karma when their post gets voted
        /// </summary>
        public async Task UpdatePostKarmaAsync(int postId, int voteType)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                var post = await context.Posts
                    .Include(p => p.UserProfile)
                    .FirstOrDefaultAsync(p => p.PostId == postId);

                if (post?.UserId == null) return;

                var karmaChange = voteType == 1 ? POST_UPVOTE_KARMA : POST_DOWNVOTE_KARMA;

                var profile = await context.UserProfiles
                    .FirstOrDefaultAsync(p => p.UserId == post.UserId);

                if (profile != null)
                {
                    profile.KarmaPoints += karmaChange;
                    await context.SaveChangesAsync();

                    _logger.LogInformation(
                        "Karma updated for user {UserId}: {Change} (Post vote)",
                        post.UserId, karmaChange);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating post karma for postId {PostId}", postId);
            }
        }

        /// <summary>
        /// Update author's karma when their comment gets voted
        /// </summary>
        public async Task UpdateCommentKarmaAsync(int commentId, int voteType)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                var comment = await context.Comments
                    .FirstOrDefaultAsync(c => c.CommentId == commentId);

                if (comment?.UserId == null) return;

                var karmaChange = voteType == 1 ? COMMENT_UPVOTE_KARMA : COMMENT_DOWNVOTE_KARMA;

                var profile = await context.UserProfiles
                    .FirstOrDefaultAsync(p => p.UserId == comment.UserId);

                if (profile != null)
                {
                    profile.KarmaPoints += karmaChange;
                    await context.SaveChangesAsync();

                    _logger.LogInformation(
                        "Karma updated for user {UserId}: {Change} (Comment vote)",
                        comment.UserId, karmaChange);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating comment karma for commentId {CommentId}", commentId);
            }
        }

        /// <summary>
        /// Award karma manually (admin, achievements, etc.)
        /// </summary>
        public async Task AwardKarmaAsync(string userId, int karma, string reason)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                var profile = await context.UserProfiles
                    .FirstOrDefaultAsync(p => p.UserId == userId);

                if (profile != null)
                {
                    profile.KarmaPoints += karma;
                    await context.SaveChangesAsync();

                    _logger.LogInformation(
                        "Karma awarded to user {UserId}: {Karma} - Reason: {Reason}",
                        userId, karma, reason);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error awarding karma to user {UserId}", userId);
            }
        }

        /// <summary>
        /// Get detailed karma breakdown for display
        /// </summary>
        public async Task<Dictionary<string, int>> GetKarmaBreakdownAsync(string userId)
        {
            var breakdown = new Dictionary<string, int>();

            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                var userPosts = await context.Posts
                    .Where(p => p.UserId == userId && p.Status == "published")
                    .ToListAsync();

                breakdown["PostUpvotes"] = userPosts.Sum(p => p.UpvoteCount);
                breakdown["PostDownvotes"] = Math.Abs(userPosts.Sum(p => p.DownvoteCount));
                breakdown["PostKarma"] = userPosts.Sum(p =>
                    (p.UpvoteCount * POST_UPVOTE_KARMA) + (p.DownvoteCount * POST_DOWNVOTE_KARMA));

                var userComments = await context.Comments
                    .Where(c => c.UserId == userId)
                    .ToListAsync();

                breakdown["CommentUpvotes"] = userComments.Sum(c => c.UpvoteCount);
                breakdown["CommentDownvotes"] = Math.Abs(userComments.Sum(c => c.DownvoteCount));
                breakdown["CommentKarma"] = userComments.Sum(c =>
                    (c.UpvoteCount * COMMENT_UPVOTE_KARMA) + (c.DownvoteCount * COMMENT_DOWNVOTE_KARMA));

                breakdown["TotalPosts"] = userPosts.Count;
                breakdown["TotalComments"] = userComments.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting karma breakdown for user {UserId}", userId);
            }

            return breakdown;
        }

        /// <summary>
        /// Get karma leaderboard (top users)
        /// </summary>
        public async Task<List<UserKarmaViewModel>> GetKarmaLeaderboardAsync(string period, int limit = 10)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                var query = context.UserProfiles.AsQueryable();

                // Filter by time period if needed (future enhancement)
                // For now, all-time leaderboard

                var leaders = await query
                    .OrderByDescending(u => u.KarmaPoints)
                    .Take(limit)
                    .Select(u => new UserKarmaViewModel
                    {
                        UserId = u.UserId,
                        DisplayName = u.DisplayName,
                        KarmaPoints = u.KarmaPoints,
                        AvatarUrl = u.AvatarUrl,
                        IsVerified = u.IsVerified
                    })
                    .ToListAsync();

                // Add rank numbers
                for (int i = 0; i < leaders.Count; i++)
                {
                    leaders[i].Rank = i + 1;
                    leaders[i].KarmaLevel = GetKarmaLevel(leaders[i].KarmaPoints);
                }

                return leaders;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching karma leaderboard");
                return new List<UserKarmaViewModel>();
            }
        }

        /// <summary>
        /// Get karma level based on points
        /// </summary>
        private string GetKarmaLevel(int karma)
        {
            return karma switch
            {
                >= 10000 => "Legend 👑",
                >= 2000 => "Expert 💫",
                >= 500 => "Regular 🎯",
                >= 100 => "Contributor 📝",
                _ => "Newbie 🌱"
            };
        }
    }

    /// <summary>
    /// ViewModel for leaderboard display
    /// </summary>
    public class UserKarmaViewModel
    {
        public string UserId { get; set; } = null!;
        public string DisplayName { get; set; } = null!;
        public int KarmaPoints { get; set; }
        public string? AvatarUrl { get; set; }
        public bool IsVerified { get; set; }
        public int Rank { get; set; }
        public string KarmaLevel { get; set; } = "Newbie";
        
        public string FormattedKarma
        {
            get
            {
                if (KarmaPoints >= 1000000) return $"{KarmaPoints / 1000000.0:F1}M";
                if (KarmaPoints >= 1000) return $"{KarmaPoints / 1000.0:F1}K";
                return KarmaPoints.ToString();
            }
        }
    }
}


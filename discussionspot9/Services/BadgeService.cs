using discussionspot9.Data.DbContext;
using discussionspot9.Interfaces;
using discussionspot9.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace discussionspot9.Services
{
    public interface IBadgeService
    {
        Task CheckAndAwardBadgesAsync(string userId);
        Task AwardBadgeAsync(string userId, int badgeId, string reason);
        Task<List<Badge>> GetUserBadgesAsync(string userId);
        Task<List<Badge>> GetAllBadgesAsync();
        Task SetDisplayedBadgesAsync(string userId, List<int> badgeIds);
        Task<bool> HasBadgeAsync(string userId, int badgeId);
    }

    public class BadgeService : IBadgeService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly ILogger<BadgeService> _logger;
        private readonly INotificationService _notificationService;

        public BadgeService(
            IDbContextFactory<ApplicationDbContext> contextFactory,
            ILogger<BadgeService> logger,
            INotificationService notificationService)
        {
            _contextFactory = contextFactory;
            _logger = logger;
            _notificationService = notificationService;
        }

        /// <summary>
        /// Check user's stats and award eligible badges
        /// </summary>
        public async Task CheckAndAwardBadgesAsync(string userId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                var userStats = await GetUserStatsAsync(context, userId);
                var existingBadgeIds = await context.UserBadges
                    .Where(ub => ub.UserId == userId)
                    .Select(ub => ub.BadgeId)
                    .ToListAsync();

                // Check each badge category
                await CheckActivityBadges(userId, userStats, existingBadgeIds);
                await CheckQualityBadges(userId, userStats, existingBadgeIds);
                await CheckCommunityBadges(userId, userStats, existingBadgeIds);
                await CheckSpecialBadges(userId, userStats, existingBadgeIds);
                await CheckEngagementBadges(userId, userStats, existingBadgeIds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking badges for user {UserId}", userId);
            }
        }

        private async Task CheckActivityBadges(string userId, UserStats stats, List<int> existing)
        {
            // Badge ID 1: First Steps - Created first post
            if (stats.PostCount >= 1 && !existing.Contains(1))
            {
                await AwardBadgeAsync(userId, 1, "Created your first post!");
            }

            // Badge ID 2: Conversation Starter - First comment
            if (stats.CommentCount >= 1 && !existing.Contains(2))
            {
                await AwardBadgeAsync(userId, 2, "Started your first conversation!");
            }

            // Badge ID 3: Century Club - 100 posts
            if (stats.PostCount >= 100 && !existing.Contains(3))
            {
                await AwardBadgeAsync(userId, 3, "Created 100 posts!");
            }

            // Badge ID 4: Comment King - 1000 comments
            if (stats.CommentCount >= 1000 && !existing.Contains(4))
            {
                await AwardBadgeAsync(userId, 4, "Posted 1,000 comments!");
            }

            // Badge ID 5: Daily Devotee - 30 day streak
            if (stats.ConsecutiveDays >= 30 && !existing.Contains(5))
            {
                await AwardBadgeAsync(userId, 5, "Active for 30 consecutive days!");
            }

            // Badge ID 6: Early Bird - First 100 members (manual award)
            
            // Badge ID 7: Dedicated - Posted every day for a week
            if (stats.ConsecutiveDays >= 7 && !existing.Contains(7))
            {
                await AwardBadgeAsync(userId, 7, "Posted every day for a week!");
            }
        }

        private async Task CheckQualityBadges(string userId, UserStats stats, List<int> existing)
        {
            // Badge ID 10: Golden Post - 1000 upvotes on a post
            if (stats.MaxPostUpvotes >= 1000 && !existing.Contains(10))
            {
                await AwardBadgeAsync(userId, 10, "Post reached 1,000 upvotes!");
            }

            // Badge ID 11: Viral Creator - Post on trending (manual award or check trending table)

            // Badge ID 12: Award Collector - 50 awards
            if (stats.AwardsReceived >= 50 && !existing.Contains(12))
            {
                await AwardBadgeAsync(userId, 12, "Received 50 awards!");
            }

            // Badge ID 13: Quality Contributor - 10 posts with 85%+ ratio
            if (stats.HighQualityPosts >= 10 && !existing.Contains(13))
            {
                await AwardBadgeAsync(userId, 13, "10 posts with 85%+ upvote ratio!");
            }

            // Badge ID 14: Silver Tongue - Comment with 100+ upvotes
            if (stats.MaxCommentUpvotes >= 100 && !existing.Contains(14))
            {
                await AwardBadgeAsync(userId, 14, "Comment reached 100 upvotes!");
            }

            // Badge ID 15: Perfectionist - 5 posts with 95%+ ratio
            if (stats.PerfectPosts >= 5 && !existing.Contains(15))
            {
                await AwardBadgeAsync(userId, 15, "5 posts with 95%+ upvote ratio!");
            }
        }

        private async Task CheckCommunityBadges(string userId, UserStats stats, List<int> existing)
        {
            // Badge ID 20: Community Founder - Created a community
            if (stats.CommunitiesCreated >= 1 && !existing.Contains(20))
            {
                await AwardBadgeAsync(userId, 20, "Created a community!");
            }

            // Badge ID 21: Super Moderator - Mod of 3+ communities
            if (stats.CommunitiesModerated >= 3 && !existing.Contains(21))
            {
                await AwardBadgeAsync(userId, 21, "Moderator of 3+ communities!");
            }

            // Badge ID 22: Helpful Helper - 500+ helpful comments (high upvote comments)
            if (stats.HelpfulComments >= 500 && !existing.Contains(22))
            {
                await AwardBadgeAsync(userId, 22, "500+ helpful comments!");
            }

            // Badge ID 42: Social Butterfly - Following 50+ users
            if (stats.FollowingCount >= 50 && !existing.Contains(42))
            {
                await AwardBadgeAsync(userId, 42, "Following 50+ users!");
            }

            // Badge ID 43: Influencer - 100+ followers
            if (stats.FollowerCount >= 100 && !existing.Contains(43))
            {
                await AwardBadgeAsync(userId, 43, "Reached 100 followers!");
            }
        }

        private async Task CheckSpecialBadges(string userId, UserStats stats, List<int> existing)
        {
            // Badge ID 33: 1 Year Club
            var accountAge = DateTime.UtcNow - stats.JoinDate;
            if (accountAge.TotalDays >= 365 && !existing.Contains(33))
            {
                await AwardBadgeAsync(userId, 33, "Member for 1 year!");
            }

            // Badge ID 34: OG Member - 5 years
            if (accountAge.TotalDays >= 1825 && !existing.Contains(34))
            {
                await AwardBadgeAsync(userId, 34, "Member for 5+ years!");
            }
        }

        private async Task CheckEngagementBadges(string userId, UserStats stats, List<int> existing)
        {
            // Badge ID 40: Pollster - Created 10 polls
            if (stats.PollsCreated >= 10 && !existing.Contains(40))
            {
                await AwardBadgeAsync(userId, 40, "Created 10 polls!");
            }

            // Badge ID 41: Storyteller - Created 5 stories
            if (stats.StoriesCreated >= 5 && !existing.Contains(41))
            {
                await AwardBadgeAsync(userId, 41, "Created 5 stories!");
            }
        }

        public async Task AwardBadgeAsync(string userId, int badgeId, string reason)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                // Check if already has badge
                var exists = await context.UserBadges
                    .AnyAsync(ub => ub.UserId == userId && ub.BadgeId == badgeId);

                if (exists)
                {
                    _logger.LogInformation("User {UserId} already has badge {BadgeId}", userId, badgeId);
                    return;
                }

                var userBadge = new UserBadge
                {
                    UserId = userId,
                    BadgeId = badgeId,
                    EarnedReason = reason,
                    EarnedAt = DateTime.UtcNow,
                    IsDisplayed = true
                };

                context.UserBadges.Add(userBadge);
                await context.SaveChangesAsync();

                // Get badge details for notification
                var badge = await context.Badges.FindAsync(badgeId);
                if (badge != null)
                {
                    // Send notification
                    await _notificationService.CreateNotificationAsync(
                        userId,
                        $"🏆 You earned the '{badge.Name}' badge!",
                        reason,
                        "badge",
                        badgeId.ToString(),
                        "Badge"
                    );

                    _logger.LogInformation(
                        "🏆 Badge awarded: {BadgeName} (ID: {BadgeId}) to user {UserId} - {Reason}",
                        badge.Name, badgeId, userId, reason);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error awarding badge {BadgeId} to user {UserId}", badgeId, userId);
            }
        }

        public async Task<List<Badge>> GetUserBadgesAsync(string userId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                return await context.UserBadges
                    .Where(ub => ub.UserId == userId)
                    .Include(ub => ub.Badge)
                    .OrderByDescending(ub => ub.Badge.Rarity)
                    .ThenByDescending(ub => ub.EarnedAt)
                    .Select(ub => ub.Badge)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting badges for user {UserId}", userId);
                return new List<Badge>();
            }
        }

        public async Task<List<Badge>> GetAllBadgesAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                return await context.Badges
                    .Where(b => b.IsActive)
                    .OrderBy(b => b.Category)
                    .ThenBy(b => b.SortOrder)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all badges");
                return new List<Badge>();
            }
        }

        public async Task SetDisplayedBadgesAsync(string userId, List<int> badgeIds)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                var userBadges = await context.UserBadges
                    .Where(ub => ub.UserId == userId)
                    .ToListAsync();

                foreach (var ub in userBadges)
                {
                    ub.IsDisplayed = badgeIds.Contains(ub.BadgeId);
                    ub.DisplayOrder = badgeIds.IndexOf(ub.BadgeId) + 1;
                }

                await context.SaveChangesAsync();
                _logger.LogInformation("Updated displayed badges for user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting displayed badges for user {UserId}", userId);
            }
        }

        public async Task<bool> HasBadgeAsync(string userId, int badgeId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                return await context.UserBadges
                    .AnyAsync(ub => ub.UserId == userId && ub.BadgeId == badgeId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if user {UserId} has badge {BadgeId}", userId, badgeId);
                return false;
            }
        }

        // Helper to get user stats
        private async Task<UserStats> GetUserStatsAsync(ApplicationDbContext context, string userId)
        {
            var profile = await context.UserProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
            {
                return new UserStats { JoinDate = DateTime.UtcNow };
            }

            var posts = await context.Posts
                .Where(p => p.UserId == userId && p.Status == "published")
                .ToListAsync();

            var comments = await context.Comments
                .Where(c => c.UserId == userId)
                .ToListAsync();

            var communities = await context.Communities
                .Where(c => c.CreatorId == userId)
                .CountAsync();

            var moderated = await context.CommunityMembers
                .Where(cm => cm.UserId == userId && (cm.Role == "Admin" || cm.Role == "Moderator"))
                .CountAsync();

            var postAwards = await context.PostAwards
                .Where(pa => posts.Select(p => p.PostId).Contains(pa.PostId))
                .CountAsync();

            var commentAwards = await context.CommentAwards
                .Where(ca => comments.Select(c => c.CommentId).Contains(ca.CommentId))
                .CountAsync();

            var polls = await context.Posts
                .Where(p => p.UserId == userId && p.HasPoll)
                .CountAsync();

            var stories = await context.Stories
                .Where(s => s.UserId == userId)
                .CountAsync();

            var following = await context.UserFollows
                .Where(uf => uf.FollowerId == userId)
                .CountAsync();

            var followers = await context.UserFollows
                .Where(uf => uf.FollowedId == userId)
                .CountAsync();

            var highQualityPosts = posts.Count(p =>
            {
                var total = p.UpvoteCount + Math.Abs(p.DownvoteCount);
                if (total < 10) return false; // Minimum votes required
                var ratio = (double)p.UpvoteCount / total;
                return ratio >= 0.85;
            });

            var perfectPosts = posts.Count(p =>
            {
                var total = p.UpvoteCount + Math.Abs(p.DownvoteCount);
                if (total < 10) return false;
                var ratio = (double)p.UpvoteCount / total;
                return ratio >= 0.95;
            });

            var helpfulComments = comments.Count(c => c.UpvoteCount >= 10);

            return new UserStats
            {
                PostCount = posts.Count,
                CommentCount = comments.Count,
                CommunitiesCreated = communities,
                CommunitiesModerated = moderated,
                AwardsReceived = postAwards + commentAwards,
                MaxPostUpvotes = posts.Any() ? posts.Max(p => p.UpvoteCount) : 0,
                MaxCommentUpvotes = comments.Any() ? comments.Max(c => c.UpvoteCount) : 0,
                HighQualityPosts = highQualityPosts,
                PerfectPosts = perfectPosts,
                HelpfulComments = helpfulComments,
                PollsCreated = polls,
                StoriesCreated = stories,
                FollowingCount = following,
                FollowerCount = followers,
                JoinDate = profile.JoinDate,
                ConsecutiveDays = 0 // TODO: Calculate from UserActivity table if needed
            };
        }
    }

    // Helper class for user statistics
    public class UserStats
    {
        public int PostCount { get; set; }
        public int CommentCount { get; set; }
        public int CommunitiesCreated { get; set; }
        public int CommunitiesModerated { get; set; }
        public int AwardsReceived { get; set; }
        public int MaxPostUpvotes { get; set; }
        public int MaxCommentUpvotes { get; set; }
        public int HighQualityPosts { get; set; }
        public int PerfectPosts { get; set; }
        public int HelpfulComments { get; set; }
        public int PollsCreated { get; set; }
        public int StoriesCreated { get; set; }
        public int FollowingCount { get; set; }
        public int FollowerCount { get; set; }
        public DateTime JoinDate { get; set; }
        public int ConsecutiveDays { get; set; }
    }
}


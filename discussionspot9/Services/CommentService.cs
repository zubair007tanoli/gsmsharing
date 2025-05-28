using discussionspot9.Data.DbContext;
using discussionspot9.Interfaces;
using discussionspot9.Models.Domain;
using discussionspot9.Models.ViewModels.CreativeViewModels;
using discussionspot9.Services.ServiceResults;
using Microsoft.EntityFrameworkCore;

namespace discussionspot9.Services
{
    public class CommentService : ICommentService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CommentService> _logger;

        public CommentService(ApplicationDbContext context, ILogger<CommentService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<CreateCommentResult> CreateCommentAsync(CreateCommentViewModel model)
        {
            var comment = new Comment
            {
                Content = model.Content,
                PostId = model.PostId,
                UserId = model.UserId,
                ParentCommentId = model.ParentCommentId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                TreeLevel = 0
            };

            if (model.ParentCommentId.HasValue)
            {
                var parentComment = await _context.Comments.FindAsync(model.ParentCommentId.Value);
                if (parentComment != null)
                {
                    comment.TreeLevel = parentComment.TreeLevel + 1;
                }
            }

            _context.Comments.Add(comment);

            // Update post comment count
            var post = await _context.Posts.FindAsync(model.PostId);
            if (post != null)
            {
                post.CommentCount++;
                post.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return new CreateCommentResult { Success = true, CommentId = comment.CommentId };
        }

        public async Task<CommentViewModel?> GetCommentByIdAsync(int commentId)
        {
            var comment = await _context.Comments
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.CommentId == commentId);

            if (comment == null) return null;

            var userProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(up => up.UserId == comment.UserId);

            return MapToCommentViewModel(comment, userProfile);
        }

        public async Task<VoteResult> VoteCommentAsync(int commentId, string userId, int voteType)
        {
            var existingVote = await _context.CommentVotes
                .FirstOrDefaultAsync(cv => cv.CommentId == commentId && cv.UserId == userId);

            var comment = await _context.Comments.FindAsync(commentId);
            if (comment == null)
            {
                return new VoteResult { Success = false, ErrorMessage = "Comment not found" };
            }

            if (existingVote != null)
            {
                if (existingVote.VoteType == voteType)
                {
                    // Remove vote
                    _context.CommentVotes.Remove(existingVote);
                    if (voteType == 1) comment.UpvoteCount--;
                    else comment.DownvoteCount--;
                    voteType = 0;
                }
                else
                {
                    // Change vote
                    if (existingVote.VoteType == 1) comment.UpvoteCount--;
                    else comment.DownvoteCount--;

                    existingVote.VoteType = voteType;
                    existingVote.VotedAt = DateTime.UtcNow;

                    if (voteType == 1) comment.UpvoteCount++;
                    else comment.DownvoteCount++;
                }
            }
            else
            {
                // New vote
                _context.CommentVotes.Add(new CommentVote
                {
                    CommentId = commentId,
                    UserId = userId,
                    VoteType = voteType,
                    VotedAt = DateTime.UtcNow
                });

                if (voteType == 1) comment.UpvoteCount++;
                else comment.DownvoteCount++;
            }

            comment.Score = comment.UpvoteCount - comment.DownvoteCount;
            await _context.SaveChangesAsync();

            return new VoteResult { Success = true, UserVote = voteType == 0 ? null : voteType };
        }

        public async Task<int> GetCommentVoteCountAsync(int commentId)
        {
            var comment = await _context.Comments.FindAsync(commentId);
            return comment?.Score ?? 0;
        }

        public async Task<ServiceResult> EditCommentAsync(int commentId, string content, string userId)
        {
            var comment = await _context.Comments.FindAsync(commentId);
            if (comment == null)
            {
                return ServiceResult.ErrorResult("Comment not found.");
            }

            if (comment.UserId != userId)
            {
                return ServiceResult.ErrorResult("You don't have permission to edit this comment.");
            }

            comment.Content = content;
            comment.UpdatedAt = DateTime.UtcNow;
            comment.IsEdited = true;

            await _context.SaveChangesAsync();
            return ServiceResult.SuccessResult();
        }

        public async Task<ServiceResult> DeleteCommentAsync(int commentId, string userId)
        {
            var comment = await _context.Comments.FindAsync(commentId);
            if (comment == null)
            {
                return ServiceResult.ErrorResult("Comment not found.");
            }

            if (comment.UserId != userId)
            {
                return ServiceResult.ErrorResult("You don't have permission to delete this comment.");
            }

            comment.IsDeleted = true;
            comment.Content = "[deleted]";
            comment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return ServiceResult.SuccessResult();
        }

        public async Task<List<CommentTreeViewModel>> GetPostCommentsAsync(int postId, string sort = "best", int page = 1)
        {
            const int pageSize = 20;
            var skip = (page - 1) * pageSize;

            var query = _context.Comments
                .Include(c => c.ChildComments)
                .Where(c => c.PostId == postId && c.ParentCommentId == null && !c.IsDeleted);

            query = sort switch
            {
                "new" => query.OrderByDescending(c => c.CreatedAt),
                "top" => query.OrderByDescending(c => c.Score),
                "controversial" => query.OrderByDescending(c => c.UpvoteCount + c.DownvoteCount)
                    .ThenBy(c => Math.Abs(c.UpvoteCount - c.DownvoteCount)),
                _ => query.OrderByDescending(c => c.Score)
                    .ThenByDescending(c => c.CreatedAt)
            };

            var topLevelComments = await query
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            var commentTrees = new List<CommentTreeViewModel>();
            foreach (var comment in topLevelComments)
            {
                var tree = await BuildCommentTree(comment, 0);
                commentTrees.Add(tree);
            }

            return commentTrees;
        }

        private async Task<CommentTreeViewModel> BuildCommentTree(Comment comment, int depth)
        {
            var userProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(up => up.UserId == comment.UserId);

            var viewModel = MapToCommentViewModel(comment, userProfile);

            var tree = new CommentTreeViewModel
            {
                Comment = viewModel,
                Depth = depth,
                IsCollapsed = false
            };

            if (depth < 10)
            {
                var childComments = await _context.Comments
                    .Where(c => c.ParentCommentId == comment.CommentId && !c.IsDeleted)
                    .OrderByDescending(c => c.Score)
                    .ToListAsync();

                foreach (var child in childComments)
                {
                    var childTree = await BuildCommentTree(child, depth + 1);
                    tree.Children.Add(childTree);
                }
            }

            return tree;
        }

        private static CommentViewModel MapToCommentViewModel(Comment comment, UserProfile? userProfile)
        {
            return new CommentViewModel
            {
                CommentId = comment.CommentId,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt,
                UpdatedAt = comment.UpdatedAt,
                UpvoteCount = comment.UpvoteCount,
                DownvoteCount = comment.DownvoteCount,
                IsEdited = comment.IsEdited,
                IsDeleted = comment.IsDeleted,
                TreeLevel = comment.TreeLevel,
                UserId = comment.UserId,
                AuthorDisplayName = userProfile?.DisplayName ?? "Unknown",
                AuthorInitials = GetInitials(userProfile?.DisplayName ?? "Unknown"),
                IsAuthorVerified = userProfile?.IsVerified ?? false,
                ParentCommentId = comment.ParentCommentId
            };
        }

        private static string GetInitials(string displayName)
        {
            if (string.IsNullOrEmpty(displayName)) return "??";
            var parts = displayName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1) return parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpper();
            return (parts[0].Substring(0, 1) + parts[^1].Substring(0, 1)).ToUpper();
        }
    }
}
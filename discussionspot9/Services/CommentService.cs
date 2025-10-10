using discussionspot9.Data.DbContext;
using discussionspot9.Interfaces;
using discussionspot9.Models.Domain;
using discussionspot9.Models.ViewModels.CreativeViewModels;
using discussionspot9.Services.ServiceResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace discussionspot9.Services
{
    public class CommentService : ICommentService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _context;
        private readonly ILogger<CommentService> _logger;
        private readonly ILinkMetadataService _linkMetadataService;

        public CommentService(IDbContextFactory<ApplicationDbContext> context, ILogger<CommentService> logger, ILinkMetadataService linkMetadataService)
        {
            _context = context;
            _logger = logger;
            _linkMetadataService = linkMetadataService;
        }

        public async Task<CreateCommentResult> CreateCommentAsync(CreateCommentViewModel model)
        {
            using var dbContext = _context.CreateDbContext(); // Use the factory to create a DbContext instance

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
                var parentComment = await dbContext.Comments.FindAsync(model.ParentCommentId.Value); // Access Comments via the created DbContext
                if (parentComment != null)
                {
                    comment.TreeLevel = parentComment.TreeLevel + 1;
                }
            }

            dbContext.Comments.Add(comment);

            // Update post comment count
            var post = await dbContext.Posts.FindAsync(model.PostId); // Access Posts via the created DbContext
            if (post != null)
            {
                post.CommentCount++;
                post.UpdatedAt = DateTime.UtcNow;
            }

            await dbContext.SaveChangesAsync();
            return new CreateCommentResult { Success = true, CommentId = comment.CommentId };
        }

        public async Task<CommentViewModel?> GetCommentByIdAsync(int commentId)
        {
            using var dbContext = _context.CreateDbContext(); // Use the factory to create a DbContext instance

            var comment = await dbContext.Comments
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.CommentId == commentId);

            if (comment == null) return null;

            var userProfile = await dbContext.UserProfiles
                .FirstOrDefaultAsync(up => up.UserId == comment.UserId);

            return MapToCommentViewModel(comment, userProfile);
        }
        public async Task<VoteResult> VoteCommentAsync(int commentId, string userId, int voteType)
        {
            using var dbContext = _context.CreateDbContext();

            var existingVote = await dbContext.CommentVotes
                .FirstOrDefaultAsync(cv => cv.CommentId == commentId && cv.UserId == userId);

            var comment = await dbContext.Comments.FindAsync(commentId);
            if (comment == null)
            {
                return new VoteResult { Success = false, ErrorMessage = "Comment not found" };
            }

            if (existingVote != null)
            {
                if (existingVote.VoteType == voteType)
                {
                    // Remove vote
                    dbContext.CommentVotes.Remove(existingVote);
                    if (voteType == 1) comment.UpvoteCount--;
                    else comment.DownvoteCount--;
                    voteType = 0; // Represents vote removal
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
                dbContext.CommentVotes.Add(new CommentVote
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
            await dbContext.SaveChangesAsync();

            return new VoteResult
            {
                Success = true,
                UserVote = voteType == 0 ? null : (int?)voteType
            };
        }
        //public async Task<VoteResult> VoteCommentAsync(int commentId, string userId, int voteType)
        //{
        //    using var dbContext = _context.CreateDbContext(); // Use the factory to create a DbContext instance

        //    var existingVote = await dbContext.CommentVotes
        //        .FirstOrDefaultAsync(cv => cv.CommentId == commentId && cv.UserId == userId);

        //    var comment = await dbContext.Comments.FindAsync(commentId);
        //    if (comment == null)
        //    {
        //        return new VoteResult { Success = false, ErrorMessage = "Comment not found" };
        //    }

        //    if (existingVote != null)
        //    {
        //        if (existingVote.VoteType == voteType)
        //        {
        //            // Remove vote
        //            dbContext.CommentVotes.Remove(existingVote);
        //            if (voteType == 1) comment.UpvoteCount--;
        //            else comment.DownvoteCount--;
        //            voteType = 0;
        //        }
        //        else
        //        {
        //            // Change vote
        //            if (existingVote.VoteType == 1) comment.UpvoteCount--;
        //            else comment.DownvoteCount--;

        //            existingVote.VoteType = voteType;
        //            existingVote.VotedAt = DateTime.UtcNow;

        //            if (voteType == 1) comment.UpvoteCount++;
        //            else comment.DownvoteCount++;
        //        }
        //    }
        //    else
        //    {
        //        // New vote
        //        dbContext.CommentVotes.Add(new CommentVote
        //        {
        //            CommentId = commentId,
        //            UserId = userId,
        //            VoteType = voteType,
        //            VotedAt = DateTime.UtcNow
        //        });

        //        if (voteType == 1) comment.UpvoteCount++;
        //        else comment.DownvoteCount++;
        //    }

        //    comment.Score = comment.UpvoteCount - comment.DownvoteCount;
        //    await dbContext.SaveChangesAsync();


        //    return new VoteResult { Success = true, UserVote = voteType == 0 ? null : voteType };
        //}

        public async Task<int> GetCommentVoteCountAsync(int commentId)
        {
            using var dbContext = _context.CreateDbContext(); // Use the factory to create a DbContext instance
            var comment = await dbContext.Comments.FindAsync(commentId); // Access Comments via the created DbContext
            return comment?.Score ?? 0;
        }

        public async Task<ServiceResult> EditCommentAsync(int commentId, string content, string userId)
        {
            using var dbContext = _context.CreateDbContext(); // Use the factory to create a DbContext instance
            var comment = await dbContext.Comments.FindAsync(commentId); // Access Comments via the created DbContext
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

            await dbContext.SaveChangesAsync();
            return ServiceResult.SuccessResult();
        }

        public async Task<ServiceResult> DeleteCommentAsync(int commentId, string userId)
        {
            using var dbContext = _context.CreateDbContext(); // Use the factory to create a DbContext instance
            var comment = await dbContext.Comments.FindAsync(commentId); // Access Comments via the created DbContext
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

            await dbContext.SaveChangesAsync();
            return ServiceResult.SuccessResult();
        }

        public async Task<List<CommentTreeViewModel>> GetPostCommentsAsync(int postId, string sort = "best", int page = 1)
        {
            const int pageSize = 20;
            var skip = (page - 1) * pageSize;

            // Create context without immediate disposal
            using var dbContext = _context.CreateDbContext();
            var allComments = await dbContext.Comments
                .Where(c => c.PostId == postId && !c.IsDeleted)
                .Include(c => c.User)
                .AsNoTracking()
                .ToListAsync();

              // Create a lookup dictionary for efficient child comment retrieval
            var commentLookup = allComments.ToLookup(c => c.ParentCommentId);

            // Get top-level comments
            var topLevelComments = allComments
                .Where(c => c.ParentCommentId == null)
                .AsQueryable();

            // Apply sorting
            topLevelComments = sort switch
            {
                "new" => topLevelComments.OrderByDescending(c => c.CreatedAt),
                "top" => topLevelComments.OrderByDescending(c => c.UpvoteCount - c.DownvoteCount),
                "controversial" => topLevelComments.OrderByDescending(c => c.UpvoteCount + c.DownvoteCount)
                    .ThenBy(c => Math.Abs(c.UpvoteCount - c.DownvoteCount)),
                _ => topLevelComments.OrderByDescending(c => c.UpvoteCount - c.DownvoteCount)
                    .ThenByDescending(c => c.CreatedAt)
            };

            // Apply pagination
            var pagedTopLevelComments = topLevelComments
                .Skip(skip)
                .Take(pageSize)
                .ToList();

            // Build comment trees using in-memory data
            var commentTrees = new List<CommentTreeViewModel>();
            foreach (var comment in pagedTopLevelComments)
            {
                var tree = BuildCommentTreeFromMemory(comment, commentLookup, 0);
                commentTrees.Add(tree);
            }

            return commentTrees;
        }

        private CommentTreeViewModel BuildCommentTreeFromMemory(Comment comment, ILookup<int?, Comment> commentLookup, int depth)
        {
            // Create the CommentViewModel with all required properties
            var commentViewModel = new CommentViewModel
            {
                CommentId = comment.CommentId, // Adjust property name based on your Comment entity
                Content = comment.Content,
                CreatedAt = comment.CreatedAt,
                UpdatedAt = comment.UpdatedAt, // Make sure this exists in your Comment entity
                UpvoteCount = comment.UpvoteCount,
                DownvoteCount = comment.DownvoteCount,
                IsEdited = comment.IsEdited, // Adjust if this property exists
                IsDeleted = comment.IsDeleted,
                TreeLevel = depth,

                // Author properties
                UserId = comment.UserId?.ToString(), // Convert to string if needed
                AuthorDisplayName = comment.User?.UserName ?? "Unknown",
                AuthorInitials = GetUserInitials(comment.User?.UserName ?? "Unknown"),
                //IsAuthorVerified = comment.User.IsVerified ?? false, // Adjust based on your User entity

                // Parent comment
                ParentCommentId = comment.ParentCommentId,

                // User interaction properties - you'll need to set these based on current user context
                CurrentUserVote = null, // Set this based on current user's vote if available
                IsCurrentUserAuthor = false, // Set this based on current user context

                ChildComments = new List<CommentViewModel>()
            };

            // Get child comments from lookup and build them recursively
            var childComments = commentLookup[comment.CommentId]
                .OrderByDescending(c => c.UpvoteCount - c.DownvoteCount)
                .ThenByDescending(c => c.CreatedAt);

            foreach (var childComment in childComments)
            {
                var childCommentViewModel = BuildCommentViewModelFromMemory(childComment, commentLookup, depth + 1);
                commentViewModel.ChildComments.Add(childCommentViewModel);
            }

            // Create the tree view model
            var viewModel = new CommentTreeViewModel
            {
                Comment = commentViewModel,
                Depth = depth,
                Children = new List<CommentTreeViewModel>()
            };

            // Build child tree view models
            foreach (var childComment in childComments)
            {
                var childTree = BuildCommentTreeFromMemory(childComment, commentLookup, depth + 1);
                viewModel.Children.Add(childTree);
            }

            return viewModel;
        }

        private CommentViewModel BuildCommentViewModelFromMemory(Comment comment, ILookup<int?, Comment> commentLookup, int treeLevel)
        {
            var commentViewModel = new CommentViewModel
            {
                CommentId = comment.CommentId,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt,
                UpdatedAt = comment.UpdatedAt,
                UpvoteCount = comment.UpvoteCount,
                DownvoteCount = comment.DownvoteCount,
                IsEdited = comment.IsEdited,
                IsDeleted = comment.IsDeleted,
                TreeLevel = treeLevel,

                // Author properties
                UserId = comment.UserId?.ToString(),
                AuthorDisplayName = comment.User?.UserName ?? "Unknown",
                AuthorInitials = GetUserInitials(comment.User?.UserName ?? "Unknown"),
                //IsAuthorVerified = comment.User?.IsVerified ?? false,

                // Parent comment
                ParentCommentId = comment.ParentCommentId,

                // User interaction properties
                CurrentUserVote = null, // Set based on current user context
                IsCurrentUserAuthor = false, // Set based on current user context

                ChildComments = new List<CommentViewModel>()
            };

            // Recursively build child comments
            var childComments = commentLookup[comment.CommentId]
                .OrderByDescending(c => c.UpvoteCount - c.DownvoteCount)
                .ThenByDescending(c => c.CreatedAt);

            foreach (var childComment in childComments)
            {
                var childCommentViewModel = BuildCommentViewModelFromMemory(childComment, commentLookup, treeLevel + 1);
                commentViewModel.ChildComments.Add(childCommentViewModel);
            }

            return commentViewModel;
        }

        private static string GetUserInitials(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
                return "?";

            var parts = userName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1)
                return parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpper();

            return string.Join("", parts.Take(2).Select(p => p[0])).ToUpper();
        }

        private async Task<CommentTreeViewModel> BuildCommentTree(Comment comment, int depth)
        {
            using var dbContext = _context.CreateDbContext(); // Use the factory to create a DbContext instance

            var userProfile = await dbContext.UserProfiles
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
                var childComments = await dbContext.Comments
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
                PostId = comment.PostId,
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

        Task ICommentService.UpdateCommentAsync(int commentId, string newContent, string? userId)
        {
            throw new NotImplementedException();
        }

        // Link Preview Methods
        public List<string> ExtractUrls(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return new List<string>();

            // Regex pattern to match URLs (http, https)
            const string urlPattern = @"https?://[^\s<>""'\)]+";
            var matches = System.Text.RegularExpressions.Regex.Matches(content, urlPattern);
            
            return matches
                .Select(m => m.Value.TrimEnd('.', ',', '!', '?', ';', ':')) // Remove trailing punctuation
                .Distinct()
                .Take(5) // Limit to 5 URLs per comment for performance
                .ToList();
        }

        public async Task<List<LinkPreviewViewModel>> ProcessLinkPreviewsAsync(int commentId, string content)
        {
            var urls = ExtractUrls(content);
            if (!urls.Any())
                return new List<LinkPreviewViewModel>();

            var linkPreviews = new List<LinkPreviewViewModel>();
            using var dbContext = _context.CreateDbContext();

            foreach (var url in urls)
            {
                try
                {
                    // Check if we have a cached preview for this URL (global cache)
                    var cachedPreview = await dbContext.CommentLinkPreviews
                        .Where(clp => clp.Url == url && clp.FetchSucceeded)
                        .OrderByDescending(clp => clp.LastFetchedAt ?? clp.CreatedAt)
                        .FirstOrDefaultAsync();

                    LinkPreviewViewModel previewModel;

                    if (cachedPreview != null && 
                        (cachedPreview.LastFetchedAt ?? cachedPreview.CreatedAt) > DateTime.UtcNow.AddDays(-7)) // 7-day cache
                    {
                        // Use cached data
                        previewModel = new LinkPreviewViewModel
                        {
                            Url = cachedPreview.Url,
                            Title = cachedPreview.Title,
                            Description = cachedPreview.Description,
                            Domain = cachedPreview.Domain,
                            ThumbnailUrl = cachedPreview.ThumbnailUrl ?? string.Empty,
                            FaviconUrl = cachedPreview.FaviconUrl ?? string.Empty
                        };
                        _logger.LogInformation($"Using cached link preview for: {url}");
                    }
                    else
                    {
                        // Fetch fresh metadata
                        previewModel = await _linkMetadataService.GetMetadataAsync(url);
                        
                        // Save to database for future use
                        var linkPreview = new CommentLinkPreview
                        {
                            CommentId = commentId,
                            Url = url,
                            Title = previewModel.Title ?? string.Empty,
                            Description = previewModel.Description ?? string.Empty,
                            Domain = previewModel.Domain ?? string.Empty,
                            ThumbnailUrl = previewModel.ThumbnailUrl,
                            FaviconUrl = previewModel.FaviconUrl,
                            CreatedAt = DateTime.UtcNow,
                            LastFetchedAt = DateTime.UtcNow,
                            FetchSucceeded = !string.IsNullOrEmpty(previewModel.Title)
                        };

                        dbContext.CommentLinkPreviews.Add(linkPreview);
                        _logger.LogInformation($"Fetched and cached new link preview for: {url}");
                    }

                    linkPreviews.Add(previewModel);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error processing link preview for URL: {url}");
                    
                    // Add failed preview to database to avoid retrying immediately
                    var failedPreview = new CommentLinkPreview
                    {
                        CommentId = commentId,
                        Url = url,
                        Title = "Preview Unavailable",
                        Description = "Unable to load link preview",
                        Domain = new Uri(url).Host,
                        CreatedAt = DateTime.UtcNow,
                        FetchSucceeded = false
                    };
                    dbContext.CommentLinkPreviews.Add(failedPreview);
                    
                    // Add basic preview to response
                    linkPreviews.Add(new LinkPreviewViewModel
                    {
                        Url = url,
                        Title = "Link",
                        Description = url,
                        Domain = new Uri(url).Host,
                        ThumbnailUrl = string.Empty,
                        FaviconUrl = string.Empty
                    });
                }
            }

            // Save all link previews in one transaction
            try
            {
                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving link previews to database");
            }

            return linkPreviews;
        }

        public async Task<List<LinkPreviewViewModel>> GetCommentLinkPreviewsAsync(int commentId)
        {
            using var dbContext = _context.CreateDbContext();
            
            var linkPreviews = await dbContext.CommentLinkPreviews
                .Where(clp => clp.CommentId == commentId && clp.FetchSucceeded)
                .ToListAsync();

            return linkPreviews.Select(lp => new LinkPreviewViewModel
            {
                Url = lp.Url,
                Title = lp.Title,
                Description = lp.Description,
                Domain = lp.Domain,
                ThumbnailUrl = lp.ThumbnailUrl ?? string.Empty,
                FaviconUrl = lp.FaviconUrl ?? string.Empty
            }).ToList();
        }
    }
}
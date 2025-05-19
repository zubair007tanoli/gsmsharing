using discussionspot.Interfaces;
using discussionspot.Models.Domain;
using discussionspot.Models.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace discussionspot.Services
{
    public class PostService : IPostService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUsers> _userManager;

        public PostService(IUnitOfWork unitOfWork, UserManager<ApplicationUsers> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public async Task<PostViewModel> GetPostByIdAsync(int postId)
        {
            var post = await _unitOfWork.Posts.GetPostWithDetailsAsync(postId);
            if (post == null)
                return null;

            return MapPostToViewModel(post);
        }

        public async Task<IEnumerable<PostViewModel>> GetPostsByCommunityAsync(int communityId, int page = 1, int pageSize = 20)
        {
            var posts = await _unitOfWork.Posts.GetPostsByCommunityAsync(communityId, page, pageSize);
            return posts.Select(MapPostToViewModel);
        }

        public async Task<IEnumerable<PostViewModel>> GetPostsAsync(string sortBy = "hot", int page = 1, int pageSize = 20)
        {
            IEnumerable<Post> posts;

            switch (sortBy.ToLower())
            {
                case "new":
                    posts = await _unitOfWork.Posts.GetNewPostsAsync(page, pageSize);
                    break;
                case "hot":
                default:
                    posts = await _unitOfWork.Posts.GetHotPostsAsync(page, pageSize);
                    break;
            }

            return posts.Select(MapPostToViewModel);
        }

        public async Task<int> CreatePostAsync(PostCreateViewModel model, string userId)
        {
            var post = new Post
            {
                Title = model.Title,
                Content = model.Content,
                CommunityId = model.CommunityId,
                UserId = userId,
                PostType = model.PostType,
                ExternalUrl = model.Url,
                IsNSFW = model.IsNSFW,
                IsSpoiler = model.IsSpoiler,
                Status = "published",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            post.GenerateSlug();

            // Check if slug exists and make it unique if needed
            string baseSlug = post.Slug;
            int counter = 1;
            while (await _unitOfWork.Posts.SlugExistsInCommunityAsync(post.CommunityId, post.Slug))
            {
                post.Slug = $"{baseSlug}-{counter}";
                counter++;
            }

            await _unitOfWork.Posts.AddAsync(post);

            // Handle poll if needed
            if (model.HasPoll && model.PollOptions?.Count > 0)
            {
                // Set poll flags
                post.HasPoll = true;
                post.PollOptionCount = model.PollOptions.Count;
                post.PollExpiresAt = model.PollEndsAt;

                // Create poll options
                foreach (var optionText in model.PollOptions)
                {
                    if (!string.IsNullOrWhiteSpace(optionText))
                    {
                        var pollOption = new PollOption
                        {
                            PostId = post.PostId,
                            OptionText = optionText,
                            DisplayOrder = model.PollOptions.IndexOf(optionText)
                        };

                        await _unitOfWork.PollOptions.AddAsync(pollOption);
                    }
                }

                // Create poll configuration
                var pollConfig = new PollConfiguration
                {
                    PostId = post.PostId,
                    AllowMultipleChoices = model.AllowMultipleChoices,
                    EndDate = model.PollEndsAt,
                    ShowResultsBeforeVoting = true
                };

                await _unitOfWork.PollConfigurations.AddAsync(pollConfig);
            }

            await _unitOfWork.SaveChangesAsync();

            return post.PostId;
        }

        // Helper method to map domain model to view model
        private PostViewModel MapPostToViewModel(Post post)
        {
            return new PostViewModel
            {
                PostId = post.PostId,
                Title = post.Title,
                Content = post.Content,
                Slug = post.Slug,
                UserId = post.UserId,
                Username = post.User?.UserName,
                CommunityId = post.CommunityId,
                CommunityName = post.Community?.Name,
                CommunitySlug = post.Community?.Slug,
                PostType = post.PostType,
                Url = post.ExternalUrl,
                CreatedAt = post.CreatedAt,
                UpdatedAt = post.UpdatedAt,
                UpvoteCount = post.UpvoteCount,
                DownvoteCount = post.DownvoteCount,
                CommentCount = post.CommentCount,
                Score = post.Score,
                Status = post.Status,
                IsPinned = post.IsPinned,
                IsLocked = post.IsLocked,
                IsNSFW = post.IsNSFW,
                IsSpoiler = post.IsSpoiler,
                ViewCount = post.ViewCount,
                HasPoll = post.HasPoll,
                Media = post.Media?.Select(m => new MediaViewModel
                {
                    MediaId = m.MediaId,
                    Url = m.Url,
                    ThumbnailUrl = m.ThumbnailUrl,
                    MediaType = m.MediaType,
                    ContentType = m.ContentType,
                    FileName = m.FileName
                }),
                Tags = post.PostTags?.Select(pt => pt.Tag?.Name)
            };
        }

        Task<bool> IPostService.UpdatePostAsync(int postId, PostEditViewModel model, string userId)
        {
            throw new NotImplementedException();
        }

        Task<bool> IPostService.DeletePostAsync(int postId, string userId)
        {
            throw new NotImplementedException();
        }

        Task<VoteResult> IPostService.VotePostAsync(int postId, string userId, bool isUpvote)
        {
            throw new NotImplementedException();
        }

        Task<bool?> IPostService.GetUserVoteForPostAsync(int postId, string userId)
        {
            throw new NotImplementedException();
        }

        Task IPostService.IncrementViewCountAsync(int postId)
        {
            throw new NotImplementedException();
        }
    }
}

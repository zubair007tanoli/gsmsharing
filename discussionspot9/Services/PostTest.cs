using discussionspot9.Data.DbContext;
using discussionspot9.Helpers;
using discussionspot9.Interfaces;
using discussionspot9.Models.Domain;
using discussionspot9.Models.ViewModels.CreativeViewModels;
using discussionspot9.Services.ServiceResults;
using Microsoft.EntityFrameworkCore;

namespace discussionspot9.Services
{
    public class PostTest: IPostTest
    {
        private readonly ApplicationDbContext _context;

        public PostTest(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CreatePostResult> CreatePostUpdatedAsync(CreatePostViewModel model)
        {
            // Generate slug from title
            var slug = model.Title.ToSlug();

            // Ensure unique slug within community
            var existingCount = await _context.Posts
                .Where(p => p.CommunityId == model.CommunityId && p.Slug.StartsWith(slug))
                .CountAsync();

            slug = existingCount > 0 ? $"{slug}-{existingCount + 1}" : slug;

            // Create the post entity
            var post = new Post
            {
                Title = model.Title,
                Slug = slug,
                Content = model.Content,
                PostType = model.PostType,
                Url = model.Url,
                UserId = model.UserId,
                CommunityId = model.CommunityId,
                IsNSFW = model.IsNSFW,
                IsSpoiler = model.IsSpoiler,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Status = !string.IsNullOrEmpty(model.Status) ? model.Status : "published",
                IsPinned = model.IsPinned,
                IsLocked = model.IsLocked,
                HasPoll = model.PostType == "poll" && model.PollOptions?.Count > 0,
                PollExpiresAt = model.PollEndDate,
                PollOptionCount = model.PollOptions?.Count ?? 0,
                PollVoteCount = 0
            };

            _context.Posts.Add(post);
            await _context.SaveChangesAsync(); // Initial save to get PostId

            // Process all content types in parallel

            await ProcessTagsAsync(post.PostId, model);
            await ProcessMediaFilesAsync(post.PostId, model);
            await ProcessMediaUrlsAsync(post.PostId, model);
            await ProcessPollAsync(post.PostId, model);
               

            // Update community post count
            var community = await _context.Communities.FindAsync(model.CommunityId);
            if (community != null)
            {
                community.PostCount++;
                await _context.SaveChangesAsync();
            }

            return new CreatePostResult { Success = true, PostSlug = slug };
        }

        private async Task ProcessTagsAsync(int postId, CreatePostViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.TagsInput))
                return;

            var tagNames = model.TagsInput.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Trim().ToLower())
                .Distinct();

            foreach (var tagName in tagNames)
            {
                var tag = await _context.Tags.FirstOrDefaultAsync(t => t.Name == tagName);

                if (tag == null)
                {
                    // Create new tag
                    tag = new Tag
                    {
                        Name = tagName,
                        Slug = tagName.ToSlug(),
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Tags.Add(tag);
                    await _context.SaveChangesAsync(); // Save to get the TagId
                }

                // Now we have a valid TagId
                _context.PostTags.Add(new PostTag
                {
                    PostId = postId,
                    TagId = tag.TagId
                });
            }

            await _context.SaveChangesAsync(); // Save the PostTag relationships
        }
        private async Task ProcessMediaFilesAsync(int postId, CreatePostViewModel model)
        {
            if (model.MediaFiles == null || model.MediaFiles.Count == 0)
                return;

            foreach (var file in model.MediaFiles)
            {
                // Determine media type based on content type
                var mediaType = file.ContentType switch
                {
                    string ct when ct.StartsWith("image/") => "image",
                    string ct when ct.StartsWith("video/") => "video",
                    string ct when ct.StartsWith("audio/") => "audio",
                    _ => "document"
                };

                var media = new Media
                {
                    PostId = postId,
                    Url = $"/uploads/{Guid.NewGuid()}_{file.FileName}",
                    MediaType = mediaType,
                    ContentType = file.ContentType,
                    FileName = file.FileName,
                    Caption = model.MediaCaption,
                    AltText = model.MediaAltText,
                    UploadedAt = DateTime.UtcNow,
                    UserId = model.UserId
                };

                _context.Media.Add(media);

                // Here you would add actual file upload logic to your storage system
                // await _fileService.UploadAsync(file, media.Url);
            }
            await _context.SaveChangesAsync();
        }

        private async Task ProcessMediaUrlsAsync(int postId, CreatePostViewModel model)
        {
            if (model.MediaUrls == null || model.MediaUrls.Count == 0)
                return;

            foreach (var url in model.MediaUrls)
            {
                var media = new Media
                {
                    PostId = postId,
                    Url = url,
                    MediaType = "image", // Default to image, consider URL analysis
                    UploadedAt = DateTime.UtcNow,
                    UserId = model.UserId
                };
                _context.Media.Add(media);
            }
            await _context.SaveChangesAsync();
        }

        private async Task ProcessPollAsync(int postId, CreatePostViewModel model)
        {
            if (model.PostType != "poll" || model.PollOptions == null || model.PollOptions.Count < 2)
                return;

            // Create poll configuration
            var pollConfig = new PollConfiguration
            {
                PostId = postId,
                PollQuestion = model.PollQuestion, // Ensure this is set
                PollDescription = model.PollDescription, // Ensure this is set
                AllowMultipleChoices = model.AllowMultipleChoices,
                ShowResultsBeforeVoting = model.ShowResultsBeforeVoting,
                ShowResultsBeforeEnd = model.ShowResultsBeforeEnd,
                AllowAddingOptions = model.AllowAddingOptions,
                MinOptions = Math.Max(2, model.MinOptions),
                MaxOptions = Math.Min(20, model.MaxOptions),
                EndDate = model.PollEndDate,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                ClosedByUserId = null,
                ClosedAt = null
            };
            _context.PollConfigurations.Add(pollConfig);

            // Add poll options
            for (int i = 0; i < model.PollOptions.Count; i++)
            {
                _context.PollOptions.Add(new PollOption
                {
                    PostId = postId,
                    OptionText = model.PollOptions[i],
                    DisplayOrder = i,
                    VoteCount = 0,
                    CreatedAt = DateTime.UtcNow
                });
            }
            await _context.SaveChangesAsync();
        }
    }
}

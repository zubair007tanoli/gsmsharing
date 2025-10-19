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
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<PostTest> _logger;

        public PostTest(ApplicationDbContext context, IFileStorageService fileStorageService, ILogger<PostTest> logger)
        {
            _context = context;
            _fileStorageService = fileStorageService;
            _logger = logger;
        }

        public async Task<CreatePostResult> CreatePostUpdatedAsync(CreatePostViewModel model)
        {
            // Sanitize data based on post type (removes irrelevant fields)
            model.SanitizeDataByPostType();
            
            // Generate slug from title
            var slug = model.Title.ToSlug();

            // Ensure unique slug within community
            var existingCount = await _context.Posts
                .Where(p => p.CommunityId == model.CommunityId && p.Slug.StartsWith(slug))
                .CountAsync();

            slug = existingCount > 0 ? $"{slug}-{existingCount + 1}" : slug;

            // Filter out empty poll options
            var validPollOptions = model.PollOptions?
                .Where(o => !string.IsNullOrWhiteSpace(o))
                .Select(o => o.Trim())
                .ToList() ?? new List<string>();

            // Create the post entity (only save non-null values)
            var post = new Post
            {
                Title = model.Title.Trim(),
                Slug = slug,
                Content = string.IsNullOrWhiteSpace(model.Content) ? null : model.Content.Trim(),
                PostType = model.PostType,
                Url = string.IsNullOrWhiteSpace(model.Url) ? null : model.Url.Trim(),
                UserId = model.UserId,
                CommunityId = model.CommunityId,
                IsNSFW = model.IsNSFW,
                IsSpoiler = model.IsSpoiler,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Status = !string.IsNullOrEmpty(model.Status) ? model.Status : "published",
                IsPinned = model.IsPinned,
                IsLocked = model.IsLocked,
                HasPoll = model.PostType == "poll" && validPollOptions.Count > 0,
                PollExpiresAt = model.PollEndDate,
                PollOptionCount = validPollOptions.Count,
                PollVoteCount = 0
            };

            _context.Posts.Add(post);
            await _context.SaveChangesAsync(); // Initial save to get PostId

            // Process content types based on post type
            await ProcessTagsAsync(post.PostId, model);
            
            if (model.PostType == "poll")
            {
                await ProcessPollAsync(post.PostId, model, validPollOptions);
            }
            
            if (model.MediaFiles?.Count > 0 || model.MediaUrls?.Count > 0)
            {
                await ProcessMediaFilesAsync(post.PostId, model);
                await ProcessMediaUrlsAsync(post.PostId, model);
            }
            
            // Generate and save SEO metadata
            await GenerateAndSaveSeoMetadataAsync(post.PostId, model, post);

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
                try
                {
                    // Determine media type based on content type
                    var mediaType = file.ContentType switch
                    {
                        string ct when ct.StartsWith("image/") => "image",
                        string ct when ct.StartsWith("video/") => "video",
                        string ct when ct.StartsWith("audio/") => "audio",
                        _ => "document"
                    };

                    // Save the actual file to disk
                    _logger.LogInformation("Saving {MediaType} file for post {PostId}: {FileName}", mediaType, postId, file.FileName);
                    var fileUrl = await _fileStorageService.SaveFileAsync(file, $"posts/{mediaType}s");

                    var media = new Media
                    {
                        PostId = postId,
                        Url = fileUrl,  // Use actual saved file URL
                        MediaType = mediaType,
                        ContentType = file.ContentType,
                        FileName = file.FileName,
                        FileSize = file.Length,
                        Caption = model.MediaCaption,
                        AltText = model.MediaAltText,
                        UploadedAt = DateTime.UtcNow,
                        UserId = model.UserId,
                        StorageProvider = "local",
                        IsProcessed = true
                    };

                    _context.Media.Add(media);
                    _logger.LogInformation("Media file saved successfully: {Url}", fileUrl);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing media file: {FileName}", file.FileName);
                    // Continue with other files even if one fails
                }
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

        private async Task ProcessPollAsync(int postId, CreatePostViewModel model, List<string> validPollOptions)
        {
            if (model.PostType != "poll" || validPollOptions == null || validPollOptions.Count < 2)
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

            // Add poll options (only valid ones)
            for (int i = 0; i < validPollOptions.Count; i++)
            {
                _context.PollOptions.Add(new PollOption
                {
                    PostId = postId,
                    OptionText = validPollOptions[i],
                    DisplayOrder = i,
                    VoteCount = 0,
                    CreatedAt = DateTime.UtcNow
                });
            }
            await _context.SaveChangesAsync();
        }
        
        private async Task GenerateAndSaveSeoMetadataAsync(int postId, CreatePostViewModel model, Post post)
        {
            try
            {
                // Generate meta description if not provided
                string metaDescription = model.MetaDescription;
                if (string.IsNullOrWhiteSpace(metaDescription))
                {
                    // Auto-generate from content or title
                    if (!string.IsNullOrWhiteSpace(post.Content))
                    {
                        metaDescription = post.Content.Length > 160
                            ? post.Content.Substring(0, 157) + "..."
                            : post.Content;
                    }
                    else
                    {
                        metaDescription = $"{post.Title} - Discussion on community";
                    }
                }
                
                // Generate keywords if not provided
                string keywords = model.Keywords;
                if (string.IsNullOrWhiteSpace(keywords))
                {
                    // Auto-generate from title and tags
                    var keywordList = new List<string>();
                    
                    // Extract important words from title
                    var titleWords = post.Title
                        .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                        .Where(w => w.Length > 3)
                        .Take(5);
                    keywordList.AddRange(titleWords);
                    
                    // Add tags if available
                    if (!string.IsNullOrWhiteSpace(model.TagsInput))
                    {
                        var tags = model.TagsInput
                            .Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(t => t.Trim());
                        keywordList.AddRange(tags);
                    }
                    
                    keywords = string.Join(", ", keywordList.Distinct());
                }
                
                // Create SEO metadata entry
                var seoMetadata = new SeoMetadata
                {
                    EntityType = "Post",
                    EntityId = postId,
                    MetaTitle = model.MetaTitle ?? post.Title,
                    MetaDescription = metaDescription,
                    CanonicalUrl = model.CanonicalUrl,
                    OgTitle = model.MetaTitle ?? post.Title,
                    OgDescription = metaDescription,
                    OgImageUrl = null, // Set when image is processed
                    Keywords = keywords,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                
                _context.SeoMetadata.Add(seoMetadata);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log error but don't fail post creation
                System.Diagnostics.Debug.WriteLine($"Failed to generate SEO metadata: {ex.Message}");
            }
        }
    }
}

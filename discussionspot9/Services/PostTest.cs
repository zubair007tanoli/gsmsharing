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
        private readonly IStoryGenerationService _storyGenerationService;

        public PostTest(ApplicationDbContext context, IFileStorageService fileStorageService, ILogger<PostTest> logger, IStoryGenerationService storyGenerationService)
        {
            _context = context;
            _fileStorageService = fileStorageService;
            _logger = logger;
            _storyGenerationService = storyGenerationService;
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
            
            _logger.LogInformation("✅ Post entity saved, PostId={PostId}. Starting related entity processing...", post.PostId);

            // ============================================
            // CRITICAL: Process all related entities SEQUENTIALLY
            // to avoid DbContext concurrency issues
            // ============================================
            
            try
            {
                // Process content types based on post type
                await ProcessTagsAsync(post.PostId, model);
                
                if (model.PostType == "poll")
                {
                    await ProcessPollAsync(post.PostId, model, validPollOptions);
                }
                
                // Process media in a unified way
                bool hasMediaFiles = model.MediaFiles?.Count > 0;
                bool hasMediaUrls = model.MediaUrls?.Count > 0;
                
                if (hasMediaFiles || hasMediaUrls)
                {
                    _logger.LogInformation("📸 ========== MEDIA PROCESSING START ==========");
                    _logger.LogInformation("📸 Post ID: {PostId}", post.PostId);
                    _logger.LogInformation("📸 Post Title: {Title}", post.Title);
                    _logger.LogInformation("📸 Post Type: {PostType}", post.PostType);
                    _logger.LogInformation("📸 MediaFiles count: {FileCount}", model.MediaFiles?.Count ?? 0);
                    _logger.LogInformation("📸 MediaUrls count: {UrlCount}", model.MediaUrls?.Count ?? 0);
                    
                    // Process uploaded files first (adds to context, doesn't save)
                    if (hasMediaFiles)
                    {
                        _logger.LogInformation("📂 Processing uploaded files...");
                        await ProcessMediaFilesAsync(post.PostId, model);
                    }
                    
                    // Then process URLs (adds to context, doesn't save)
                    if (hasMediaUrls)
                    {
                        _logger.LogInformation("🔗 Processing media URLs...");
                        await ProcessMediaUrlsAsync(post.PostId, model);
                    }
                    
                    // CRITICAL: Save all media in ONE transaction to avoid concurrency issues
                    try
                    {
                        _logger.LogInformation("💾 Saving all media to database...");
                        await _context.SaveChangesAsync();
                        _logger.LogInformation("✅ All media saved successfully to database for post {PostId}", post.PostId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "❌ CRITICAL: Failed to save media to database for post {PostId}", post.PostId);
                        throw;
                    }
                    
                    _logger.LogInformation("✅ ========== MEDIA PROCESSING COMPLETE ==========");
                }
                else
                {
                    _logger.LogInformation("ℹ️ No media files or URLs provided for post {PostId}", post.PostId);
                }
                
                // Generate and save SEO metadata
                await GenerateAndSaveSeoMetadataAsync(post.PostId, model, post);

                // Update community post count - Last operation
                var community = await _context.Communities.FindAsync(model.CommunityId);
                if (community != null)
                {
                    community.PostCount++;
                    await _context.SaveChangesAsync();
                }
                
                _logger.LogInformation("✅ All related entities processed for post {PostId}", post.PostId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error processing related entities for post {PostId}", post.PostId);
                // Post is created, but related data might be incomplete
                // This is better than failing the entire post creation
            }

            // IMPORTANT: Ensure ALL database operations complete before queuing background tasks
            // This prevents DbContext concurrency issues
            _logger.LogInformation("✅ All database operations complete for post {PostId}. Ready to queue background tasks.", post.PostId);

            // Queue story generation AFTER all DB operations complete
            // This runs in its own scope and won't interfere with the current context
            try
            {
                var storyOptions = new StoryGenerationOptions
                {
                    AutoGenerate = true,
                    UseAI = true,
                    Style = DetermineOptimalStyle(model.PostType, model.Content),
                    Length = DetermineOptimalLength(model.PostType, model.Content),
                    Keywords = ExtractKeywords(model.Title, model.Content, model.TagsInput)
                };
                
                // Don't await - let it run in background with its own scope
                _ = _storyGenerationService.QueueStoryGenerationAsync(post.PostId, storyOptions);
                _logger.LogInformation("📝 Queued story generation for post {PostId} (background task)", post.PostId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to queue story generation for post {PostId}", post.PostId);
                // Don't fail the post creation if story generation fails
            }

            _logger.LogInformation("🎉 Post creation workflow complete for PostId={PostId}, Slug={Slug}", post.PostId, slug);
            return new CreatePostResult { Success = true, PostId = post.PostId, PostSlug = slug };
        }

        private string DetermineOptimalStyle(string postType, string? content)
        {
            if (string.IsNullOrEmpty(content)) return "informative";
            
            var contentLower = content.ToLower();
            
            return postType switch
            {
                "poll" => "engaging",
                "image" => "visual",
                "video" => "dynamic",
                "link" => "informative",
                _ when contentLower.Contains("question") || contentLower.Contains("?") => "engaging",
                _ when contentLower.Contains("tutorial") || contentLower.Contains("how to") => "educational",
                _ when contentLower.Contains("news") || contentLower.Contains("update") => "informative",
                _ when contentLower.Contains("funny") || contentLower.Contains("joke") => "entertaining",
                _ => "informative"
            };
        }

        private string DetermineOptimalLength(string postType, string? content)
        {
            if (string.IsNullOrEmpty(content)) return "medium";
            
            var wordCount = content.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
            
            return postType switch
            {
                "poll" => "short",
                "image" => wordCount > 200 ? "medium" : "short",
                "video" => "medium",
                "link" => wordCount > 300 ? "long" : "medium",
                _ when wordCount < 100 => "short",
                _ when wordCount > 500 => "long",
                _ => "medium"
            };
        }

        private string? ExtractKeywords(string title, string? content, string? tagsInput)
        {
            var keywords = new List<string>();
            
            // Add title words (excluding common words)
            var titleWords = title.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Where(w => w.Length > 3 && !IsCommonWord(w))
                .Take(3);
            keywords.AddRange(titleWords);
            
            // Add tags if provided
            if (!string.IsNullOrEmpty(tagsInput))
            {
                var tags = tagsInput.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => t.Trim())
                    .Where(t => !string.IsNullOrEmpty(t))
                    .Take(5);
                keywords.AddRange(tags);
            }
            
            // Extract key phrases from content
            if (!string.IsNullOrEmpty(content))
            {
                var contentWords = content.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Where(w => w.Length > 4 && !IsCommonWord(w))
                    .GroupBy(w => w.ToLower())
                    .OrderByDescending(g => g.Count())
                    .Take(3)
                    .Select(g => g.Key);
                keywords.AddRange(contentWords);
            }
            
            return keywords.Any() ? string.Join(", ", keywords.Distinct().Take(8)) : null;
        }

        private bool IsCommonWord(string word)
        {
            var commonWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "the", "and", "or", "but", "in", "on", "at", "to", "for", "of", "with", "by",
                "this", "that", "these", "those", "is", "are", "was", "were", "be", "been",
                "have", "has", "had", "do", "does", "did", "will", "would", "could", "should",
                "can", "may", "might", "must", "shall", "a", "an", "as", "if", "when", "where",
                "why", "how", "what", "which", "who", "whom", "whose", "about", "above", "below"
            };
            return commonWords.Contains(word.ToLower());
        }

        private async Task ProcessTagsAsync(int postId, CreatePostViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.TagsInput))
            {
                _logger.LogInformation("No tags to process for post {PostId}", postId);
                return;
            }

            var tagNames = model.TagsInput.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Trim().ToLower())
                .Distinct()
                .ToList();
                
            _logger.LogInformation("Processing {Count} tags for post {PostId}", tagNames.Count, postId);

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
                    // IMPORTANT: Save immediately to get TagId for FK relationship
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("✅ Created new tag: {TagName} (TagId={TagId})", tagName, tag.TagId);
                }

                // Now we have a valid TagId
                _context.PostTags.Add(new PostTag
                {
                    PostId = postId,
                    TagId = tag.TagId
                });
            }

            // Save all PostTag relationships in one transaction
            if (tagNames.Any())
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("✅ {Count} tags linked to post {PostId}", tagNames.Count, postId);
            }
        }
        private async Task ProcessMediaFilesAsync(int postId, CreatePostViewModel model)
        {
            if (model.MediaFiles == null || model.MediaFiles.Count == 0)
            {
                _logger.LogInformation("No media files to process for post {PostId}", postId);
                return;
            }

            _logger.LogInformation("📁 Processing {Count} media files for post {PostId}", model.MediaFiles.Count, postId);

            int successCount = 0;
            int failCount = 0;

            foreach (var file in model.MediaFiles)
            {
                try
                {
                    _logger.LogInformation("📎 Processing file {FileNumber}/{Total}: {FileName}, Size: {Size} bytes, ContentType: {ContentType}", 
                        successCount + failCount + 1, model.MediaFiles.Count, file.FileName, file.Length, file.ContentType);

                    // Determine media type based on content type
                    var mediaType = file.ContentType switch
                    {
                        string ct when ct.StartsWith("image/") => "image",
                        string ct when ct.StartsWith("video/") => "video",
                        string ct when ct.StartsWith("audio/") => "audio",
                        _ => "document"
                    };

                    // Save the actual file to disk
                    _logger.LogInformation("💾 Saving {MediaType} file to disk...", mediaType);
                    var fileUrl = await _fileStorageService.SaveFileAsync(file, $"posts/{mediaType}s");
                    
                    _logger.LogInformation("✅ File saved successfully to: {FileUrl}", fileUrl);
                    
                    // Ensure the URL is properly formatted for web access
                    if (!fileUrl.StartsWith("/") && !fileUrl.StartsWith("http"))
                    {
                        fileUrl = "/" + fileUrl.TrimStart('/');
                        _logger.LogInformation("🔧 URL adjusted to: {FileUrl}", fileUrl);
                    }

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
                    successCount++;
                    _logger.LogInformation("✅ Media record #{Number} added to context: PostId={PostId}, Url={Url}, Type={Type}, FileName={FileName}", 
                        successCount, postId, fileUrl, mediaType, file.FileName);
                }
                catch (Exception ex)
                {
                    failCount++;
                    _logger.LogError(ex, "❌ ERROR processing media file {FileNumber}/{Total}: {FileName} - {ErrorMessage}", 
                        successCount + failCount, model.MediaFiles.Count, file.FileName, ex.Message);
                    // Continue with other files even if one fails
                }
            }
            
            // DON'T save yet - let caller save all media together
            _logger.LogInformation("ℹ️ {SuccessCount} media file record(s) added to context (not yet saved)", successCount);
            
            if (failCount > 0)
            {
                _logger.LogWarning("⚠️ WARNING: {FailCount} media file(s) failed to process", failCount);
            }
        }

        private async Task ProcessMediaUrlsAsync(int postId, CreatePostViewModel model)
        {
            // MediaUrls might come as a List or might need to be parsed from form data
            List<string> urlList = new();
            
            if (model.MediaUrls != null && model.MediaUrls.Count > 0)
            {
                urlList = model.MediaUrls;
            }
            
            if (urlList.Count == 0)
            {
                _logger.LogInformation("No media URLs to process for post {PostId}", postId);
                return;
            }

            _logger.LogInformation("Processing {Count} media URLs for post {PostId}", urlList.Count, postId);

            int urlCount = 0;
            foreach (var url in urlList)
            {
                if (string.IsNullOrWhiteSpace(url))
                {
                    _logger.LogWarning("Skipping empty URL for post {PostId}", postId);
                    continue;
                }

                _logger.LogInformation("Processing URL: {Url}", url);

                // Ensure URL is properly formatted
                var processedUrl = url.Trim();
                if (!processedUrl.StartsWith("http") && !processedUrl.StartsWith("/"))
                {
                    processedUrl = "/" + processedUrl.TrimStart('/');
                    _logger.LogInformation("URL adjusted to: {ProcessedUrl}", processedUrl);
                }

                var media = new Media
                {
                    PostId = postId,
                    Url = processedUrl,
                    MediaType = "image", // Default to image, consider URL analysis
                    UploadedAt = DateTime.UtcNow,
                    UserId = model.UserId,
                    StorageProvider = "external",
                    IsProcessed = true
                };
                _context.Media.Add(media);
                urlCount++;
                _logger.LogInformation("✅ Media URL record #{Number} added to context: {Url}", urlCount, processedUrl);
            }
            
            // DON'T save yet - let caller save all media together
            _logger.LogInformation("ℹ️ {Count} media URL record(s) added to context (not yet saved)", urlCount);
        }

        private async Task ProcessPollAsync(int postId, CreatePostViewModel model, List<string> validPollOptions)
        {
            if (model.PostType != "poll" || validPollOptions == null || validPollOptions.Count < 2)
            {
                _logger.LogInformation("No poll to process for post {PostId}", postId);
                return;
            }
            
            _logger.LogInformation("📊 Processing poll with {Count} options for post {PostId}", validPollOptions.Count, postId);

            // Create poll configuration
            var pollConfig = new PollConfiguration
            {
                PostId = postId,
                PollQuestion = model.PollQuestion,
                PollDescription = model.PollDescription,
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
            
            // Save poll configuration and options in one transaction
            await _context.SaveChangesAsync();
            _logger.LogInformation("✅ Poll configuration and {Count} options saved for post {PostId}", validPollOptions.Count, postId);
        }
        
        private async Task GenerateAndSaveSeoMetadataAsync(int postId, CreatePostViewModel model, Post post)
        {
            try
            {
                _logger.LogInformation("🔍 Generating SEO metadata for post {PostId}", postId);
                
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
                _logger.LogInformation("✅ SEO metadata saved for post {PostId}", postId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to generate SEO metadata for post {PostId}", postId);
                // Log error but don't fail post creation
            }
        }
    }
}

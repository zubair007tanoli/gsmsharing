using discussionspot9.Data.DbContext;
using discussionspot9.Interfaces;
using discussionspot9.Models.ViewModels.CreativeViewModels;
using Microsoft.EntityFrameworkCore;

namespace discussionspot9.Services
{
    /// <summary>
    /// Background service for processing SEO optimizations asynchronously
    /// </summary>
    public class BackgroundSeoService : IBackgroundSeoService
    {
        private readonly ILogger<BackgroundSeoService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public BackgroundSeoService(
            ILogger<BackgroundSeoService> logger,
            IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        /// <summary>
        /// Process SEO optimization for a post in the background
        /// </summary>
        public void ProcessPostSeoAsync(int postId, CreatePostViewModel model, int communityId)
        {
            // Fire and forget - runs in background
            _ = Task.Run(async () =>
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var seoAnalyzerService = scope.ServiceProvider.GetRequiredService<ISeoAnalyzerService>();

                    _logger.LogInformation("🔍 [Background] Running SEO analysis on post ID: {PostId}", postId);

                    // Get the post from database
                    var post = await context.Posts
                        .FirstOrDefaultAsync(p => p.PostId == postId);

                    if (post == null)
                    {
                        _logger.LogWarning("⚠️ [Background] Post not found: {PostId}", postId);
                        return;
                    }

                    // Run SEO optimization
                    var optimizedModel = await seoAnalyzerService.OptimizePostAsync(model);

                    if (optimizedModel?.SeoMetadata != null)
                    {
                        // Update SEO metadata
                        var existingSeo = await context.SeoMetadata
                            .FirstOrDefaultAsync(s => s.EntityType == "Post" && s.EntityId == postId);

                        if (existingSeo != null)
                        {
                            // Update with AI-optimized data
                            if (!string.IsNullOrWhiteSpace(optimizedModel.SeoMetadata.MetaDescription))
                            {
                                existingSeo.MetaDescription = optimizedModel.SeoMetadata.MetaDescription;
                                existingSeo.OgDescription = optimizedModel.SeoMetadata.MetaDescription;
                                existingSeo.TwitterDescription = optimizedModel.SeoMetadata.MetaDescription;
                            }

                            if (!string.IsNullOrWhiteSpace(optimizedModel.SeoMetadata.Keywords))
                            {
                                existingSeo.Keywords = optimizedModel.SeoMetadata.Keywords;
                            }

                            existingSeo.UpdatedAt = DateTime.UtcNow;

                            await context.SaveChangesAsync();

                            _logger.LogInformation(
                                "✅ [Background] SEO update complete for post: {PostId}. Score: {Score}, Improvements: {Count}",
                                postId,
                                optimizedModel.SeoMetadata.SeoScore,
                                optimizedModel.SeoMetadata.ImprovementsMade?.Count ?? 0
                            );
                        }
                        else
                        {
                            _logger.LogWarning("⚠️ [Background] SEO metadata not found for post: {PostId}", postId);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ [Background] SEO analysis failed for post: {PostId}", postId);
                }
            });
        }

        /// <summary>
        /// Batch process SEO for multiple posts
        /// </summary>
        public void ProcessMultiplePostsSeoAsync(List<int> postIds)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var seoAnalyzerService = scope.ServiceProvider.GetRequiredService<ISeoAnalyzerService>();

                    _logger.LogInformation("🔍 [Background] Batch processing SEO for {Count} posts", postIds.Count);

                    foreach (var postId in postIds)
                    {
                        try
                        {
                            var post = await context.Posts.FindAsync(postId);
                            if (post == null) continue;

                            var model = new CreatePostViewModel
                            {
                                Title = post.Title,
                                Content = post.Content,
                                PostType = post.PostType,
                                Url = post.Url
                            };

                            var optimizedModel = await seoAnalyzerService.OptimizePostAsync(model);

                            if (optimizedModel?.SeoMetadata != null)
                            {
                                var existingSeo = await context.SeoMetadata
                                    .FirstOrDefaultAsync(s => s.EntityType == "Post" && s.EntityId == postId);

                                if (existingSeo != null)
                                {
                                    existingSeo.MetaDescription = optimizedModel.SeoMetadata.MetaDescription ?? existingSeo.MetaDescription;
                                    existingSeo.Keywords = optimizedModel.SeoMetadata.Keywords ?? existingSeo.Keywords;
                                    existingSeo.UpdatedAt = DateTime.UtcNow;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "⚠️ [Background] Failed to process SEO for post: {PostId}", postId);
                        }

                        // Small delay between posts to avoid overwhelming the system
                        await Task.Delay(500);
                    }

                    await context.SaveChangesAsync();
                    _logger.LogInformation("✅ [Background] Batch SEO processing complete");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ [Background] Batch SEO processing failed");
                }
            });
        }
    }
}


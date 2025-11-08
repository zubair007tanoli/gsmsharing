using discussionspot9.Data.DbContext;
using discussionspot9.Interfaces;
using discussionspot9.Models.ViewModels.CreativeViewModels;
using Microsoft.EntityFrameworkCore;

namespace discussionspot9.Services
{
    /// <summary>
    /// Background service that runs weekly SEO optimization
    /// Runs every Sunday at 2 AM
    /// </summary>
    public class WeeklySeoOptimizationService : BackgroundService
    {
        private readonly ILogger<WeeklySeoOptimizationService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly TimeSpan _scheduledTime = new TimeSpan(2, 0, 0); // 2 AM

        public WeeklySeoOptimizationService(
            ILogger<WeeklySeoOptimizationService> logger,
            IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("🚀 Weekly SEO Optimization Service started");

            // Run an initial cycle so freshly started instances don't wait a full week
            try
            {
                await RunOptimizationCycleAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Initial weekly optimization cycle failed");
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var now = DateTime.Now;
                    var nextRun = GetNextSundayAt2AM(now);
                    var delay = nextRun - now;

                    _logger.LogInformation("⏰ Next optimization run scheduled for: {NextRun}", nextRun);

                    await Task.Delay(delay, stoppingToken);

                    if (!stoppingToken.IsCancellationRequested)
                    {
                        await RunOptimizationCycleAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error in weekly optimization service");
                    // Wait 1 hour before retrying
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                }
            }
        }

        private DateTime GetNextSundayAt2AM(DateTime from)
        {
            var daysUntilSunday = ((int)DayOfWeek.Sunday - (int)from.DayOfWeek + 7) % 7;
            
            if (daysUntilSunday == 0 && from.TimeOfDay > _scheduledTime)
            {
                daysUntilSunday = 7; // Next Sunday
            }

            var nextSunday = from.Date.AddDays(daysUntilSunday);
            return nextSunday.Add(_scheduledTime);
        }

        private async Task RunOptimizationCycleAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var selectorService = scope.ServiceProvider.GetRequiredService<SmartPostSelectorService>();
            var seoService = scope.ServiceProvider.GetRequiredService<ISeoAnalyzerService>();

            _logger.LogInformation("🎯 Starting weekly optimization cycle...");

            try
            {
                // Step 1: Select posts for optimization (revenue-focused)
                var candidates = await selectorService.SelectPostsForOptimizationAsync(maxPosts: 40); // Conservative: 40 posts/week

                if (!candidates.Any())
                {
                    _logger.LogInformation("ℹ️ No posts need optimization this week");
                    return;
                }

                _logger.LogInformation("📝 Found {Count} posts to optimize", candidates.Count);

                // Step 2: Queue them
                await selectorService.QueuePostsForOptimizationAsync(candidates);

                // Step 3: Process pending queue items (semi-automated)
                var queueItems = await context.PostSeoQueues
                    .Where(q => q.Status == "Pending" && !q.RequiresApproval)
                    .OrderBy(q => q.Priority)
                    .Take(20) // Process 20 at a time
                    .Include(q => q.Post)
                    .ToListAsync();

                foreach (var item in queueItems)
                {
                    try
                    {
                        item.Status = "Processing";
                        await context.SaveChangesAsync();

                        await OptimizePostAsync(item, context, seoService);

                        item.Status = "Completed";
                        item.ProcessedAt = DateTime.UtcNow;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "❌ Failed to optimize post {PostId}", item.PostId);
                        item.Status = "Failed";
                        item.ErrorMessage = ex.Message;
                    }

                    await context.SaveChangesAsync();
                    await Task.Delay(1000); // 1 second between posts
                }

                _logger.LogInformation("✅ Weekly optimization cycle completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error during optimization cycle");
            }
        }

        private async Task OptimizePostAsync(
            Models.Domain.PostSeoQueue queueItem,
            ApplicationDbContext context,
            ISeoAnalyzerService seoService)
        {
            var post = queueItem.Post;
            
            if (post == null)
                return;

            _logger.LogInformation("🔧 Optimizing post: {Title} (ID: {PostId})", post.Title, post.PostId);

            // Create view model for SEO analysis
            var model = new CreatePostViewModel
            {
                Title = post.Title,
                Content = post.Content,
                PostType = post.PostType,
                Url = post.Url
            };

            // Run SEO analysis
            var optimized = await seoService.OptimizePostAsync(model);

            if (optimized?.SeoMetadata != null)
            {
                // Update SEO metadata in database
                var seoMetadata = await context.SeoMetadata
                    .FirstOrDefaultAsync(s => s.EntityType == "Post" && s.EntityId == post.PostId);

                bool metaDescriptionChanged = false;
                bool keywordsChanged = false;

                if (seoMetadata != null)
                {
                    if (!string.IsNullOrWhiteSpace(optimized.SeoMetadata.MetaDescription) &&
                        seoMetadata.MetaDescription != optimized.SeoMetadata.MetaDescription)
                    {
                        // Log the change
                        context.SeoOptimizationLogs.Add(new Models.Domain.SeoOptimizationLog
                        {
                            PostId = post.PostId,
                            OptimizedAt = DateTime.UtcNow,
                            ChangeType = "MetaDescription",
                            OldValue = seoMetadata.MetaDescription,
                            NewValue = optimized.SeoMetadata.MetaDescription,
                            Trigger = queueItem.Reason,
                            Status = "Applied",
                            IsApproved = false // Requires review
                        });

                        seoMetadata.MetaDescription = optimized.SeoMetadata.MetaDescription;
                        seoMetadata.OgDescription = optimized.SeoMetadata.MetaDescription;
                        metaDescriptionChanged = true;
                    }

                    if (!string.IsNullOrWhiteSpace(optimized.SeoMetadata.Keywords) &&
                        seoMetadata.Keywords != optimized.SeoMetadata.Keywords)
                    {
                        context.SeoOptimizationLogs.Add(new Models.Domain.SeoOptimizationLog
                        {
                            PostId = post.PostId,
                            OptimizedAt = DateTime.UtcNow,
                            ChangeType = "Keywords",
                            OldValue = seoMetadata.Keywords,
                            NewValue = optimized.SeoMetadata.Keywords,
                            Trigger = queueItem.Reason,
                            Status = "Applied",
                            IsApproved = false
                        });

                        seoMetadata.Keywords = optimized.SeoMetadata.Keywords;
                        keywordsChanged = true;
                    }

                    seoMetadata.UpdatedAt = DateTime.UtcNow;
                }

                if (metaDescriptionChanged || keywordsChanged)
                {
                    await context.SaveChangesAsync();
                    _logger.LogInformation("✅ Updated SEO for post {PostId}", post.PostId);
                }
                else
                {
                    _logger.LogInformation("ℹ️ No changes needed for post {PostId}", post.PostId);
                }
            }
        }
    }
}


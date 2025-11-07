using discussionspot9.Data.DbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace discussionspot9.Services
{
    /// <summary>
    /// Nightly background service to re-score all posts for SEO
    /// Runs at 2:00 AM server time
    /// </summary>
    public class NightlySeoRescoreService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<NightlySeoRescoreService> _logger;
        private readonly TimeSpan _runTime = new TimeSpan(2, 0, 0); // 2:00 AM

        public NightlySeoRescoreService(
            IServiceProvider serviceProvider,
            ILogger<NightlySeoRescoreService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("🌙 Nightly SEO Re-score Service started. Will run at {RunTime} server time.", _runTime);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var now = DateTime.Now;
                    var nextRun = DateTime.Today.Add(_runTime);

                    // If it's past 2 AM today, schedule for tomorrow
                    if (now >= nextRun)
                    {
                        nextRun = nextRun.AddDays(1);
                    }

                    var delay = nextRun - now;
                    _logger.LogInformation("⏰ Next SEO re-score scheduled for: {NextRun} (in {Hours} hours)", 
                        nextRun, delay.TotalHours.ToString("F1"));

                    await Task.Delay(delay, stoppingToken);

                    if (!stoppingToken.IsCancellationRequested)
                    {
                        await PerformRescoreAsync(stoppingToken);
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Nightly SEO Re-score Service is stopping.");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error in Nightly SEO Re-score Service");
                    // Wait 1 hour before retrying on error
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                }
            }
        }

        private async Task PerformRescoreAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("🔄 Starting nightly SEO re-score...");

            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var scoringService = scope.ServiceProvider.GetRequiredService<SeoScoringService>();

            try
            {
                // Get all published posts
                var posts = await context.Posts
                    .Where(p => p.Status == "published")
                    .Select(p => p.PostId)
                    .ToListAsync(cancellationToken);

                _logger.LogInformation("📊 Found {Count} published posts to re-score", posts.Count);

                var totalPosts = posts.Count;
                var processed = 0;
                var successCount = 0;
                var failedCount = 0;

                // Process in batches of 10 to avoid overwhelming the system
                var batchSize = 10;
                for (int i = 0; i < posts.Count; i += batchSize)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        _logger.LogInformation("⏹️ Re-score cancelled");
                        break;
                    }

                    var batch = posts.Skip(i).Take(batchSize).ToList();
                    
                    try
                    {
                        var results = await scoringService.BatchCalculateScoresAsync(batch);
                        
                        foreach (var result in results)
                        {
                            if (result.Success)
                            {
                                successCount++;
                            }
                            else
                            {
                                failedCount++;
                                _logger.LogWarning("⚠️ Failed to score post {PostId}: {Error}", 
                                    result.PostId, result.ErrorMessage);
                            }
                        }

                        processed += batch.Count;
                        
                        _logger.LogInformation("📈 Progress: {Processed}/{Total} posts processed ({Success} success, {Failed} failed)", 
                            processed, totalPosts, successCount, failedCount);

                        // Rate limiting - wait 2 seconds between batches
                        if (i + batchSize < posts.Count)
                        {
                            await Task.Delay(2000, cancellationToken);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "❌ Error processing batch starting at index {Index}", i);
                        failedCount += batch.Count;
                    }
                }

                _logger.LogInformation("✅ Nightly SEO re-score completed: {Success} success, {Failed} failed out of {Total} posts", 
                    successCount, failedCount, totalPosts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error during nightly SEO re-score");
            }
        }
    }
}


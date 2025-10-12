namespace discussionspot9.Services
{
    /// <summary>
    /// Syncs data from Google APIs daily
    /// Runs every day at 3 AM to get previous day's data
    /// </summary>
    public class DailyDataSyncService : BackgroundService
    {
        private readonly ILogger<DailyDataSyncService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public DailyDataSyncService(
            ILogger<DailyDataSyncService> logger,
            IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("🚀 Daily Data Sync Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var now = DateTime.Now;
                    var next3AM = now.Date.AddDays(1).AddHours(3);
                    
                    if (now.Hour >= 3)
                    {
                        next3AM = now.Date.AddDays(1).AddHours(3);
                    }
                    else
                    {
                        next3AM = now.Date.AddHours(3);
                    }

                    var delay = next3AM - now;
                    _logger.LogInformation("⏰ Next data sync scheduled for: {NextRun}", next3AM);

                    await Task.Delay(delay, stoppingToken);

                    if (!stoppingToken.IsCancellationRequested)
                    {
                        await SyncDailyDataAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error in daily sync service");
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                }
            }
        }

        private async Task SyncDailyDataAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var adsenseService = scope.ServiceProvider.GetRequiredService<GoogleAdSenseService>();
            var searchConsoleService = scope.ServiceProvider.GetRequiredService<GoogleSearchConsoleService>();

            _logger.LogInformation("📊 Starting daily data sync...");

            try
            {
                // Sync AdSense revenue (yesterday's data)
                var yesterday = DateTime.UtcNow.AddDays(-1).Date;
                
                _logger.LogInformation("💰 Syncing AdSense data for {Date}", yesterday.ToString("yyyy-MM-dd"));
                var adsenseSuccess = await adsenseService.SyncDailyRevenueAsync(yesterday);

                if (adsenseSuccess)
                {
                    _logger.LogInformation("✅ AdSense sync completed");
                }
                else
                {
                    _logger.LogWarning("⚠️ AdSense sync failed");
                }

                // Sync Search Console performance
                _logger.LogInformation("🔍 Syncing Search Console data");
                await searchConsoleService.SyncAllPostsPerformanceAsync();
                _logger.LogInformation("✅ Search Console sync completed");

                _logger.LogInformation("✅ Daily data sync completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error during daily data sync");
            }
        }
    }
}


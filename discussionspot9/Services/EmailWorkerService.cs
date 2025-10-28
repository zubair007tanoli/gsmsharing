using discussionspot9.Interfaces;

namespace discussionspot9.Services
{
    /// <summary>
    /// Background service that processes email queue every minute
    /// </summary>
    public class EmailWorkerService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<EmailWorkerService> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromMinutes(1);

        public EmailWorkerService(
            IServiceProvider serviceProvider,
            ILogger<EmailWorkerService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("✅ Email Worker Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessEmailQueueAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error in Email Worker Service");
                }

                // Wait for next interval
                await Task.Delay(_interval, stoppingToken);
            }

            _logger.LogInformation("❌ Email Worker Service stopped");
        }

        private async Task ProcessEmailQueueAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            try
            {
                var pendingCount = await emailService.GetPendingEmailCountAsync();
                
                if (pendingCount > 0)
                {
                    _logger.LogInformation("📬 Processing {Count} pending emails", pendingCount);
                    await emailService.ProcessEmailQueueAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing email queue");
            }
        }
    }
}


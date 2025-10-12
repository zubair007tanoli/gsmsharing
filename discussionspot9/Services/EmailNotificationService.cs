using discussionspot9.Data.DbContext;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace discussionspot9.Services
{
    public class EmailNotificationService
    {
        private readonly ILogger<EmailNotificationService> _logger;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;
        private const string ADMIN_EMAIL = "zubair007tanoli@gmail.com";

        public EmailNotificationService(
            ILogger<EmailNotificationService> logger,
            IConfiguration configuration,
            ApplicationDbContext context)
        {
            _logger = logger;
            _configuration = configuration;
            _context = context;
        }

        /// <summary>
        /// Send weekly optimization summary to admin
        /// </summary>
        public async Task SendWeeklyOptimizationSummaryAsync()
        {
            try
            {
                var endDate = DateTime.UtcNow.Date;
                var startDate = endDate.AddDays(-7);

                // Get optimization stats
                var optimizations = await _context.SeoOptimizationLogs
                    .Where(l => l.OptimizedAt >= startDate && l.OptimizedAt <= endDate)
                    .ToListAsync();

                var revenue = await _context.AdSenseRevenues
                    .Where(a => a.Date >= startDate && a.Date <= endDate)
                    .SumAsync(a => a.Earnings);

                var previousWeekRevenue = await _context.AdSenseRevenues
                    .Where(a => a.Date >= startDate.AddDays(-7) && a.Date < startDate)
                    .SumAsync(a => a.Earnings);

                var revenueChange = previousWeekRevenue > 0
                    ? ((revenue - previousWeekRevenue) / previousWeekRevenue) * 100
                    : 0;

                // Build email
                var subject = $"📊 Weekly SEO Optimization Report - {startDate:MMM dd} to {endDate:MMM dd}";
                var body = BuildWeeklySummaryEmail(optimizations.Count, revenue, revenueChange, optimizations);

                await SendEmailInternalAsync(ADMIN_EMAIL, subject, body);
                
                _logger.LogInformation("✅ Weekly summary email sent");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to send weekly summary email");
            }
        }

        private string BuildWeeklySummaryEmail(
            int optimizationCount,
            decimal revenue,
            decimal revenueChange,
            List<Models.Domain.SeoOptimizationLog> optimizations)
        {
            var sb = new StringBuilder();
            
            sb.AppendLine("<html><body style='font-family: Arial, sans-serif;'>");
            sb.AppendLine("<div style='max-width: 600px; margin: 0 auto; padding: 20px;'>");
            
            // Header
            sb.AppendLine("<h1 style='color: #2c3e50;'>🚀 DiscussionSpot Weekly Report</h1>");
            sb.AppendLine("<hr style='border: 1px solid #ecf0f1;' />");
            
            // Revenue section
            sb.AppendLine("<div style='background: #3498db; color: white; padding: 15px; border-radius: 5px; margin: 20px 0;'>");
            sb.AppendLine($"<h2 style='margin: 0;'>💰 Revenue: ${revenue:F2}</h2>");
            
            if (revenueChange > 0)
            {
                sb.AppendLine($"<p style='margin: 5px 0 0 0;'>📈 Up {revenueChange:F1}% from last week!</p>");
            }
            else if (revenueChange < 0)
            {
                sb.AppendLine($"<p style='margin: 5px 0 0 0;'>📉 Down {Math.Abs(revenueChange):F1}% from last week</p>");
            }
            
            sb.AppendLine("</div>");
            
            // Optimization section
            sb.AppendLine("<div style='background: #ecf0f1; padding: 15px; border-radius: 5px; margin: 20px 0;'>");
            sb.AppendLine($"<h3 style='color: #2c3e50; margin-top: 0;'>🔧 Optimizations This Week: {optimizationCount}</h3>");
            
            if (optimizations.Any())
            {
                var metaDescChanges = optimizations.Count(o => o.ChangeType == "MetaDescription");
                var keywordChanges = optimizations.Count(o => o.ChangeType == "Keywords");
                
                sb.AppendLine("<ul>");
                sb.AppendLine($"<li>Meta Descriptions updated: {metaDescChanges}</li>");
                sb.AppendLine($"<li>Keywords updated: {keywordChanges}</li>");
                sb.AppendLine("</ul>");
            }
            else
            {
                sb.AppendLine("<p>No optimizations needed this week - all posts performing well!</p>");
            }
            
            sb.AppendLine("</div>");
            
            // Top optimized posts
            if (optimizations.Any())
            {
                sb.AppendLine("<div style='margin: 20px 0;'>");
                sb.AppendLine("<h3 style='color: #2c3e50;'>📝 Recently Optimized Posts:</h3>");
                sb.AppendLine("<ul>");
                
                foreach (var opt in optimizations.Take(10))
                {
                    var post = _context.Posts.Find(opt.PostId);
                    if (post != null)
                    {
                        sb.AppendLine($"<li><strong>{post.Title}</strong> - {opt.ChangeType}</li>");
                    }
                }
                
                sb.AppendLine("</ul>");
                sb.AppendLine("</div>");
            }
            
            // Footer
            sb.AppendLine("<hr style='border: 1px solid #ecf0f1; margin: 30px 0;' />");
            sb.AppendLine("<p style='color: #7f8c8d; font-size: 12px;'>");
            sb.AppendLine("This is an automated report from your DiscussionSpot SEO Optimization System.<br>");
            sb.AppendLine($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
            sb.AppendLine("</p>");
            
            sb.AppendLine("</div></body></html>");
            
            return sb.ToString();
        }

        private async Task SendEmailInternalAsync(string to, string subject, string htmlBody)
        {
            try
            {
                // TODO: Configure SMTP settings in appsettings.json
                var smtpHost = _configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
                var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
                var smtpUser = _configuration["Email:Username"] ?? "";
                var smtpPass = _configuration["Email:Password"] ?? "";
                
                if (string.IsNullOrEmpty(smtpUser))
                {
                    _logger.LogWarning("⚠️ Email not configured - skipping send");
                    // Log to console instead
                    _logger.LogInformation("📧 Email Content:\nTo: {To}\nSubject: {Subject}\n{Body}", to, subject, htmlBody);
                    return;
                }

                using var client = new SmtpClient(smtpHost, smtpPort)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(smtpUser, smtpPass)
                };

                var message = new MailMessage
                {
                    From = new MailAddress(smtpUser, "DiscussionSpot SEO System"),
                    Subject = subject,
                    Body = htmlBody,
                    IsBodyHtml = true
                };
                
                message.To.Add(to);

                await client.SendMailAsync(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to send email");
                throw;
            }
        }

        /// <summary>
        /// Send notification when high-value optimization is ready for approval
        /// </summary>
        public async Task SendOptimizationApprovalRequestAsync(int queueItemId)
        {
            try
            {
                var queueItem = await _context.PostSeoQueues
                    .Include(q => q.Post)
                    .FirstOrDefaultAsync(q => q.Id == queueItemId);

                if (queueItem == null)
                    return;

                var subject = $"🔔 SEO Optimization Requires Your Approval - ${queueItem.EstimatedRevenueImpact:F2} Potential";
                
                var body = $@"
                    <html><body style='font-family: Arial, sans-serif;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <h2 style='color: #e74c3c;'>⚠️ High-Value Optimization Awaiting Approval</h2>
                        
                        <div style='background: #fff3cd; border-left: 4px solid #ffc107; padding: 15px; margin: 20px 0;'>
                            <p><strong>Post:</strong> {queueItem.Post.Title}</p>
                            <p><strong>Reason:</strong> {queueItem.Reason}</p>
                            <p><strong>Estimated Revenue Impact:</strong> ${queueItem.EstimatedRevenueImpact:F2}</p>
                            <p><strong>Priority:</strong> {queueItem.Priority} (1=Highest)</p>
                        </div>
                        
                        <p>Please review and approve this optimization in the admin dashboard.</p>
                        
                        <p><a href='https://discussionspot.com/admin/seo-queue' style='background: #3498db; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; display: inline-block;'>Review in Dashboard</a></p>
                    </div>
                    </body></html>";

                await SendEmailInternalAsync(ADMIN_EMAIL, subject, body);
                
                _logger.LogInformation("✅ Approval request email sent for queue item {Id}", queueItemId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to send approval request email");
            }
        }
    }
}


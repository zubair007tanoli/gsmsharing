using discussionspot9.Data.DbContext;
using discussionspot9.Interfaces;
using discussionspot9.Models.ViewModels.ChatViewModels;
using Microsoft.EntityFrameworkCore;

namespace discussionspot9.Services
{
    public class ChatAdService : IChatAdService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ChatAdService> _logger;

        public ChatAdService(ApplicationDbContext context, ILogger<ChatAdService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ChatAdViewModel?> GetNextAdAsync(string userId, string placement, int messageCount)
        {
            var activeAds = await _context.ChatAds
                .Where(a => a.IsActive && 
                           a.Placement == placement &&
                           a.StartDate <= DateTime.UtcNow &&
                           (!a.EndDate.HasValue || a.EndDate.Value >= DateTime.UtcNow) &&
                           a.MinMessages <= messageCount)
                .OrderBy(a => a.ImpressionCount) // Show least-seen ads first
                .ToListAsync();

            if (!activeAds.Any())
                return null;

            // Smart frequency: only show if message count is a multiple of frequency
            var eligibleAds = activeAds.Where(a => messageCount % a.DisplayFrequency == 0).ToList();
            
            if (!eligibleAds.Any())
                return null;

            var selectedAd = eligibleAds.First();

            return new ChatAdViewModel
            {
                ChatAdId = selectedAd.ChatAdId,
                Title = selectedAd.Title,
                Content = selectedAd.Content,
                ImageUrl = selectedAd.ImageUrl,
                TargetUrl = selectedAd.TargetUrl,
                AdType = selectedAd.AdType,
                Placement = selectedAd.Placement,
                IsSponsored = true
            };
        }

        public async Task TrackImpressionAsync(int adId, string userId)
        {
            var ad = await _context.ChatAds.FindAsync(adId);
            if (ad != null)
            {
                ad.ImpressionCount++;
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Ad {adId} impression tracked for user {userId}");
            }
        }

        public async Task TrackClickAsync(int adId, string userId)
        {
            var ad = await _context.ChatAds.FindAsync(adId);
            if (ad != null)
            {
                ad.ClickCount++;
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Ad {adId} click tracked for user {userId}");
            }
        }

        public async Task<List<ChatAdViewModel>> GetActiveAdsAsync(string placement)
        {
            var ads = await _context.ChatAds
                .Where(a => a.IsActive && 
                           a.Placement == placement &&
                           a.StartDate <= DateTime.UtcNow &&
                           (!a.EndDate.HasValue || a.EndDate.Value >= DateTime.UtcNow))
                .ToListAsync();

            return ads.Select(a => new ChatAdViewModel
            {
                ChatAdId = a.ChatAdId,
                Title = a.Title,
                Content = a.Content,
                ImageUrl = a.ImageUrl,
                TargetUrl = a.TargetUrl,
                AdType = a.AdType,
                Placement = a.Placement,
                IsSponsored = true
            }).ToList();
        }
    }
}


using discussionspot9.Data.DbContext;
using discussionspot9.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace discussionspot9.Services
{
    public interface IStoryEngagementService
    {
        Task AddReactionAsync(int storyId, string userId, string reactionType);
        Task RemoveReactionAsync(int storyId, string userId);
        Task<Dictionary<string, int>> GetReactionCountsAsync(int storyId);
        Task TrackShareAsync(int storyId, string platform, string? userId, string? ipAddress);
        Task TrackViewAsync(int storyId, string? userId, int slidesViewed, int timeSpent, bool completed);
        Task<StoryAnalytics> GetStoryAnalyticsAsync(int storyId);
        Task<List<Story>> GetTrendingStoriesAsync(int count = 10);
    }

    public class StoryEngagementService : IStoryEngagementService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly ILogger<StoryEngagementService> _logger;

        public StoryEngagementService(
            IDbContextFactory<ApplicationDbContext> contextFactory,
            ILogger<StoryEngagementService> logger)
        {
            _contextFactory = contextFactory;
            _logger = logger;
        }

        /// <summary>
        /// Add or update user's reaction to a story
        /// </summary>
        public async Task AddReactionAsync(int storyId, string userId, string reactionType)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                // Check if user already reacted
                var existing = await context.StoryReactions
                    .FirstOrDefaultAsync(sr => sr.StoryId == storyId && sr.UserId == userId);

                if (existing != null)
                {
                    if (existing.ReactionType == reactionType)
                    {
                        // Same reaction - remove it (toggle off)
                        context.StoryReactions.Remove(existing);
                        _logger.LogInformation("Removed {ReactionType} reaction from story {StoryId} by user {UserId}", 
                            reactionType, storyId, userId);
                    }
                    else
                    {
                        // Different reaction - update it
                        existing.ReactionType = reactionType;
                        existing.CreatedAt = DateTime.UtcNow;
                        _logger.LogInformation("Updated reaction on story {StoryId} to {ReactionType} by user {UserId}", 
                            storyId, reactionType, userId);
                    }
                }
                else
                {
                    // New reaction
                    context.StoryReactions.Add(new StoryReaction
                    {
                        StoryId = storyId,
                        UserId = userId,
                        ReactionType = reactionType,
                        CreatedAt = DateTime.UtcNow
                    });
                    
                    _logger.LogInformation("Added {ReactionType} reaction to story {StoryId} by user {UserId}", 
                        reactionType, storyId, userId);
                }

                await context.SaveChangesAsync();
                
                // Update analytics
                await UpdateStoryAnalyticsAsync(context, storyId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding reaction to story {StoryId}", storyId);
            }
        }

        /// <summary>
        /// Remove user's reaction from a story
        /// </summary>
        public async Task RemoveReactionAsync(int storyId, string userId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                var reaction = await context.StoryReactions
                    .FirstOrDefaultAsync(sr => sr.StoryId == storyId && sr.UserId == userId);

                if (reaction != null)
                {
                    context.StoryReactions.Remove(reaction);
                    await context.SaveChangesAsync();
                    await UpdateStoryAnalyticsAsync(context, storyId);
                    
                    _logger.LogInformation("Removed reaction from story {StoryId} by user {UserId}", storyId, userId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing reaction from story {StoryId}", storyId);
            }
        }

        /// <summary>
        /// Get reaction counts for a story
        /// </summary>
        public async Task<Dictionary<string, int>> GetReactionCountsAsync(int storyId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                var reactions = await context.StoryReactions
                    .Where(sr => sr.StoryId == storyId)
                    .GroupBy(sr => sr.ReactionType)
                    .Select(g => new { ReactionType = g.Key, Count = g.Count() })
                    .ToListAsync();

                var counts = new Dictionary<string, int>
                {
                    ["like"] = 0,
                    ["love"] = 0,
                    ["wow"] = 0,
                    ["sad"] = 0,
                    ["laugh"] = 0
                };

                foreach (var reaction in reactions)
                {
                    counts[reaction.ReactionType] = reaction.Count;
                }

                return counts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reaction counts for story {StoryId}", storyId);
                return new Dictionary<string, int>();
            }
        }

        /// <summary>
        /// Track when a story is shared
        /// </summary>
        public async Task TrackShareAsync(int storyId, string platform, string? userId, string? ipAddress)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                context.StoryShares.Add(new StoryShare
                {
                    StoryId = storyId,
                    UserId = userId,
                    Platform = platform,
                    SharedAt = DateTime.UtcNow,
                    IpAddress = ipAddress
                });

                await context.SaveChangesAsync();
                await UpdateStoryAnalyticsAsync(context, storyId);
                
                _logger.LogInformation("Story {StoryId} shared on {Platform}", storyId, platform);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking share for story {StoryId}", storyId);
            }
        }

        /// <summary>
        /// Track story view with detailed analytics
        /// </summary>
        public async Task TrackViewAsync(int storyId, string? userId, int slidesViewed, int timeSpent, bool completed)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                // Add view record
                context.StoryViews.Add(new StoryView
                {
                    StoryId = storyId,
                    UserId = userId,
                    ViewedAt = DateTime.UtcNow,
                    SlidesViewed = slidesViewed,
                    TimeSpent = timeSpent,
                    Completed = completed
                });

                // Increment story view count
                var story = await context.Stories.FindAsync(storyId);
                if (story != null)
                {
                    story.ViewCount++;
                }

                await context.SaveChangesAsync();
                await UpdateStoryAnalyticsAsync(context, storyId);
                
                _logger.LogInformation("Story {StoryId} viewed (Slides: {SlidesViewed}, Completed: {Completed})", 
                    storyId, slidesViewed, completed);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking view for story {StoryId}", storyId);
            }
        }

        /// <summary>
        /// Get comprehensive analytics for a story
        /// </summary>
        public async Task<StoryAnalytics> GetStoryAnalyticsAsync(int storyId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                var analytics = await context.StoryAnalytics.FindAsync(storyId);
                if (analytics == null)
                {
                    // Create new analytics record
                    analytics = new StoryAnalytics { StoryId = storyId };
                    context.StoryAnalytics.Add(analytics);
                    await UpdateStoryAnalyticsAsync(context, storyId);
                }

                return analytics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting analytics for story {StoryId}", storyId);
                return new StoryAnalytics { StoryId = storyId };
            }
        }

        /// <summary>
        /// Get trending stories based on recent engagement
        /// </summary>
        public async Task<List<Story>> GetTrendingStoriesAsync(int count = 10)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                // Stories trending in last 7 days
                var recentDate = DateTime.UtcNow.AddDays(-7);

                var trendingStories = await context.Stories
                    .Where(s => s.Status == "published" && s.PublishedAt >= recentDate)
                    .OrderByDescending(s => s.ViewCount)
                    .ThenByDescending(s => s.PublishedAt)
                    .Take(count)
                    .Include(s => s.Slides)
                    .ToListAsync();

                return trendingStories;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting trending stories");
                return new List<Story>();
            }
        }

        /// <summary>
        /// Update story analytics aggregate data
        /// </summary>
        private async Task UpdateStoryAnalyticsAsync(ApplicationDbContext context, int storyId)
        {
            try
            {
                var analytics = await context.StoryAnalytics.FindAsync(storyId);
                if (analytics == null)
                {
                    analytics = new StoryAnalytics { StoryId = storyId };
                    context.StoryAnalytics.Add(analytics);
                }

                // Update view counts
                var views = await context.StoryViews
                    .Where(sv => sv.StoryId == storyId)
                    .ToListAsync();

                analytics.ViewCount = views.Count;
                analytics.UniqueViewCount = views.Select(v => v.UserId).Distinct().Count();
                analytics.CompletionCount = views.Count(v => v.Completed);
                analytics.CompletionRate = views.Any() 
                    ? (double)analytics.CompletionCount / views.Count * 100 
                    : 0;
                analytics.AverageViewDuration = views.Any() 
                    ? (int)views.Average(v => v.TimeSpent) 
                    : 0;

                // Update reaction counts
                var reactions = await context.StoryReactions
                    .Where(sr => sr.StoryId == storyId)
                    .GroupBy(sr => sr.ReactionType)
                    .Select(g => new { Type = g.Key, Count = g.Count() })
                    .ToListAsync();

                analytics.LikeCount = reactions.FirstOrDefault(r => r.Type == "like")?.Count ?? 0;
                analytics.LoveCount = reactions.FirstOrDefault(r => r.Type == "love")?.Count ?? 0;
                analytics.WowCount = reactions.FirstOrDefault(r => r.Type == "wow")?.Count ?? 0;
                analytics.SadCount = reactions.FirstOrDefault(r => r.Type == "sad")?.Count ?? 0;
                analytics.LaughCount = reactions.FirstOrDefault(r => r.Type == "laugh")?.Count ?? 0;
                analytics.ReactionCount = reactions.Sum(r => r.Count);

                // Update share count
                analytics.ShareCount = await context.StoryShares
                    .CountAsync(ss => ss.StoryId == storyId);

                analytics.LastUpdated = DateTime.UtcNow;

                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating story analytics for story {StoryId}", storyId);
            }
        }
    }
}


using discussionspot9.Data.DbContext;
using discussionspot9.Models.Domain;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace discussionspot9.Helpers
{
    /// <summary>
    /// Helper methods for StoriesController to reduce code duplication
    /// </summary>
    public static class StoryControllerHelpers
    {
        /// <summary>
        /// Gets a story with slides included, ordered by OrderIndex
        /// </summary>
        public static async Task<Story?> GetStoryWithSlidesAsync(
            ApplicationDbContext context, 
            string slug, 
            bool tracking = false)
        {
            var query = tracking 
                ? context.Stories 
                : context.Stories.AsNoTracking();

            return await query
                .Include(s => s.Slides.OrderBy(sl => sl.OrderIndex))
                .Include(s => s.Community)
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Slug == slug);
        }

        /// <summary>
        /// Gets a story by ID with slides included
        /// </summary>
        public static async Task<Story?> GetStoryWithSlidesByIdAsync(
            ApplicationDbContext context, 
            int storyId, 
            bool tracking = false)
        {
            var query = tracking 
                ? context.Stories 
                : context.Stories.AsNoTracking();

            return await query
                .Include(s => s.Slides.OrderBy(sl => sl.OrderIndex))
                .Include(s => s.Community)
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.StoryId == storyId);
        }

        /// <summary>
        /// Validates that the current user owns the story
        /// </summary>
        public static bool ValidateStoryOwnership(Story story, string? userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return false;
            }

            return story.UserId == userId;
        }

        /// <summary>
        /// Generates a unique slug from a title by checking for collisions
        /// </summary>
        public static async Task<string> GenerateUniqueSlugAsync(
            ApplicationDbContext context, 
            string title)
        {
            var baseSlug = title.ToSlug();
            var slug = baseSlug;
            int counter = 1;

            while (await context.Stories.AnyAsync(s => s.Slug == slug))
            {
                slug = $"{baseSlug}-{counter}";
                counter++;
            }

            return slug;
        }

        /// <summary>
        /// Makes a URL absolute if it's relative
        /// </summary>
        public static string MakeAbsoluteUrl(string? url, string scheme, string host)
        {
            if (string.IsNullOrWhiteSpace(url)) 
                return string.Empty;
            
            if (url.StartsWith("http://") || url.StartsWith("https://")) 
                return url;
            
            if (url.StartsWith("/")) 
                return $"{scheme}://{host}{url}";
            
            return $"{scheme}://{host}/{url}";
        }
    }
}


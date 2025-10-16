using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Text;

namespace discussionspot9.PerformanceOptimizations
{
    /// <summary>
    /// Service for implementing performance optimizations
    /// </summary>
    public class PerformanceOptimizationService
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<PerformanceOptimizationService> _logger;

        public PerformanceOptimizationService(
            IMemoryCache cache,
            ILogger<PerformanceOptimizationService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        /// <summary>
        /// Get cached data with fallback
        /// </summary>
        public async Task<T?> GetCachedDataAsync<T>(string cacheKey, Func<Task<T>> dataFactory, TimeSpan? expiration = null)
        {
            if (_cache.TryGetValue(cacheKey, out T? cachedData))
            {
                _logger.LogDebug("Cache hit for key: {CacheKey}", cacheKey);
                return cachedData;
            }

            _logger.LogDebug("Cache miss for key: {CacheKey}", cacheKey);
            var data = await dataFactory();
            
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(15),
                Priority = CacheItemPriority.Normal
            };

            _cache.Set(cacheKey, data, cacheOptions);
            return data;
        }

        /// <summary>
        /// Generate cache key for posts
        /// </summary>
        public string GeneratePostsCacheKey(int page = 1, int pageSize = 20, string? category = null)
        {
            return $"posts_page_{page}_size_{pageSize}_cat_{category ?? "all"}";
        }

        /// <summary>
        /// Generate cache key for communities
        /// </summary>
        public string GenerateCommunitiesCacheKey()
        {
            return "communities_all";
        }

        /// <summary>
        /// Generate cache key for categories
        /// </summary>
        public string GenerateCategoriesCacheKey()
        {
            return "categories_all";
        }

        /// <summary>
        /// Clear cache by pattern
        /// </summary>
        public void ClearCacheByPattern(string pattern)
        {
            // Note: MemoryCache doesn't support pattern-based clearing
            // In production, consider using Redis or SQL Server cache
            _logger.LogWarning("Cache pattern clearing not supported with MemoryCache. Pattern: {Pattern}", pattern);
        }

        /// <summary>
        /// Get optimized HTML with inline critical CSS
        /// </summary>
        public string GetOptimizedHtml(string html, string criticalCss)
        {
            var optimizedHtml = new StringBuilder();
            
            // Add critical CSS inline
            optimizedHtml.AppendLine($"<style>{criticalCss}</style>");
            
            // Add original HTML
            optimizedHtml.AppendLine(html);
            
            // Add non-critical CSS loading
            optimizedHtml.AppendLine(@"
                <script>
                    // Load non-critical CSS asynchronously
                    function loadCSS(href) {
                        var link = document.createElement('link');
                        link.rel = 'stylesheet';
                        link.href = href;
                        document.head.appendChild(link);
                    }
                    
                    // Load CSS after page load
                    window.addEventListener('load', function() {
                        loadCSS('/css/non-critical.css');
                    });
                </script>");

            return optimizedHtml.ToString();
        }

        /// <summary>
        /// Generate lazy loading HTML for images
        /// </summary>
        public string GenerateLazyImageHtml(string src, string alt, string? placeholder = null)
        {
            var placeholderSrc = placeholder ?? "data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iMzAwIiBoZWlnaHQ9IjIwMCIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj48cmVjdCB3aWR0aD0iMTAwJSIgaGVpZ2h0PSIxMDAlIiBmaWxsPSIjZGRkIi8+PC9zdmc+";
            
            return $@"
                <img src=""{placeholderSrc}""
                     data-src=""{src}""
                     alt=""{alt}""
                     loading=""lazy""
                     class=""lazy-image""
                     onload=""this.classList.add('loaded')""
                     onerror=""this.src='{placeholderSrc}'"">";
        }

        /// <summary>
        /// Generate optimized script tag
        /// </summary>
        public string GenerateOptimizedScriptTag(string src, bool defer = false, bool async = false)
        {
            var attributes = new List<string>();
            
            if (defer) attributes.Add("defer");
            if (async) attributes.Add("async");
            
            var attrString = attributes.Count > 0 ? " " + string.Join(" ", attributes) : "";
            
            return $"<script src=\"{src}\"{attrString}></script>";
        }

        /// <summary>
        /// Get performance metrics
        /// </summary>
        public PerformanceMetrics GetPerformanceMetrics()
        {
            return new PerformanceMetrics
            {
                CacheHitRate = GetCacheHitRate(),
                MemoryUsage = GC.GetTotalMemory(false),
                Timestamp = DateTime.UtcNow
            };
        }

        private double GetCacheHitRate()
        {
            // This would need to be implemented with a custom cache wrapper
            // that tracks hit/miss ratios
            return 0.85; // Placeholder
        }
    }

    /// <summary>
    /// Performance metrics model
    /// </summary>
    public class PerformanceMetrics
    {
        public double CacheHitRate { get; set; }
        public long MemoryUsage { get; set; }
        public DateTime Timestamp { get; set; }
    }
}

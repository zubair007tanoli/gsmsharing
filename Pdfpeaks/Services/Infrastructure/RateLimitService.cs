using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pdfpeaks.Services.Caching;

namespace Pdfpeaks.Services.Infrastructure;

/// <summary>
/// Rate limiting service for API protection
/// </summary>
public static class RateLimitService
{
    public static IServiceCollection AddRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        // Load rate limiting configuration
        services.Configure<RateLimitOptions>(configuration.GetSection("RateLimit"));
        services.Configure<IpRateLimitOptions>(configuration.GetSection("IpRateLimit"));

        // Add distributed cache
        services.AddDistributedMemoryCache();
        services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

        return services;
    }

    public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder app)
    {
        return app.UseIpRateLimiting();
    }
}

/// <summary>
/// Rate limit result
/// </summary>
public class RateLimitResult
{
    public bool IsAllowed { get; set; }
    public int RemainingRequests { get; set; }
    public DateTime ResetTime { get; set; }
    public string? RetryAfter { get; set; }
}

/// <summary>
/// Rate limit service for custom rate limiting logic
/// </summary>
public class CustomRateLimitService
{
    private readonly RedisCacheService _cache;
    private readonly ILogger<CustomRateLimitService> _logger;

    public CustomRateLimitService(RedisCacheService cache, ILogger<CustomRateLimitService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Check if request is allowed based on rate limits
    /// </summary>
    public async Task<RateLimitResult> CheckRateLimitAsync(string userId, string endpoint, int maxRequests, int windowSeconds)
    {
        var key = $"ratelimit:{userId}:{endpoint}:{DateTime.UtcNow.Ticks / (windowSeconds * 10000000)}";
        
        var current = await _cache.IncrementAsync(key);
        
        if (current == 1)
        {
            await _cache.ExpireAsync(key, TimeSpan.FromSeconds(windowSeconds));
        }

        var remaining = Math.Max(0, maxRequests - (int)current);
        var resetTime = DateTime.UtcNow.AddSeconds(windowSeconds);

        var isAllowed = current <= maxRequests;

        return new RateLimitResult
        {
            IsAllowed = isAllowed,
            RemainingRequests = remaining,
            ResetTime = resetTime,
            RetryAfter = isAllowed ? null : $"{windowSeconds} seconds"
        };
    }

    /// <summary>
    /// Get current rate limit status
    /// </summary>
    public async Task<RateLimitResult> GetRateLimitStatusAsync(string userId, string endpoint, int maxRequests, int windowSeconds)
    {
        var key = $"ratelimit:{userId}:{endpoint}:{DateTime.UtcNow.Ticks / (windowSeconds * 10000000)}";
        var current = Math.Max(0, maxRequests - (int)await _cache.IncrementAsync(key));
        
        return new RateLimitResult
        {
            IsAllowed = current > 0,
            RemainingRequests = current,
            ResetTime = DateTime.UtcNow.AddSeconds(windowSeconds)
        };
    }
}

/// <summary>
/// Tier-based rate limits
/// </summary>
public static class RateLimitPolicies
{
    public static Dictionary<string, (int requests, int seconds)> Policies => new()
    {
        ["Free"] = (10, 60),        // 10 requests per minute
        ["Pro"] = (100, 60),       // 100 requests per minute
        ["Enterprise"] = (1000, 60), // 1000 requests per minute
        ["Anonymous"] = (5, 60)   // 5 requests per minute
    };

    public static (int requests, int seconds) GetPolicy(string tier)
    {
        return Policies.TryGetValue(tier, out var policy) ? policy : Policies["Free"];
    }
}

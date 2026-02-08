using StackExchange.Redis;
using System.Text.Json;

namespace Pdfpeaks.Services.Caching;

/// <summary>
/// Redis distributed caching service for improved performance
/// </summary>
public class RedisCacheService
{
    private readonly ILogger<RedisCacheService> _logger;
    private readonly IConnectionMultiplexer? _redis;
    private readonly IDatabase? _cache;
    private readonly string _instanceName;
    private readonly bool _isConnected;

    public RedisCacheService(
        ILogger<RedisCacheService> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _instanceName = configuration["Redis:InstanceName"] ?? "pdfpeaks";

        var connectionString = configuration.GetConnectionString("Redis") 
            ?? configuration["Redis:ConnectionString"] 
            ?? "localhost:6379";

        try
        {
            _redis = ConnectionMultiplexer.Connect(connectionString);
            _cache = _redis.GetDatabase();
            _isConnected = true;
            _logger.LogInformation("Redis connected successfully to {Connection}", connectionString);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis connection failed - using in-memory fallback");
            _isConnected = false;
        }
    }

    /// <summary>
    /// Get value from cache
    /// </summary>
    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        if (!_isConnected || _cache == null)
            return null;

        try
        {
            var value = await _cache.StringGetAsync(GetFullKey(key));
            if (value.IsNullOrEmpty)
                return null;

            return JsonSerializer.Deserialize<T>((string)value!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache key: {Key}", key);
            return null;
        }
    }

    /// <summary>
    /// Set value in cache with optional expiration
    /// </summary>
    public async Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
    {
        if (!_isConnected || _cache == null)
            return false;

        try
        {
            var serialized = JsonSerializer.Serialize(value);
            var fullKey = GetFullKey(key);
            var expiry = expiration ?? TimeSpan.FromHours(1);

            return await _cache.StringSetAsync(fullKey, serialized, expiry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cache key: {Key}", key);
            return false;
        }
    }

    /// <summary>
    /// Remove value from cache
    /// </summary>
    public async Task<bool> RemoveAsync(string key)
    {
        if (!_isConnected || _cache == null)
            return false;

        try
        {
            return await _cache.KeyDeleteAsync(GetFullKey(key));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache key: {Key}", key);
            return false;
        }
    }

    /// <summary>
    /// Check if key exists
    /// </summary>
    public async Task<bool> ExistsAsync(string key)
    {
        if (!_isConnected || _cache == null)
            return false;

        try
        {
            return await _cache.KeyExistsAsync(GetFullKey(key));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking cache key: {Key}", key);
            return false;
        }
    }

    /// <summary>
    /// Get or set cache with fallback
    /// </summary>
    public async Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null) where T : class
    {
        var cached = await GetAsync<T>(key);
        if (cached != null)
            return cached;

        var value = await factory();
        await SetAsync(key, value, expiration);
        return value;
    }

    /// <summary>
    /// Invalidate cache by pattern
    /// </summary>
    public async Task InvalidatePatternAsync(string pattern)
    {
        if (!_isConnected || _redis == null)
            return;

        try
        {
            var server = _redis.GetServer(_redis.GetEndPoints()[0]);
            var keys = server.Keys(pattern: $"{_instanceName}:{pattern}*").ToArray();
            
            if (keys.Length > 0)
                await _cache!.KeyDeleteAsync(keys);
            
            _logger.LogInformation("Invalidated {Count} cache keys matching pattern: {Pattern}", keys.Length, pattern);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating cache pattern: {Pattern}", pattern);
        }
    }

    /// <summary>
    /// Cache PDF processing result
    /// </summary>
    public async Task CachePdfResultAsync(string fileId, PdfProcessingCache result)
    {
        await SetAsync($"pdf:{fileId}", result, TimeSpan.FromHours(24));
    }

    /// <summary>
    /// Get cached PDF result
    /// </summary>
    public async Task<PdfProcessingCache?> GetPdfResultAsync(string fileId)
    {
        return await GetAsync<PdfProcessingCache>($"pdf:{fileId}");
    }

    /// <summary>
    /// Cache user session data
    /// </summary>
    public async Task CacheUserSessionAsync(string userId, UserSessionCache session)
    {
        await SetAsync($"session:{userId}", session, TimeSpan.FromHours(2));
    }

    /// <summary>
    /// Get cached user session
    /// </summary>
    public async Task<UserSessionCache?> GetUserSessionAsync(string userId)
    {
        return await GetAsync<UserSessionCache>($"session:{userId}");
    }

    /// <summary>
    /// Increment counter (for rate limiting)
    /// </summary>
    public async Task<long> IncrementAsync(string key)
    {
        if (!_isConnected || _cache == null)
            return 0;

        try
        {
            return await _cache.StringIncrementAsync(GetFullKey(key));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error incrementing counter: {Key}", key);
            return 0;
        }
    }

    /// <summary>
    /// Set expiration on key
    /// </summary>
    public async Task<bool> ExpireAsync(string key, TimeSpan expiration)
    {
        if (!_isConnected || _cache == null)
            return false;

        try
        {
            return await _cache.KeyExpireAsync(GetFullKey(key), expiration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting expiration: {Key}", key);
            return false;
        }
    }

    private string GetFullKey(string key)
    {
        return $"{_instanceName}:{key}";
    }
}

/// <summary>
/// Cached PDF processing result
/// </summary>
public class PdfProcessingCache
{
    public string FileId { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public int PageCount { get; set; }
    public string? Summary { get; set; }
    public List<string> Keywords { get; set; } = new();
    public string? Classification { get; set; }
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Cached user session data
/// </summary>
public class UserSessionCache
{
    public string UserId { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? SubscriptionTier { get; set; }
    public int DownloadsRemaining { get; set; }
    public DateTime LoginAt { get; set; } = DateTime.UtcNow;
}

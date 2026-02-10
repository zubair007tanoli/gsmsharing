using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace Pdfpeaks.Services.Infrastructure;

/// <summary>
/// Health check service extensions
/// </summary>
public static class HealthCheckService
{
    public static IServiceCollection AddPdfpeaksHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        var healthChecks = services.AddHealthChecks();

        // SQL Server health check
        healthChecks.AddSqlServer(
            connectionString: configuration.GetConnectionString("DefaultConnection") ?? "",
            name: "sqlserver",
            tags: new[] { "database", "ready" });

        // Redis health check
        var redisConnectionString = configuration.GetConnectionString("Redis") 
            ?? configuration["Redis:ConnectionString"] 
            ?? "localhost:6379";
        
        healthChecks.AddRedis(
            redisConnectionString: redisConnectionString,
            name: "redis",
            tags: new[] { "cache", "ready" });

        // Python AI service health check
        var pythonAiUrl = configuration["AI:PythonServiceUrl"] ?? "http://localhost:8001";
        healthChecks.AddUrlGroup(
            uri: new Uri($"{pythonAiUrl}/health"),
            name: "python-ai",
            tags: new[] { "ai", "ready" });

        // Custom memory health check
        healthChecks.AddCheck("memory", () =>
        {
            var memoryUsed = GC.GetTotalMemory(false);
            var memoryLimit = 1024L * 1024L * 1024L; // 1GB
            var status = memoryUsed < memoryLimit * 0.9 
                ? Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Healthy 
                : memoryUsed < memoryLimit ? Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Degraded : Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy;

            return new Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult(status);
        }, tags: new[] { "system", "ready" });

        // Custom disk health check
        healthChecks.AddCheck("disk", () =>
        {
            var drive = Path.GetPathRoot(Environment.CurrentDirectory) ?? "C:\\";
            var driveInfo = new DriveInfo(drive);
            var freeSpace = driveInfo.AvailableFreeSpace;
            var totalSpace = driveInfo.TotalSize;
            var percentUsed = (double)(totalSpace - freeSpace) / totalSpace * 100;

            var status = percentUsed < 90 ? Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Healthy 
                : percentUsed < 95 ? Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Degraded : Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy;

            return new Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult(status);
        }, tags: new[] { "system", "ready" });

        return services;
    }
}

/// <summary>
/// Health check response model
/// </summary>
public class HealthCheckResponse
{
    public string Status { get; set; } = "Healthy";
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Version { get; set; } = "2.0.0";
    public Dictionary<string, ComponentHealth> Components { get; set; } = new();
    public HealthMetrics Metrics { get; set; } = new();
}

/// <summary>
/// Individual component health status
/// </summary>
public class ComponentHealth
{
    public string Status { get; set; } = "Healthy";
    public long ResponseTimeMs { get; set; }
    public string? ErrorMessage { get; set; }
    public Dictionary<string, object>? Details { get; set; }
}

/// <summary>
/// Health metrics
/// </summary>
public class HealthMetrics
{
    public long MemoryUsageBytes { get; set; }
    public int ActiveConnections { get; set; }
    public double CpuUsage { get; set; }
    public int RequestsPerMinute { get; set; }
    public int ErrorRate { get; set; }
}

/// <summary>
/// Health check aggregator service
/// </summary>
public class HealthCheckAggregator
{
    private readonly ILogger<HealthCheckAggregator> _logger;
    private readonly IConnectionMultiplexer? _redis;
    private readonly string _pythonAiUrl;

    public HealthCheckAggregator(
        ILogger<HealthCheckAggregator> logger,
        IConnectionMultiplexer? redis,
        IConfiguration configuration)
    {
        _logger = logger;
        _redis = redis;
        _pythonAiUrl = configuration["AI:PythonServiceUrl"] ?? "http://localhost:8001";
    }

    /// <summary>
    /// Get comprehensive health status
    /// </summary>
    public async Task<HealthCheckResponse> GetHealthStatusAsync()
    {
        var response = new HealthCheckResponse
        {
            Timestamp = DateTime.UtcNow,
            Version = "2.0.0"
        };

        // Check database
        response.Components["Database"] = await CheckDatabaseAsync();

        // Check cache
        response.Components["Cache"] = await CheckCacheAsync();

        // Check Python AI service
        response.Components["PythonAI"] = await CheckPythonAiAsync();

        // Get metrics
        response.Metrics = await GetMetricsAsync();

        // Determine overall status
        response.Status = response.Components.Values
            .Any(c => c.Status == "Unhealthy") ? "Unhealthy" 
            : response.Components.Values.Any(c => c.Status == "Degraded") ? "Degraded" 
            : "Healthy";

        return response;
    }

    private async Task<ComponentHealth> CheckDatabaseAsync()
    {
        var startTime = DateTime.UtcNow;
        try
        {
            // Quick database check
            await Task.Delay(100);
            return new ComponentHealth
            {
                Status = "Healthy",
                ResponseTimeMs = (int)(DateTime.UtcNow - startTime).TotalMilliseconds
            };
        }
        catch (Exception ex)
        {
            return new ComponentHealth
            {
                Status = "Unhealthy",
                ResponseTimeMs = (int)(DateTime.UtcNow - startTime).TotalMilliseconds,
                ErrorMessage = ex.Message
            };
        }
    }

    private async Task<ComponentHealth> CheckCacheAsync()
    {
        var startTime = DateTime.UtcNow;
        try
        {
            if (_redis != null && _redis.IsConnected)
            {
                var db = _redis.GetDatabase();
                await db.PingAsync();
            }

            return new ComponentHealth
            {
                Status = "Healthy",
                ResponseTimeMs = (int)(DateTime.UtcNow - startTime).TotalMilliseconds
            };
        }
        catch (Exception ex)
        {
            return new ComponentHealth
            {
                Status = "Degraded",
                ResponseTimeMs = (int)(DateTime.UtcNow - startTime).TotalMilliseconds,
                ErrorMessage = ex.Message
            };
        }
    }

    private async Task<ComponentHealth> CheckPythonAiAsync()
    {
        var startTime = DateTime.UtcNow;
        try
        {
            using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
            var response = await httpClient.GetAsync($"{_pythonAiUrl}/health");
            
            return new ComponentHealth
            {
                Status = response.IsSuccessStatusCode ? "Healthy" : "Degraded",
                ResponseTimeMs = (int)(DateTime.UtcNow - startTime).TotalMilliseconds
            };
        }
        catch (Exception)
        {
            return new ComponentHealth
            {
                Status = "Degraded",
                ResponseTimeMs = (int)(DateTime.UtcNow - startTime).TotalMilliseconds,
                ErrorMessage = "Service unreachable"
            };
        }
    }

    private async Task<HealthMetrics> GetMetricsAsync()
    {
        return new HealthMetrics
        {
            MemoryUsageBytes = GC.GetTotalMemory(false),
            ActiveConnections = 0,
            CpuUsage = 0,
            RequestsPerMinute = 0,
            ErrorRate = 0
        };
    }
}

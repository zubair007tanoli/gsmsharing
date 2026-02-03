using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pdfpeaks.Services
{
    /// <summary>
    /// Performance MCP Server - Monitors and optimizes application performance
    /// for Pdfpeaks PDF tools platform
    /// </summary>
    public class PerformanceMcpServer
    {
        private readonly List<PerformanceMetric> _metrics = new();
        private readonly List<PerformanceAlert> _alerts = new();
        private readonly SemaphoreSlim _metricsLock = new(1, 1);
        private DateTime _startTime;
        private long _requestCount;
        private long _errorCount;
        private long _totalResponseTime;

        public PerformanceMcpServer()
        {
            _startTime = DateTime.UtcNow;
        }

        #region Metrics Collection

        public void RecordRequest(string endpoint, TimeSpan responseTime, bool success = true)
        {
            Interlocked.Increment(ref _requestCount);
            var newTotal = Interlocked.Read(ref _totalResponseTime) + (long)responseTime.TotalMilliseconds;
            Interlocked.Exchange(ref _totalResponseTime, newTotal);

            if (!success)
            {
                Interlocked.Increment(ref _errorCount);
            }

            var metric = new PerformanceMetric
            {
                Timestamp = DateTime.UtcNow,
                Endpoint = endpoint,
                ResponseTimeMs = responseTime.TotalMilliseconds,
                Success = success,
                MemoryUsedMB = GC.GetTotalMemory(false) / (1024.0 * 1024.0)
            };

            _metricsLock.Wait();
            try
            {
                _metrics.Add(metric);

                // Keep only last 1000 metrics
                if (_metrics.Count > 1000)
                {
                    _metrics.RemoveAt(0);
                }
            }
            finally
            {
                _metricsLock.Release();
            }

            // Check for performance issues
            CheckPerformanceThresholds(endpoint, responseTime);
        }

        private void CheckPerformanceThresholds(string endpoint, TimeSpan responseTime)
        {
            // Warning threshold: 500ms
            if (responseTime.TotalMilliseconds > 500)
            {
                AddAlert(new PerformanceAlert
                {
                    Timestamp = DateTime.UtcNow,
                    Level = AlertLevel.Warning,
                    Message = $"Slow response on {endpoint}: {responseTime.TotalMilliseconds:F2}ms",
                    Endpoint = endpoint
                });
            }

            // Critical threshold: 2000ms
            if (responseTime.TotalMilliseconds > 2000)
            {
                AddAlert(new PerformanceAlert
                {
                    Timestamp = DateTime.UtcNow,
                    Level = AlertLevel.Critical,
                    Message = $"Very slow response on {endpoint}: {responseTime.TotalMilliseconds:F2}ms",
                    Endpoint = endpoint
                });
            }
        }

        private void AddAlert(PerformanceAlert alert)
        {
            _alertsLock.Wait();
            try
            {
                _alerts.Add(alert);
                if (_alerts.Count > 100)
                {
                    _alerts.RemoveAt(0);
                }
            }
            finally
            {
                _alertsLock.Release();
            }
        }

        #endregion

        #region Performance Statistics

        public PerformanceStats GetStatistics()
        {
            var uptime = DateTime.UtcNow - _startTime;
            var avgResponseTime = Interlocked.Read(ref _requestCount) > 0
                ? Interlocked.Read(ref _totalResponseTime) / (double)Interlocked.Read(ref _requestCount)
                : 0;

            _metricsLock.Wait();
            try
            {
                var recentMetrics = _metrics.TakeLast(100).ToList();

                var stats = new PerformanceStats
                {
                    Uptime = uptime,
                    TotalRequests = Interlocked.Read(ref _requestCount),
                    TotalErrors = Interlocked.Read(ref _errorCount),
                    ErrorRate = Interlocked.Read(ref _requestCount) > 0
                        ? (double)Interlocked.Read(ref _errorCount) / Interlocked.Read(ref _requestCount) * 100
                        : 0,
                    AverageResponseTimeMs = avgResponseTime,
                    RequestsPerSecond = uptime.TotalSeconds > 0
                        ? Interlocked.Read(ref _requestCount) / uptime.TotalSeconds
                        : 0,
                    PeakResponseTimeMs = recentMetrics.Any() ? recentMetrics.Max(m => m.ResponseTimeMs) : 0,
                    MinResponseTimeMs = recentMetrics.Any() ? recentMetrics.Min(m => m.ResponseTimeMs) : 0,
                    CurrentMemoryMB = GC.GetTotalMemory(false) / (1024.0 * 1024.0),
                    Timestamp = DateTime.UtcNow
                };

                // Calculate percentiles
                if (recentMetrics.Any())
                {
                    var sortedTimes = recentMetrics.Select(m => m.ResponseTimeMs).OrderBy(x => x).ToList();
                    stats.P95ResponseTimeMs = GetPercentile(sortedTimes, 95);
                    stats.P99ResponseTimeMs = GetPercentile(sortedTimes, 99);
                }

                return stats;
            }
            finally
            {
                _metricsLock.Release();
            }
        }

        private static double GetPercentile(List<double> sortedValues, double percentile)
        {
            if (!sortedValues.Any()) return 0;
            
            var index = (int)Math.Ceiling((percentile / 100) * sortedValues.Count) - 1;
            return sortedValues[Math.Max(0, Math.Min(index, sortedValues.Count - 1))];
        }

        #endregion

        #region Endpoint Analysis

        public List<EndpointPerformance> GetEndpointPerformance()
        {
            _metricsLock.Wait();
            try
            {
                var endpointGroups = _metrics
                    .GroupBy(m => m.Endpoint)
                    .Select(g => new EndpointPerformance
                    {
                        Endpoint = g.Key,
                        RequestCount = g.Count(),
                        AverageResponseTimeMs = g.Average(m => m.ResponseTimeMs),
                        MaxResponseTimeMs = g.Max(m => m.ResponseTimeMs),
                        MinResponseTimeMs = g.Min(m => m.ResponseTimeMs),
                        ErrorCount = g.Count(m => !m.Success),
                        ErrorRate = g.Any() ? (double)g.Count(m => !m.Success) / g.Count() * 100 : 0
                    })
                    .OrderByDescending(e => e.RequestCount)
                    .ToList();

                return endpointGroups;
            }
            finally
            {
                _metricsLock.Release();
            }
        }

        public List<PerformanceAlert> GetAlerts(AlertLevel? level = null)
        {
            _alertsLock.Wait();
            try
            {
                var alerts = _alerts.AsEnumerable();
                if (level.HasValue)
                {
                    alerts = alerts.Where(a => a.Level == level.Value);
                }
                return alerts.OrderByDescending(a => a.Timestamp).Take(50).ToList();
            }
            finally
            {
                _alertsLock.Release();
            }
        }

        public void ClearAlerts()
        {
            _alertsLock.Wait();
            try
            {
                _alerts.Clear();
            }
            finally
            {
                _alertsLock.Release();
            }
        }

        #endregion

        #region Caching & Optimization

        public CacheStats GetCacheStats()
        {
            return new CacheStats
            {
                TotalCacheKeys = _cacheKeys.Count,
                CacheHits = _cacheHits,
                CacheMisses = _cacheMisses,
                HitRate = _cacheHits + _cacheMisses > 0
                    ? (double)_cacheHits / (_cacheHits + _cacheMisses) * 100
                    : 0
            };
        }

        private long _cacheHits;
        private long _cacheMisses;
        private readonly HashSet<string> _cacheKeys = new();

        public void RecordCacheHit(string key)
        {
            Interlocked.Increment(ref _cacheHits);
            _cacheKeys.Add(key);
        }

        public void RecordCacheMiss(string key)
        {
            Interlocked.Increment(ref _cacheMisses);
            _cacheKeys.Add(key);
        }

        #endregion

        #region Health Checks

        public HealthCheckResult PerformHealthCheck()
        {
            var result = new HealthCheckResult
            {
                Timestamp = DateTime.UtcNow,
                Status = HealthStatus.Healthy
            };

            // Check memory
            var memoryMB = GC.GetTotalMemory(false) / (1024.0 * 1024.0);
            if (memoryMB > 500)
            {
                result.Status = HealthStatus.Degraded;
                result.Checks.Add(new HealthCheck
                {
                    Name = "Memory",
                    Status = HealthStatus.Unhealthy,
                    Message = $"High memory usage: {memoryMB:F2}MB"
                });
            }
            else
            {
                result.Checks.Add(new HealthCheck
                {
                    Name = "Memory",
                    Status = HealthStatus.Healthy,
                    Message = $"Memory usage: {memoryMB:F2}MB"
                });
            }

            // Check error rate
            var stats = GetStatistics();
            if (stats.ErrorRate > 5)
            {
                result.Status = HealthStatus.Degraded;
                result.Checks.Add(new HealthCheck
                {
                    Name = "Error Rate",
                    Status = HealthStatus.Unhealthy,
                    Message = $"High error rate: {stats.ErrorRate:F2}%"
                });
            }
            else
            {
                result.Checks.Add(new HealthCheck
                {
                    Name = "Error Rate",
                    Status = HealthStatus.Healthy,
                    Message = $"Error rate: {stats.ErrorRate:F2}%"
                });
            }

            // Check response time
            if (stats.P95ResponseTimeMs > 1000)
            {
                result.Status = HealthStatus.Degraded;
                result.Checks.Add(new HealthCheck
                {
                    Name = "Response Time",
                    Status = HealthStatus.Unhealthy,
                    Message = $"Slow P95 response: {stats.P95ResponseTimeMs:F2}ms"
                });
            }
            else
            {
                result.Checks.Add(new HealthCheck
                {
                    Name = "Response Time",
                    Status = HealthStatus.Healthy,
                    Message = $"P95 response: {stats.P95ResponseTimeMs:F2}ms"
                });
            }

            return result;
        }

        #endregion

        #region Optimization Suggestions

        public List<OptimizationSuggestion> GetOptimizationSuggestions()
        {
            var suggestions = new List<OptimizationSuggestion>();
            var stats = GetStatistics();
            var endpointStats = GetEndpointPerformance();

            // Check for slow endpoints
            var slowEndpoints = endpointStats.Where(e => e.AverageResponseTimeMs > 500).ToList();
            if (slowEndpoints.Any())
            {
                suggestions.Add(new OptimizationSuggestion
                {
                    Priority = OptimizationPriority.High,
                    Category = "Response Time",
                    Message = $"Found {slowEndpoints.Count} endpoints with average response time > 500ms",
                    Recommendation = "Consider adding caching, optimizing database queries, or implementing lazy loading"
                });
            }

            // Check cache hit rate
            var cacheStats = GetCacheStats();
            if (cacheStats.HitRate < 50)
            {
                suggestions.Add(new OptimizationSuggestion
                {
                    Priority = OptimizationPriority.Medium,
                    Category = "Caching",
                    Message = $"Cache hit rate is low: {cacheStats.HitRate:F1}%",
                    Recommendation = "Review cache eviction policy and increase cache TTL for frequently accessed data"
                });
            }

            // Check error rate
            if (stats.ErrorRate > 1)
            {
                suggestions.Add(new OptimizationSuggestion
                {
                    Priority = OptimizationPriority.High,
                    Category = "Reliability",
                    Message = $"Error rate is elevated: {stats.ErrorRate:F2}%",
                    Recommendation = "Review error logs and implement proper exception handling"
                });
            }

            // Check memory usage
            if (stats.CurrentMemoryMB > 200)
            {
                suggestions.Add(new OptimizationSuggestion
                {
                    Priority = OptimizationPriority.Medium,
                    Category = "Memory",
                    Message = $"Memory usage is high: {stats.CurrentMemoryMB:F2}MB",
                    Recommendation = "Consider implementing memory-efficient data structures and proper disposal"
                });
            }

            return suggestions;
        }

        #endregion

        private readonly SemaphoreSlim _alertsLock = new(1, 1);
    }

    #region Data Models

    public class PerformanceMetric
    {
        public DateTime Timestamp { get; set; }
        public string Endpoint { get; set; } = "";
        public double ResponseTimeMs { get; set; }
        public bool Success { get; set; }
        public double MemoryUsedMB { get; set; }
    }

    public class PerformanceStats
    {
        public TimeSpan Uptime { get; set; }
        public long TotalRequests { get; set; }
        public long TotalErrors { get; set; }
        public double ErrorRate { get; set; }
        public double AverageResponseTimeMs { get; set; }
        public double PeakResponseTimeMs { get; set; }
        public double MinResponseTimeMs { get; set; }
        public double P95ResponseTimeMs { get; set; }
        public double P99ResponseTimeMs { get; set; }
        public double RequestsPerSecond { get; set; }
        public double CurrentMemoryMB { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class EndpointPerformance
    {
        public string Endpoint { get; set; } = "";
        public int RequestCount { get; set; }
        public double AverageResponseTimeMs { get; set; }
        public double MaxResponseTimeMs { get; set; }
        public double MinResponseTimeMs { get; set; }
        public int ErrorCount { get; set; }
        public double ErrorRate { get; set; }
    }

    public class PerformanceAlert
    {
        public DateTime Timestamp { get; set; }
        public AlertLevel Level { get; set; }
        public string Message { get; set; } = "";
        public string? Endpoint { get; set; }
    }

    public enum AlertLevel
    {
        Info,
        Warning,
        Critical
    }

    public class CacheStats
    {
        public int TotalCacheKeys { get; set; }
        public long CacheHits { get; set; }
        public long CacheMisses { get; set; }
        public double HitRate { get; set; }
    }

    public class HealthCheckResult
    {
        public DateTime Timestamp { get; set; }
        public HealthStatus Status { get; set; }
        public List<HealthCheck> Checks { get; set; } = new();
    }

    public class HealthCheck
    {
        public string Name { get; set; } = "";
        public HealthStatus Status { get; set; }
        public string Message { get; set; } = "";
    }

    public enum HealthStatus
    {
        Healthy,
        Degraded,
        Unhealthy
    }

    public class OptimizationSuggestion
    {
        public OptimizationPriority Priority { get; set; }
        public string Category { get; set; } = "";
        public string Message { get; set; } = "";
        public string Recommendation { get; set; } = "";
    }

    public enum OptimizationPriority
    {
        Low,
        Medium,
        High
    }

    #endregion
}

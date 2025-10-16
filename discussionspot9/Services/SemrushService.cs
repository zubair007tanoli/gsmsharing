using discussionspot9.Interfaces;
using discussionspot9.Models.Semrush;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Web;

namespace discussionspot9.Services
{
    /// <summary>
    /// Service for integrating with Semrush API
    /// </summary>
    public class SemrushService : ISemrushService
    {
        private readonly HttpClient _httpClient;
        private readonly SemrushConfig _config;
        private readonly ILogger<SemrushService> _logger;
        private readonly SemaphoreSlim _rateLimitSemaphore;

        public SemrushService(
            HttpClient httpClient,
            IOptions<SemrushConfig> config,
            ILogger<SemrushService> logger)
        {
            _httpClient = httpClient;
            _config = config.Value;
            _logger = logger;
            _rateLimitSemaphore = new SemaphoreSlim(1, 1);

            // Configure HTTP client
            _httpClient.BaseAddress = new Uri(_config.BaseUrl);
            _httpClient.Timeout = TimeSpan.FromSeconds(_config.TimeoutSeconds);
            _httpClient.DefaultRequestHeaders.Add("x-rapidapi-key", _config.ApiKey);
            _httpClient.DefaultRequestHeaders.Add("x-rapidapi-host", _config.Host);
        }

        public async Task<KeywordOverviewResult?> GetKeywordOverviewAsync(string keyword, string database = "us")
        {
            try
            {
                await _rateLimitSemaphore.WaitAsync();
                await Task.Delay(_config.RateLimitDelayMs);

                var encodedKeyword = HttpUtility.UrlEncode(keyword);
                var endpoint = $"/keyword_overview?keyword={encodedKeyword}&database={database}";

                var response = await _httpClient.GetAsync(endpoint);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Semrush API returned {StatusCode} for keyword: {Keyword}", 
                        response.StatusCode, keyword);
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<KeywordOverviewResult>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                _logger.LogInformation("Retrieved keyword overview for: {Keyword}", keyword);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting keyword overview for: {Keyword}", keyword);
                return null;
            }
            finally
            {
                _rateLimitSemaphore.Release();
            }
        }

        public async Task<List<KeywordSuggestion>> GetKeywordSuggestionsAsync(string keyword, string database = "us", int limit = 10)
        {
            try
            {
                await _rateLimitSemaphore.WaitAsync();
                await Task.Delay(_config.RateLimitDelayMs);

                var encodedKeyword = HttpUtility.UrlEncode(keyword);
                var endpoint = $"/keyword_suggestions?keyword={encodedKeyword}&database={database}&limit={limit}";

                var response = await _httpClient.GetAsync(endpoint);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Semrush API returned {StatusCode} for keyword suggestions: {Keyword}", 
                        response.StatusCode, keyword);
                    return new List<KeywordSuggestion>();
                }

                var content = await response.Content.ReadAsStringAsync();
                var results = JsonSerializer.Deserialize<List<KeywordSuggestion>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                _logger.LogInformation("Retrieved {Count} keyword suggestions for: {Keyword}", 
                    results?.Count ?? 0, keyword);
                return results ?? new List<KeywordSuggestion>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting keyword suggestions for: {Keyword}", keyword);
                return new List<KeywordSuggestion>();
            }
            finally
            {
                _rateLimitSemaphore.Release();
            }
        }

        public async Task<List<CompetitorKeyword>> GetCompetitorKeywordsAsync(string domain, string database = "us", int limit = 20)
        {
            try
            {
                await _rateLimitSemaphore.WaitAsync();
                await Task.Delay(_config.RateLimitDelayMs);

                var encodedDomain = HttpUtility.UrlEncode(domain);
                var endpoint = $"/domain_organic_keywords?domain={encodedDomain}&database={database}&limit={limit}";

                var response = await _httpClient.GetAsync(endpoint);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Semrush API returned {StatusCode} for competitor keywords: {Domain}", 
                        response.StatusCode, domain);
                    return new List<CompetitorKeyword>();
                }

                var content = await response.Content.ReadAsStringAsync();
                var results = JsonSerializer.Deserialize<List<CompetitorKeyword>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                _logger.LogInformation("Retrieved {Count} competitor keywords for: {Domain}", 
                    results?.Count ?? 0, domain);
                return results ?? new List<CompetitorKeyword>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting competitor keywords for: {Domain}", domain);
                return new List<CompetitorKeyword>();
            }
            finally
            {
                _rateLimitSemaphore.Release();
            }
        }

        public async Task<UrlTrafficResult?> GetUrlTrafficAsync(string url, string database = "us")
        {
            try
            {
                await _rateLimitSemaphore.WaitAsync();
                await Task.Delay(_config.RateLimitDelayMs);

                var encodedUrl = HttpUtility.UrlEncode(url);
                var endpoint = $"/url_traffic?url={encodedUrl}&database={database}";

                var response = await _httpClient.GetAsync(endpoint);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Semrush API returned {StatusCode} for URL traffic: {Url}", 
                        response.StatusCode, url);
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<UrlTrafficResult>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                _logger.LogInformation("Retrieved URL traffic data for: {Url}", url);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting URL traffic for: {Url}", url);
                return null;
            }
            finally
            {
                _rateLimitSemaphore.Release();
            }
        }

        public async Task<List<KeywordOverviewResult>> AnalyzeKeywordsBatchAsync(List<string> keywords, string database = "us")
        {
            var results = new List<KeywordOverviewResult>();
            var tasks = keywords.Select(keyword => GetKeywordOverviewAsync(keyword, database));
            
            var batchResults = await Task.WhenAll(tasks);
            
            foreach (var result in batchResults)
            {
                if (result != null)
                {
                    results.Add(result);
                }
            }

            _logger.LogInformation("Analyzed {Count} keywords in batch", results.Count);
            return results;
        }

        public void Dispose()
        {
            _rateLimitSemaphore?.Dispose();
            _httpClient?.Dispose();
        }
    }
}

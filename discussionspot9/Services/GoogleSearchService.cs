using discussionspot9.Models.GoogleSearch;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using System.Web;
using SerpApi;

namespace discussionspot9.Services
{
    /// <summary>
    /// Google Search API service for keyword research (C# - FAST API calls)
    /// </summary>
    public class GoogleSearchService
    {
        private readonly HttpClient _httpClient;
        private readonly GoogleSearchConfig _config;
        private readonly IMemoryCache _cache;
        private readonly ILogger<GoogleSearchService> _logger;

        private const string CacheKeyTemplate = "google_search_{0}_{1}_{2}";

        public GoogleSearchService(
            HttpClient httpClient,
            IOptions<GoogleSearchConfig> config,
            IMemoryCache cache,
            ILogger<GoogleSearchService> logger)
        {
            _httpClient = httpClient;
            _config = config.Value;
            _cache = cache;
            _logger = logger;

            // Configure HTTP client
            if (_config.EnableRapidApi)
            {
                _httpClient.BaseAddress = new Uri(_config.BaseUrl);
                _httpClient.Timeout = TimeSpan.FromSeconds(_config.TimeoutSeconds);
                _httpClient.DefaultRequestHeaders.Add("x-rapidapi-key", _config.ApiKey);
                _httpClient.DefaultRequestHeaders.Add("x-rapidapi-host", _config.Host);
            }
        }

        /// <summary>
        /// Search Google and get related keywords (FAST - with caching)
        /// </summary>
        public async Task<GoogleSearchResponse?> SearchAsync(string query, int limit = 10, bool includeRelatedKeywords = true)
        {
            try
            {
                // Check cache first (avoid API calls)
                var effectiveLimit = limit > 0 ? limit : _config.DefaultLimit;
                var includeRelated = includeRelatedKeywords && _config.IncludeRelatedKeywords;
                var cacheKey = string.Format(CacheKeyTemplate, query, effectiveLimit, includeRelated);
                
                if (_cache.TryGetValue(cacheKey, out GoogleSearchResponse? cachedResult))
                {
                    _logger.LogInformation("Cache hit for query: {Query}", query);
                    return cachedResult;
                }

                GoogleSearchResponse? result = null;

                if (_config.EnableRapidApi)
                {
                    result = await SearchWithRapidApiAsync(query, effectiveLimit, includeRelated);
                }

                if (result == null && _config.EnableSerpApiFallback && !string.IsNullOrWhiteSpace(_config.SerpApiKey))
                {
                    _logger.LogInformation("Falling back to SerpApi for query: {Query}", query);
                    result = await SearchWithSerpApiAsync(query, effectiveLimit, includeRelated);
                }

                if (result != null)
                {
                    _cache.Set(cacheKey, result, new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(_config.CacheDurationHours),
                        Priority = CacheItemPriority.Normal
                    });

                    _logger.LogInformation("Retrieved {Count} results for query: {Query}",
                        result.Results.Count, query);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Google Search API for query: {Query}", query);
                return null;
            }
        }

        /// <summary>
        /// Get only related keywords (lightweight call)
        /// </summary>
        public async Task<List<string>> GetRelatedKeywordsAsync(string query)
        {
            var result = await SearchAsync(query, limit: 10, includeRelatedKeywords: true);
            
            return result?.RelatedKeywords?.Keywords
                .Select(k => k.Keyword)
                .Where(k => !string.IsNullOrEmpty(k))
                .ToList() ?? new List<string>();
        }

        /// <summary>
        /// Get competitor analysis from search results
        /// </summary>
        public async Task<List<CompetitorInsight>> GetCompetitorInsightsAsync(string query, int limit = 10)
        {
            var result = await SearchAsync(query, limit, includeRelatedKeywords: false);
            
            if (result == null || result.Results.Count == 0)
            {
                return new List<CompetitorInsight>();
            }

            return result.Results.Select(r => new CompetitorInsight
            {
                Position = r.Position,
                Url = r.Url,
                Domain = ExtractDomain(r.Url),
                Title = r.Title,
                Description = r.Description,
                TitleLength = r.Title.Length,
                DescriptionLength = r.Description.Length
            }).ToList();
        }

        /// <summary>
        /// Analyze keyword opportunity (combines search results + related keywords)
        /// </summary>
        public async Task<KeywordOpportunityAnalysis> AnalyzeKeywordOpportunityAsync(string keyword)
        {
            var result = await SearchAsync(keyword, limit: 10, includeRelatedKeywords: true);
            
            if (result == null)
            {
                return new KeywordOpportunityAnalysis
                {
                    Keyword = keyword,
                    Success = false,
                    ErrorMessage = "Failed to fetch data from Google Search API"
                };
            }

            var relatedKeywords = result.RelatedKeywords?.Keywords
                .Select(k => k.Keyword)
                .Where(k => !string.IsNullOrEmpty(k))
                .ToList() ?? new List<string>();

            var topCompetitors = result.Results.Take(5)
                .Select(r => ExtractDomain(r.Url))
                .Distinct()
                .ToList();

            // Estimate difficulty based on competitor quality
            var difficulty = EstimateKeywordDifficulty(result.Results);
            var opportunity = CalculateOpportunityScore(relatedKeywords.Count, difficulty);

            return new KeywordOpportunityAnalysis
            {
                Success = true,
                Keyword = keyword,
                RelatedKeywords = relatedKeywords,
                TopCompetitors = topCompetitors,
                TotalResults = result.Results.Count,
                EstimatedDifficulty = difficulty,
                OpportunityScore = opportunity,
                SuggestedKeywords = relatedKeywords.Take(5).ToList(),
                TopCompetitorTitles = result.Results.Take(3).Select(r => r.Title).ToList(),
                TopCompetitorDescriptions = result.Results.Take(3).Select(r => r.Description).ToList()
            };
        }

        /// <summary>
        /// Batch analyze multiple keywords
        /// </summary>
        public async Task<List<ProcessedKeywordData>> BatchAnalyzeKeywordsAsync(List<string> keywords)
        {
            var results = new List<ProcessedKeywordData>();

            foreach (var keyword in keywords.Take(10)) // Limit to 10 to avoid rate limits
            {
                try
                {
                    var searchResult = await SearchAsync(keyword, limit: 5, includeRelatedKeywords: false);
                    
                    if (searchResult != null && searchResult.Results.Count > 0)
                    {
                        var topResult = searchResult.Results.First();
                        
                        results.Add(new ProcessedKeywordData
                        {
                            Keyword = keyword,
                            Position = topResult.Position,
                            RelevanceScore = CalculateRelevanceScore(keyword, searchResult.Results),
                            HasKnowledgePanel = searchResult.KnowledgePanel != null,
                            CompetitorUrls = searchResult.Results.Take(3).Select(r => r.Url).ToList(),
                            CompetitorTitles = searchResult.Results.Take(3).Select(r => r.Title).ToList(),
                            SuggestedMetaDescription = topResult.Description
                        });
                    }

                    // Rate limiting - 1 second delay between requests
                    await Task.Delay(1000);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error analyzing keyword: {Keyword}", keyword);
                }
            }

            return results;
        }

        /// <summary>
        /// Get SEO insights for a topic
        /// </summary>
        public async Task<TopicSeoInsights> GetTopicInsightsAsync(string topic)
        {
            var searchResult = await SearchAsync(topic, limit: 10, includeRelatedKeywords: true);
            
            if (searchResult == null)
            {
                return new TopicSeoInsights { Success = false };
            }

            var insights = new TopicSeoInsights
            {
                Success = true,
                Topic = topic,
                RelatedKeywords = searchResult.RelatedKeywords?.Keywords.Select(k => k.Keyword).ToList() ?? new(),
                TopRankingDomains = searchResult.Results.Take(5).Select(r => ExtractDomain(r.Url)).Distinct().ToList(),
                CommonTitlePatterns = AnalyzeTitlePatterns(searchResult.Results),
                CommonDescriptionPatterns = AnalyzeDescriptionPatterns(searchResult.Results),
                AverageTitleLength = searchResult.Results.Any() ? (int)searchResult.Results.Average(r => r.Title.Length) : 0,
                AverageDescriptionLength = searchResult.Results.Any() ? (int)searchResult.Results.Average(r => r.Description.Length) : 0,
                SuggestedKeywords = searchResult.RelatedKeywords?.Keywords.Take(10).Select(k => k.Keyword).ToList() ?? new()
            };

            return insights;
        }

        // Helper methods
        private string ExtractDomain(string url)
        {
            try
            {
                var uri = new Uri(url);
                return uri.Host.Replace("www.", "");
            }
            catch
            {
                return url;
            }
        }

        private int EstimateKeywordDifficulty(List<SearchResult> results)
        {
            // Analyze top domains to estimate difficulty
            var topDomains = results.Take(5).Select(r => ExtractDomain(r.Url)).ToList();
            
            // High authority domains indicate higher difficulty
            var highAuthorityDomains = new[] { "wikipedia.org", "amazon.com", "youtube.com", "reddit.com", "medium.com" };
            var authorityCount = topDomains.Count(d => highAuthorityDomains.Any(ha => d.Contains(ha)));

            // Calculate difficulty (0-100)
            var difficulty = (authorityCount * 20) + 20; // Base 20, +20 per authority domain
            return Math.Min(100, difficulty);
        }

        private int CalculateOpportunityScore(int relatedKeywordsCount, int difficulty)
        {
            // More related keywords = more opportunity
            // Lower difficulty = better opportunity
            var keywordScore = Math.Min(relatedKeywordsCount * 5, 50);
            var difficultyScore = 50 - (difficulty / 2);
            
            return keywordScore + difficultyScore;
        }

        private int CalculateRelevanceScore(string keyword, List<SearchResult> results)
        {
            // Score based on how many top results contain the keyword
            var keywordLower = keyword.ToLower();
            var relevantResults = results.Take(5).Count(r => 
                r.Title.ToLower().Contains(keywordLower) || 
                r.Description.ToLower().Contains(keywordLower));

            return (relevantResults * 20); // 0-100 scale
        }

        private List<string> AnalyzeTitlePatterns(List<SearchResult> results)
        {
            // Extract common words from titles
            var allWords = results.SelectMany(r => r.Title.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            return allWords.GroupBy(w => w.ToLower())
                .OrderByDescending(g => g.Count())
                .Take(5)
                .Select(g => g.Key)
                .ToList();
        }

        private List<string> AnalyzeDescriptionPatterns(List<SearchResult> results)
        {
            // Extract common patterns from descriptions
            var allWords = results.SelectMany(r => r.Description.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            return allWords.GroupBy(w => w.ToLower())
                .Where(g => g.Key.Length > 4) // Skip short words
                .OrderByDescending(g => g.Count())
                .Take(5)
                .Select(g => g.Key)
                .ToList();
        }

        private async Task<GoogleSearchResponse?> SearchWithRapidApiAsync(string query, int limit, bool includeRelatedKeywords)
        {
            try
            {
                var queryParams = $"?query={HttpUtility.UrlEncode(query)}&limit={limit}&related_keywords={includeRelatedKeywords.ToString().ToLower()}";

                _logger.LogInformation("Calling RapidAPI Google Search for: {Query}", query);

                var response = await _httpClient.GetAsync(queryParams);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("RapidAPI Google Search returned {StatusCode} for query: {Query}",
                        response.StatusCode, query);
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<GoogleSearchResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "RapidAPI Google Search failed for query: {Query}", query);
                return null;
            }
        }

        private async Task<GoogleSearchResponse?> SearchWithSerpApiAsync(string query, int limit, bool includeRelatedKeywords)
        {
            var attempts = Math.Max(1, _config.SerpApiRetryCount);
            var timeout = TimeSpan.FromSeconds(Math.Max(5, _config.SerpApiTimeoutSeconds));

            for (var attempt = 1; attempt <= attempts; attempt++)
            {
                try
                {
                    var parameters = new Dictionary<string, string?>
                    {
                        ["engine"] = _config.SerpApiEngine,
                        ["q"] = query,
                        ["num"] = limit.ToString(CultureInfo.InvariantCulture),
                        ["google_domain"] = _config.SerpApiGoogleDomain,
                        ["gl"] = _config.SerpApiGl,
                        ["hl"] = _config.SerpApiHl,
                        ["location"] = _config.SerpApiLocation,
                        ["api_key"] = _config.SerpApiKey
                    };

                    var queryString = string.Join("&", parameters
                        .Where(kvp => !string.IsNullOrWhiteSpace(kvp.Value))
                        .Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value!)}"));

                    using var httpClient = new HttpClient { Timeout = timeout };
                    var response = await httpClient.GetAsync($"https://serpapi.com/search.json?{queryString}");

                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogWarning("SerpApi returned {StatusCode} for {Query} on attempt {Attempt}/{Attempts}",
                            response.StatusCode, query, attempt, attempts);

                        if (attempt == attempts)
                        {
                            return null;
                        }

                        await Task.Delay(TimeSpan.FromMilliseconds(500));
                        continue;
                    }

                    var json = await response.Content.ReadAsStringAsync();
                    var data = JObject.Parse(json);

                    var result = new GoogleSearchResponse
                    {
                        SearchTerm = query,
                        Results = MapSerpApiResults(data, limit).ToList()
                    };

                    if (includeRelatedKeywords)
                    {
                        result.RelatedKeywords = new RelatedKeywords
                        {
                            Keywords = MapSerpApiRelatedKeywords(data, limit).ToList()
                        };
                    }

                    return result;
                }
                catch (Exception ex)
                {
                    if (attempt == attempts)
                    {
                        _logger.LogError(ex, "SerpApi request failed for query: {Query}", query);
                        return null;
                    }

                    _logger.LogWarning(ex, "SerpApi request failed on attempt {Attempt}/{Attempts} for {Query}", attempt, attempts, query);
                    await Task.Delay(TimeSpan.FromMilliseconds(500));
                }
            }

            return null;
        }

        private IEnumerable<SearchResult> MapSerpApiResults(JObject data, int limit)
        {
            if (data["organic_results"] is not JArray organicResults)
            {
                yield break;
            }

            var position = 1;

            foreach (var item in organicResults.Take(limit))
            {
                var fallbackPosition = position++;

                var title = item.Value<string>("title") ?? string.Empty;
                var snippet = item.Value<string>("snippet") ??
                              item.Value<string>("description") ??
                              string.Empty;
                var link = item.Value<string>("link") ??
                           item.Value<string>("url") ??
                           string.Empty;

                yield return new SearchResult
                {
                    Position = item.Value<int?>("position") ?? fallbackPosition,
                    Title = title,
                    Description = snippet,
                    Url = link
                };
            }
        }

        private IEnumerable<KeywordItem> MapSerpApiRelatedKeywords(JObject data, int limit)
        {
            if (data["related_searches"] is not JArray relatedSearches)
            {
                yield break;
            }

            var index = 1;

            foreach (var item in relatedSearches.Take(limit))
            {
                var keyword = item.Value<string>("query") ??
                              item.Value<string>("title") ??
                              item.Value<string>("keyword") ??
                              string.Empty;

                if (string.IsNullOrWhiteSpace(keyword))
                {
                    continue;
                }

                yield return new KeywordItem
                {
                    Position = index++,
                    Keyword = keyword
                };
            }
        }
    }

    /// <summary>
    /// Competitor insight model
    /// </summary>
    public class CompetitorInsight
    {
        public int Position { get; set; }
        public string Url { get; set; } = string.Empty;
        public string Domain { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int TitleLength { get; set; }
        public int DescriptionLength { get; set; }
    }

    /// <summary>
    /// Keyword opportunity analysis
    /// </summary>
    public class KeywordOpportunityAnalysis
    {
        public bool Success { get; set; }
        public string Keyword { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
        public List<string> RelatedKeywords { get; set; } = new();
        public List<string> TopCompetitors { get; set; } = new();
        public int TotalResults { get; set; }
        public int EstimatedDifficulty { get; set; } // 0-100
        public int OpportunityScore { get; set; } // 0-100
        public List<string> SuggestedKeywords { get; set; } = new();
        public List<string> TopCompetitorTitles { get; set; } = new();
        public List<string> TopCompetitorDescriptions { get; set; } = new();
    }

}

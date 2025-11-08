using System.Text.Json.Serialization;

namespace discussionspot9.Models.GoogleSearch
{
    /// <summary>
    /// Google Search API response model
    /// </summary>
    public class GoogleSearchResponse
    {
        [JsonPropertyName("search_term")]
        public string SearchTerm { get; set; } = string.Empty;

        [JsonPropertyName("knowledge_panel")]
        public object? KnowledgePanel { get; set; }

        [JsonPropertyName("results")]
        public List<SearchResult> Results { get; set; } = new();

        [JsonPropertyName("related_keywords")]
        public RelatedKeywords? RelatedKeywords { get; set; }
    }

    /// <summary>
    /// Individual search result
    /// </summary>
    public class SearchResult
    {
        [JsonPropertyName("position")]
        public int Position { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// Related keywords data
    /// </summary>
    public class RelatedKeywords
    {
        [JsonPropertyName("spelling_suggestion_html")]
        public string? SpellingSuggestionHtml { get; set; }

        [JsonPropertyName("spelling_suggestion")]
        public string? SpellingSuggestion { get; set; }

        [JsonPropertyName("keywords")]
        public List<KeywordItem> Keywords { get; set; } = new();
    }

    /// <summary>
    /// Individual keyword item
    /// </summary>
    public class KeywordItem
    {
        [JsonPropertyName("position")]
        public int Position { get; set; }

        [JsonPropertyName("knowledge")]
        public KnowledgeInfo? Knowledge { get; set; }

        [JsonPropertyName("keyword_html")]
        public string KeywordHtml { get; set; } = string.Empty;

        [JsonPropertyName("keyword")]
        public string Keyword { get; set; } = string.Empty;
    }

    /// <summary>
    /// Knowledge panel info for keywords
    /// </summary>
    public class KnowledgeInfo
    {
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("label")]
        public string Label { get; set; } = string.Empty;

        [JsonPropertyName("image")]
        public string? Image { get; set; }
    }

    /// <summary>
    /// Google Search API configuration
    /// </summary>
    public class GoogleSearchConfig
    {
        public string ApiKey { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = "https://google-search74.p.rapidapi.com";
        public string Host { get; set; } = "google-search74.p.rapidapi.com";
        public bool EnableRapidApi { get; set; } = true;
        public bool EnableSerpApiFallback { get; set; } = false;
        public string SerpApiKey { get; set; } = string.Empty;
        public int SerpApiTimeoutSeconds { get; set; } = 30;
        public int SerpApiRetryCount { get; set; } = 1;
        public string SerpApiEngine { get; set; } = "google";
        public string SerpApiLocation { get; set; } = "Austin, Texas, United States";
        public string SerpApiGoogleDomain { get; set; } = "google.com";
        public string SerpApiGl { get; set; } = "us";
        public string SerpApiHl { get; set; } = "en";
        public CompetitorContentConfig CompetitorContent { get; set; } = new();
        public int DefaultLimit { get; set; } = 10;
        public bool IncludeRelatedKeywords { get; set; } = true;
        public int TimeoutSeconds { get; set; } = 30;
        public int CacheDurationHours { get; set; } = 24;
    }

    public class CompetitorContentConfig
    {
        public bool Enabled { get; set; } = true;
        public int MaxResults { get; set; } = 5;
        public int MaxContentLength { get; set; } = 2000;
        public int RequestTimeoutSeconds { get; set; } = 12;
        public bool IncludeFullContent { get; set; } = false;
    }

    /// <summary>
    /// Processed keyword data for SEO
    /// </summary>
    public class ProcessedKeywordData
    {
        public string Keyword { get; set; } = string.Empty;
        public int Position { get; set; }
        public int RelevanceScore { get; set; } // 0-100
        public bool HasKnowledgePanel { get; set; }
        public string? KnowledgeTitle { get; set; }
        public List<string> CompetitorUrls { get; set; } = new();
        public List<string> CompetitorTitles { get; set; } = new();
        public string SuggestedMetaDescription { get; set; } = string.Empty;
    }

    /// <summary>
    /// Aggregated topic insights from a Google search query.
    /// </summary>
    public class TopicSeoInsights
    {
        public bool Success { get; set; }
        public string Topic { get; set; } = string.Empty;
        public List<string> RelatedKeywords { get; set; } = new();
        public List<string> TopRankingDomains { get; set; } = new();
        public List<string> CommonTitlePatterns { get; set; } = new();
        public List<string> CommonDescriptionPatterns { get; set; } = new();
        public int AverageTitleLength { get; set; }
        public int AverageDescriptionLength { get; set; }
        public List<string> SuggestedKeywords { get; set; } = new();
    }
}

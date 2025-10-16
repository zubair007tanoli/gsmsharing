using System.Text.Json.Serialization;

namespace discussionspot9.Models.Semrush
{
    /// <summary>
    /// Result from Semrush keyword overview API
    /// </summary>
    public class KeywordOverviewResult
    {
        [JsonPropertyName("keyword")]
        public string Keyword { get; set; } = string.Empty;

        [JsonPropertyName("search_volume")]
        public int SearchVolume { get; set; }

        [JsonPropertyName("keyword_difficulty")]
        public decimal KeywordDifficulty { get; set; }

        [JsonPropertyName("cpc")]
        public decimal Cpc { get; set; }

        [JsonPropertyName("competition")]
        public string Competition { get; set; } = string.Empty;

        [JsonPropertyName("number_of_results")]
        public long NumberOfResults { get; set; }

        [JsonPropertyName("trends")]
        public List<int> Trends { get; set; } = new();

        [JsonPropertyName("related_keywords")]
        public List<string> RelatedKeywords { get; set; } = new();
    }

    /// <summary>
    /// Keyword suggestion from Semrush
    /// </summary>
    public class KeywordSuggestion
    {
        [JsonPropertyName("keyword")]
        public string Keyword { get; set; } = string.Empty;

        [JsonPropertyName("search_volume")]
        public int SearchVolume { get; set; }

        [JsonPropertyName("keyword_difficulty")]
        public decimal KeywordDifficulty { get; set; }

        [JsonPropertyName("cpc")]
        public decimal Cpc { get; set; }

        [JsonPropertyName("competition")]
        public string Competition { get; set; } = string.Empty;

        [JsonPropertyName("relevance")]
        public decimal Relevance { get; set; }
    }

    /// <summary>
    /// Competitor keyword data
    /// </summary>
    public class CompetitorKeyword
    {
        [JsonPropertyName("keyword")]
        public string Keyword { get; set; } = string.Empty;

        [JsonPropertyName("search_volume")]
        public int SearchVolume { get; set; }

        [JsonPropertyName("keyword_difficulty")]
        public decimal KeywordDifficulty { get; set; }

        [JsonPropertyName("cpc")]
        public decimal Cpc { get; set; }

        [JsonPropertyName("position")]
        public int Position { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;
    }

    /// <summary>
    /// URL traffic analysis result
    /// </summary>
    public class UrlTrafficResult
    {
        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;

        [JsonPropertyName("organic_traffic")]
        public int OrganicTraffic { get; set; }

        [JsonPropertyName("organic_keywords")]
        public int OrganicKeywords { get; set; }

        [JsonPropertyName("organic_cost")]
        public decimal OrganicCost { get; set; }

        [JsonPropertyName("paid_traffic")]
        public int PaidTraffic { get; set; }

        [JsonPropertyName("paid_keywords")]
        public int PaidKeywords { get; set; }

        [JsonPropertyName("paid_cost")]
        public decimal PaidCost { get; set; }

        [JsonPropertyName("top_keywords")]
        public List<CompetitorKeyword> TopKeywords { get; set; } = new();
    }

    /// <summary>
    /// Semrush API configuration
    /// </summary>
    public class SemrushConfig
    {
        public string ApiKey { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = "https://semrush8.p.rapidapi.com";
        public string Host { get; set; } = "semrush8.p.rapidapi.com";
        public int TimeoutSeconds { get; set; } = 30;
        public int MaxRetries { get; set; } = 3;
        public int RateLimitDelayMs { get; set; } = 1000;
    }

    /// <summary>
    /// Semrush API error response
    /// </summary>
    public class SemrushErrorResponse
    {
        [JsonPropertyName("error")]
        public string Error { get; set; } = string.Empty;

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("code")]
        public int Code { get; set; }
    }
}

using System;
using System.Collections.Generic;
using discussionspot9.Models.GoogleSearch;

namespace discussionspot9.Models.Seo
{
    public class SearchContentAggregationResult
    {
        public string PrimaryKeyword { get; set; } = string.Empty;
        public GoogleSearchResponse? SearchResponse { get; set; }
        public TopicSeoInsights? TopicInsights { get; set; }
        public GoogleSearchAnalysisContext? AnalysisContext { get; set; }
    }

    public class GoogleSearchAnalysisContext
    {
        public string SearchTerm { get; set; } = string.Empty;
        public List<string> RelatedKeywords { get; set; } = new();
        public List<CompetitorContentInsight> Competitors { get; set; } = new();
        public DateTime RetrievedAtUtc { get; set; } = DateTime.UtcNow;
    }

    public class CompetitorContentInsight
    {
        public int Position { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Domain { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ContentSnippet { get; set; } = string.Empty;
        public int WordCount { get; set; }
        public double EstimatedDomainAuthority { get; set; }
        public double EstimatedUrlAuthority { get; set; }
        public List<string> KeyPhrases { get; set; } = new();
    }
}


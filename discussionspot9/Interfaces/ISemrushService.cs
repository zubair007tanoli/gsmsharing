using discussionspot9.Models.Semrush;

namespace discussionspot9.Interfaces
{
    /// <summary>
    /// Service interface for Semrush API integration
    /// </summary>
    public interface ISemrushService
    {
        /// <summary>
        /// Get keyword overview data including search volume, difficulty, and CPC
        /// </summary>
        /// <param name="keyword">The keyword to analyze</param>
        /// <param name="database">Database to use (default: us)</param>
        /// <returns>Keyword overview data</returns>
        Task<KeywordOverviewResult?> GetKeywordOverviewAsync(string keyword, string database = "us");

        /// <summary>
        /// Get keyword suggestions based on a seed keyword
        /// </summary>
        /// <param name="keyword">Seed keyword</param>
        /// <param name="database">Database to use (default: us)</param>
        /// <param name="limit">Number of suggestions to return (default: 10)</param>
        /// <returns>List of keyword suggestions</returns>
        Task<List<KeywordSuggestion>> GetKeywordSuggestionsAsync(string keyword, string database = "us", int limit = 10);

        /// <summary>
        /// Get competitor keyword analysis for a domain
        /// </summary>
        /// <param name="domain">Domain to analyze</param>
        /// <param name="database">Database to use (default: us)</param>
        /// <param name="limit">Number of keywords to return (default: 20)</param>
        /// <returns>List of competitor keywords</returns>
        Task<List<CompetitorKeyword>> GetCompetitorKeywordsAsync(string domain, string database = "us", int limit = 20);

        /// <summary>
        /// Get URL traffic data
        /// </summary>
        /// <param name="url">URL to analyze</param>
        /// <param name="database">Database to use (default: us)</param>
        /// <returns>URL traffic data</returns>
        Task<UrlTrafficResult?> GetUrlTrafficAsync(string url, string database = "us");

        /// <summary>
        /// Analyze multiple keywords in batch
        /// </summary>
        /// <param name="keywords">List of keywords to analyze</param>
        /// <param name="database">Database to use (default: us)</param>
        /// <returns>List of keyword analysis results</returns>
        Task<List<KeywordOverviewResult>> AnalyzeKeywordsBatchAsync(List<string> keywords, string database = "us");
    }
}

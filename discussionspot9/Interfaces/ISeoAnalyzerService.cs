using discussionspot9.Models.ViewModels.CreativeViewModels;

namespace discussionspot9.Interfaces
{
    /// <summary>
    /// Service for analyzing and optimizing post content for SEO
    /// </summary>
    public interface ISeoAnalyzerService
    {
        /// <summary>
        /// Analyzes a post and returns SEO optimization suggestions
        /// </summary>
        Task<SeoAnalysisResult> AnalyzePostAsync(CreatePostViewModel model);
        
        /// <summary>
        /// Analyzes and automatically applies SEO optimizations to a post
        /// </summary>
        Task<CreatePostViewModel> OptimizePostAsync(CreatePostViewModel model);
    }
    
    /// <summary>
    /// Result of SEO analysis
    /// </summary>
    public class SeoAnalysisResult
    {
        public string OriginalTitle { get; set; } = string.Empty;
        public string OptimizedTitle { get; set; } = string.Empty;
        public string OriginalContent { get; set; } = string.Empty;
        public string OptimizedContent { get; set; } = string.Empty;
        public string SuggestedMetaDescription { get; set; } = string.Empty;
        public List<string> SuggestedKeywords { get; set; } = new List<string>();
        public double SeoScore { get; set; }
        public List<string> IssuesFound { get; set; } = new List<string>();
        public List<string> ImprovementsMade { get; set; } = new List<string>();
        public bool TitleChanged { get; set; }
        public bool ContentChanged { get; set; }
        public bool Error { get; set; }
        public string? Message { get; set; }
    }
}


namespace discussionspot9.Models.ViewModels.CreativeViewModels
{
    /// <summary>
    /// SEO Metadata View Model
    /// </summary>
    public class SeoMetadataViewModel
    {
        public string? MetaDescription { get; set; }
        public string? Keywords { get; set; }
        public double SeoScore { get; set; }
        public bool OptimizationApplied { get; set; }
        public List<string> IssuesFound { get; set; } = new List<string>();
        public List<string> ImprovementsMade { get; set; } = new List<string>();
    }
}

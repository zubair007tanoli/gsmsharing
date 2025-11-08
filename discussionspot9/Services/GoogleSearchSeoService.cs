using discussionspot9.Data.DbContext;
using discussionspot9.Interfaces;
using discussionspot9.Models.Domain;
using discussionspot9.Models.GoogleSearch;
using discussionspot9.Models.Seo;
using discussionspot9.Models.ViewModels.CreativeViewModels;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace discussionspot9.Services
{
    /// <summary>
    /// Enhanced SEO service using Google Search API (HYBRID: C# API + Python AI)
    /// </summary>
    public class GoogleSearchSeoService
    {
        private readonly GoogleSearchService _googleSearchService;
        private readonly ISeoAnalyzerService _pythonAnalyzer;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<GoogleSearchSeoService> _logger;
        private readonly SearchContentAggregator _searchContentAggregator;

        public GoogleSearchSeoService(
            GoogleSearchService googleSearchService,
            ISeoAnalyzerService pythonAnalyzer,
            ApplicationDbContext context,
            SearchContentAggregator searchContentAggregator,
            ILogger<GoogleSearchSeoService> logger)
        {
            _googleSearchService = googleSearchService;
            _pythonAnalyzer = pythonAnalyzer;
            _context = context;
            _searchContentAggregator = searchContentAggregator;
            _logger = logger;
        }

        /// <summary>
        /// Optimize post SEO with Google Search API data + Python AI
        /// </summary>
        public async Task<GoogleSeoOptimizationResult> OptimizePostAsync(int postId)
        {
            try
            {
                // Step 1: Get post from database (C# - FAST)
                var post = await _context.Posts
                    .Include(p => p.Community)
                    .Include(p => p.PostTags)
                    .ThenInclude(pt => pt.Tag)
                    .FirstOrDefaultAsync(p => p.PostId == postId);

                if (post == null)
                {
                    return new GoogleSeoOptimizationResult
                    {
                        Success = false,
                        ErrorMessage = "Post not found"
                    };
                }

                // Step 2: Extract initial keywords from post
                var initialKeywords = ExtractKeywords(post.Title, post.Content ?? "");
                var topKeyword = initialKeywords.FirstOrDefault() ?? post.Title;

                // Step 3: Build Google Search + competitor context
                var searchAggregation = await _searchContentAggregator.AggregateAsync(
                    post.Title,
                    post.Content ?? string.Empty,
                    initialKeywords);

                if (searchAggregation?.SearchResponse == null)
                {
                    return new GoogleSeoOptimizationResult
                    {
                        Success = false,
                        ErrorMessage = "Failed to fetch Google Search insights"
                    };
                }

                var googleSearchResult = searchAggregation.SearchResponse;
                var topicInsights = searchAggregation.TopicInsights ?? new TopicSeoInsights { Success = false };

                // Step 5: Prepare data for Python analyzer (AI processing)
                var pythonInput = new CreatePostViewModel
                {
                    Title = post.Title,
                    Content = post.Content ?? "",
                    CommunitySlug = post.Community?.Slug ?? "",
                    PostType = post.PostType
                };

                // Step 6: Call Python SEO Analyzer (AI-powered analysis)
                var pythonResult = await _pythonAnalyzer.AnalyzePostAsync(
                    pythonInput,
                    new SeoAnalysisContext
                    {
                        GoogleSearchData = searchAggregation.AnalysisContext
                    });

                // Step 7: Combine Google Search data with Python analysis
                var optimizedKeywords = CombineKeywords(
                    initialKeywords,
                    googleSearchResult.RelatedKeywords?.Keywords.Select(k => k.Keyword).ToList() ?? new(),
                    pythonResult.SuggestedKeywords,
                    searchAggregation.AnalysisContext?.Competitors.SelectMany(c => c.KeyPhrases).ToList()
                );

                // Step 8: Update SeoMetadata (C# - FAST database operation)
                await UpdateSeoMetadata(
                    postId,
                    optimizedKeywords,
                    googleSearchResult,
                    topicInsights,
                    pythonResult,
                    searchAggregation.AnalysisContext);

                // Step 9: Update post tags
                await UpdatePostTags(post, optimizedKeywords.Take(5).ToList());

                return new GoogleSeoOptimizationResult
                {
                    Success = true,
                    PostId = postId,
                    OriginalTitle = post.Title,
                    OptimizedTitle = pythonResult.OptimizedTitle,
                    OriginalKeywords = initialKeywords,
                    GoogleRelatedKeywords = googleSearchResult.RelatedKeywords?.Keywords.Select(k => k.Keyword).ToList() ?? new(),
                    OptimizedKeywords = optimizedKeywords,
                    SeoScore = (float)pythonResult.SeoScore,
                    MetaDescription = pythonResult.SuggestedMetaDescription,
                    TopCompetitors = searchAggregation.AnalysisContext?.Competitors
                        .Select(c => c.Domain)
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .Take(10)
                        .ToList() ?? topicInsights.TopRankingDomains,
                    ImprovementsMade = pythonResult.ImprovementsMade,
                    PrimaryKeyword = searchAggregation.PrimaryKeyword,
                    CompetitorInsights = searchAggregation.AnalysisContext?.Competitors ?? new List<CompetitorContentInsight>(),
                    ContentGaps = pythonResult.ContentGaps,
                    AuthoritySignals = pythonResult.AuthoritySignals
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error optimizing post {PostId} with Google Search", postId);
                return new GoogleSeoOptimizationResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        private List<string> ExtractKeywords(string title, string content)
        {
            var text = $"{title} {content}".ToLower();
            var words = text.Split(new[] { ' ', '\n', '\r', '\t', '.', ',', '!', '?', ';', ':', '-', '(', ')', '[', ']' },
                                   StringSplitOptions.RemoveEmptyEntries)
                .Where(w => w.Length >= 4)
                .ToList();

            var stopWords = new HashSet<string>
            {
                "this", "that", "these", "those", "what", "which", "who", "when",
                "where", "why", "how", "with", "from", "have", "been", "will"
            };

            return words
                .Where(w => !stopWords.Contains(w))
                .GroupBy(w => w)
                .OrderByDescending(g => g.Count())
                .Take(10)
                .Select(g => g.Key)
                .ToList();
        }

        private List<string> CombineKeywords(List<string> initial, List<string> google, List<string> python, List<string>? competitorPhrases)
        {
            var combined = new HashSet<string>();
            
            // Priority: Google related keywords (they're from actual search data)
            foreach (var k in google.Take(5))
                combined.Add(k);
            
            // Add Python suggestions
            foreach (var k in python.Take(3))
                combined.Add(k);
            
            // Add initial keywords if space remains
            foreach (var k in initial.Take(2))
                combined.Add(k);

            if (competitorPhrases != null)
            {
                foreach (var phrase in competitorPhrases.Take(5))
                {
                    combined.Add(phrase);
                }
            }
            
            return combined.Take(10).ToList();
        }

        private async Task UpdateSeoMetadata(int postId, List<string> keywords, 
            GoogleSearchResponse googleData, TopicSeoInsights insights, SeoAnalysisResult pythonResult, GoogleSearchAnalysisContext? analysisContext)
        {
            var seoMetadata = await _context.SeoMetadata
                .FirstOrDefaultAsync(s => s.EntityType == "post" && s.EntityId == postId);

            if (seoMetadata == null)
            {
                seoMetadata = new SeoMetadata
                {
                    EntityType = "post",
                    EntityId = postId,
                    CreatedAt = DateTime.UtcNow
                };
                _context.SeoMetadata.Add(seoMetadata);
            }

            // Update basic SEO fields
            seoMetadata.MetaTitle = pythonResult.OptimizedTitle;
            seoMetadata.MetaDescription = pythonResult.SuggestedMetaDescription;
            seoMetadata.Keywords = string.Join(", ", keywords);
            seoMetadata.UpdatedAt = DateTime.UtcNow;

            // Store comprehensive data in StructuredData
            var structuredData = new
            {
                google_search_analysis = new
                {
                    search_term = googleData.SearchTerm,
                    related_keywords = googleData.RelatedKeywords?.Keywords.Select(k => k.Keyword).ToList(),
                    top_competitors = insights.TopRankingDomains,
                    avg_title_length = insights.AverageTitleLength,
                    avg_description_length = insights.AverageDescriptionLength,
                    common_title_patterns = insights.CommonTitlePatterns,
                    analyzed_at = DateTime.UtcNow,
                    competitor_content = analysisContext?.Competitors.Select(c => new
                    {
                        c.Position,
                        c.Domain,
                        c.Title,
                        c.Url,
                        c.EstimatedDomainAuthority,
                        c.EstimatedUrlAuthority,
                        c.ContentSnippet,
                        c.KeyPhrases
                    }).ToList(),
                    authority_signals = pythonResult.AuthoritySignals,
                    content_gaps = pythonResult.ContentGaps
                },
                python_analysis = new
                {
                    seo_score = pythonResult.SeoScore,
                    issues_found = pythonResult.IssuesFound,
                    improvements_made = pythonResult.ImprovementsMade,
                    title_optimized = pythonResult.TitleChanged,
                    content_optimized = pythonResult.ContentChanged
                },
                recommended_keywords = keywords,
                optimization_source = "google_search_hybrid",
                last_optimized = DateTime.UtcNow
            };

            seoMetadata.StructuredData = JsonSerializer.Serialize(structuredData, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            await _context.SaveChangesAsync();
        }

        private async Task UpdatePostTags(Post post, List<string> keywords)
        {
            foreach (var keyword in keywords)
            {
                var tag = await _context.Tags
                    .FirstOrDefaultAsync(t => t.Name.ToLower() == keyword.ToLower());

                if (tag == null)
                {
                    tag = new Tag
                    {
                        Name = keyword,
                        Slug = keyword.ToLower().Replace(" ", "-"),
                        CreatedAt = DateTime.UtcNow,
                        PostCount = 0
                    };
                    _context.Tags.Add(tag);
                    await _context.SaveChangesAsync();
                }

                var existingPostTag = await _context.PostTags
                    .AnyAsync(pt => pt.PostId == post.PostId && pt.TagId == tag.TagId);

                if (!existingPostTag)
                {
                    _context.PostTags.Add(new PostTag
                    {
                        PostId = post.PostId,
                        TagId = tag.TagId
                    });
                }
            }

            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Google SEO optimization result
    /// </summary>
    public class GoogleSeoOptimizationResult
    {
        public bool Success { get; set; }
        public int PostId { get; set; }
        public string? ErrorMessage { get; set; }
        public string OriginalTitle { get; set; } = string.Empty;
        public string OptimizedTitle { get; set; } = string.Empty;
        public List<string> OriginalKeywords { get; set; } = new();
        public List<string> GoogleRelatedKeywords { get; set; } = new();
        public List<string> OptimizedKeywords { get; set; } = new();
        public float SeoScore { get; set; }
        public string MetaDescription { get; set; } = string.Empty;
        public List<string> TopCompetitors { get; set; } = new();
        public List<string> ImprovementsMade { get; set; } = new();
        public string PrimaryKeyword { get; set; } = string.Empty;
        public List<CompetitorContentInsight> CompetitorInsights { get; set; } = new();
        public List<string> ContentGaps { get; set; } = new();
        public List<string> AuthoritySignals { get; set; } = new();
    }
}

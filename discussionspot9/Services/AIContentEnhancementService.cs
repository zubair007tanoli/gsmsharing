using System.Text.Json;
using System.Text;

namespace discussionspot9.Services
{
    public interface IAIContentEnhancementService
    {
        Task<ContentEnhancementResult> EnhancePostContentAsync(string content, string title, string postType);
        Task<SEOOptimizationResult> OptimizeForSEOAsync(string content, string title, string[] keywords);
        Task<RelatedContentResult> FindRelatedContentAsync(string content, string title, int maxResults = 5);
        Task<ContentAnalysisResult> AnalyzeContentAsync(string content, string title);
    }

    public class AIContentEnhancementService : IAIContentEnhancementService
    {
        private readonly ILogger<AIContentEnhancementService> _logger;
        private readonly IConfiguration _configuration;

        public AIContentEnhancementService(ILogger<AIContentEnhancementService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<ContentEnhancementResult> EnhancePostContentAsync(string content, string title, string postType)
        {
            try
            {
                _logger.LogInformation($"Enhancing content for post: {title}");

                // Prepare request data
                var requestData = new
                {
                    content = content,
                    title = title,
                    post_type = postType,
                    enhancement_type = "content_improvement"
                };

                // Call Python AI service
                var result = await CallAIServiceAsync("enhance_content", requestData);

                if (result.Success)
                {
                    return new ContentEnhancementResult
                    {
                        Success = true,
                        EnhancedContent = result.Data?.GetProperty("enhanced_content").GetString() ?? content,
                        Suggestions = result.Data?.GetProperty("suggestions").EnumerateArray().Select(x => x.GetString()).ToList() ?? new List<string>(),
                        ReadabilityScore = result.Data?.GetProperty("readability_score").GetInt32() ?? 0,
                        EngagementScore = result.Data?.GetProperty("engagement_score").GetInt32() ?? 0
                    };
                }

                return new ContentEnhancementResult
                {
                    Success = false,
                    ErrorMessage = result.ErrorMessage ?? "Content enhancement failed"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error enhancing content for post: {title}");
                return new ContentEnhancementResult
                {
                    Success = false,
                    ErrorMessage = "An error occurred while enhancing content"
                };
            }
        }

        public async Task<SEOOptimizationResult> OptimizeForSEOAsync(string content, string title, string[] keywords)
        {
            try
            {
                _logger.LogInformation($"Optimizing SEO for post: {title}");

                var requestData = new
                {
                    content = content,
                    title = title,
                    keywords = keywords,
                    optimization_type = "seo"
                };

                var result = await CallAIServiceAsync("optimize_seo", requestData);

                if (result.Success)
                {
                    return new SEOOptimizationResult
                    {
                        Success = true,
                        OptimizedTitle = result.Data?.GetProperty("optimized_title").GetString() ?? title,
                        OptimizedContent = result.Data?.GetProperty("optimized_content").GetString() ?? content,
                        MetaDescription = result.Data?.GetProperty("meta_description").GetString() ?? "",
                        SuggestedKeywords = result.Data?.GetProperty("suggested_keywords").EnumerateArray().Select(x => x.GetString()).ToList() ?? new List<string>(),
                        SEOScore = result.Data?.GetProperty("seo_score").GetInt32() ?? 0,
                        Improvements = result.Data?.GetProperty("improvements").EnumerateArray().Select(x => x.GetString()).ToList() ?? new List<string>()
                    };
                }

                return new SEOOptimizationResult
                {
                    Success = false,
                    ErrorMessage = result.ErrorMessage ?? "SEO optimization failed"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error optimizing SEO for post: {title}");
                return new SEOOptimizationResult
                {
                    Success = false,
                    ErrorMessage = "An error occurred while optimizing SEO"
                };
            }
        }

        public async Task<RelatedContentResult> FindRelatedContentAsync(string content, string title, int maxResults = 5)
        {
            try
            {
                _logger.LogInformation($"Finding related content for post: {title}");

                var requestData = new
                {
                    content = content,
                    title = title,
                    max_results = maxResults,
                    analysis_type = "related_content"
                };

                var result = await CallAIServiceAsync("find_related", requestData);

                if (result.Success)
                {
                    var relatedItems = new List<RelatedContentItem>();
                    if (result.Data?.TryGetProperty("related_content", out var relatedContent) == true)
                    {
                        foreach (var item in relatedContent.EnumerateArray())
                        {
                            relatedItems.Add(new RelatedContentItem
                            {
                                Title = item.GetProperty("title").GetString() ?? "",
                                Url = item.GetProperty("url").GetString() ?? "",
                                RelevanceScore = item.GetProperty("relevance_score").GetDouble(),
                                Snippet = item.GetProperty("snippet").GetString() ?? "",
                                ContentType = item.GetProperty("content_type").GetString() ?? "post"
                            });
                        }
                    }

                    return new RelatedContentResult
                    {
                        Success = true,
                        RelatedContent = relatedItems,
                        TotalFound = relatedItems.Count
                    };
                }

                return new RelatedContentResult
                {
                    Success = false,
                    ErrorMessage = result.ErrorMessage ?? "Related content search failed"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error finding related content for post: {title}");
                return new RelatedContentResult
                {
                    Success = false,
                    ErrorMessage = "An error occurred while finding related content"
                };
            }
        }

        public async Task<ContentAnalysisResult> AnalyzeContentAsync(string content, string title)
        {
            try
            {
                _logger.LogInformation($"Analyzing content for post: {title}");

                var requestData = new
                {
                    content = content,
                    title = title,
                    analysis_type = "content_analysis"
                };

                var result = await CallAIServiceAsync("analyze_content", requestData);

                if (result.Success)
                {
                    return new ContentAnalysisResult
                    {
                        Success = true,
                        Sentiment = result.Data?.GetProperty("sentiment").GetString() ?? "neutral",
                        Topics = result.Data?.GetProperty("topics").EnumerateArray().Select(x => x.GetString()).ToList() ?? new List<string>(),
                        Keywords = result.Data?.GetProperty("keywords").EnumerateArray().Select(x => x.GetString()).ToList() ?? new List<string>(),
                        ReadabilityLevel = result.Data?.GetProperty("readability_level").GetString() ?? "medium",
                        WordCount = result.Data?.GetProperty("word_count").GetInt32() ?? 0,
                        EstimatedReadTime = result.Data?.GetProperty("estimated_read_time").GetInt32() ?? 0,
                        QualityScore = result.Data?.GetProperty("quality_score").GetInt32() ?? 0,
                        Suggestions = result.Data?.GetProperty("suggestions").EnumerateArray().Select(x => x.GetString()).ToList() ?? new List<string>()
                    };
                }

                return new ContentAnalysisResult
                {
                    Success = false,
                    ErrorMessage = result.ErrorMessage ?? "Content analysis failed"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error analyzing content for post: {title}");
                return new ContentAnalysisResult
                {
                    Success = false,
                    ErrorMessage = "An error occurred while analyzing content"
                };
            }
        }

        private async Task<AIServiceResult> CallAIServiceAsync(string endpoint, object requestData)
        {
            try
            {
                var pythonScript = Path.Combine(Directory.GetCurrentDirectory(), "Scripts", "ai_enhancement.py");
                var requestJson = JsonSerializer.Serialize(requestData);
                
                var processInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "python",
                    Arguments = $"\"{pythonScript}\" \"{endpoint}\" \"{requestJson}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = System.Diagnostics.Process.Start(processInfo);
                if (process != null)
                {
                    var output = await process.StandardOutput.ReadToEndAsync();
                    var error = await process.StandardError.ReadToEndAsync();
                    await process.WaitForExitAsync();

                    if (process.ExitCode == 0)
                    {
                        var response = JsonSerializer.Deserialize<AIServiceResult>(output);
                        return response ?? new AIServiceResult { Success = false, ErrorMessage = "Invalid response from AI service" };
                    }
                    else
                    {
                        _logger.LogWarning($"Python AI service failed: {error}");
                        return new AIServiceResult { Success = false, ErrorMessage = error };
                    }
                }

                return new AIServiceResult { Success = false, ErrorMessage = "Failed to start AI service" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error calling AI service: {endpoint}");
                return new AIServiceResult { Success = false, ErrorMessage = ex.Message };
            }
        }
    }

    // Result classes
    public class ContentEnhancementResult
    {
        public bool Success { get; set; }
        public string EnhancedContent { get; set; } = string.Empty;
        public List<string> Suggestions { get; set; } = new();
        public int ReadabilityScore { get; set; }
        public int EngagementScore { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }

    public class SEOOptimizationResult
    {
        public bool Success { get; set; }
        public string OptimizedTitle { get; set; } = string.Empty;
        public string OptimizedContent { get; set; } = string.Empty;
        public string MetaDescription { get; set; } = string.Empty;
        public List<string> SuggestedKeywords { get; set; } = new();
        public int SEOScore { get; set; }
        public List<string> Improvements { get; set; } = new();
        public string ErrorMessage { get; set; } = string.Empty;
    }

    public class RelatedContentResult
    {
        public bool Success { get; set; }
        public List<RelatedContentItem> RelatedContent { get; set; } = new();
        public int TotalFound { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }

    public class RelatedContentItem
    {
        public string Title { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public double RelevanceScore { get; set; }
        public string Snippet { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
    }

    public class ContentAnalysisResult
    {
        public bool Success { get; set; }
        public string Sentiment { get; set; } = string.Empty;
        public List<string> Topics { get; set; } = new();
        public List<string> Keywords { get; set; } = new();
        public string ReadabilityLevel { get; set; } = string.Empty;
        public int WordCount { get; set; }
        public int EstimatedReadTime { get; set; }
        public int QualityScore { get; set; }
        public List<string> Suggestions { get; set; } = new();
        public string ErrorMessage { get; set; } = string.Empty;
    }

    public class AIServiceResult
    {
        public bool Success { get; set; }
        public JsonElement? Data { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }
}

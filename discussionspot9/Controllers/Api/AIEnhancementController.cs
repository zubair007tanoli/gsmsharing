using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using discussionspot9.Services;

namespace discussionspot9.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AIEnhancementController : ControllerBase
    {
        private readonly IAIContentEnhancementService _aiService;
        private readonly ILogger<AIEnhancementController> _logger;

        public AIEnhancementController(IAIContentEnhancementService aiService, ILogger<AIEnhancementController> logger)
        {
            _aiService = aiService;
            _logger = logger;
        }

        [HttpPost("enhance-content")]
        public async Task<IActionResult> EnhanceContent([FromBody] EnhanceContentRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Content) || string.IsNullOrEmpty(request.Title))
                {
                    return BadRequest(new { success = false, message = "Content and title are required" });
                }

                var result = await _aiService.EnhancePostContentAsync(request.Content, request.Title, request.PostType ?? "text");
                
                if (result.Success)
                {
                    return Ok(new
                    {
                        success = true,
                        enhancedContent = result.EnhancedContent,
                        suggestions = result.Suggestions,
                        readabilityScore = result.ReadabilityScore,
                        engagementScore = result.EngagementScore
                    });
                }
                else
                {
                    return BadRequest(new { success = false, message = result.ErrorMessage });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enhancing content");
                return StatusCode(500, new { success = false, message = "An error occurred while enhancing content" });
            }
        }

        [HttpPost("optimize-seo")]
        public async Task<IActionResult> OptimizeSEO([FromBody] OptimizeSEORequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Content) || string.IsNullOrEmpty(request.Title))
                {
                    return BadRequest(new { success = false, message = "Content and title are required" });
                }

                var result = await _aiService.OptimizeForSEOAsync(request.Content, request.Title, request.Keywords ?? new string[0]);
                
                if (result.Success)
                {
                    return Ok(new
                    {
                        success = true,
                        optimizedTitle = result.OptimizedTitle,
                        optimizedContent = result.OptimizedContent,
                        metaDescription = result.MetaDescription,
                        suggestedKeywords = result.SuggestedKeywords,
                        seoScore = result.SEOScore,
                        improvements = result.Improvements
                    });
                }
                else
                {
                    return BadRequest(new { success = false, message = result.ErrorMessage });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error optimizing SEO");
                return StatusCode(500, new { success = false, message = "An error occurred while optimizing SEO" });
            }
        }

        [HttpPost("find-related")]
        public async Task<IActionResult> FindRelated([FromBody] FindRelatedRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Content) || string.IsNullOrEmpty(request.Title))
                {
                    return BadRequest(new { success = false, message = "Content and title are required" });
                }

                var result = await _aiService.FindRelatedContentAsync(request.Content, request.Title, request.MaxResults ?? 5);
                
                if (result.Success)
                {
                    return Ok(new
                    {
                        success = true,
                        relatedContent = result.RelatedContent,
                        totalFound = result.TotalFound
                    });
                }
                else
                {
                    return BadRequest(new { success = false, message = result.ErrorMessage });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding related content");
                return StatusCode(500, new { success = false, message = "An error occurred while finding related content" });
            }
        }

        [HttpPost("analyze-content")]
        public async Task<IActionResult> AnalyzeContent([FromBody] AnalyzeContentRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Content) || string.IsNullOrEmpty(request.Title))
                {
                    return BadRequest(new { success = false, message = "Content and title are required" });
                }

                var result = await _aiService.AnalyzeContentAsync(request.Content, request.Title);
                
                if (result.Success)
                {
                    return Ok(new
                    {
                        success = true,
                        sentiment = result.Sentiment,
                        topics = result.Topics,
                        keywords = result.Keywords,
                        readabilityLevel = result.ReadabilityLevel,
                        wordCount = result.WordCount,
                        estimatedReadTime = result.EstimatedReadTime,
                        qualityScore = result.QualityScore,
                        suggestions = result.Suggestions
                    });
                }
                else
                {
                    return BadRequest(new { success = false, message = result.ErrorMessage });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing content");
                return StatusCode(500, new { success = false, message = "An error occurred while analyzing content" });
            }
        }
    }

    // Request models
    public class EnhanceContentRequest
    {
        public string Content { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? PostType { get; set; }
    }

    public class OptimizeSEORequest
    {
        public string Content { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string[]? Keywords { get; set; }
    }

    public class FindRelatedRequest
    {
        public string Content { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public int? MaxResults { get; set; }
    }

    public class AnalyzeContentRequest
    {
        public string Content { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
    }
}

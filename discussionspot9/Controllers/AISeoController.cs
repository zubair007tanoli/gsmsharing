using discussionspot9.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace discussionspot9.Controllers
{
    /// <summary>
    /// Controller for AI-powered SEO optimization
    /// </summary>
    [Authorize]
    public class AISeoController : Controller
    {
        private readonly AISeoService _aiSeoService;
        private readonly ILogger<AISeoController> _logger;

        public AISeoController(
            AISeoService aiSeoService,
            ILogger<AISeoController> logger)
        {
            _aiSeoService = aiSeoService;
            _logger = logger;
        }

        /// <summary>
        /// Optimize a post using AI
        /// </summary>
        [HttpPost]
        [Route("api/ai-seo/optimize/{postId}")]
        public async Task<IActionResult> OptimizePost(int postId)
        {
            try
            {
                var result = await _aiSeoService.OptimizePostWithAIAsync(postId);
                
                if (!result.Success)
                {
                    return BadRequest(new { error = result.ErrorMessage });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error optimizing post {PostId}", postId);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Get AI optimization suggestions (GET endpoint for testing)
        /// </summary>
        [HttpGet]
        [Route("ai-seo/suggest/{postId}")]
        public async Task<IActionResult> GetSuggestions(int postId)
        {
            try
            {
                var result = await _aiSeoService.OptimizePostWithAIAsync(postId);
                
                if (!result.Success)
                {
                    return View("Error", new { Message = result.ErrorMessage });
                }

                return View("OptimizationResult", result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting suggestions for post {PostId}", postId);
                return View("Error", new { Message = ex.Message });
            }
        }
    }
}


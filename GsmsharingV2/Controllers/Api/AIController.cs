using GsmsharingV2.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GsmsharingV2.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AIController : ControllerBase
    {
        private readonly IAIService _aiService;
        private readonly ILogger<AIController> _logger;

        public AIController(IAIService aiService, ILogger<AIController> logger)
        {
            _aiService = aiService;
            _logger = logger;
        }

        [HttpPost("generate-title")]
        public async Task<IActionResult> GenerateTitle([FromBody] GenerateTitleRequest request)
        {
            try
            {
                var title = await _aiService.GeneratePostTitleAsync(request.Topic, request.Context ?? "");
                return Ok(new { title = title });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating title");
                return StatusCode(500, new { error = "Failed to generate title" });
            }
        }

        [HttpPost("generate-content")]
        public async Task<IActionResult> GenerateContent([FromBody] GenerateContentRequest request)
        {
            try
            {
                var content = await _aiService.GeneratePostContentAsync(
                    request.Title, 
                    request.Topic, 
                    request.MaxLength ?? 1000);
                return Ok(new { content = content });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating content");
                return StatusCode(500, new { error = "Failed to generate content" });
            }
        }

        [HttpPost("generate-excerpt")]
        public async Task<IActionResult> GenerateExcerpt([FromBody] GenerateExcerptRequest request)
        {
            try
            {
                var excerpt = await _aiService.GenerateExcerptAsync(request.Content, request.MaxLength ?? 200);
                return Ok(new { excerpt = excerpt });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating excerpt");
                return StatusCode(500, new { error = "Failed to generate excerpt" });
            }
        }

        [HttpPost("generate-tags")]
        public async Task<IActionResult> GenerateTags([FromBody] GenerateTagsRequest request)
        {
            try
            {
                var tags = await _aiService.GenerateTagsAsync(request.Content, request.MaxTags ?? 5);
                return Ok(new { tags = tags });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating tags");
                return StatusCode(500, new { error = "Failed to generate tags" });
            }
        }

        [HttpPost("improve-content")]
        public async Task<IActionResult> ImproveContent([FromBody] ImproveContentRequest request)
        {
            try
            {
                var improved = await _aiService.ImproveContentAsync(request.Content);
                return Ok(new { content = improved });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error improving content");
                return StatusCode(500, new { error = "Failed to improve content" });
            }
        }

        [HttpPost("moderate")]
        public async Task<IActionResult> Moderate([FromBody] ModerateRequest request)
        {
            try
            {
                var isSafe = await _aiService.ModerateContentAsync(request.Content);
                return Ok(new { isSafe = isSafe });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error moderating content");
                return StatusCode(500, new { error = "Failed to moderate content" });
            }
        }
    }

    public class GenerateTitleRequest
    {
        public string Topic { get; set; } = string.Empty;
        public string? Context { get; set; }
    }

    public class GenerateContentRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Topic { get; set; } = string.Empty;
        public int? MaxLength { get; set; }
    }

    public class GenerateExcerptRequest
    {
        public string Content { get; set; } = string.Empty;
        public int? MaxLength { get; set; }
    }

    public class GenerateTagsRequest
    {
        public string Content { get; set; } = string.Empty;
        public int? MaxTags { get; set; }
    }

    public class ImproveContentRequest
    {
        public string Content { get; set; } = string.Empty;
    }

    public class ModerateRequest
    {
        public string Content { get; set; } = string.Empty;
    }
}

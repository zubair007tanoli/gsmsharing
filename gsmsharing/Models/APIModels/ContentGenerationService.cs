using Azure;
using gsmsharing.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace gsmsharing.Models.APIModels
{
    public class ContentGenerationService
    {
        private readonly IGptService _gptService;
        private readonly ILogger<ContentGenerationService> _logger;

        public ContentGenerationService(
            IGptService gptService,
            ILogger<ContentGenerationService> logger)
        {
            _gptService = gptService;
            _logger = logger;
        }

        public async Task<ParsedContent> GenerateContentAsync(string originalTitle)
        {
            try
            {
                var gptResponse = await _gptService.GenerateContentAsync(originalTitle);
                var parsedResponse = JsonSerializer.Deserialize<AiResponse>(gptResponse);

                var content = parsedResponse?.Choices?.FirstOrDefault()?.Message?.Content;
                if (string.IsNullOrEmpty(content))
                {
                    throw new ApplicationException("No content generated from GPT service.");
                }

                return ParseGptResponse(content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating content for title: {Title}", originalTitle);
                throw;
            }
        }

        private ParsedContent ParseGptResponse(string content)
        {
            var result = new ParsedContent();
            var lines = content.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();

                if (trimmedLine.StartsWith("Title:", StringComparison.OrdinalIgnoreCase))
                {
                    result.Title = ExtractValue(trimmedLine, "Title:");
                }
                else if (trimmedLine.StartsWith("Meta description:", StringComparison.OrdinalIgnoreCase))
                {
                    result.Description = ExtractValue(trimmedLine, "Meta description:");
                }
                else if (trimmedLine.StartsWith("Meta keywords:", StringComparison.OrdinalIgnoreCase))
                {
                    result.Keywords = ExtractValue(trimmedLine, "Meta keywords:");
                }
            }

            // Clean up any quotes around the title
            result.Title = result.Title?.Trim('"', ' ');

            return result;
        }

        private string ExtractValue(string line, string prefix)
        {
            return line.Substring(prefix.Length).Trim();
        }
    }

    // Controller implementation for the parsed response
    [ApiController]
    [Route("api/[controller]")]
    public class ContentController : ControllerBase
    {
        private readonly ContentGenerationService _contentService;

        public ContentController(ContentGenerationService contentService)
        {
            _contentService = contentService;
        }

        [HttpPost("generate")]
        public async Task<ActionResult<ParsedContent>> GenerateContent([FromBody] string title)
        {
            try
            {
                var result = await _contentService.GenerateContentAsync(title);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in content generation: {Message}", ex.Message);
                return StatusCode(500, "Error generating content");
            }
        }
    }
}

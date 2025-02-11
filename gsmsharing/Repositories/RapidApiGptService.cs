using Azure;
using gsmsharing.Interfaces;
using gsmsharing.Models.APIModels;
using Newtonsoft.Json;
using System.Text;

namespace gsmsharing.Repositories
{
    public class RapidApiGptService:IGptService
    {
        private readonly HttpClient _httpClient;
        private readonly RapidApiConfig _config;
        private readonly ILogger<RapidApiGptService> _logger;

        public RapidApiGptService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<RapidApiGptService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            _config = new RapidApiConfig
            {
                ApiKey = configuration["RapidApi:ApiKey"],
                ApiHost = configuration["RapidApi:Host"],
                BaseUrl = configuration["RapidApi:BaseUrl"]
            };

            ConfigureHttpClient();
        }

        private void ConfigureHttpClient()
        {
            _httpClient.DefaultRequestHeaders.Add("x-rapidapi-key", _config.ApiKey);
            _httpClient.DefaultRequestHeaders.Add("x-rapidapi-host", _config.ApiHost);
        }

        public async Task<string> GenerateContentAsync(string title)
        {
            try
            {
                var request = new GptRequest
                {
                    Model = "gpt-3.5-turbo",
                    Messages = new List<GptMessage>
                {
                    new GptMessage
                    {
                        Role = "user",
                        Content = $"Generate Meta description and meta keywords for this title: {title}"
                    }
                }
                };

                var requestMessage = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(_config.BaseUrl),
                    Content = new StringContent(
                        JsonSerializer.Serialize(request),
                        Encoding.UTF8,
                        """application/json"""
                    )
                };

                var response = await _httpClient.SendAsync(requestMessage);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error calling RapidAPI GPT service for title: {Title}", title);
                throw new ApplicationException("Failed to generate content from GPT service.", ex);
            }
        }
    }

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

        public async Task<AiContentResult> GenerateContentAsync(string title)
        {
            try
            {
                var gptResponse = await _gptService.GenerateContentAsync(title);
                var parsedResponse = JsonSerializer.Deserialize<AiResponse>(gptResponse);

                var content = parsedResponse?.Choices?.FirstOrDefault()?.Message?.Content;
                if (string.IsNullOrEmpty(content))
                {
                    throw new ApplicationException("No content generated from GPT service.");
                }

                return new AiContentResult
                {
                    OriginalTitle = title,
                    Title = ExtractTitle(content),          // Add this line
                    Description = ExtractDescription(content),
                    Keywords = ExtractKeywords(content),
                    GeneratedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating content for title: {Title}", title);
                throw;
            }
        }

        private string ExtractTitle(string content)
        {
            var titleStart = content.IndexOf("Title:", StringComparison.OrdinalIgnoreCase);
            var titleEnd = content.IndexOf("Meta description:", StringComparison.OrdinalIgnoreCase);

            if (titleStart == -1 || titleEnd == -1)
                return string.Empty;

            var title = content.Substring(titleStart + 6, titleEnd - titleStart - 6).Trim();
            return title.Trim('"', ' '); // Remove any surrounding quotes
        }

        private string ExtractDescription(string content)
        {
            // Extract meta description from GPT response
            var descriptionStart = content.IndexOf("Meta description:", StringComparison.OrdinalIgnoreCase);
            var descriptionEnd = content.IndexOf("Meta keywords:", StringComparison.OrdinalIgnoreCase);

            if (descriptionStart == -1 || descriptionEnd == -1)
                return string.Empty;

            return content.Substring(descriptionStart + 16, descriptionEnd - descriptionStart - 16).Trim();
        }

        private string[] ExtractKeywords(string content)
        {
            // Extract keywords from GPT response
            var keywordsStart = content.IndexOf("Meta keywords:", StringComparison.OrdinalIgnoreCase);

            if (keywordsStart == -1)
                return Array.Empty<string>();

            var keywordsString = content.Substring(keywordsStart + 13).Trim();
            return keywordsString.Split(',').Select(k => k.Trim()).ToArray();
        }
    }
}

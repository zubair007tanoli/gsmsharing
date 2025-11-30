using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace discussionspot9.Services
{
    /// <summary>
    /// Free local AI service using Ollama (no API costs!)
    /// </summary>
    public interface ILocalAIService
    {
        Task<string?> OptimizeTitleAsync(string title, string? context = null);
        Task<string?> GenerateMetaDescriptionAsync(string content, string? title = null);
        Task<List<string>> ExtractKeywordsAsync(string content, int maxKeywords = 10);
        Task<string?> OptimizeContentAsync(string content, string? title = null);
        Task<string?> GenerateContentAsync(string topic, string? keywords = null);
        Task<bool> IsAvailableAsync();
    }

    public class LocalAIService : ILocalAIService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<LocalAIService> _logger;
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly string _model;
        private readonly bool _enabled;

        public LocalAIService(
            IConfiguration configuration,
            ILogger<LocalAIService> logger,
            IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient("LocalAI");
            _httpClient.Timeout = TimeSpan.FromSeconds(60);

            _baseUrl = _configuration["LocalAI:Ollama:BaseUrl"] ?? "http://localhost:11434";
            _model = _configuration["LocalAI:Ollama:Model"] ?? "llama3.2";
            _enabled = _configuration.GetValue<bool>("LocalAI:Ollama:Enabled", true);
        }

        public async Task<bool> IsAvailableAsync()
        {
            if (!_enabled)
                return false;

            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/tags");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<string?> OptimizeTitleAsync(string title, string? context = null)
        {
            if (!_enabled || string.IsNullOrWhiteSpace(title))
                return null;

            var prompt = $@"Optimize this post title for SEO and engagement. Make it compelling, clear, and under 60 characters.

Original title: {title}
{(context != null ? $"Context: {context}" : "")}

Return ONLY the optimized title, nothing else.";

            return await CallOllamaAsync(prompt);
        }

        public async Task<string?> GenerateMetaDescriptionAsync(string content, string? title = null)
        {
            if (!_enabled || string.IsNullOrWhiteSpace(content))
                return null;

            var contentPreview = content.Length > 500 ? content.Substring(0, 500) : content;
            var prompt = $@"Generate a compelling SEO meta description (150-160 characters) for this content.

Title: {title ?? "N/A"}
Content: {contentPreview}

Return ONLY the meta description, nothing else. Make it engaging and include key information.";

            var result = await CallOllamaAsync(prompt);
            return result?.Length > 160 ? result.Substring(0, 157) + "..." : result;
        }

        public async Task<List<string>> ExtractKeywordsAsync(string content, int maxKeywords = 10)
        {
            if (!_enabled || string.IsNullOrWhiteSpace(content))
                return new List<string>();

            var contentPreview = content.Length > 1000 ? content.Substring(0, 1000) : content;
            var prompt = $@"Extract the top {maxKeywords} most relevant SEO keywords from this content. Return ONLY a comma-separated list, nothing else.

Content: {contentPreview}";

            var result = await CallOllamaAsync(prompt);
            if (string.IsNullOrWhiteSpace(result))
                return new List<string>();

            return result.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(k => k.Trim())
                .Where(k => !string.IsNullOrWhiteSpace(k))
                .Take(maxKeywords)
                .ToList();
        }

        public async Task<string?> OptimizeContentAsync(string content, string? title = null)
        {
            if (!_enabled || string.IsNullOrWhiteSpace(content))
                return null;

            var contentPreview = content.Length > 2000 ? content.Substring(0, 2000) : content;
            var prompt = $@"Optimize this post content for SEO and readability. Improve structure, add headings if needed, and ensure it's engaging.

Title: {title ?? "N/A"}
Content: {contentPreview}

Return the optimized content with proper HTML formatting (use <p>, <h2>, <h3> tags). Keep the same meaning but improve clarity and SEO.";

            return await CallOllamaAsync(prompt);
        }

        public async Task<string?> GenerateContentAsync(string topic, string? keywords = null)
        {
            if (!_enabled || string.IsNullOrWhiteSpace(topic))
                return null;

            var prompt = $@"Write a well-structured, SEO-optimized blog post about: {topic}
{(keywords != null ? $"Include these keywords naturally: {keywords}" : "")}

Format with HTML tags (<h2>, <h3>, <p>). Make it informative and engaging. Minimum 500 words.";

            return await CallOllamaAsync(prompt);
        }

        private async Task<string?> CallOllamaAsync(string prompt)
        {
            try
            {
                var request = new
                {
                    model = _model,
                    prompt = prompt,
                    stream = false,
                    options = new
                    {
                        temperature = 0.7,
                        top_p = 0.9,
                        num_predict = 500
                    }
                };

                var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/api/generate", request);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<OllamaResponse>();
                return result?.Response?.Trim();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error calling Ollama at {BaseUrl}", _baseUrl);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error calling Ollama");
                return null;
            }
        }

        private class OllamaResponse
        {
            public string? Response { get; set; }
            public bool Done { get; set; }
        }
    }
}


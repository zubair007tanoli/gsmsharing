using GsmsharingV2.Interfaces;
using System.Net.Http.Json;
using System.Text.Json;

namespace GsmsharingV2.Services
{
    public class AIService : IAIService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<AIService> _logger;
        private readonly IConfiguration _configuration;

        public AIService(
            IHttpClientFactory httpClientFactory,
            ILogger<AIService> logger,
            IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<string> GeneratePostTitleAsync(string topic, string context)
        {
            try
            {
                // Call Python AI service
                var client = _httpClientFactory.CreateClient("PythonAI");
                var response = await client.PostAsJsonAsync("/api/ai/generate-title", new
                {
                    topic = topic,
                    context = context
                });

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<AIResponse>();
                    return result?.Content ?? $"Post about {topic}";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating post title");
            }

            // Fallback
            return $"Post about {topic}";
        }

        public async Task<string> GeneratePostContentAsync(string title, string topic, int maxLength = 1000)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("PythonAI");
                var response = await client.PostAsJsonAsync("/api/ai/generate-content", new
                {
                    title = title,
                    topic = topic,
                    maxLength = maxLength
                });

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<AIResponse>();
                    return result?.Content ?? $"Content about {topic}";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating post content");
            }

            return $"Content about {topic}";
        }

        public async Task<string> GenerateExcerptAsync(string content, int maxLength = 200)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("PythonAI");
                var response = await client.PostAsJsonAsync("/api/ai/generate-excerpt", new
                {
                    content = content,
                    maxLength = maxLength
                });

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<AIResponse>();
                    return result?.Content ?? content.Substring(0, Math.Min(maxLength, content.Length));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating excerpt");
            }

            // Fallback: simple truncation
            return content.Length > maxLength 
                ? content.Substring(0, maxLength) + "..." 
                : content;
        }

        public async Task<string> GenerateTagsAsync(string content, int maxTags = 5)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("PythonAI");
                var response = await client.PostAsJsonAsync("/api/ai/generate-tags", new
                {
                    content = content,
                    maxTags = maxTags
                });

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<AIResponse>();
                    return result?.Content ?? "";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating tags");
            }

            return "";
        }

        public async Task<string> ImproveContentAsync(string content)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("PythonAI");
                var response = await client.PostAsJsonAsync("/api/ai/improve-content", new
                {
                    content = content
                });

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<AIResponse>();
                    return result?.Content ?? content;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error improving content");
            }

            return content;
        }

        public async Task<string> SummarizeContentAsync(string content, int maxLength = 300)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("PythonAI");
                var response = await client.PostAsJsonAsync("/api/ai/summarize", new
                {
                    content = content,
                    maxLength = maxLength
                });

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<AIResponse>();
                    return result?.Content ?? content;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error summarizing content");
            }

            return content;
        }

        public async Task<bool> ModerateContentAsync(string content)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("PythonAI");
                var response = await client.PostAsJsonAsync("/api/ai/moderate", new
                {
                    content = content
                });

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ModerationResponse>();
                    return result?.IsSafe ?? true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error moderating content");
            }

            return true; // Default to safe if moderation fails
        }
    }

    public class AIResponse
    {
        public string Content { get; set; } = string.Empty;
    }

    public class ModerationResponse
    {
        public bool IsSafe { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}

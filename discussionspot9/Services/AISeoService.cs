using discussionspot9.Data.DbContext;
using discussionspot9.Interfaces;
using discussionspot9.Models.Domain;
using discussionspot9.Models.ViewModels.CreativeViewModels;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;

namespace discussionspot9.Services
{
    /// <summary>
    /// AI-powered SEO optimization service
    /// Integrates with LLM APIs (OpenAI, Claude, etc.) for advanced content optimization
    /// </summary>
    public class AISeoService
    {
        private readonly ISeoAnalyzerService _pythonAnalyzer;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AISeoService> _logger;
        private readonly IConfiguration _configuration;
        private readonly string? _openAiApiKey;
        private readonly string? _anthropicApiKey;
        private readonly string? _geminiApiKey;
        private readonly string _aiProvider;

        public AISeoService(
            ISeoAnalyzerService pythonAnalyzer,
            ApplicationDbContext context,
            ILogger<AISeoService> logger,
            IConfiguration configuration)
        {
            _pythonAnalyzer = pythonAnalyzer;
            _context = context;
            _logger = logger;
            _configuration = configuration;
            
            _openAiApiKey = _configuration["AI:OpenAI:ApiKey"];
            _anthropicApiKey = _configuration["AI:Anthropic:ApiKey"];
            _geminiApiKey = _configuration["AI:GoogleGemini:ApiKey"];
            _aiProvider = _configuration["AI:Provider"] ?? "openai";
        }

        /// <summary>
        /// Optimize content using AI (without requiring postId)
        /// </summary>
        public async Task<AISeoOptimizationResult> OptimizeContentAsync(
            string title,
            string content,
            string community,
            SeoAnalysisResult baseline)
        {
            try
            {
                // AI-powered optimization
                var aiOptimizations = await OptimizeWithAIAsync(
                    title,
                    content,
                    community,
                    baseline
                );

                // Determine which provider was actually used
                var actualProvider = _aiProvider;
                if (actualProvider == "openai" && !string.IsNullOrEmpty(_geminiApiKey))
                {
                    actualProvider = "openai (with Gemini fallback)";
                }

                // Return result
                return new AISeoOptimizationResult
                {
                    Success = true,
                    BaselineScore = baseline.SeoScore,
                    OptimizedTitle = aiOptimizations.OptimizedTitle ?? title,
                    OptimizedContent = aiOptimizations.OptimizedContent ?? content,
                    SuggestedMetaDescription = aiOptimizations.MetaDescription ?? baseline.SuggestedMetaDescription,
                    SuggestedKeywords = aiOptimizations.Keywords ?? baseline.SuggestedKeywords,
                    EstimatedScore = aiOptimizations.EstimatedScore,
                    Improvements = aiOptimizations.Improvements,
                    AiProvider = actualProvider
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error optimizing content with AI");
                return new AISeoOptimizationResult
                {
                    Success = false,
                    ErrorMessage = $"AI optimization failed: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Optimize post content using AI
        /// </summary>
        public async Task<AISeoOptimizationResult> OptimizePostWithAIAsync(int postId)
        {
            try
            {
                var post = await _context.Posts
                    .Include(p => p.Community)
                    .Include(p => p.PostTags)
                    .ThenInclude(pt => pt.Tag)
                    .FirstOrDefaultAsync(p => p.PostId == postId);

                if (post == null)
                {
                    return new AISeoOptimizationResult
                    {
                        Success = false,
                        ErrorMessage = "Post not found"
                    };
                }

                // Step 1: Get baseline SEO analysis (existing Python analyzer)
                var baselineAnalysis = await _pythonAnalyzer.AnalyzePostAsync(new CreatePostViewModel
                {
                    Title = post.Title,
                    Content = post.Content ?? "",
                    CommunitySlug = post.Community?.Slug ?? "",
                    PostType = post.PostType
                });

                // Step 2: AI-powered optimization
                var aiOptimizations = await OptimizeWithAIAsync(
                    post.Title,
                    post.Content ?? "",
                    post.Community?.Name ?? "",
                    baselineAnalysis
                );

                // Determine which provider was actually used
                var actualProvider = _aiProvider;
                if (actualProvider == "openai" && !string.IsNullOrEmpty(_geminiApiKey))
                {
                    // Check if we fell back to Gemini (this is a simple check, could be improved)
                    actualProvider = "openai (with Gemini fallback)";
                }

                // Step 3: Combine results
                return new AISeoOptimizationResult
                {
                    Success = true,
                    PostId = postId,
                    BaselineScore = baselineAnalysis.SeoScore,
                    OptimizedTitle = aiOptimizations.OptimizedTitle ?? post.Title,
                    OptimizedContent = aiOptimizations.OptimizedContent ?? post.Content,
                    SuggestedMetaDescription = aiOptimizations.MetaDescription ?? baselineAnalysis.SuggestedMetaDescription,
                    SuggestedKeywords = aiOptimizations.Keywords ?? baselineAnalysis.SuggestedKeywords,
                    EstimatedScore = aiOptimizations.EstimatedScore,
                    Improvements = aiOptimizations.Improvements,
                    AiProvider = actualProvider
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error optimizing post {PostId} with AI", postId);
                return new AISeoOptimizationResult
                {
                    Success = false,
                    ErrorMessage = $"AI optimization failed: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Optimize content using AI (OpenAI, Claude, Gemini, or local model)
        /// </summary>
        private async Task<AIOptimizationResult> OptimizeWithAIAsync(
            string title,
            string content,
            string community,
            SeoAnalysisResult baseline)
        {
            if (_aiProvider == "openai" && !string.IsNullOrEmpty(_openAiApiKey))
            {
                try
                {
                    return await OptimizeWithOpenAIAsync(title, content, community, baseline);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "OpenAI optimization failed, trying Gemini fallback");
                    // Fallback to Gemini if OpenAI fails
                    if (!string.IsNullOrEmpty(_geminiApiKey))
                    {
                        return await OptimizeWithGeminiAsync(title, content, community, baseline);
                    }
                    throw;
                }
            }
            else if (_aiProvider == "anthropic" && !string.IsNullOrEmpty(_anthropicApiKey))
            {
                try
                {
                    return await OptimizeWithAnthropicAsync(title, content, community, baseline);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Anthropic optimization failed, trying Gemini fallback");
                    // Fallback to Gemini if Anthropic fails
                    if (!string.IsNullOrEmpty(_geminiApiKey))
                    {
                        return await OptimizeWithGeminiAsync(title, content, community, baseline);
                    }
                    throw;
                }
            }
            else if (_aiProvider == "gemini" && !string.IsNullOrEmpty(_geminiApiKey))
            {
                return await OptimizeWithGeminiAsync(title, content, community, baseline);
            }
            else if (!string.IsNullOrEmpty(_geminiApiKey))
            {
                // Use Gemini as default fallback
                _logger.LogInformation("Using Gemini as fallback AI provider");
                return await OptimizeWithGeminiAsync(title, content, community, baseline);
            }
            else
            {
                _logger.LogWarning("AI provider not configured, falling back to Python analyzer");
                // Fallback to Python analyzer
                return new AIOptimizationResult
                {
                    OptimizedTitle = baseline.OptimizedTitle,
                    OptimizedContent = baseline.OptimizedContent,
                    MetaDescription = baseline.SuggestedMetaDescription,
                    Keywords = baseline.SuggestedKeywords,
                    EstimatedScore = baseline.SeoScore,
                    Improvements = baseline.ImprovementsMade
                };
            }
        }

        /// <summary>
        /// Optimize using OpenAI API
        /// </summary>
        private async Task<AIOptimizationResult> OptimizeWithOpenAIAsync(
            string title,
            string content,
            string community,
            SeoAnalysisResult baseline)
        {
            try
            {
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_openAiApiKey}");

                // Use current model names - try gpt-4o or gpt-4-turbo, fallback to gpt-3.5-turbo
                var configuredModel = _configuration["AI:OpenAI:Model"] ?? "gpt-4o";
                var model = configuredModel;
                var maxTokens = int.Parse(_configuration["AI:OpenAI:MaxTokens"] ?? "2000");

                // Build optimization prompt
                var prompt = BuildOptimizationPrompt(title, content, community, baseline);

                var requestBody = new
                {
                    model = model,
                    messages = new[]
                    {
                        new { role = "system", content = "You are an expert SEO content optimizer. Optimize content for search engines while maintaining readability and user value." },
                        new { role = "user", content = prompt }
                    },
                    temperature = 0.7,
                    max_tokens = maxTokens
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content_data = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content_data);
                
                // Get response content for error details
                var responseContent = await response.Content.ReadAsStringAsync();
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("OpenAI API Error: Status {StatusCode}, Response: {Response}", 
                        response.StatusCode, responseContent);
                    
                    // Try fallback model if 404 (model not found)
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound && model != "gpt-3.5-turbo")
                    {
                        _logger.LogWarning("Model {Model} not found, trying fallback gpt-3.5-turbo", model);
                        try
                        {
                            return await OptimizeWithOpenAIAsyncFallback(title, content, community, baseline, "gpt-3.5-turbo");
                        }
                        catch
                        {
                            // If OpenAI fallback also fails, try Gemini
                            if (!string.IsNullOrEmpty(_geminiApiKey))
                            {
                                _logger.LogWarning("OpenAI fallback failed, trying Gemini");
                                return await OptimizeWithGeminiAsync(title, content, community, baseline);
                            }
                            throw;
                        }
                    }
                    
                    // For other errors, try Gemini as fallback
                    if (!string.IsNullOrEmpty(_geminiApiKey))
                    {
                        _logger.LogWarning("OpenAI API error, trying Gemini fallback");
                        return await OptimizeWithGeminiAsync(title, content, community, baseline);
                    }
                    
                    // Try to parse error message
                    try
                    {
                        var errorResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
                        var errorMessage = errorResponse.TryGetProperty("error", out var error) 
                            ? error.TryGetProperty("message", out var msg) ? msg.GetString() : "Unknown error"
                            : responseContent;
                        
                        throw new Exception($"OpenAI API error ({response.StatusCode}): {errorMessage}");
                    }
                    catch
                    {
                        throw new Exception($"OpenAI API error ({response.StatusCode}): {responseContent}");
                    }
                }

                var result = JsonSerializer.Deserialize<OpenAIResponse>(responseContent);

                if (result?.Choices == null || result.Choices.Length == 0)
                {
                    throw new Exception("No response from OpenAI");
                }

                var aiResponse = result.Choices[0].Message.Content;
                return ParseAIResponse(aiResponse, baseline);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling OpenAI API");
                throw;
            }
        }

        /// <summary>
        /// Fallback method with specific model
        /// </summary>
        private async Task<AIOptimizationResult> OptimizeWithOpenAIAsyncFallback(
            string title,
            string content,
            string community,
            SeoAnalysisResult baseline,
            string fallbackModel)
        {
            try
            {
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_openAiApiKey}");

                var maxTokens = int.Parse(_configuration["AI:OpenAI:MaxTokens"] ?? "2000");
                var prompt = BuildOptimizationPrompt(title, content, community, baseline);

                var requestBody = new
                {
                    model = fallbackModel,
                    messages = new[]
                    {
                        new { role = "system", content = "You are an expert SEO content optimizer. Optimize content for search engines while maintaining readability and user value." },
                        new { role = "user", content = prompt }
                    },
                    temperature = 0.7,
                    max_tokens = maxTokens
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content_data = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content_data);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("OpenAI Fallback API Error: Status {StatusCode}, Response: {Response}", 
                        response.StatusCode, errorContent);
                    throw new Exception($"OpenAI API error ({response.StatusCode}): {errorContent}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<OpenAIResponse>(responseContent);

                if (result?.Choices == null || result.Choices.Length == 0)
                {
                    throw new Exception("No response from OpenAI");
                }

                var aiResponse = result.Choices[0].Message.Content;
                return ParseAIResponse(aiResponse, baseline);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling OpenAI API with fallback model");
                throw;
            }
        }

        /// <summary>
        /// Optimize using Anthropic Claude API
        /// </summary>
        private async Task<AIOptimizationResult> OptimizeWithAnthropicAsync(
            string title,
            string content,
            string community,
            SeoAnalysisResult baseline)
        {
            try
            {
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("x-api-key", _anthropicApiKey);
                httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

                var model = _configuration["AI:Anthropic:Model"] ?? "claude-3-opus-20240229";
                var maxTokens = int.Parse(_configuration["AI:Anthropic:MaxTokens"] ?? "2000");

                var prompt = BuildOptimizationPrompt(title, content, community, baseline);

                var requestBody = new
                {
                    model = model,
                    max_tokens = maxTokens,
                    messages = new[]
                    {
                        new { role = "user", content = prompt }
                    }
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content_data = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync("https://api.anthropic.com/v1/messages", content_data);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<AnthropicResponse>(responseContent);

                if (result?.Content == null || result.Content.Length == 0)
                {
                    throw new Exception("No response from Anthropic");
                }

                var aiResponse = result.Content[0].Text;
                return ParseAIResponse(aiResponse, baseline);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Anthropic API");
                throw;
            }
        }

        /// <summary>
        /// Optimize using Google Gemini API
        /// </summary>
        private async Task<AIOptimizationResult> OptimizeWithGeminiAsync(
            string title,
            string content,
            string community,
            SeoAnalysisResult baseline)
        {
            try
            {
                using var httpClient = new HttpClient();
                
                // Use correct model names: gemini-2.5-flash or gemini-2.5-pro
                var model = _configuration["AI:GoogleGemini:Model"] ?? "gemini-2.5-flash";
                var maxTokens = int.Parse(_configuration["AI:GoogleGemini:MaxTokens"] ?? "2000");

                var prompt = BuildOptimizationPrompt(title, content, community, baseline);

                // Gemini API uses v1beta endpoint with x-goog-api-key header
                var apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent";
                
                // Add API key to headers instead of query string (more secure)
                httpClient.DefaultRequestHeaders.Add("x-goog-api-key", _geminiApiKey);

                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new { text = prompt }
                            }
                        }
                    },
                    generationConfig = new
                    {
                        temperature = 0.7,
                        maxOutputTokens = Math.Max(maxTokens, 8000), // Ensure minimum 8000 tokens
                        topP = 0.8,
                        topK = 40
                    }
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content_data = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync(apiUrl, content_data);
                
                var responseContent = await response.Content.ReadAsStringAsync();
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Gemini API Error: Status {StatusCode}, Response: {Response}", 
                        response.StatusCode, responseContent);
                    
                    // Try to parse error message
                    try
                    {
                        var errorResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
                        var errorMessage = errorResponse.TryGetProperty("error", out var error) 
                            ? error.TryGetProperty("message", out var msg) ? msg.GetString() : "Unknown error"
                            : responseContent;
                        
                        throw new Exception($"Gemini API error ({response.StatusCode}): {errorMessage}");
                    }
                    catch
                    {
                        throw new Exception($"Gemini API error ({response.StatusCode}): {responseContent}");
                    }
                }

                _logger.LogInformation("Gemini API Response: {Response}", responseContent);
                
                string aiResponse;
                
                try
                {
                    var options = new JsonSerializerOptions 
                    { 
                        PropertyNameCaseInsensitive = true
                    };
                    var result = JsonSerializer.Deserialize<GeminiResponse>(responseContent, options);

                    if (result == null)
                    {
                        throw new Exception("Failed to deserialize response");
                    }

                    if (result.Candidates == null || result.Candidates.Length == 0)
                    {
                        _logger.LogError("Gemini response has no candidates. Full response: {Response}", responseContent);
                        throw new Exception("No response from Gemini - empty candidates array");
                    }

                    var candidate = result.Candidates[0];
                    
                    // Check finish reason
                    if (!string.IsNullOrEmpty(candidate.FinishReason) && candidate.FinishReason != "STOP")
                    {
                        _logger.LogWarning("Gemini finish reason: {FinishReason}", candidate.FinishReason);
                        if (candidate.FinishReason == "SAFETY")
                        {
                            throw new Exception("Gemini blocked the response due to safety filters");
                        }
                        if (candidate.FinishReason == "MAX_TOKENS")
                        {
                            throw new Exception("Gemini response was cut off due to token limit");
                        }
                    }
                    
                    if (candidate?.Content == null || candidate.Content.Parts == null || candidate.Content.Parts.Length == 0)
                    {
                        _logger.LogError("Gemini candidate has no content parts. FinishReason: {FinishReason}, Response: {Response}", 
                            candidate?.FinishReason, responseContent);
                        throw new Exception("No response from Gemini - empty content parts");
                    }

                    aiResponse = candidate.Content.Parts[0].Text ?? string.Empty;
                }
                catch (Exception parseEx)
                {
                    // Fallback: Try to extract text manually from JSON
                    _logger.LogWarning(parseEx, "Failed to parse Gemini response with model, trying manual extraction");
                    
                    try
                    {
                        var jsonDoc = JsonDocument.Parse(responseContent);
                        var root = jsonDoc.RootElement;
                        
                        if (root.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
                        {
                            var firstCandidate = candidates[0];
                            if (firstCandidate.TryGetProperty("content", out var contentElement))
                            {
                                if (contentElement.TryGetProperty("parts", out var parts) && parts.GetArrayLength() > 0)
                                {
                                    var firstPart = parts[0];
                                    if (firstPart.TryGetProperty("text", out var text))
                                    {
                                        aiResponse = text.GetString() ?? string.Empty;
                                    }
                                    else
                                    {
                                        throw new Exception("No 'text' property found in response parts");
                                    }
                                }
                                else
                                {
                                    throw new Exception("No 'parts' array found in content");
                                }
                            }
                            else
                            {
                                throw new Exception("No 'content' property found in candidate");
                            }
                        }
                        else
                        {
                            throw new Exception("No 'candidates' array found in response");
                        }
                    }
                    catch (Exception manualEx)
                    {
                        _logger.LogError(manualEx, "Failed to manually extract text from Gemini response");
                        throw new Exception($"Failed to parse Gemini response: {parseEx.Message}. Raw response: {responseContent.Substring(0, Math.Min(500, responseContent.Length))}");
                    }
                }
                
                if (string.IsNullOrWhiteSpace(aiResponse))
                {
                    _logger.LogError("Gemini response text is empty. Response: {Response}", responseContent);
                    throw new Exception("No response from Gemini - empty text");
                }

                _logger.LogInformation("Successfully received response from Gemini, length: {Length}", aiResponse.Length);
                return ParseAIResponse(aiResponse, baseline);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Gemini API");
                throw;
            }
        }

        /// <summary>
        /// Build optimization prompt for AI
        /// </summary>
        private string BuildOptimizationPrompt(string title, string content, string community, SeoAnalysisResult baseline)
        {
            // Limit content length to avoid token limits (keep first 2000 chars)
            var truncatedContent = content.Length > 2000 ? content.Substring(0, 2000) + "..." : content;
            
            // Limit issues to top 5
            var topIssues = baseline.IssuesFound.Take(5).ToList();
            
            var sb = new StringBuilder();
            sb.AppendLine("Optimize for SEO:");
            sb.AppendLine($"Community: {community}");
            sb.AppendLine($"SEO Score: {baseline.SeoScore:F1}/100");
            if (topIssues.Any())
            {
                sb.AppendLine($"Issues: {string.Join(", ", topIssues)}");
            }
            sb.AppendLine();
            sb.AppendLine($"Title: {title}");
            sb.AppendLine($"Content: {truncatedContent}");
            sb.AppendLine();
            sb.AppendLine("Provide JSON:");
            sb.AppendLine("{");
            sb.AppendLine("  \"optimizedTitle\": \"...\",");
            sb.AppendLine("  \"optimizedContent\": \"...\",");
            sb.AppendLine("  \"metaDescription\": \"...\",");
            sb.AppendLine("  \"keywords\": [\"keyword1\", \"keyword2\"],");
            sb.AppendLine("  \"estimatedScore\": 85.0,");
            sb.AppendLine("  \"improvements\": [\"improvement1\", \"improvement2\"]");
            sb.AppendLine("}");

            return sb.ToString();
        }

        /// <summary>
        /// Parse AI response into structured result
        /// </summary>
        private AIOptimizationResult ParseAIResponse(string aiResponse, SeoAnalysisResult baseline)
        {
            try
            {
                // Try to extract JSON from response
                var jsonStart = aiResponse.IndexOf('{');
                var jsonEnd = aiResponse.LastIndexOf('}') + 1;
                
                if (jsonStart >= 0 && jsonEnd > jsonStart)
                {
                    var json = aiResponse.Substring(jsonStart, jsonEnd - jsonStart);
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var parsed = JsonSerializer.Deserialize<AIOptimizationResult>(json, options);
                    
                    if (parsed != null)
                    {
                        return parsed;
                    }
                }

                // Fallback: parse manually or use baseline
                _logger.LogWarning("Could not parse AI response as JSON, using baseline");
                return new AIOptimizationResult
                {
                    OptimizedTitle = baseline.OptimizedTitle,
                    OptimizedContent = baseline.OptimizedContent,
                    MetaDescription = baseline.SuggestedMetaDescription,
                    Keywords = baseline.SuggestedKeywords,
                    EstimatedScore = baseline.SeoScore,
                    Improvements = baseline.ImprovementsMade
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing AI response");
                return new AIOptimizationResult
                {
                    OptimizedTitle = baseline.OptimizedTitle,
                    OptimizedContent = baseline.OptimizedContent,
                    MetaDescription = baseline.SuggestedMetaDescription,
                    Keywords = baseline.SuggestedKeywords,
                    EstimatedScore = baseline.SeoScore,
                    Improvements = baseline.ImprovementsMade
                };
            }
        }
    }

    #region Result Models

    public class AISeoOptimizationResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public int PostId { get; set; }
        public double BaselineScore { get; set; }
        public string OptimizedTitle { get; set; } = string.Empty;
        public string OptimizedContent { get; set; } = string.Empty;
        public string SuggestedMetaDescription { get; set; } = string.Empty;
        public List<string> SuggestedKeywords { get; set; } = new();
        public double EstimatedScore { get; set; }
        public List<string> Improvements { get; set; } = new();
        public string AiProvider { get; set; } = string.Empty;
    }

    internal class AIOptimizationResult
    {
        public string? OptimizedTitle { get; set; }
        public string? OptimizedContent { get; set; }
        public string? MetaDescription { get; set; }
        public List<string>? Keywords { get; set; }
        public double EstimatedScore { get; set; }
        public List<string>? Improvements { get; set; }
    }

    internal class OpenAIResponse
    {
        public OpenAIChoice[]? Choices { get; set; }
    }

    internal class OpenAIChoice
    {
        public OpenAIMessage? Message { get; set; }
    }

    internal class OpenAIMessage
    {
        public string Content { get; set; } = string.Empty;
    }

    internal class AnthropicResponse
    {
        public AnthropicContent[]? Content { get; set; }
    }

    internal class AnthropicContent
    {
        public string Text { get; set; } = string.Empty;
    }

    internal class GeminiResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("candidates")]
        public GeminiCandidate[]? Candidates { get; set; }
    }

    internal class GeminiCandidate
    {
        [System.Text.Json.Serialization.JsonPropertyName("content")]
        public GeminiContent? Content { get; set; }
        
        [System.Text.Json.Serialization.JsonPropertyName("finishReason")]
        public string? FinishReason { get; set; }
    }

    internal class GeminiContent
    {
        [System.Text.Json.Serialization.JsonPropertyName("parts")]
        public GeminiPart[]? Parts { get; set; }
    }

    internal class GeminiPart
    {
        [System.Text.Json.Serialization.JsonPropertyName("text")]
        public string? Text { get; set; }
    }

    #endregion
}


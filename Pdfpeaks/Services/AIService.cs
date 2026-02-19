using System.Net.Http.Json;
using System.Text.Json;

namespace Pdfpeaks.Services;

/// <summary>
/// C# AI Service for document processing using the Python AI microservice
/// Provides modern AI capabilities including text extraction, summarization, Q&A, and classification
/// </summary>
public class AIService
{
    private readonly ILogger<AIService> _logger;
    private readonly HttpClient _httpClient;
    private readonly string _aiServiceUrl;
    private readonly string _apiKey;
    private readonly bool _isEnabled;

    public AIService(ILogger<AIService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _httpClient = new HttpClient();
        
        // Get configuration from appsettings
        _aiServiceUrl = configuration["AIService:Url"] ?? "http://localhost:8000";
        _apiKey = configuration["AIService:ApiKey"] ?? "default-key";
        _isEnabled = configuration.GetValue<bool>("AIService:Enabled", true);
        
        _httpClient.BaseAddress = new Uri(_aiServiceUrl);
        _httpClient.Timeout = TimeSpan.FromMinutes(5);
        
        _logger.LogInformation("AI Service initialized. URL: {Url}, Enabled: {Enabled}", _aiServiceUrl, _isEnabled);
    }

    /// <summary>
    /// Process document with specified AI operation
    /// </summary>
    public async Task<AIProcessResult> ProcessDocumentAsync(
        string filePath, 
        AIProcessType processType, 
        CancellationToken cancellationToken = default)
    {
        if (!_isEnabled)
        {
            return new AIProcessResult
            {
                Success = false,
                ErrorMessage = "AI service is not enabled. Please configure AIService:Enabled in appsettings.json"
            };
        }

        try
        {
            using var content = new MultipartFormDataContent();
            
            // Read file
            var fileBytes = await File.ReadAllBytesAsync(filePath, cancellationToken);
            var fileContent = new ByteArrayContent(fileBytes);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
            content.Add(fileContent, "file", Path.GetFileName(filePath));
            content.Add(new StringContent(processType.ToString().ToLower()), "process_type");

            var request = new HttpRequestMessage(HttpMethod.Post, $"{_aiServiceUrl}/api/v1/process");
            request.Content = content;
            request.Headers.Add("Authorization", $"Bearer {_apiKey}");

            var response = await _httpClient.SendAsync(request, cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<AIProcessResponse>(cancellationToken: cancellationToken);
                
                if (result != null)
                {
                    return new AIProcessResult
                    {
                        Success = true,
                        TaskId = result.TaskId,
                        Status = result.Status,
                        Message = result.Message,
                        Result = result.Result,
                        ProcessingTimeMs = result.ProcessingTimeMs
                    };
                }
            }
            
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning("AI processing failed: {StatusCode} - {Error}", response.StatusCode, errorContent);
            
            return new AIProcessResult
            {
                Success = false,
                ErrorMessage = $"AI processing failed: {response.StatusCode}"
            };
        }
        catch (TaskCanceledException)
        {
            _logger.LogWarning("AI processing was cancelled");
            return new AIProcessResult
            {
                Success = false,
                ErrorMessage = "Processing was cancelled"
            };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to connect to AI service at {Url}", _aiServiceUrl);
            return new AIProcessResult
            {
                Success = false,
                ErrorMessage = $"Failed to connect to AI service: {ex.Message}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing document with AI");
            return new AIProcessResult
            {
                Success = false,
                ErrorMessage = $"Error: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Ask a question about a document
    /// </summary>
    public async Task<AIQnAResult> AskQuestionAsync(
        string filePath, 
        string question, 
        int maxSources = 5,
        CancellationToken cancellationToken = default)
    {
        if (!_isEnabled)
        {
            return new AIQnAResult
            {
                Success = false,
                ErrorMessage = "AI service is not enabled"
            };
        }

        try
        {
            using var content = new MultipartFormDataContent();
            
            var fileBytes = await File.ReadAllBytesAsync(filePath, cancellationToken);
            var fileContent = new ByteArrayContent(fileBytes);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
            content.Add(fileContent, "file", Path.GetFileName(filePath));
            content.Add(new StringContent(question), "question");
            content.Add(new StringContent(maxSources.ToString()), "max_sources");

            var request = new HttpRequestMessage(HttpMethod.Post, $"{_aiServiceUrl}/api/v1/qa");
            request.Content = content;
            request.Headers.Add("Authorization", $"Bearer {_apiKey}");

            var response = await _httpClient.SendAsync(request, cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<AIQnAResponse>(cancellationToken: cancellationToken);
                
                if (result != null)
                {
                    return new AIQnAResult
                    {
                        Success = true,
                        Answer = result.Answer,
                        Sources = result.Sources,
                        Confidence = result.Confidence
                    };
                }
            }
            
            return new AIQnAResult
            {
                Success = false,
                ErrorMessage = $"Q&A failed: {response.StatusCode}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Q&A");
            return new AIQnAResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// Summarize a document
    /// </summary>
    public async Task<AISummaryResult> SummarizeAsync(
        string filePath, 
        int maxLength = 200, 
        string style = "concise",
        CancellationToken cancellationToken = default)
    {
        if (!_isEnabled)
        {
            return new AISummaryResult
            {
                Success = false,
                ErrorMessage = "AI service is not enabled"
            };
        }

        try
        {
            using var content = new MultipartFormDataContent();
            
            var fileBytes = await File.ReadAllBytesAsync(filePath, cancellationToken);
            var fileContent = new ByteArrayContent(fileBytes);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
            content.Add(fileContent, "file", Path.GetFileName(filePath));
            content.Add(new StringContent(maxLength.ToString()), "max_length");
            content.Add(new StringContent(style), "style");

            var request = new HttpRequestMessage(HttpMethod.Post, $"{_aiServiceUrl}/api/v1/summarize");
            request.Content = content;
            request.Headers.Add("Authorization", $"Bearer {_apiKey}");

            var response = await _httpClient.SendAsync(request, cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<AISummaryResponse>(cancellationToken: cancellationToken);
                
                if (result != null)
                {
                    return new AISummaryResult
                    {
                        Success = true,
                        Summary = result.Summary,
                        OriginalLength = result.OriginalLength,
                        SummaryLength = result.SummaryLength,
                        CompressionRatio = result.CompressionRatio
                    };
                }
            }
            
            return new AISummaryResult
            {
                Success = false,
                ErrorMessage = $"Summarization failed: {response.StatusCode}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error summarizing document");
            return new AISummaryResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// Classify a document
    /// </summary>
    public async Task<AIClassifyResult> ClassifyAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        if (!_isEnabled)
        {
            return new AIClassifyResult
            {
                Success = false,
                ErrorMessage = "AI service is not enabled"
            };
        }

        try
        {
            using var content = new MultipartFormDataContent();
            
            var fileBytes = await File.ReadAllBytesAsync(filePath, cancellationToken);
            var fileContent = new ByteArrayContent(fileBytes);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
            content.Add(fileContent, "file", Path.GetFileName(filePath));

            var request = new HttpRequestMessage(HttpMethod.Post, $"{_aiServiceUrl}/api/v1/classify");
            request.Content = content;
            request.Headers.Add("Authorization", $"Bearer {_apiKey}");

            var response = await _httpClient.SendAsync(request, cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<AIClassifyResponse>(cancellationToken: cancellationToken);
                
                if (result != null)
                {
                    return new AIClassifyResult
                    {
                        Success = true,
                        Category = result.Category,
                        Confidence = result.Confidence,
                        AllScores = result.AllScores
                    };
                }
            }
            
            return new AIClassifyResult
            {
                Success = false,
                ErrorMessage = $"Classification failed: {response.StatusCode}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error classifying document");
            return new AIClassifyResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// Check if AI service is healthy
    /// </summary>
    public async Task<bool> IsHealthyAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_aiServiceUrl}/health");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}

/// <summary>
/// AI processing types
/// </summary>
public enum AIProcessType
{
    Extract,
    Summarize,
    Classify,
    ExtractEntities,
    FullAnalysis
}

// Response models from Python AI service
#pragma warning disable CS8618 // Non-nullable field is not initialized
public class AIProcessResponse
{
    public string TaskId { get; set; }
    public string Status { get; set; }
    public string Message { get; set; }
    public Dictionary<string, object> Result { get; set; }
    public double ProcessingTimeMs { get; set; }
}

public class AIQnAResponse
{
    public string Answer { get; set; }
    public List<Dictionary<string, object>> Sources { get; set; }
    public double Confidence { get; set; }
}

public class AISummaryResponse
{
    public string Summary { get; set; }
    public int OriginalLength { get; set; }
    public int SummaryLength { get; set; }
    public double CompressionRatio { get; set; }
}

public class AIClassifyResponse
{
    public string Category { get; set; }
    public double Confidence { get; set; }
    public Dictionary<string, double> AllScores { get; set; }
}
#pragma warning restore CS8618

/// <summary>
/// Result models for C# consumption
/// </summary>
public class AIProcessResult
{
    public bool Success { get; set; }
    public string? TaskId { get; set; }
    public string? Status { get; set; }
    public string? Message { get; set; }
    public Dictionary<string, object>? Result { get; set; }
    public double ProcessingTimeMs { get; set; }
    public string? ErrorMessage { get; set; }
}

public class AIQnAResult
{
    public bool Success { get; set; }
    public string? Answer { get; set; }
    public List<Dictionary<string, object>>? Sources { get; set; }
    public double Confidence { get; set; }
    public string? ErrorMessage { get; set; }
}

public class AISummaryResult
{
    public bool Success { get; set; }
    public string? Summary { get; set; }
    public int OriginalLength { get; set; }
    public int SummaryLength { get; set; }
    public double CompressionRatio { get; set; }
    public string? ErrorMessage { get; set; }
}

public class AIClassifyResult
{
    public bool Success { get; set; }
    public string? Category { get; set; }
    public double Confidence { get; set; }
    public Dictionary<string, double>? AllScores { get; set; }
    public string? ErrorMessage { get; set; }
}

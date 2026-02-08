using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Pdfpeaks.Models;
using Pdfpeaks.Services.AI;
using Pdfpeaks.Services.Auth;
using Pdfpeaks.Services.Caching;
using Pdfpeaks.Services.Infrastructure;
using System.Security.Claims;

namespace Pdfpeaks.Controllers.Api;

/// <summary>
/// Public API v2 Controller - Free AI-powered PDF processing endpoints
/// No external APIs required - runs entirely locally
/// </summary>
[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public class PublicApiController : ControllerBase
{
    private readonly ILogger<PublicApiController> _logger;
    private readonly DocumentAnalysisService _documentAnalysis;
    private readonly RedisCacheService _cache;
    private readonly JwtTokenService _jwtService;
    private readonly CustomRateLimitService _rateLimit;

    public PublicApiController(
        ILogger<PublicApiController> logger,
        DocumentAnalysisService documentAnalysis,
        RedisCacheService cache,
        JwtTokenService jwtService,
        CustomRateLimitService rateLimit)
    {
        _logger = logger;
        _documentAnalysis = documentAnalysis;
        _cache = cache;
        _jwtService = jwtService;
        _rateLimit = rateLimit;
    }

    #region Authentication

    /// <summary>
    /// Generate API token for authenticated users
    /// </summary>
    [HttpPost("auth/token")]
    [ProducesResponseType(typeof(JwtAuthResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GenerateToken([FromBody] TokenRequest request)
    {
        var rateLimit = await _rateLimit.CheckRateLimitAsync(
            "anonymous", "auth.token", 10, 60);
        
        if (!rateLimit.IsAllowed)
        {
            return StatusCode(429, new ErrorResponse
            {
                Error = "Too Many Requests",
                Message = $"Rate limit exceeded. Retry after {rateLimit.RetryAfter}"
            });
        }

        if (request.Email != "demo@pdfpeaks.com" || request.Password != "demo123")
        {
            return Unauthorized(new ErrorResponse
            {
                Error = "Invalid Credentials",
                Message = "Email or password is incorrect"
            });
        }

        var user = new ApplicationUser
        {
            Id = "demo-user-id",
            Email = request.Email,
            UserName = "demo_user",
            SubscriptionTier = SubscriptionTier.Pro
        };

        var token = await _jwtService.GenerateTokenAsync(user);

        return Ok(token);
    }

    #endregion

    #region Document Analysis

    /// <summary>
    /// Extract text from PDF (free algorithm)
    /// </summary>
    [HttpPost("documents/extract")]
    [Authorize]
    [RequestSizeLimit(150_000_000)]
    [ProducesResponseType(typeof(ExtractionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExtractText(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new ErrorResponse { Error = "File required" });
        }

        try
        {
            var fileId = Guid.NewGuid().ToString("N");
            var filePath = Path.Combine(Path.GetTempPath(), $"{fileId}.pdf");
            
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var result = await _documentAnalysis.ExtractTextFromPdfAsync(filePath);
            System.IO.File.Delete(filePath);

            if (!result.Success)
            {
                return BadRequest(new ErrorResponse { Error = result.ErrorMessage });
            }

            return Ok(new ExtractionResponse
            {
                FileId = fileId,
                FileName = file.FileName,
                TotalPages = result.TotalPages,
                WordCount = result.WordCount,
                CharCount = result.CharCount,
                Text = result.FullText
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting text");
            return StatusCode(500, new ErrorResponse { Error = "Extraction failed" });
        }
    }

    /// <summary>
    /// Summarize document (free extractive algorithm)
    /// </summary>
    [HttpPost("documents/summarize")]
    [Authorize]
    [ProducesResponseType(typeof(SummaryResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Summarize([FromBody] SummarizeRequest request)
    {
        if (string.IsNullOrEmpty(request.Text))
        {
            return BadRequest(new ErrorResponse { Error = "Text required" });
        }

        try
        {
            var result = _documentAnalysis.Summarize(
                request.Text, 
                request.MaxWords ?? 200, 
                (SummarizationStyle)(request.Style ?? 0));

            return Ok(new SummaryResponse
            {
                Summary = result.Summary,
                OriginalLength = result.OriginalLength,
                SummaryLength = result.SummaryLength,
                SentenceCount = result.SentenceCount,
                Confidence = result.Confidence,
                Style = request.Style ?? 0
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error summarizing");
            return StatusCode(500, new ErrorResponse { Error = "Summarization failed" });
        }
    }

    /// <summary>
    /// Classify document type (free keyword algorithm)
    /// </summary>
    [HttpPost("documents/classify")]
    [Authorize]
    [ProducesResponseType(typeof(ClassifyResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Classify([FromBody] TextRequest request)
    {
        if (string.IsNullOrEmpty(request.Text))
        {
            return BadRequest(new ErrorResponse { Error = "Text required" });
        }

        try
        {
            var result = _documentAnalysis.ClassifyDocument(request.Text);
            var keywords = _documentAnalysis.ExtractKeywords(request.Text, 10);

            return Ok(new ClassifyResponse
            {
                Category = result.Category,
                Confidence = result.Confidence,
                AllScores = result.AllScores.ToDictionary(kvp => kvp.Key, kvp => (double)kvp.Value),
                TopKeywords = keywords.Select(k => new KeywordInfo
                {
                    Word = k.Word,
                    Score = k.Score,
                    Frequency = k.Frequency
                }).ToList()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error classifying");
            return StatusCode(500, new ErrorResponse { Error = "Classification failed" });
        }
    }

    /// <summary>
    /// Extract keywords (free TF-IDF algorithm)
    /// </summary>
    [HttpPost("documents/keywords")]
    [Authorize]
    [ProducesResponseType(typeof(KeywordsResponse), StatusCodes.Status200OK)]
    public IActionResult ExtractKeywords([FromBody] KeywordsRequest request)
    {
        if (string.IsNullOrEmpty(request.Text))
        {
            return BadRequest(new ErrorResponse { Error = "Text required" });
        }

        try
        {
            var keywords = _documentAnalysis.ExtractKeywords(request.Text, request.TopN ?? 10);

            return Ok(new KeywordsResponse
            {
                Keywords = keywords.Select(k => new KeywordInfo
                {
                    Word = k.Word,
                    Score = k.Score,
                    Frequency = k.Frequency
                }).ToList()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting keywords");
            return StatusCode(500, new ErrorResponse { Error = "Keyword extraction failed" });
        }
    }

    /// <summary>
    /// Analyze sentiment (free algorithm)
    /// </summary>
    [HttpPost("documents/sentiment")]
    [Authorize]
    [ProducesResponseType(typeof(SentimentResponse), StatusCodes.Status200OK)]
    public IActionResult AnalyzeSentiment([FromBody] TextRequest request)
    {
        if (string.IsNullOrEmpty(request.Text))
        {
            return BadRequest(new ErrorResponse { Error = "Text required" });
        }

        try
        {
            var result = _documentAnalysis.AnalyzeSentiment(request.Text);

            return Ok(new SentimentResponse
            {
                Score = result.Score,
                Label = result.Label,
                PositiveCount = result.PositiveWordCount,
                NegativeCount = result.NegativeWordCount,
                Confidence = result.Confidence
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing sentiment");
            return StatusCode(500, new ErrorResponse { Error = "Sentiment analysis failed" });
        }
    }

    /// <summary>
    /// Detect language (free algorithm)
    /// </summary>
    [HttpPost("documents/language")]
    [Authorize]
    [ProducesResponseType(typeof(LanguageResponse), StatusCodes.Status200OK)]
    public IActionResult DetectLanguage([FromBody] TextRequest request)
    {
        if (string.IsNullOrEmpty(request.Text))
        {
            return BadRequest(new ErrorResponse { Error = "Text required" });
        }

        try
        {
            var result = _documentAnalysis.DetectLanguage(request.Text);

            return Ok(new LanguageResponse
            {
                Language = result.Language,
                Confidence = result.Confidence,
                AllScores = result.AllScores
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting language");
            return StatusCode(500, new ErrorResponse { Error = "Language detection failed" });
        }
    }

    /// <summary>
    /// Calculate complexity metrics (free algorithm)
    /// </summary>
    [HttpPost("documents/complexity")]
    [Authorize]
    [ProducesResponseType(typeof(ComplexityResponse), StatusCodes.Status200OK)]
    public IActionResult CalculateComplexity([FromBody] TextRequest request)
    {
        if (string.IsNullOrEmpty(request.Text))
        {
            return BadRequest(new ErrorResponse { Error = "Text required" });
        }

        try
        {
            var result = _documentAnalysis.CalculateComplexity(request.Text);

            return Ok(new ComplexityResponse
            {
                WordCount = result.WordCount,
                SentenceCount = result.SentenceCount,
                UniqueWordCount = result.UniqueWordCount,
                AvgWordsPerSentence = result.AvgWordsPerSentence,
                FleschScore = result.FleschScore,
                GradeLevel = result.GradeLevel,
                Difficulty = result.Difficulty,
                ReadingTimeMinutes = (int)Math.Ceiling(result.ReadingTimeMinutes)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating complexity");
            return StatusCode(500, new ErrorResponse { Error = "Complexity calculation failed" });
        }
    }

    /// <summary>
    /// Answer questions (free extractive QA)
    /// </summary>
    [HttpPost("documents/qa")]
    [Authorize]
    [ProducesResponseType(typeof(QAResponse), StatusCodes.Status200OK)]
    public IActionResult AnswerQuestion([FromBody] QARequest request)
    {
        if (string.IsNullOrEmpty(request.Text) || string.IsNullOrEmpty(request.Question))
        {
            return BadRequest(new ErrorResponse { Error = "Text and question required" });
        }

        try
        {
            var result = _documentAnalysis.AnswerQuestion(request.Text, request.Question);

            return Ok(new QAResponse
            {
                Answer = result.Answer,
                Confidence = result.Confidence,
                Sources = result.Sources.Select(s => new QASourceInfo
                {
                    Text = s.Text,
                    RelevanceScore = s.RelevanceScore
                }).ToList()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error answering question");
            return StatusCode(500, new ErrorResponse { Error = "Q&A failed" });
        }
    }

    /// <summary>
    /// Extract entities (free pattern matching)
    /// </summary>
    [HttpPost("documents/entities")]
    [Authorize]
    [ProducesResponseType(typeof(EntitiesResponse), StatusCodes.Status200OK)]
    public IActionResult ExtractEntities([FromBody] TextRequest request)
    {
        if (string.IsNullOrEmpty(request.Text))
        {
            return BadRequest(new ErrorResponse { Error = "Text required" });
        }

        try
        {
            var result = _documentAnalysis.ExtractEntities(request.Text);

            return Ok(new EntitiesResponse
            {
                Emails = result.Emails,
                Phones = result.Phones,
                Dates = result.Dates,
                Currencies = result.Currencies,
                URLs = result.URLs,
                IPAddresses = result.IPAddresses
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting entities");
            return StatusCode(500, new ErrorResponse { Error = "Entity extraction failed" });
        }
    }

    /// <summary>
    /// Full document analysis (all features)
    /// </summary>
    [HttpPost("documents/analyze")]
    [Authorize]
    [ProducesResponseType(typeof(FullAnalysisResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> FullAnalysis(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new ErrorResponse { Error = "File required" });
        }

        try
        {
            var fileId = Guid.NewGuid().ToString("N");
            var filePath = Path.Combine(Path.GetTempPath(), $"{fileId}.pdf");
            
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var extraction = await _documentAnalysis.ExtractTextFromPdfAsync(filePath);
            System.IO.File.Delete(filePath);

            if (!extraction.Success)
            {
                return BadRequest(new ErrorResponse { Error = extraction.ErrorMessage });
            }

            var text = extraction.FullText;
            var classification = _documentAnalysis.ClassifyDocument(text);
            var keywords = _documentAnalysis.ExtractKeywords(text, 10);
            var sentiment = _documentAnalysis.AnalyzeSentiment(text);
            var language = _documentAnalysis.DetectLanguage(text);
            var complexity = _documentAnalysis.CalculateComplexity(text);
            var summary = _documentAnalysis.Summarize(text, 200, SummarizationStyle.Concise);

            return Ok(new FullAnalysisResponse
            {
                FileId = fileId,
                FileName = file.FileName,
                TotalPages = extraction.TotalPages,
                WordCount = extraction.WordCount,
                Summary = summary.Summary,
                Classification = new ClassificationInfo
                {
                    Category = classification.Category,
                    Confidence = classification.Confidence
                },
                Keywords = keywords.Select(k => k.Word).ToList(),
                Sentiment = new SentimentInfo
                {
                    Score = sentiment.Score,
                    Label = sentiment.Label
                },
                Language = language.Language,
                Complexity = new ComplexityInfo
                {
                    Difficulty = complexity.Difficulty,
                    GradeLevel = complexity.GradeLevel,
                    ReadingTimeMinutes = (int)Math.Ceiling(complexity.ReadingTimeMinutes)
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in full analysis");
            return StatusCode(500, new ErrorResponse { Error = "Analysis failed" });
        }
    }

    #endregion

    #region Health & Status

    /// <summary>
    /// Get API status
    /// </summary>
    [HttpGet("status")]
    [ProducesResponseType(typeof(ApiStatusResponse), StatusCodes.Status200OK)]
    public IActionResult GetApiStatus()
    {
        return Ok(new ApiStatusResponse
        {
            Version = "2.0.0",
            Status = "operational",
            Timestamp = DateTime.UtcNow,
            Features = new List<string>
            {
                "Text Extraction",
                "Summarization",
                "Classification",
                "Keyword Extraction",
                "Sentiment Analysis",
                "Language Detection",
                "Complexity Analysis",
                "Question Answering",
                "Entity Extraction"
            },
            RateLimits = new RateLimitInfo
            {
                Free = RateLimitPolicies.GetPolicy("Free"),
                Pro = RateLimitPolicies.GetPolicy("Pro"),
                Enterprise = RateLimitPolicies.GetPolicy("Enterprise")
            }
        });
    }

    #endregion
}

#region Request Models

public class TokenRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class TextRequest
{
    public string Text { get; set; } = string.Empty;
}

public class SummarizeRequest
{
    public string Text { get; set; } = string.Empty;
    public int? MaxWords { get; set; }
    public int? Style { get; set; }
}

public class KeywordsRequest
{
    public string Text { get; set; } = string.Empty;
    public int? TopN { get; set; }
}

public class QARequest
{
    public string Text { get; set; } = string.Empty;
    public string Question { get; set; } = string.Empty;
}

#endregion

#region Response Models

public class ErrorResponse
{
    public string Error { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class ExtractionResponse
{
    public string FileId { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public int TotalPages { get; set; }
    public int WordCount { get; set; }
    public int CharCount { get; set; }
    public string Text { get; set; } = string.Empty;
}

public class SummaryResponse
{
    public string Summary { get; set; } = string.Empty;
    public int OriginalLength { get; set; }
    public int SummaryLength { get; set; }
    public int SentenceCount { get; set; }
    public double Confidence { get; set; }
    public int Style { get; set; }
}

public class ClassifyResponse
{
    public string Category { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public Dictionary<string, double> AllScores { get; set; } = new();
    public List<KeywordInfo> TopKeywords { get; set; } = new();
}

public class KeywordInfo
{
    public string Word { get; set; } = string.Empty;
    public double Score { get; set; }
    public int Frequency { get; set; }
}

public class KeywordsResponse
{
    public List<KeywordInfo> Keywords { get; set; } = new();
}

public class SentimentResponse
{
    public double Score { get; set; }
    public string Label { get; set; } = string.Empty;
    public int PositiveCount { get; set; }
    public int NegativeCount { get; set; }
    public double Confidence { get; set; }
}

public class LanguageResponse
{
    public string Language { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public Dictionary<string, double> AllScores { get; set; } = new();
}

public class ComplexityResponse
{
    public int WordCount { get; set; }
    public int SentenceCount { get; set; }
    public int UniqueWordCount { get; set; }
    public double AvgWordsPerSentence { get; set; }
    public double FleschScore { get; set; }
    public double GradeLevel { get; set; }
    public string Difficulty { get; set; } = string.Empty;
    public int ReadingTimeMinutes { get; set; }
}

public class QAResponse
{
    public string Answer { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public List<QASourceInfo> Sources { get; set; } = new();
}

public class QASourceInfo
{
    public string Text { get; set; } = string.Empty;
    public double RelevanceScore { get; set; }
}

public class EntitiesResponse
{
    public List<string> Emails { get; set; } = new();
    public List<string> Phones { get; set; } = new();
    public List<string> Dates { get; set; } = new();
    public List<string> Currencies { get; set; } = new();
    public List<string> URLs { get; set; } = new();
    public List<string> IPAddresses { get; set; } = new();
}

public class FullAnalysisResponse
{
    public string FileId { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public int TotalPages { get; set; }
    public int WordCount { get; set; }
    public string Summary { get; set; } = string.Empty;
    public ClassificationInfo Classification { get; set; } = new();
    public List<string> Keywords { get; set; } = new();
    public SentimentInfo Sentiment { get; set; } = new();
    public string Language { get; set; } = string.Empty;
    public ComplexityInfo Complexity { get; set; } = new();
}

public class ClassificationInfo
{
    public string Category { get; set; } = string.Empty;
    public double Confidence { get; set; }
}

public class SentimentInfo
{
    public double Score { get; set; }
    public string Label { get; set; } = string.Empty;
}

public class ComplexityInfo
{
    public string Difficulty { get; set; } = string.Empty;
    public double GradeLevel { get; set; }
    public int ReadingTimeMinutes { get; set; }
}

public class ApiStatusResponse
{
    public string Version { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public List<string> Features { get; set; } = new();
    public RateLimitInfo RateLimits { get; set; } = new();
}

public class RateLimitInfo
{
    public (int requests, int seconds) Free { get; set; }
    public (int requests, int seconds) Pro { get; set; }
    public (int requests, int seconds) Enterprise { get; set; }
}

#endregion

using Microsoft.AspNetCore.Mvc;
using Pdfpeaks.Services;
using Pdfpeaks.Services.AI;

namespace Pdfpeaks.Controllers;

/// <summary>
/// Controller for AI-powered document processing operations
/// Supports both local C# AI processing and remote Python AI microservice
/// </summary>
public class AIController : Controller
{
    private readonly ILogger<AIController> _logger;
    private readonly AIService _aiService;
    private readonly DocumentAnalysisService _localAiService;
    private readonly FileProcessingService _fileProcessingService;
    private readonly IWebHostEnvironment _environment;

    public AIController(
        ILogger<AIController> logger,
        AIService aiService,
        DocumentAnalysisService localAiService,
        FileProcessingService fileProcessingService,
        IWebHostEnvironment environment)
    {
        _logger = logger;
        _aiService = aiService;
        _localAiService = localAiService;
        _fileProcessingService = fileProcessingService;
        _environment = environment;
    }

    /// <summary>
    /// Main AI analysis page
    /// </summary>
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// Health check for AI service
    /// </summary>
    [HttpGet("api/ai/health")]
    public async Task<IActionResult> HealthCheck()
    {
        var isHealthy = await _aiService.IsHealthyAsync();
        return Ok(new 
        { 
            status = isHealthy ? "healthy" : "unhealthy",
            localAi = "available",
            remoteAi = isHealthy ? "available" : "unavailable",
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Process document with local AI (no external API needed)
    /// </summary>
    [HttpPost("api/ai/local/analyze")]
    public async Task<IActionResult> AnalyzeLocal(IFormFile file, [FromForm] string analysisType = "full")
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { success = false, message = "Please upload a file." });
        }

        try
        {
            var fileName = await _fileProcessingService.SaveUploadedFileAsync(file, "ai_analyze");
            var filePath = _fileProcessingService.GetFilePath(fileName);

            // Extract text from PDF
            var extraction = await _localAiService.ExtractTextFromPdfAsync(filePath);
            
            // Cleanup
            try { System.IO.File.Delete(filePath); } catch { }
            
            if (!extraction.Success)
            {
                return BadRequest(new { success = false, message = extraction.ErrorMessage });
            }

            switch (analysisType.ToLower())
            {
                case "summarize":
                    var summary = _localAiService.Summarize(extraction.FullText);
                    return Ok(new { success = true, type = "summary", data = summary });

                case "classify":
                    var classification = _localAiService.ClassifyDocument(extraction.FullText);
                    return Ok(new { success = true, type = "classification", data = classification });

                case "keywords":
                    var keywords = _localAiService.ExtractKeywords(extraction.FullText);
                    return Ok(new { success = true, type = "keywords", data = keywords });

                case "sentiment":
                    var sentiment = _localAiService.AnalyzeSentiment(extraction.FullText);
                    return Ok(new { success = true, type = "sentiment", data = sentiment });

                case "entities":
                    var entities = _localAiService.ExtractEntities(extraction.FullText);
                    return Ok(new { success = true, type = "entities", data = entities });

                case "complexity":
                    var complexity = _localAiService.CalculateComplexity(extraction.FullText);
                    return Ok(new { success = true, type = "complexity", data = complexity });

                case "language":
                    var language = _localAiService.DetectLanguage(extraction.FullText);
                    return Ok(new { success = true, type = "language", data = language });

                case "full":
                default:
                    return Ok(new 
                    { 
                        success = true, 
                        type = "full_analysis",
                        data = new
                        {
                            extraction = new { 
                                totalPages = extraction.TotalPages, 
                                wordCount = extraction.WordCount, 
                                charCount = extraction.CharCount 
                            },
                            summary = _localAiService.Summarize(extraction.FullText),
                            classification = _localAiService.ClassifyDocument(extraction.FullText),
                            keywords = _localAiService.ExtractKeywords(extraction.FullText),
                            sentiment = _localAiService.AnalyzeSentiment(extraction.FullText),
                            entities = _localAiService.ExtractEntities(extraction.FullText),
                            complexity = _localAiService.CalculateComplexity(extraction.FullText),
                            language = _localAiService.DetectLanguage(extraction.FullText)
                        }
                    });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in local AI analysis");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Ask a question about a document using local AI
    /// </summary>
    [HttpPost("api/ai/local/qa")]
    public async Task<IActionResult> QuestionAnswerLocal(IFormFile file, [FromForm] string question)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { success = false, message = "Please upload a file." });
        }

        if (string.IsNullOrWhiteSpace(question))
        {
            return BadRequest(new { success = false, message = "Please provide a question." });
        }

        try
        {
            var fileName = await _fileProcessingService.SaveUploadedFileAsync(file, "ai_qa");
            var filePath = _fileProcessingService.GetFilePath(fileName);

            var extraction = await _localAiService.ExtractTextFromPdfAsync(filePath);
            
            // Cleanup
            try { System.IO.File.Delete(filePath); } catch { }
            
            if (!extraction.Success)
            {
                return BadRequest(new { success = false, message = extraction.ErrorMessage });
            }

            var result = _localAiService.AnswerQuestion(extraction.FullText, question);
            
            return Ok(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in local Q&A");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Process document with remote AI (Python microservice)
    /// </summary>
    [HttpPost("api/ai/remote/process")]
    public async Task<IActionResult> ProcessDocument(
        IFormFile file,
        [FromForm] string processType = "extract")
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { success = false, message = "Please upload a file." });
        }

        try
        {
            var fileName = await _fileProcessingService.SaveUploadedFileAsync(file, "ai_process");
            var filePath = _fileProcessingService.GetFilePath(fileName);

            var aiProcessType = processType.ToLower() switch
            {
                "summarize" => AIProcessType.Summarize,
                "classify" => AIProcessType.Classify,
                "extract_entities" => AIProcessType.ExtractEntities,
                "full_analysis" => AIProcessType.FullAnalysis,
                _ => AIProcessType.Extract
            };

            var result = await _aiService.ProcessDocumentAsync(filePath, aiProcessType);

            try { System.IO.File.Delete(filePath); } catch { }

            if (result.Success)
            {
                return Ok(new
                {
                    success = true,
                    taskId = result.TaskId,
                    status = result.Status,
                    message = result.Message,
                    processingTimeMs = result.ProcessingTimeMs,
                    result = result.Result
                });
            }

            return BadRequest(new { success = false, message = result.ErrorMessage });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing document with remote AI");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Ask a question about a document using remote AI
    /// </summary>
    [HttpPost("api/ai/remote/qa")]
    public async Task<IActionResult> QuestionAnswerRemote(
        IFormFile file,
        [FromForm] string question,
        [FromForm] int maxSources = 5)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { success = false, message = "Please upload a file." });
        }

        if (string.IsNullOrWhiteSpace(question))
        {
            return BadRequest(new { success = false, message = "Please provide a question." });
        }

        try
        {
            var fileName = await _fileProcessingService.SaveUploadedFileAsync(file, "ai_qa");
            var filePath = _fileProcessingService.GetFilePath(fileName);

            var result = await _aiService.AskQuestionAsync(filePath, question, maxSources);

            try { System.IO.File.Delete(filePath); } catch { }

            if (result.Success)
            {
                return Ok(new
                {
                    success = true,
                    answer = result.Answer,
                    sources = result.Sources,
                    confidence = result.Confidence
                });
            }

            return BadRequest(new { success = false, message = result.ErrorMessage });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in remote Q&A");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Summarize a document using remote AI
    /// </summary>
    [HttpPost("api/ai/remote/summarize")]
    public async Task<IActionResult> SummarizeRemote(
        IFormFile file,
        [FromForm] int maxLength = 200,
        [FromForm] string style = "concise")
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { success = false, message = "Please upload a file." });
        }

        try
        {
            var fileName = await _fileProcessingService.SaveUploadedFileAsync(file, "ai_summarize");
            var filePath = _fileProcessingService.GetFilePath(fileName);

            var result = await _aiService.SummarizeAsync(filePath, maxLength, style);

            try { System.IO.File.Delete(filePath); } catch { }

            if (result.Success)
            {
                return Ok(new
                {
                    success = true,
                    summary = result.Summary,
                    originalLength = result.OriginalLength,
                    summaryLength = result.SummaryLength,
                    compressionRatio = result.CompressionRatio
                });
            }

            return BadRequest(new { success = false, message = result.ErrorMessage });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error summarizing document with remote AI");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Classify a document using remote AI
    /// </summary>
    [HttpPost("api/ai/remote/classify")]
    public async Task<IActionResult> ClassifyRemote(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { success = false, message = "Please upload a file." });
        }

        try
        {
            var fileName = await _fileProcessingService.SaveUploadedFileAsync(file, "ai_classify");
            var filePath = _fileProcessingService.GetFilePath(fileName);

            var result = await _aiService.ClassifyAsync(filePath);

            try { System.IO.File.Delete(filePath); } catch { }

            if (result.Success)
            {
                return Ok(new
                {
                    success = true,
                    category = result.Category,
                    confidence = result.Confidence,
                    allScores = result.AllScores
                });
            }

            return BadRequest(new { success = false, message = result.ErrorMessage });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error classifying document with remote AI");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }
}

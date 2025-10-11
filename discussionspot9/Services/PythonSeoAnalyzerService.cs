using discussionspot9.Interfaces;
using discussionspot9.Models.ViewModels.CreativeViewModels;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Security.Claims;

namespace discussionspot9.Services
{
    /// <summary>
    /// SEO Analyzer service that uses Python for advanced text analysis
    /// </summary>
    public class PythonSeoAnalyzerService : ISeoAnalyzerService
    {
        private readonly ILogger<PythonSeoAnalyzerService> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _pythonScriptPath;
        private readonly string _pythonExecutable;

        public PythonSeoAnalyzerService(
            ILogger<PythonSeoAnalyzerService> logger, 
            IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            
            // Get Python executable path from configuration or use default
            _pythonExecutable = _configuration["Python:ExecutablePath"] ?? "python";
            
            // Get script path
            var scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PythonScripts", "seo_analyzer.py");
            _pythonScriptPath = scriptPath;
            
            _logger.LogInformation("Python SEO Analyzer initialized. Script path: {ScriptPath}", _pythonScriptPath);
        }

        public async Task<SeoAnalysisResult> AnalyzePostAsync(CreatePostViewModel model)
        {
            try
            {
                _logger.LogInformation("Starting SEO analysis for post: {Title}", model.Title ?? "Untitled");
                
                // Prepare input data for Python script
                var inputData = new
                {
                    title = model.Title ?? string.Empty,
                    content = model.Content ?? string.Empty,
                    communitySlug = model.CommunitySlug ?? string.Empty,
                    postType = model.PostType ?? "text"
                };

                var inputJson = JsonSerializer.Serialize(inputData);
                
                // Run Python script
                var outputJson = await RunPythonScriptAsync(inputJson);
                
                // Parse result
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                
                var result = JsonSerializer.Deserialize<SeoAnalysisResult>(outputJson, options);
                
                if (result == null)
                {
                    _logger.LogError("Failed to deserialize SEO analysis result");
                    return CreateErrorResult(model, "Failed to parse analysis result");
                }
                
                _logger.LogInformation(
                    "SEO Analysis complete. Score: {Score}, Issues: {IssueCount}, Improvements: {ImprovementCount}",
                    result.SeoScore, 
                    result.IssuesFound.Count, 
                    result.ImprovementsMade.Count
                );
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during SEO analysis");
                return CreateErrorResult(model, $"SEO Analysis error: {ex.Message}");
            }
        }

        public async Task<CreatePostViewModel> OptimizePostAsync(CreatePostViewModel model)
        {
            try
            {
                var analysisResult = await AnalyzePostAsync(model);
                
                if (analysisResult.Error)
                {
                    _logger.LogWarning("SEO analysis returned error, using original content");
                    return model;
                }
                
                // Apply optimizations if changes were made
                if (analysisResult.TitleChanged || analysisResult.ContentChanged)
                {
                    _logger.LogInformation(
                        "Applying SEO optimizations. Title changed: {TitleChanged}, Content changed: {ContentChanged}",
                        analysisResult.TitleChanged,
                        analysisResult.ContentChanged
                    );
                    
                    // Update model with optimized content
                    if (analysisResult.TitleChanged && !string.IsNullOrWhiteSpace(analysisResult.OptimizedTitle))
                    {
                        model.Title = analysisResult.OptimizedTitle;
                    }
                    
                    if (analysisResult.ContentChanged && !string.IsNullOrWhiteSpace(analysisResult.OptimizedContent))
                    {
                        model.Content = analysisResult.OptimizedContent;
                    }
                    
                    // Store SEO metadata for future use
                    model.SeoMetadata = new SeoMetadataViewModel
                    {
                        MetaDescription = analysisResult.SuggestedMetaDescription,
                        Keywords = string.Join(", ", analysisResult.SuggestedKeywords),
                        SeoScore = analysisResult.SeoScore,
                        OptimizationApplied = true,
                        IssuesFound = analysisResult.IssuesFound,
                        ImprovementsMade = analysisResult.ImprovementsMade
                    };
                }
                
                return model;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error optimizing post");
                // Return original model on error
                return model;
            }
        }

        private async Task<string> RunPythonScriptAsync(string inputJson)
        {
            try
            {
                // Check if Python script exists
                if (!File.Exists(_pythonScriptPath))
                {
                    throw new FileNotFoundException($"Python script not found: {_pythonScriptPath}");
                }
                
                var startInfo = new ProcessStartInfo
                {
                    FileName = _pythonExecutable,
                    Arguments = $"\"{_pythonScriptPath}\"",
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    StandardInputEncoding = Encoding.UTF8,
                    StandardOutputEncoding = Encoding.UTF8
                };

                using var process = new Process { StartInfo = startInfo };
                
                var outputBuilder = new StringBuilder();
                var errorBuilder = new StringBuilder();
                
                process.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        outputBuilder.AppendLine(e.Data);
                    }
                };
                
                process.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        errorBuilder.AppendLine(e.Data);
                    }
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                
                // Write input to stdin
                await process.StandardInput.WriteAsync(inputJson);
                await process.StandardInput.FlushAsync();
                process.StandardInput.Close();
                
                // Wait for process to complete with timeout
                var completed = await Task.Run(() => process.WaitForExit(30000)); // 30 second timeout
                
                if (!completed)
                {
                    process.Kill();
                    throw new TimeoutException("Python script execution timed out");
                }
                
                var output = outputBuilder.ToString().Trim();
                var error = errorBuilder.ToString().Trim();
                
                if (process.ExitCode != 0)
                {
                    _logger.LogError("Python script exited with code {ExitCode}. Error: {Error}", process.ExitCode, error);
                    throw new Exception($"Python script failed: {error}");
                }
                
                if (!string.IsNullOrEmpty(error))
                {
                    _logger.LogWarning("Python script stderr: {Error}", error);
                }
                
                if (string.IsNullOrEmpty(output))
                {
                    throw new Exception("Python script produced no output");
                }
                
                return output;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running Python script");
                throw;
            }
        }

        private SeoAnalysisResult CreateErrorResult(CreatePostViewModel model, string errorMessage)
        {
            return new SeoAnalysisResult
            {
                Error = true,
                Message = errorMessage,
                OriginalTitle = model.Title ?? string.Empty,
                OptimizedTitle = model.Title ?? string.Empty,
                OriginalContent = model.Content ?? string.Empty,
                OptimizedContent = model.Content ?? string.Empty,
                SuggestedMetaDescription = string.Empty,
                SuggestedKeywords = new List<string>(),
                SeoScore = 0,
                IssuesFound = new List<string> { errorMessage },
                ImprovementsMade = new List<string>(),
                TitleChanged = false,
                ContentChanged = false
            };
        }
    }
}


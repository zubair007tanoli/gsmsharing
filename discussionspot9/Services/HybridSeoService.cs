using discussionspot9.Data.DbContext;
using discussionspot9.Models.ViewModels.CreativeViewModels;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace discussionspot9.Services
{
    /// <summary>
    /// Hybrid SEO Service using Python scripts that integrate multiple RapidAPI services
    /// Combines: AI Writer, UberSuggest, ChatGPT Vision, Google Search
    /// </summary>
    public class HybridSeoService
    {
        private readonly ILogger<HybridSeoService> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _pythonScriptPath;
        private readonly string _pythonExecutable;

        public HybridSeoService(
            ILogger<HybridSeoService> logger,
            IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;

            _pythonExecutable = _configuration["Python:ExecutablePath"] ?? "python";
            var scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PythonScripts", "hybrid_seo_optimizer.py");
            _pythonScriptPath = scriptPath;

            _logger.LogInformation("Hybrid SEO Service initialized. Script path: {ScriptPath}", _pythonScriptPath);
        }

        /// <summary>
        /// Optimize content using hybrid approach (all APIs)
        /// </summary>
        public async Task<HybridSeoResult> OptimizeAsync(CreatePostViewModel model)
        {
            try
            {
                _logger.LogInformation("Starting hybrid SEO optimization for: {Title}", model.Title ?? "Untitled");

                var inputData = new
                {
                    title = model.Title ?? string.Empty,
                    content = model.Content ?? string.Empty,
                    communitySlug = model.CommunitySlug ?? string.Empty,
                    postType = model.PostType ?? "text"
                };

                var inputJson = JsonSerializer.Serialize(inputData);
                var outputJson = await RunPythonScriptAsync(inputJson);

                var result = JsonSerializer.Deserialize<HybridSeoResult>(outputJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (result == null)
                {
                    throw new Exception("Failed to parse Python script output");
                }

                if (!result.Success)
                {
                    _logger.LogWarning("Hybrid SEO optimization failed: {Error}", result.Error);
                }
                else
                {
                    _logger.LogInformation("Hybrid SEO optimization completed. Score: {Score}/100", result.FinalSeoScore);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in hybrid SEO optimization");
                return new HybridSeoResult
                {
                    Success = false,
                    Error = ex.Message,
                    OriginalTitle = model.Title ?? "",
                    OptimizedTitle = model.Title ?? "",
                    OriginalContent = model.Content ?? "",
                    OptimizedContent = model.Content ?? ""
                };
            }
        }

        private async Task<string> RunPythonScriptAsync(string inputJson)
        {
            try
            {
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

                await process.StandardInput.WriteAsync(inputJson);
                await process.StandardInput.FlushAsync();
                process.StandardInput.Close();

                var completed = await Task.Run(() => process.WaitForExit(60000)); // 60 second timeout for API calls

                if (!completed)
                {
                    process.Kill();
                    throw new TimeoutException("Hybrid SEO script execution timed out");
                }

                var output = outputBuilder.ToString().Trim();
                var error = errorBuilder.ToString().Trim();

                if (process.ExitCode != 0)
                {
                    _logger.LogError("Python script exited with code {ExitCode}. Error: {Error}", process.ExitCode, error);
                    throw new Exception($"Hybrid SEO script failed: {error}");
                }

                if (!string.IsNullOrEmpty(error))
                {
                    _logger.LogWarning("Python script stderr: {Error}", error);
                }

                if (string.IsNullOrEmpty(output))
                {
                    throw new Exception("Hybrid SEO script produced no output");
                }

                return output;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running hybrid SEO Python script");
                throw;
            }
        }
    }

    /// <summary>
    /// Result from hybrid SEO optimization
    /// </summary>
    public class HybridSeoResult
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
        public string OriginalTitle { get; set; } = string.Empty;
        public string OptimizedTitle { get; set; } = string.Empty;
        public string OriginalContent { get; set; } = string.Empty;
        public string OptimizedContent { get; set; } = string.Empty;
        public string MetaDescription { get; set; } = string.Empty;
        public List<string> Keywords { get; set; } = new();
        public double SeoScore { get; set; }
        public double FinalSeoScore { get; set; }
        public List<string> Improvements { get; set; } = new();
        public Dictionary<string, object>? Optimizations { get; set; }
    }
}


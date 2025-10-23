using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using discussionspot9.Models.Domain;

namespace discussionspot9.Services
{
    public class PythonStoryEnhancerService
    {
        private readonly ILogger<PythonStoryEnhancerService> _logger;
        private readonly string _pythonScriptPath;
        private readonly string _pythonExecutable;

        public PythonStoryEnhancerService(ILogger<PythonStoryEnhancerService> logger)
        {
            _logger = logger;
            _pythonScriptPath = Path.Combine(Directory.GetCurrentDirectory(), "PythonScripts", "story_enhancer.py");
            _pythonExecutable = "python"; // or "python3" on some systems
        }

        public async Task<List<StorySlide>> EnhanceStoryAsync(StoryContentInput content, StoryOptionsInput options)
        {
            try
            {
                // Prepare input data
                var inputData = new
                {
                    content = new
                    {
                        title = content.Title,
                        content = content.Content,
                        post_type = content.PostType,
                        tags = content.Tags,
                        media_urls = content.MediaUrls,
                        community_name = content.CommunityName,
                        author_name = content.AuthorName
                    },
                    options = new
                    {
                        style = options.Style,
                        length = options.Length,
                        use_ai = options.UseAI,
                        keywords = options.Keywords,
                        auto_generate = options.AutoGenerate
                    }
                };

                // Serialize to JSON
                var jsonInput = JsonSerializer.Serialize(inputData, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                // Write to temporary file
                var tempFile = Path.GetTempFileName();
                await File.WriteAllTextAsync(tempFile, jsonInput);

                try
                {
                    // Execute Python script
                    var result = await ExecutePythonScriptAsync(tempFile);
                    
                    // Parse result
                    var resultData = JsonSerializer.Deserialize<StoryEnhancementResult>(result);
                    
                    // Convert to StorySlide objects
                    var slides = new List<StorySlide>();
                    foreach (var slideData in resultData.Slides)
                    {
                        var slide = new StorySlide
                        {
                            OrderIndex = slideData.OrderIndex,
                            SlideType = slideData.SlideType,
                            Headline = slideData.Headline,
                            Text = slideData.Text,
                            Caption = slideData.Caption,
                            BackgroundColor = slideData.BackgroundColor,
                            TextColor = slideData.TextColor,
                            Alignment = slideData.Alignment,
                            FontSize = slideData.FontSize,
                            Duration = slideData.Duration,
                            MediaUrl = slideData.MediaUrl,
                            MediaType = slideData.MediaType
                        };
                        slides.Add(slide);
                    }

                    _logger.LogInformation("Successfully enhanced story with {SlideCount} slides", slides.Count);
                    return slides;
                }
                finally
                {
                    // Clean up temporary file
                    if (File.Exists(tempFile))
                    {
                        File.Delete(tempFile);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enhancing story with Python script");
                throw;
            }
        }

        private async Task<string> ExecutePythonScriptAsync(string inputFile)
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = _pythonExecutable,
                Arguments = $"\"{_pythonScriptPath}\" \"{inputFile}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = processInfo };
            
            _logger.LogDebug("Executing Python script: {Command}", $"{_pythonExecutable} \"{_pythonScriptPath}\" \"{inputFile}\"");

            process.Start();

            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync();

            var output = await outputTask;
            var error = await errorTask;

            if (process.ExitCode != 0)
            {
                _logger.LogError("Python script failed with exit code {ExitCode}. Error: {Error}", process.ExitCode, error);
                throw new InvalidOperationException($"Python script failed: {error}");
            }

            if (!string.IsNullOrEmpty(error))
            {
                _logger.LogWarning("Python script warnings: {Warnings}", error);
            }

            return output;
        }

        public async Task<bool> IsPythonAvailableAsync()
        {
            try
            {
                var processInfo = new ProcessStartInfo
                {
                    FileName = _pythonExecutable,
                    Arguments = "--version",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = new Process { StartInfo = processInfo };
                process.Start();
                await process.WaitForExitAsync();

                return process.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> IsScriptAvailableAsync()
        {
            return await Task.FromResult(File.Exists(_pythonScriptPath));
        }
    }

    public class StoryContentInput
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string PostType { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new();
        public List<string> MediaUrls { get; set; } = new();
        public string CommunityName { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
    }

    public class StoryOptionsInput
    {
        public string Style { get; set; } = "informative";
        public string Length { get; set; } = "medium";
        public bool UseAI { get; set; } = true;
        public string? Keywords { get; set; }
        public bool AutoGenerate { get; set; } = true;
    }

    public class StoryEnhancementResult
    {
        public List<StorySlideData> Slides { get; set; } = new();
        public StoryMetadata Metadata { get; set; } = new();
    }

    public class StorySlideData
    {
        public int OrderIndex { get; set; }
        public string SlideType { get; set; } = string.Empty;
        public string Headline { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public string Caption { get; set; } = string.Empty;
        public string BackgroundColor { get; set; } = string.Empty;
        public string TextColor { get; set; } = string.Empty;
        public string Alignment { get; set; } = string.Empty;
        public string FontSize { get; set; } = string.Empty;
        public int Duration { get; set; }
        public string? MediaUrl { get; set; }
        public string? MediaType { get; set; }
    }

    public class StoryMetadata
    {
        public string GeneratedAt { get; set; } = string.Empty;
        public int TotalSlides { get; set; }
        public string Style { get; set; } = string.Empty;
        public string Length { get; set; } = string.Empty;
    }
}

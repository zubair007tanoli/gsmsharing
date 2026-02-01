using System.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace discussionspot9.Services
{
	public interface IMediaOptimizationService
	{
		Task<string> OptimizeImageAsync(string inputPathOrUrl, int maxWidth, int maxHeight, CancellationToken ct = default);
		/// <summary>Transcode/compress video to H.264 MP4. Returns path to compressed file (or original if FFmpeg unavailable).</summary>
		Task<(string mp4Path, string webmPath)> TranscodeVideoAsync(string inputPathOrUrl, CancellationToken ct = default);
		/// <summary>Compress video to reduce file size. Returns relative path to compressed file, or original path if compression fails.</summary>
		Task<string> CompressVideoAsync(string relativePath, CancellationToken ct = default);
	}

	public class MediaOptimizationService : IMediaOptimizationService
	{
		private readonly IWebHostEnvironment _environment;
		private readonly ILogger<MediaOptimizationService> _logger;
		private static readonly string[] VideoExtensions = { ".mp4", ".webm", ".mov", ".avi", ".mkv" };

		public MediaOptimizationService(IWebHostEnvironment environment, ILogger<MediaOptimizationService> logger)
		{
			_environment = environment;
			_logger = logger;
		}

		public async Task<string> OptimizeImageAsync(string inputPathOrUrl, int maxWidth, int maxHeight, CancellationToken ct = default)
		{
			_logger.LogInformation("OptimizeImageAsync called for {Input}", inputPathOrUrl);
			await Task.CompletedTask;
			return inputPathOrUrl;
		}

		public async Task<(string mp4Path, string webmPath)> TranscodeVideoAsync(string inputPathOrUrl, CancellationToken ct = default)
		{
			_logger.LogInformation("TranscodeVideoAsync called for {Input}", inputPathOrUrl);
			var compressed = await CompressVideoAsync(inputPathOrUrl, ct);
			return (compressed, compressed);
		}

		/// <summary>
		/// Compress video using FFmpeg (H.264, CRF 28, AAC audio, faststart for web playback).
		/// Requires FFmpeg on PATH. If FFmpeg is not available, returns the original path.
		/// </summary>
		public async Task<string> CompressVideoAsync(string relativePath, CancellationToken ct = default)
		{
			if (string.IsNullOrWhiteSpace(relativePath)) return relativePath;

			string physicalPath = relativePath.TrimStart('/');
			physicalPath = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath ?? "", physicalPath.Replace("/", Path.DirectorySeparatorChar.ToString()));
			if (!File.Exists(physicalPath))
			{
				_logger.LogWarning("Video file not found for compression: {Path}", physicalPath);
				return relativePath;
			}

			var ext = Path.GetExtension(physicalPath).ToLowerInvariant();
			if (!VideoExtensions.Contains(ext))
			{
				_logger.LogDebug("Not a video file, skipping compression: {Path}", physicalPath);
				return relativePath;
			}

			string outDir = Path.GetDirectoryName(physicalPath) ?? "";
			string outName = Path.GetFileNameWithoutExtension(physicalPath) + "_compressed.mp4";
			string outPath = Path.Combine(outDir, outName);

			try
			{
				// FFmpeg: H.264, CRF 28 (good quality, smaller size), AAC 128k, faststart for web
				var args = $"-y -i \"{physicalPath}\" -c:v libx264 -crf 28 -preset medium -c:a aac -b:a 128k -movflags +faststart \"{outPath}\"";
				var startInfo = new ProcessStartInfo
				{
					FileName = "ffmpeg",
					Arguments = args,
					UseShellExecute = false,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					CreateNoWindow = true
				};

				using var process = Process.Start(startInfo);
				if (process == null)
				{
					_logger.LogWarning("FFmpeg not found or could not start. Using original video.");
					return relativePath;
				}

				var stderr = await process.StandardError.ReadToEndAsync(ct);
				await process.WaitForExitAsync(ct);

				if (process.ExitCode != 0)
				{
					_logger.LogWarning("FFmpeg compression failed (exit {Code}). Using original. stderr: {Err}", process.ExitCode, stderr.Length > 200 ? stderr[..200] + "..." : stderr);
					return relativePath;
				}

				if (!File.Exists(outPath))
				{
					_logger.LogWarning("FFmpeg did not produce output file. Using original.");
					return relativePath;
				}

				// Return relative URL for compressed file (e.g. /uploads/posts/videos/xxx_compressed.mp4)
				var webRoot = _environment.WebRootPath ?? _environment.ContentRootPath ?? "";
				var relOut = outPath.StartsWith(webRoot, StringComparison.OrdinalIgnoreCase)
					? "/" + outPath[webRoot.Length..].TrimStart(Path.DirectorySeparatorChar, '/').Replace('\\', '/')
					: "/" + Path.Combine(Path.GetDirectoryName(relativePath.TrimStart('/')) ?? "", outName).Replace("\\", "/");
				_logger.LogInformation("Video compressed: {Original} -> {Compressed}", relativePath, relOut);
				return relOut;
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex, "Video compression failed for {Path}. Using original.", relativePath);
				return relativePath;
			}
		}
	}
}

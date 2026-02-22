using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;
using PdfSharpCore.Pdf;
using PdfSharpCore.Drawing;
using Pdfpeaks.Models;
using SkiaSharp;
using UglyToad.PdfPig;
using System.Diagnostics;

namespace Pdfpeaks.Services;

/// <summary>
/// Service for image processing operations using ImageSharp.
/// Includes CamScanner-style document scanning via the Python enhancer.
/// </summary>
public class ImageProcessingService
{
    private readonly ILogger<ImageProcessingService> _logger;
    private readonly string _tempFilePath;
    private readonly string _scriptsPath;

    // Standard paper sizes in points (72 pt = 1 inch) for PdfSharpCore
    private static readonly Dictionary<string, (double W, double H)> PaperSizesPts = new(StringComparer.OrdinalIgnoreCase)
    {
        ["a4"]     = (595.28, 841.89),
        ["a3"]     = (841.89, 1190.55),
        ["a5"]     = (419.53, 595.28),
        ["letter"] = (612.0,  792.0),
        ["legal"]  = (612.0,  1008.0),
        ["auto"]   = (0, 0),   // determined from image
    };

    public ImageProcessingService(ILogger<ImageProcessingService> logger, IWebHostEnvironment environment)
    {
        _logger = logger;
        _tempFilePath = Path.Combine(environment.ContentRootPath, "temp_files");
        _scriptsPath = Path.Combine(environment.ContentRootPath, "scripts");

        if (!Directory.Exists(_tempFilePath))
            Directory.CreateDirectory(_tempFilePath);
    }

    // -----------------------------------------------------------------------
    // Resize
    // -----------------------------------------------------------------------
    public async Task<(bool success, string outputPath, string message)> ResizeImageAsync(
        string inputPath, string outputFileName, int width, int height, ResizeMode resizeMode)
    {
        try
        {
            if (!File.Exists(inputPath))
                return (false, "", "Input file not found.");

            using var image = await Image.LoadAsync(inputPath);
            image.Mutate(ctx => ctx.Resize(new ResizeOptions { Mode = resizeMode, Size = new Size(width, height) }));

            var outputPath = Path.Combine(_tempFilePath, outputFileName);
            await image.SaveAsync(outputPath);

            _logger.LogInformation("Image resized to {Width}x{Height}", width, height);
            return (true, outputPath, $"Successfully resized image to {width}x{height}.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resizing image");
            return (false, "", $"Error resizing image: {ex.Message}");
        }
    }

    // -----------------------------------------------------------------------
    // Crop  (fixed: coordinates are now clamped correctly)
    // -----------------------------------------------------------------------
    public async Task<(bool success, string outputPath, string message)> CropImageAsync(
        string inputPath, string outputFileName, int x, int y, int width, int height)
    {
        try
        {
            if (!File.Exists(inputPath))
                return (false, "", "Input file not found.");

            using var image = await Image.LoadAsync(inputPath);

            // Clamp to image bounds
            x = Math.Max(0, x);
            y = Math.Max(0, y);
            width  = Math.Max(1, width);
            height = Math.Max(1, height);

            if (x + width  > image.Width)  width  = image.Width  - x;
            if (y + height > image.Height) height = image.Height - y;

            if (width <= 0 || height <= 0)
                return (false, "", "Invalid crop dimensions after clamping to image bounds.");

            image.Mutate(ctx => ctx.Crop(new Rectangle(x, y, width, height)));

            var outputPath = Path.Combine(_tempFilePath, outputFileName);
            await image.SaveAsync(outputPath);

            _logger.LogInformation("Image cropped at ({X},{Y}) {W}x{H}", x, y, width, height);
            return (true, outputPath, "Successfully cropped image.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cropping image");
            return (false, "", $"Error cropping image: {ex.Message}");
        }
    }

    // -----------------------------------------------------------------------
    // Rotate
    // -----------------------------------------------------------------------
    public async Task<(bool success, string outputPath, string message)> RotateImageAsync(
        string inputPath, string outputFileName, float degrees)
    {
        try
        {
            if (!File.Exists(inputPath))
                return (false, "", "Input file not found.");

            using var image = await Image.LoadAsync(inputPath);
            image.Mutate(ctx => ctx.Rotate(degrees));

            var outputPath = Path.Combine(_tempFilePath, outputFileName);
            await image.SaveAsync(outputPath);

            return (true, outputPath, $"Successfully rotated image by {degrees} degrees.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rotating image");
            return (false, "", $"Error rotating image: {ex.Message}");
        }
    }

    // -----------------------------------------------------------------------
    // Flip
    // -----------------------------------------------------------------------
    public async Task<(bool success, string outputPath, string message)> FlipImageAsync(
        string inputPath, string outputFileName, bool flipHorizontal)
    {
        try
        {
            if (!File.Exists(inputPath))
                return (false, "", "Input file not found.");

            using var image = await Image.LoadAsync(inputPath);
            image.Mutate(ctx => ctx.Flip(flipHorizontal ? FlipMode.Horizontal : FlipMode.Vertical));

            var outputPath = Path.Combine(_tempFilePath, outputFileName);
            await image.SaveAsync(outputPath);

            return (true, outputPath, "Successfully flipped image.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error flipping image");
            return (false, "", $"Error flipping image: {ex.Message}");
        }
    }

    // -----------------------------------------------------------------------
    // Convert format
    // -----------------------------------------------------------------------
    public async Task<(bool success, string outputPath, string message)> ConvertImageFormatAsync(
        string inputPath, string outputFileName, string targetFormat)
    {
        try
        {
            if (!File.Exists(inputPath))
                return (false, "", "Input file not found.");

            using var image = await Image.LoadAsync(inputPath);

            IImageEncoder encoder = targetFormat.ToLower() switch
            {
                "png"            => new PngEncoder(),
                "jpg" or "jpeg"  => new JpegEncoder(),
                "bmp"            => new BmpEncoder(),
                "gif"            => new GifEncoder(),
                "webp"           => new WebpEncoder(),
                _                => new PngEncoder()
            };

            var outputPath = Path.Combine(_tempFilePath, outputFileName);
            await image.SaveAsync(outputPath, encoder);

            return (true, outputPath, $"Successfully converted image to {targetFormat} format.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting image format");
            return (false, "", $"Error converting image: {ex.Message}");
        }
    }

    // -----------------------------------------------------------------------
    // Convert single image to PDF with optional paper size
    // -----------------------------------------------------------------------
    public async Task<(bool success, string outputPath, string message)> ConvertToPdfAsync(
        string inputPath, string outputFileName, string paperSize = "auto")
    {
        try
        {
            if (!File.Exists(inputPath))
                return (false, "", "Input file not found.");

            using var image = await Image.LoadAsync(inputPath);

            var document = new PdfDocument();
            document.Info.Title = Path.GetFileNameWithoutExtension(inputPath);

            var page = document.AddPage();

            if (paperSize.Equals("auto", StringComparison.OrdinalIgnoreCase) ||
                !PaperSizesPts.TryGetValue(paperSize, out var ps) || ps.W == 0)
            {
                // Size page to match image at 96 DPI
                double dpiX = image.Metadata.HorizontalResolution > 0 ? image.Metadata.HorizontalResolution : 96;
                double dpiY = image.Metadata.VerticalResolution  > 0 ? image.Metadata.VerticalResolution  : 96;
                page.Width  = image.Width  * 72.0 / dpiX;
                page.Height = image.Height * 72.0 / dpiY;
            }
            else
            {
                // Use standard paper — rotate if image is landscape
                if (image.Width > image.Height)
                { page.Width = ps.H; page.Height = ps.W; }
                else
                { page.Width = ps.W; page.Height = ps.H; }
            }

            using var gfx = XGraphics.FromPdfPage(page);

            var tempImg = Path.Combine(_tempFilePath, $"tmp_{Guid.NewGuid():N}.png");
            await image.SaveAsync(tempImg, new PngEncoder());

            using var xImg = XImage.FromFile(tempImg);

            // Fit image inside page with margins (8 pt each side)
            const double margin = 8;
            double maxW = page.Width  - margin * 2;
            double maxH = page.Height - margin * 2;
            double scale = Math.Min(maxW / xImg.PixelWidth, maxH / xImg.PixelHeight);
            double drawW = xImg.PixelWidth  * scale;
            double drawH = xImg.PixelHeight * scale;
            double drawX = margin + (maxW - drawW) / 2;
            double drawY = margin + (maxH - drawH) / 2;

            gfx.DrawImage(xImg, drawX, drawY, drawW, drawH);

            try { File.Delete(tempImg); } catch { }

            var outputPath = Path.Combine(_tempFilePath, outputFileName);
            document.Save(outputPath);

            return (true, outputPath, "Successfully converted image to PDF.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting image to PDF");
            return (false, "", $"Error converting to PDF: {ex.Message}");
        }
    }

    // -----------------------------------------------------------------------
    // Convert MULTIPLE images to a single PDF (one page per image)
    // -----------------------------------------------------------------------
    public async Task<ProcessResult> ImagesToPdfAsync(string[] inputPaths, string outputFileName, PdfPageSize pageSize)
    {
        try
        {
            if (inputPaths == null || inputPaths.Length == 0)
                return new ProcessResult(false, "", "No input files provided.");

            var paperKey = pageSize switch
            {
                PdfPageSize.A4     => "a4",
                PdfPageSize.A3     => "a3",
                PdfPageSize.Letter => "letter",
                PdfPageSize.Legal  => "legal",
                _                  => "auto"
            };

            var document = new PdfDocument();

            foreach (var inputPath in inputPaths)
            {
                if (!File.Exists(inputPath)) continue;

                using var image = await Image.LoadAsync(inputPath);
                var page = document.AddPage();

                if (paperKey == "auto" || !PaperSizesPts.TryGetValue(paperKey, out var ps) || ps.W == 0)
                {
                    double dpiX = image.Metadata.HorizontalResolution > 0 ? image.Metadata.HorizontalResolution : 96;
                    double dpiY = image.Metadata.VerticalResolution  > 0 ? image.Metadata.VerticalResolution  : 96;
                    page.Width  = image.Width  * 72.0 / dpiX;
                    page.Height = image.Height * 72.0 / dpiY;
                }
                else
                {
                    if (image.Width > image.Height)
                    { page.Width = ps.H; page.Height = ps.W; }
                    else
                    { page.Width = ps.W; page.Height = ps.H; }
                }

                using var gfx = XGraphics.FromPdfPage(page);
                var tempImg = Path.Combine(_tempFilePath, $"tmp_{Guid.NewGuid():N}.png");
                await image.SaveAsync(tempImg, new PngEncoder());
                using var xImg = XImage.FromFile(tempImg);

                const double margin = 8;
                double maxW = page.Width  - margin * 2;
                double maxH = page.Height - margin * 2;
                double scale = Math.Min(maxW / xImg.PixelWidth, maxH / xImg.PixelHeight);
                double drawW = xImg.PixelWidth  * scale;
                double drawH = xImg.PixelHeight * scale;
                gfx.DrawImage(xImg, margin + (maxW - drawW) / 2, margin + (maxH - drawH) / 2, drawW, drawH);

                try { File.Delete(tempImg); } catch { }
            }

            if (document.PageCount == 0)
                return new ProcessResult(false, "", "No valid images were processed.");

            var outputPath = Path.Combine(_tempFilePath, outputFileName);
            document.Save(outputPath);

            return new ProcessResult(true, outputPath,
                $"Successfully created PDF with {document.PageCount} page(s).");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating multi-image PDF");
            return new ProcessResult(false, "", $"Error: {ex.Message}");
        }
    }

    // -----------------------------------------------------------------------
    // Compress
    // -----------------------------------------------------------------------
    public async Task<(bool success, string outputPath, string message, long originalSize, long compressedSize)> CompressImageAsync(
        string inputPath, string outputFileName, int quality)
    {
        try
        {
            if (!File.Exists(inputPath))
                return (false, "", "Input file not found.", 0, 0);

            var originalSize = new FileInfo(inputPath).Length;
            using var image = await Image.LoadAsync(inputPath);
            var outputPath = Path.Combine(_tempFilePath, outputFileName);
            await image.SaveAsync(outputPath, new JpegEncoder { Quality = quality });
            var compressedSize = new FileInfo(outputPath).Length;

            return (true, outputPath, "Successfully compressed image.", originalSize, compressedSize);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error compressing image");
            return (false, "", $"Error compressing image: {ex.Message}", 0, 0);
        }
    }

    // -----------------------------------------------------------------------
    // Adjust
    // -----------------------------------------------------------------------
    public async Task<(bool success, string outputPath, string message)> AdjustImageAsync(
        string inputPath, string outputFileName, float brightness, float contrast, float saturation)
    {
        try
        {
            if (!File.Exists(inputPath))
                return (false, "", "Input file not found.");

            using var image = await Image.LoadAsync(inputPath);
            image.Mutate(ctx => ctx.Brightness(brightness).Contrast(contrast).Saturate(saturation));

            var outputPath = Path.Combine(_tempFilePath, outputFileName);
            await image.SaveAsync(outputPath);

            return (true, outputPath, "Successfully adjusted image.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adjusting image");
            return (false, "", $"Error adjusting image: {ex.Message}");
        }
    }

    // -----------------------------------------------------------------------
    // Grayscale
    // -----------------------------------------------------------------------
    public async Task<(bool success, string outputPath, string message)> ApplyGrayscaleAsync(
        string inputPath, string outputFileName)
    {
        try
        {
            if (!File.Exists(inputPath))
                return (false, "", "Input file not found.");

            using var image = await Image.LoadAsync(inputPath);
            image.Mutate(ctx => ctx.Grayscale());

            var outputPath = Path.Combine(_tempFilePath, outputFileName);
            await image.SaveAsync(outputPath);

            return (true, outputPath, "Successfully applied grayscale filter.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying grayscale");
            return (false, "", $"Error applying filter: {ex.Message}");
        }
    }

    // -----------------------------------------------------------------------
    // Thumbnail
    // -----------------------------------------------------------------------
    public async Task<(bool success, string outputPath, string message)> CreateThumbnailAsync(
        string inputPath, string outputFileName, int maxSize)
    {
        try
        {
            if (!File.Exists(inputPath))
                return (false, "", "Input file not found.");

            using var image = await Image.LoadAsync(inputPath);
            image.Mutate(ctx => ctx.Resize(new ResizeOptions
            {
                Mode = ResizeMode.Max,
                Size = new Size(maxSize, maxSize)
            }));

            var outputPath = Path.Combine(_tempFilePath, outputFileName);
            await image.SaveAsync(outputPath);

            return (true, outputPath, $"Thumbnail created ({maxSize}px max).");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating thumbnail");
            return (false, "", $"Error creating thumbnail: {ex.Message}");
        }
    }

    // -----------------------------------------------------------------------
    // Watermark
    // -----------------------------------------------------------------------
    public async Task<(bool success, string outputPath, string message)> AddWatermarkAsync(
        string inputPath, string outputFileName, string text, float opacity, int fontSize)
    {
        try
        {
            if (!File.Exists(inputPath))
                return (false, "", "Input file not found.");

            using var bmp = SKBitmap.Decode(inputPath);
            using var surface = SKSurface.Create(new SKImageInfo(bmp.Width, bmp.Height));
            var canvas = surface.Canvas;
            canvas.DrawBitmap(bmp, 0, 0);

            using var paint = new SKPaint
            {
                Color   = SKColors.Gray.WithAlpha((byte)(opacity * 255)),
                TextSize = fontSize,
                IsAntialias = true,
            };

            float x = bmp.Width  / 2f;
            float y = bmp.Height / 2f;
            canvas.Save();
            canvas.Translate(x, y);
            canvas.RotateDegrees(-30);
            canvas.DrawText(text, 0, 0, paint);
            canvas.Restore();

            using var img  = surface.Snapshot();
            using var data = img.Encode(SKEncodedImageFormat.Png, 100);

            var outputPath = Path.Combine(_tempFilePath, outputFileName);
            await File.WriteAllBytesAsync(outputPath, data.ToArray());

            return (true, outputPath, "Watermark added successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding watermark");
            return (false, "", $"Error adding watermark: {ex.Message}");
        }
    }

    // -----------------------------------------------------------------------
    // PDF → Images
    // -----------------------------------------------------------------------
    public async Task<(bool success, List<string> outputPaths, string message)> ConvertPdfToImagesAsync(
        string inputPath, string outputPrefix, int dpi = 150)
    {
        var outputPaths = new List<string>();
        try
        {
            if (!File.Exists(inputPath))
                return (false, outputPaths, "Input file not found.");

            using var pdfDoc = UglyToad.PdfPig.PdfDocument.Open(inputPath);
            int pageCount = pdfDoc.NumberOfPages;

            for (int pn = 1; pn <= pageCount; pn++)
            {
                var page  = pdfDoc.GetPage(pn);
                double sc = dpi / 72.0;
                int w = (int)(page.Width * sc), h = (int)(page.Height * sc);

                using var skSurface = SKSurface.Create(new SKImageInfo(w, h));
                skSurface.Canvas.Clear(SKColors.White);

                using var p = new SKPaint { Color = SKColors.Black, TextSize = 14, IsAntialias = true };
                skSurface.Canvas.DrawText($"Page {pn} / {pageCount}", 20, 30, p);
                skSurface.Canvas.DrawText($"{page.Width:F0} × {page.Height:F0} pt", 20, 55, p);

                var fn   = $"{outputPrefix}_page_{pn}.png";
                var path = Path.Combine(_tempFilePath, fn);
                using var snapshot = skSurface.Snapshot();
                using var encoded  = snapshot.Encode(SKEncodedImageFormat.Png, 100);
                using var stream   = File.OpenWrite(path);
                encoded.SaveTo(stream);
                outputPaths.Add(path);
            }

            return (true, outputPaths, $"Converted {pageCount} page(s) to images.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting PDF to images");
            return (false, outputPaths, $"Error: {ex.Message}");
        }
    }

    // -----------------------------------------------------------------------
    // Get image info
    // -----------------------------------------------------------------------
    public (int width, int height, string format, long fileSize) GetImageInfo(string inputPath)
    {
        try
        {
            if (!File.Exists(inputPath)) return (0, 0, "", 0);
            using var image = Image.Load(inputPath);
            var fi = new FileInfo(inputPath);
            return (image.Width, image.Height, fi.Extension.TrimStart('.').ToUpper(), fi.Length);
        }
        catch { return (0, 0, "", 0); }
    }

    // -----------------------------------------------------------------------
    // AI-powered document scanning (CamScanner style) via Python
    // -----------------------------------------------------------------------
    public async Task<(bool success, string outputPath, string message)> ScanDocumentWithAIAsync(
        string inputPath, string outputFileName, string mode = "document", string paper = "a4")
    {
        try
        {
            if (!File.Exists(inputPath))
                return (false, "", "Input file not found.");

            var scriptPath = ResolveScriptPath();
            if (!File.Exists(scriptPath))
            {
                _logger.LogWarning("Python script not found, using C# fallback");
                return await EnhanceImageLocalAsync(inputPath, outputFileName, mode);
            }

            var outputPath = Path.Combine(_tempFilePath, outputFileName);
            var pythonExe  = GetPythonExecutable();
            var args = $"\"{scriptPath}\" \"{inputPath}\" \"{outputPath}\" --mode {mode} --operation scan --paper {paper}";

            var (exitCode, stdout, stderr) = await RunProcessAsync(pythonExe, args);

            _logger.LogInformation("AI scan output: {Out}", stdout);
            if (!string.IsNullOrEmpty(stderr)) _logger.LogWarning("AI scan stderr: {Err}", stderr);

            if (exitCode == 0 && File.Exists(outputPath))
                return (true, outputPath, $"Document scanned successfully as {paper.ToUpper()}.");

            _logger.LogWarning("AI scan failed (exit {Code}), using C# fallback", exitCode);
            return await EnhanceImageLocalAsync(inputPath, outputFileName, mode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in AI document scan");
            return await EnhanceImageLocalAsync(inputPath, outputFileName, mode);
        }
    }

    // -----------------------------------------------------------------------
    // AI Enhancement (original pipeline, now delegates to scan)
    // -----------------------------------------------------------------------
    public async Task<(bool success, string outputPath, string message)> EnhanceImageWithAIAsync(
        string inputPath, string outputFileName, string mode = "document")
        => await ScanDocumentWithAIAsync(inputPath, outputFileName, mode, "auto");

    // -----------------------------------------------------------------------
    // C# fallback enhancement (no Python required)
    // -----------------------------------------------------------------------
    private async Task<(bool success, string outputPath, string message)> EnhanceImageLocalAsync(
        string inputPath, string outputFileName, string mode)
    {
        try
        {
            if (!File.Exists(inputPath))
                return (false, "", "Input file not found.");

            using var image = await Image.LoadAsync(inputPath);

            if (mode == "document")
            {
                image.Mutate(ctx => ctx
                    .Grayscale()
                    .Contrast(1.35f)
                    .Brightness(1.05f)
                    .GaussianSharpen(1.8f));
            }
            else
            {
                image.Mutate(ctx => ctx
                    .Contrast(1.2f)
                    .Saturate(1.1f)
                    .Brightness(1.05f)
                    .GaussianSharpen(1.0f));
            }

            var outputPath = Path.Combine(_tempFilePath, outputFileName);
            await image.SaveAsync(outputPath);

            return (true, outputPath, "Image enhanced successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enhancing image locally");
            return (false, "", $"Error enhancing image: {ex.Message}");
        }
    }

    // -----------------------------------------------------------------------
    // Auto-straighten
    // -----------------------------------------------------------------------
    public async Task<(bool success, string outputPath, string message)> StraightenImageAsync(
        string inputPath, string outputFileName)
    {
        try
        {
            if (!File.Exists(inputPath))
                return (false, "", "Input file not found.");

            var scriptPath = ResolveScriptPath();
            if (File.Exists(scriptPath))
            {
                var outputPath = Path.Combine(_tempFilePath, outputFileName);
                var pythonExe  = GetPythonExecutable();
                var args = $"\"{scriptPath}\" \"{inputPath}\" \"{outputPath}\" --operation deskew";

                var (exitCode, _, _) = await RunProcessAsync(pythonExe, args);
                if (exitCode == 0 && File.Exists(outputPath))
                    return (true, outputPath, "Image straightened successfully.");
            }

            return await RotateImageAsync(inputPath, outputFileName, 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error straightening image");
            return (false, "", $"Error straightening: {ex.Message}");
        }
    }

    // -----------------------------------------------------------------------
    // Scan document (implements stub)
    // -----------------------------------------------------------------------
    public async Task<ProcessResult> ScanDocumentAsync(string inputPath, string outputFileName, ScanOptions opts)
    {
        var paper = opts?.PaperSize?.ToLower() ?? "a4";
        var mode  = opts?.Mode?.ToLower()      ?? "document";
        var (success, path, message) = await ScanDocumentWithAIAsync(inputPath, outputFileName, mode, paper);
        return new ProcessResult(success, path, message);
    }

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------
    private string ResolveScriptPath()
    {
        var p1 = Path.Combine(_scriptsPath, "image_enhancer.py");
        if (File.Exists(p1)) return p1;
        var p2 = Path.Combine("/var/www/pdfpeaks", "scripts", "image_enhancer.py");
        return File.Exists(p2) ? p2 : p1;
    }

    private static string GetPythonExecutable()
        => Environment.OSVersion.Platform == PlatformID.Win32NT ? "python" : "python3";

    private async Task<(int exitCode, string stdout, string stderr)> RunProcessAsync(string exe, string args)
    {
        var psi = new ProcessStartInfo
        {
            FileName               = exe,
            Arguments              = args,
            UseShellExecute        = false,
            RedirectStandardOutput = true,
            RedirectStandardError  = true,
            CreateNoWindow         = true
        };

        using var process = Process.Start(psi)!;
        var stdout = await process.StandardOutput.ReadToEndAsync();
        var stderr = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();
        return (process.ExitCode, stdout, stderr);
    }

    // -----------------------------------------------------------------------
    // Legacy compatibility shims
    // -----------------------------------------------------------------------
    public (int Width, int Height, string Format, long FileSize) GetInfo(string p) => GetImageInfo(p);

    public async Task<ProcessResult> ResizeAsync(string i, string o, int w, int h, ResizeMode m)
    { var (s, p, msg) = await ResizeImageAsync(i, o, w, h, m); return new ProcessResult(s, p, msg); }

    public async Task<ProcessResult> CropAsync(string i, string o, int x, int y, int w, int h)
    { var (s, p, msg) = await CropImageAsync(i, o, x, y, w, h); return new ProcessResult(s, p, msg); }

    public async Task<ProcessResult> RotateAsync(string i, string o, float d)
    { var (s, p, msg) = await RotateImageAsync(i, o, d); return new ProcessResult(s, p, msg); }

    public async Task<ProcessResult> FlipAsync(string i, string o, bool h)
    { var (s, p, msg) = await FlipImageAsync(i, o, h); return new ProcessResult(s, p, msg); }

    public async Task<ProcessResult> ConvertFormatAsync(string i, string o, string f)
    { var (s, p, msg) = await ConvertImageFormatAsync(i, o, f); return new ProcessResult(s, p, msg); }

    public async Task<CompressResult> CompressAsync(string i, string o, int q)
    { var (s, p, msg, orig, comp) = await CompressImageAsync(i, o, q); return new CompressResult(s, p, msg, orig, comp); }

    public async Task<ProcessResult> AdjustAsync(string i, string o, float b, float c, float sat, float hue)
    { var (s, p, msg) = await AdjustImageAsync(i, o, b, c, sat); return new ProcessResult(s, p, msg); }

    public async Task<ProcessResult> GrayscaleAsync(string i, string o)
    { var (s, p, msg) = await ApplyGrayscaleAsync(i, o); return new ProcessResult(s, p, msg); }

    public async Task<ProcessResult> ThumbnailAsync(string i, string o, int maxSize)
    { var (s, p, msg) = await CreateThumbnailAsync(i, o, maxSize); return new ProcessResult(s, p, msg); }

    public async Task<ProcessResult> AddWatermarkSkiaAsync(string i, string o, string text, float opacity, int fontSize, string hex, float angle)
    { var (s, p, msg) = await AddWatermarkAsync(i, o, text, opacity, fontSize); return new ProcessResult(s, p, msg); }

    public Task<ProcessResult> ApplyColorFilterSkiaAsync(string i, string o, string f)
        => Task.FromResult(new ProcessResult(false, "", "Colour filter not implemented."));

    public Task<ProcessResult> BlurSkiaAsync(string i, string o, float sx, float sy)
        => Task.FromResult(new ProcessResult(false, "", "Blur not implemented."));

    public Task<ProcessResult> SharpenSkiaAsync(string i, string o, float sigma, float gain)
        => Task.FromResult(new ProcessResult(false, "", "Sharpen not implemented."));

    public Task<ProcessResult> AiEnhanceAsync(string i, string o, int scale, bool denoise, bool sharpen)
        => Task.FromResult(new ProcessResult(false, "", "Use ScanDocumentAsync instead."));

    public async Task<ProcessResult> ImageToPdfAsync(string inputPath, string outputFileName, PdfPageSize pageSize)
    {
        var paper = pageSize switch
        {
            PdfPageSize.A4     => "a4",
            PdfPageSize.Letter => "letter",
            PdfPageSize.Legal  => "legal",
            PdfPageSize.A3     => "a3",
            _                  => "auto"
        };
        var (s, p, msg) = await ConvertToPdfAsync(inputPath, outputFileName, paper);
        return new ProcessResult(s, p, msg);
    }

    public async Task<ProcessResult> ConvertMultipleImagesToPdfAsync(string[] paths, string outputFileName)
        => await ImagesToPdfAsync(paths, outputFileName, PdfPageSize.Auto);
}

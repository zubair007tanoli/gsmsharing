using SkiaSharp;
using Pdfpeaks.Models;
using System.Runtime.InteropServices;

namespace Pdfpeaks.Services;

/// <summary>
/// Image enhancement service using SkiaSharp for CamScanner-style document processing
/// Provides: auto-contrast, sharpen, deskew, binarization, and shadow removal
/// </summary>
public class ImageEnhancementService
{
    private readonly ILogger<ImageEnhancementService> _logger;
    private readonly string _tempFilePath;

    public ImageEnhancementService(ILogger<ImageEnhancementService> logger, IWebHostEnvironment environment)
    {
        _logger = logger;
        _tempFilePath = Path.Combine(environment.ContentRootPath, "temp_files");
        
        if (!Directory.Exists(_tempFilePath))
        {
            Directory.CreateDirectory(_tempFilePath);
        }
    }

    /// <summary>
    /// Enhance a document image with CamScanner-style processing
    /// </summary>
    public async Task<(bool success, string outputPath, string message)> EnhanceDocumentAsync(
        string inputPath, 
        string outputFileName,
        ImageEnhancementOptions? options = null)
    {
        options ??= new ImageEnhancementOptions();
        
        try
        {
            if (!File.Exists(inputPath))
            {
                return (false, "", "Input file not found.");
            }

            _logger.LogInformation("Starting document enhancement: {Input}", inputPath);

            // Load the image
            using var inputStream = File.OpenRead(inputPath);
            using var originalBitmap = SKBitmap.Decode(inputStream);
            
            if (originalBitmap == null)
            {
                return (false, "", "Failed to decode image.");
            }

            // Process based on mode
            using var enhancedBitmap = options.Mode.ToLowerInvariant() switch
            {
                "document" => EnhanceForDocument(originalBitmap, options),
                "photo" => EnhanceForPhoto(originalBitmap, options),
                "grayscale" => ConvertToGrayscale(originalBitmap),
                "blackwhite" => ConvertToBlackWhite(originalBitmap, options.Threshold),
                _ => EnhanceForDocument(originalBitmap, options)
            };

            // Save the result
            var outputPath = Path.Combine(_tempFilePath, outputFileName);
            using var outputStream = File.Create(outputPath);
            
            // Determine format from filename
            var extension = Path.GetExtension(outputFileName).ToLowerInvariant();
            var encodedImage = GetEncodedImage(enhancedBitmap, extension);
            
            encodedImage.SaveTo(outputStream);

            _logger.LogInformation("Document enhancement completed: {Output}", outputPath);
            return (true, outputPath, "Image enhanced successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enhancing document image");
            return (false, "", $"Error enhancing image: {ex.Message}");
        }
    }

    /// <summary>
    /// Auto-contrast and levels adjustment
    /// </summary>
    public async Task<(bool success, string outputPath, string message)> AutoContrastAsync(
        string inputPath, 
        string outputFileName)
    {
        try
        {
            if (!File.Exists(inputPath))
            {
                return (false, "", "Input file not found.");
            }

            using var inputStream = File.OpenRead(inputPath);
            using var originalBitmap = SKBitmap.Decode(inputStream);
            
            if (originalBitmap == null)
            {
                return (false, "", "Failed to decode image.");
            }

            using var adjustedBitmap = ApplyAutoLevels(originalBitmap);

            var outputPath = Path.Combine(_tempFilePath, outputFileName);
            using var outputStream = File.Create(outputPath);
            var encodedImage = GetEncodedImage(adjustedBitmap, ".png");
            encodedImage.SaveTo(outputStream);

            return (true, outputPath, "Auto-contrast applied successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying auto-contrast");
            return (false, "", $"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Sharpen the image
    /// </summary>
    public async Task<(bool success, string outputPath, string message)> SharpenAsync(
        string inputPath, 
        string outputFileName,
        float strength = 1.0f)
    {
        try
        {
            if (!File.Exists(inputPath))
            {
                return (false, "", "Input file not found.");
            }

            using var inputStream = File.OpenRead(inputPath);
            using var originalBitmap = SKBitmap.Decode(inputStream);
            
            if (originalBitmap == null)
            {
                return (false, "", "Failed to decode image.");
            }

            using var sharpenedBitmap = ApplySharpen(originalBitmap, strength);

            var outputPath = Path.Combine(_tempFilePath, outputFileName);
            using var outputStream = File.Create(outputPath);
            var encodedImage = GetEncodedImage(sharpenedBitmap, ".png");
            encodedImage.SaveTo(outputStream);

            return (true, outputPath, "Image sharpened successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sharpening image");
            return (false, "", $"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Deskew - detect and correct rotation in scanned documents
    /// </summary>
    public async Task<(bool success, string outputPath, string message)> DeskewAsync(
        string inputPath, 
        string outputFileName)
    {
        try
        {
            if (!File.Exists(inputPath))
            {
                return (false, "", "Input file not found.");
            }

            using var inputStream = File.OpenRead(inputPath);
            using var originalBitmap = SKBitmap.Decode(inputStream);
            
            if (originalBitmap == null)
            {
                return (false, "", "Failed to decode image.");
            }

            // Detect skew angle
            var angle = DetectSkewAngle(originalBitmap);
            _logger.LogInformation("Detected skew angle: {Angle} degrees", angle);

            // Rotate to correct
            using var rotatedBitmap = RotateBitmap(originalBitmap, angle);

            var outputPath = Path.Combine(_tempFilePath, outputFileName);
            using var outputStream = File.Create(outputPath);
            var encodedImage = GetEncodedImage(rotatedBitmap, ".png");
            encodedImage.SaveTo(outputStream);

            return (true, outputPath, $"Image deskewed by {angle:F2} degrees.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deskewing image");
            return (false, "", $"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Binarization - convert to black and white for OCR-friendly output
    /// </summary>
    public async Task<(bool success, string outputPath, string message)> BinarizeAsync(
        string inputPath, 
        string outputFileName,
        int threshold = 128)
    {
        try
        {
            if (!File.Exists(inputPath))
            {
                return (false, "", "Input file not found.");
            }

            using var inputStream = File.OpenRead(inputPath);
            using var originalBitmap = SKBitmap.Decode(inputStream);
            
            if (originalBitmap == null)
            {
                return (false, "", "Failed to decode image.");
            }

            using var bwBitmap = ConvertToBlackWhite(originalBitmap, threshold);

            var outputPath = Path.Combine(_tempFilePath, outputFileName);
            using var outputStream = File.Create(outputPath);
            var encodedImage = GetEncodedImage(bwBitmap, ".png");
            encodedImage.SaveTo(outputStream);

            return (true, outputPath, "Image binarized successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error binarizing image");
            return (false, "", $"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Remove shadows from scanned documents
    /// </summary>
    public async Task<(bool success, string outputPath, string message)> RemoveShadowAsync(
        string inputPath, 
        string outputFileName)
    {
        try
        {
            if (!File.Exists(inputPath))
            {
                return (false, "", "Input file not found.");
            }

            using var inputStream = File.OpenRead(inputPath);
            using var originalBitmap = SKBitmap.Decode(inputStream);
            
            if (originalBitmap == null)
            {
                return (false, "", "Failed to decode image.");
            }

            using var cleanedBitmap = RemoveShadow(originalBitmap);

            var outputPath = Path.Combine(_tempFilePath, outputFileName);
            using var outputStream = File.Create(outputPath);
            var encodedImage = GetEncodedImage(cleanedBitmap, ".png");
            encodedImage.SaveTo(outputStream);

            return (true, outputPath, "Shadows removed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing shadows");
            return (false, "", $"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Get image dimensions without loading the full image
    /// </summary>
    public (int width, int height) GetImageDimensions(string inputPath)
    {
        try
        {
            using var stream = File.OpenRead(inputPath);
            var codec = SKCodec.Create(stream);
            if (codec != null)
            {
                return (codec.Info.Width, codec.Info.Height);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not get image dimensions");
        }
        return (0, 0);
    }

    #region Private Enhancement Methods

    private SKBitmap EnhanceForDocument(SKBitmap source, ImageEnhancementOptions options)
    {
        var result = new SKBitmap(source.Width, source.Height, source.ColorType, source.AlphaType);
        
        // First convert to grayscale if not already
        using var grayscale = ConvertToGrayscale(source);
        
        // Apply auto-levels (contrast)
        using var contrasted = ApplyAutoLevels(grayscale);
        
        // Apply sharpening
        using var sharpened = ApplySharpen(contrasted, options.SharpenStrength);
        
        // Copy to result
        using var canvas = new SKCanvas(result);
        canvas.Clear(SKColors.White);
        canvas.DrawBitmap(sharpened, 0, 0);
        
        return result;
    }

    private SKBitmap EnhanceForPhoto(SKBitmap source, ImageEnhancementOptions options)
    {
        var result = new SKBitmap(source.Width, source.Height, source.ColorType, source.AlphaType);
        
        using var canvas = new SKCanvas(result);
        canvas.Clear(SKColors.White);
        
        // Apply contrast using SKColorFilter
        using var contrastPaint = new SKPaint
        {
            ColorFilter = SKColorFilter.CreateColorMatrix(new float[]
            {
                1.2f, 0, 0, 0, 0,   // Red - increased contrast
                0, 1.2f, 0, 0, 0,   // Green
                0, 0, 1.2f, 0, 0,   // Blue
                0, 0, 0, 1, 0        // Alpha
            })
        };
        
        canvas.DrawBitmap(source, 0, 0, contrastPaint);
        
        // Apply slight sharpen
        var sharpened = ApplySharpen(result, 0.5f);
        result.Dispose();
        return sharpened;
    }

    private SKBitmap ConvertToGrayscale(SKBitmap source)
    {
        var result = new SKBitmap(source.Width, source.Height);
        
        for (int y = 0; y < source.Height; y++)
        {
            for (int x = 0; x < source.Width; x++)
            {
                var pixel = source.GetPixel(x, y);
                // Use luminance formula: 0.299R + 0.587G + 0.114B
                var gray = (byte)(0.299 * pixel.Red + 0.587 * pixel.Green + 0.114 * pixel.Blue);
                result.SetPixel(x, y, new SKColor(gray, gray, gray));
            }
        }
        
        return result;
    }

    private SKBitmap ConvertToBlackWhite(SKBitmap source, int threshold = 128)
    {
        var result = new SKBitmap(source.Width, source.Height);
        
        for (int y = 0; y < source.Height; y++)
        {
            for (int x = 0; x < source.Width; x++)
            {
                var pixel = source.GetPixel(x, y);
                // Use luminance
                var luminance = (byte)(0.299 * pixel.Red + 0.587 * pixel.Green + 0.114 * pixel.Blue);
                var bw = luminance > threshold ? (byte)255 : (byte)0;
                result.SetPixel(x, y, new SKColor(bw, bw, bw));
            }
        }
        
        return result;
    }

    private SKBitmap ApplyAutoLevels(SKBitmap source)
    {
        // Find min and max values
        byte min = 255, max = 0;
        
        for (int y = 0; y < source.Height; y++)
        {
            for (int x = 0; x < source.Width; x++)
            {
                var pixel = source.GetPixel(x, y);
                var value = (byte)(0.299 * pixel.Red + 0.587 * pixel.Green + 0.114 * pixel.Blue);
                if (value < min) min = value;
                if (value > max) max = value;
            }
        }
        
        // Apply contrast stretch
        var result = new SKBitmap(source.Width, source.Height);
        var range = max - min;
        
        if (range < 1) range = 1;
        
        for (int y = 0; y < source.Height; y++)
        {
            for (int x = 0; x < source.Width; x++)
            {
                var pixel = source.GetPixel(x, y);
                var value = (byte)(0.299 * pixel.Red + 0.587 * pixel.Green + 0.114 * pixel.Blue);
                var stretched = (byte)((value - min) * 255 / range);
                result.SetPixel(x, y, new SKColor(stretched, stretched, stretched));
            }
        }
        
        return result;
    }

    private SKBitmap ApplySharpen(SKBitmap source, float strength = 1.0f)
    {
        // Use Gaussian blur with sigma inversely proportional to strength for sharpening effect
        // Sharpening = original - blur, but we can approximate with high-sigma blur
        var sigma = Math.Max(0.5f, 2.0f - strength);
        
        var result = new SKBitmap(source.Width, source.Height);
        
        using var canvas = new SKCanvas(result);
        
        // Create a high-contrast unsharp mask effect manually
        // First, get the original
        canvas.Clear();
        canvas.DrawBitmap(source, 0, 0);
        
        // Create a slightly blurred version
        using var blurred = new SKBitmap(source.Width, source.Height);
        using var blurCanvas = new SKCanvas(blurred);
        using var blurPaint = new SKPaint
        {
            ImageFilter = SKImageFilter.CreateBlur(sigma, sigma)
        };
        blurCanvas.DrawBitmap(source, 0, 0, blurPaint);
        
        // Combine: sharpened = source + (source - blurred) * strength
        for (int y = 0; y < source.Height; y++)
        {
            for (int x = 0; x < source.Width; x++)
            {
                var origPixel = source.GetPixel(x, y);
                var blurPixel = blurred.GetPixel(x, y);
                
                // Unsharp mask: original + (original - blurred) * strength
                var r = Math.Clamp(origPixel.Red + (origPixel.Red - blurPixel.Red) * strength, 0f, 255f);
                var g = Math.Clamp(origPixel.Green + (origPixel.Green - blurPixel.Green) * strength, 0f, 255f);
                var b = Math.Clamp(origPixel.Blue + (origPixel.Blue - blurPixel.Blue) * strength, 0f, 255f);
                
                result.SetPixel(x, y, new SKColor((byte)r, (byte)g, (byte)b, origPixel.Alpha));
            }
        }
        
        return result;
    }

    private float DetectSkewAngle(SKBitmap source)
    {
        // Simple skew detection using projection profile
        // Convert to grayscale first
        using var grayscale = ConvertToGrayscale(source);
        
        var height = grayscale.Height;
        var width = grayscale.Width;
        
        // Create horizontal projection (sum of dark pixels per row)
        var projection = new int[height];
        
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var pixel = grayscale.GetPixel(x, y);
                // Count dark pixels (likely text)
                if (pixel.Red < 128)
                {
                    projection[y]++;
                }
            }
        }
        
        // Find the range with most content
        int maxSum = 0;
        int bestStart = 0;
        
        for (int i = 0; i < height - 10; i++)
        {
            int sum = 0;
            for (int j = i; j < i + 10 && j < height; j++)
            {
                sum += projection[j];
            }
            
            if (sum > maxSum)
            {
                maxSum = sum;
                bestStart = i;
            }
        }
        
        // Calculate skew based on where the content is centered
        // This is a simplified approach - for production, use Hough transform
        var center = bestStart + 5;
        var expectedCenter = height / 2;
        var skew = (center - expectedCenter) * 0.1f; // Scale factor
        
        // Clamp to reasonable range
        return Math.Clamp(skew, -15f, 15f);
    }

    private SKBitmap RotateBitmap(SKBitmap source, float angle)
    {
        var centerX = source.Width / 2f;
        var centerY = source.Height / 2f;
        
        // Calculate new dimensions
        var radians = angle * Math.PI / 180f;
        var cos = (float)Math.Abs(Math.Cos(radians));
        var sin = (float)Math.Abs(Math.Sin(radians));
        
        var newWidth = (int)(source.Width * cos + source.Height * sin);
        var newHeight = (int)(source.Width * sin + source.Height * cos);
        
        var result = new SKBitmap(newWidth, newHeight);
        
        using var canvas = new SKCanvas(result);
        canvas.Clear(SKColors.White);
        canvas.Translate(newWidth / 2f, newHeight / 2f);
        canvas.RotateDegrees(angle);
        canvas.Translate(-centerX, -centerY);
        canvas.DrawBitmap(source, 0, 0);
        
        return result;
    }

    private SKBitmap RemoveShadow(SKBitmap source)
    {
        // Estimate background by finding the brightest pixels
        var result = new SKBitmap(source.Width, source.Height);
        
        // First convert to grayscale
        using var grayscale = ConvertToGrayscale(source);
        
        // Estimate background using large median filter (simplified)
        var bgEstimate = EstimateBackground(grayscale);
        
        // Divide by background estimate to remove shadow
        for (int y = 0; y < source.Height; y++)
        {
            for (int x = 0; x < source.Width; x++)
            {
                var pixel = grayscale.GetPixel(x, y);
                var bg = bgEstimate[x, y];
                
                // Normalize against background
                var normalized = bg > 0 ? pixel.Red * 255 / bg : pixel.Red;
                normalized = Math.Clamp(normalized, 0, 255);
                
                result.SetPixel(x, y, new SKColor((byte)normalized, (byte)normalized, (byte)normalized));
            }
        }
        
        return result;
    }

    private byte[,] EstimateBackground(SKBitmap grayscale)
    {
        // Simple background estimation using local averaging
        var bg = new byte[grayscale.Width, grayscale.Height];
        var windowSize = 50;
        
        for (int y = 0; y < grayscale.Height; y++)
        {
            for (int x = 0; x < grayscale.Width; x++)
            {
                int sum = 0, count = 0;
                
                for (int dy = -windowSize; dy <= windowSize; dy += 10)
                {
                    for (int dx = -windowSize; dx <= windowSize; dx += 10)
                    {
                        var nx = x + dx;
                        var ny = y + dy;
                        
                        if (nx >= 0 && nx < grayscale.Width && ny >= 0 && ny < grayscale.Height)
                        {
                            var pixel = grayscale.GetPixel(nx, ny);
                            sum += pixel.Red;
                            count++;
                        }
                    }
                }
                
                bg[x, y] = count > 0 ? (byte)(sum / count) : (byte)255;
            }
        }
        
        return bg;
    }

    private SKData GetEncodedImage(SKBitmap bitmap, string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => bitmap.Encode(SKEncodedImageFormat.Jpeg, 90),
            ".webp" => bitmap.Encode(SKEncodedImageFormat.Webp, 90),
            _ => bitmap.Encode(SKEncodedImageFormat.Png, 100)
        };
    }

    #endregion
}

/// <summary>
/// Options for image enhancement
/// </summary>
public class ImageEnhancementOptions
{
    /// <summary>
    /// Enhancement mode: document, photo, grayscale, blackwhite
    /// </summary>
    public string Mode { get; set; } = "document";
    
    /// <summary>
    /// Sharpening strength (0-2)
    /// </summary>
    public float SharpenStrength { get; set; } = 1.0f;
    
    /// <summary>
    /// Threshold for binarization (0-255)
    /// </summary>
    public int Threshold { get; set; } = 128;
    
    /// <summary>
    /// Apply contrast enhancement
    /// </summary>
    public bool AutoContrast { get; set; } = true;
    
    /// <summary>
    /// Apply deskew correction
    /// </summary>
    public bool AutoDeskew { get; set; } = false;
}

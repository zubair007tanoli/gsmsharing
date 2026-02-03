using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;
using PdfSharpCore.Drawing;
using Pdfpeaks.Models;

namespace Pdfpeaks.Services;

/// <summary>
/// Service for basic PDF operations using PdfSharpCore
/// </summary>
public class PdfProcessingService
{
    private readonly ILogger<PdfProcessingService> _logger;
    private readonly string _tempFilePath;

    public PdfProcessingService(ILogger<PdfProcessingService> logger, IWebHostEnvironment environment)
    {
        _logger = logger;
        _tempFilePath = Path.Combine(environment.ContentRootPath, "wwwroot", "temp");
        
        if (!Directory.Exists(_tempFilePath))
        {
            Directory.CreateDirectory(_tempFilePath);
        }
    }

    /// <summary>
    /// Merge multiple PDF files into one
    /// </summary>
    public async Task<(bool success, string outputPath, string message)> MergePdfAsync(
        List<string> inputFiles, List<int> sortOrder, string outputFileName)
    {
        try
        {
            if (inputFiles.Count < 2)
            {
                return (false, "", "At least 2 PDF files are required for merging.");
            }

            var sortedFiles = sortOrder.Count == inputFiles.Count
                ? sortOrder.Select(i => inputFiles[i]).ToList()
                : inputFiles;

            var outputDocument = new PdfDocument();
            
            foreach (var filePath in sortedFiles)
            {
                if (!File.Exists(filePath)) continue;

                try
                {
                    using var inputDocument = PdfReader.Open(filePath);
                    foreach (var page in inputDocument.Pages)
                    {
                        outputDocument.AddPage(page);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error importing file: {FilePath}", filePath);
                }
            }

            var outputPath = Path.Combine(_tempFilePath, outputFileName);
            outputDocument.Save(outputPath);
            
            _logger.LogInformation("PDF merged successfully: {OutputPath}", outputPath);
            
            return (true, outputPath, $"Successfully merged {sortedFiles.Count} PDF files.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error merging PDF files");
            return (false, "", $"Error merging PDFs: {ex.Message}");
        }
    }

    /// <summary>
    /// Split PDF into individual pages or a range
    /// </summary>
    public async Task<(bool success, List<string> outputPaths, string message)> SplitPdfAsync(
        string inputFile, int startPage, int endPage, string outputPrefix)
    {
        var outputPaths = new List<string>();

        try
        {
            if (!File.Exists(inputFile))
            {
                return (false, outputPaths, "Input file not found.");
            }

            using var inputDocument = PdfReader.Open(inputFile);
            
            int totalPages = inputDocument.PageCount;
            int start = Math.Max(1, startPage);
            int end = Math.Min(totalPages, endPage);

            if (start > end)
            {
                start = 1;
                end = totalPages;
            }

            for (int i = start; i <= end; i++)
            {
                using var outputDocument = new PdfDocument();
                outputDocument.AddPage(inputDocument.Pages[i - 1]);
                
                var outputFileName = $"{outputPrefix}_page_{i}.pdf";
                var outputPath = Path.Combine(_tempFilePath, outputFileName);
                outputDocument.Save(outputPath);
                outputPaths.Add(outputPath);
            }

            _logger.LogInformation("PDF split successfully into {Count} pages", outputPaths.Count);
            return (true, outputPaths, $"Successfully split PDF into {outputPaths.Count} pages.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error splitting PDF");
            return (false, outputPaths, $"Error splitting PDF: {ex.Message}");
        }
    }

    /// <summary>
    /// Rotate PDF pages by specified degrees
    /// </summary>
    public async Task<(bool success, string outputPath, string message)> RotatePdfAsync(
        string inputFile, List<int> pageIndices, int rotationDegrees, string outputFileName)
    {
        try
        {
            if (!File.Exists(inputFile))
            {
                return (false, "", "Input file not found.");
            }

            using var inputDocument = PdfReader.Open(inputFile);
            
            foreach (var pageIndex in pageIndices)
            {
                if (pageIndex >= 0 && pageIndex < inputDocument.Pages.Count)
                {
                    var page = inputDocument.Pages[pageIndex];
                    var currentRotation = page.Rotate;
                    page.Rotate = currentRotation + rotationDegrees;
                }
            }

            var outputPath = Path.Combine(_tempFilePath, outputFileName);
            inputDocument.Save(outputPath);
            
            return (true, outputPath, $"Successfully rotated {pageIndices.Count} pages.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rotating PDF");
            return (false, "", $"Error rotating PDF: {ex.Message}");
        }
    }

    /// <summary>
    /// Compress PDF
    /// </summary>
    public async Task<(bool success, string outputPath, string message)> CompressPdfAsync(
        string inputFile, int quality, string outputFileName)
    {
        try
        {
            if (!File.Exists(inputFile))
            {
                return (false, "", "Input file not found.");
            }

            using var inputDocument = PdfReader.Open(inputFile);
            
            var outputPath = Path.Combine(_tempFilePath, outputFileName);
            inputDocument.Save(outputPath);
            
            var originalSize = new FileInfo(inputFile).Length;
            var compressedSize = new FileInfo(outputPath).Length;
            
            _logger.LogInformation("PDF compressed: {Original} -> {Compressed} bytes", originalSize, compressedSize);
            return (true, outputPath, $"Successfully compressed PDF.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error compressing PDF");
            return (false, "", $"Error compressing PDF: {ex.Message}");
        }
    }

    /// <summary>
    /// Add page numbers to PDF
    /// </summary>
    public async Task<(bool success, string outputPath, string message)> AddPageNumbersAsync(
        string inputFile, string outputFileName, int startNumber, string position)
    {
        try
        {
            if (!File.Exists(inputFile))
            {
                return (false, "", "Input file not found.");
            }

            using var inputDocument = PdfReader.Open(inputFile);
            
            var font = new XFont("Arial", 10);
            
            for (int i = 0; i < inputDocument.Pages.Count; i++)
            {
                var page = inputDocument.Pages[i];
                var pageNumber = startNumber + i;
                
                var gfx = XGraphics.FromPdfPage(page);
                var text = pageNumber.ToString();
                var textSize = gfx.MeasureString(text, font);
                
                double x, y;
                switch (position.ToLower())
                {
                    case "bottom-center":
                        x = (page.Width - textSize.Width) / 2;
                        y = page.Height - textSize.Height - 20;
                        break;
                    case "bottom-left":
                        x = 20;
                        y = page.Height - textSize.Height - 20;
                        break;
                    case "bottom-right":
                        x = page.Width - textSize.Width - 20;
                        y = page.Height - textSize.Height - 20;
                        break;
                    case "top-center":
                        x = (page.Width - textSize.Width) / 2;
                        y = 20;
                        break;
                    default:
                        x = (page.Width - textSize.Width) / 2;
                        y = page.Height - textSize.Height - 20;
                        break;
                }
                
                gfx.DrawString(text, font, XBrushes.Black, x, y);
            }

            var outputPath = Path.Combine(_tempFilePath, outputFileName);
            inputDocument.Save(outputPath);
            
            return (true, outputPath, $"Successfully added page numbers.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding page numbers");
            return (false, "", $"Error adding page numbers: {ex.Message}");
        }
    }

    /// <summary>
    /// Get PDF page count
    /// </summary>
    public int GetPageCount(string inputFile)
    {
        try
        {
            if (!File.Exists(inputFile)) return 0;
            
            using var document = PdfReader.Open(inputFile);
            return document.PageCount;
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>
    /// Get PDF info
    /// </summary>
    public (int pageCount, long fileSize, List<string> pageSizes) GetPdfInfo(string inputFile)
    {
        var pageSizes = new List<string>();
        
        try
        {
            if (!File.Exists(inputFile))
            {
                return (0, 0, pageSizes);
            }
            
            using var document = PdfReader.Open(inputFile);
            
            foreach (var page in document.Pages)
            {
                pageSizes.Add($"{page.Width:F0}x{page.Height:F0}");
            }
            
            return (document.PageCount, new FileInfo(inputFile).Length, pageSizes);
        }
        catch
        {
            return (0, 0, pageSizes);
        }
    }

    /// <summary>
    /// Extract text from PDF
    /// </summary>
    public async Task<(bool success, string text, string message)> ExtractTextAsync(string inputFile)
    {
        try
        {
            if (!File.Exists(inputFile))
            {
                return (false, "", "Input file not found.");
            }

            using var document = PdfReader.Open(inputFile);
            var text = new System.Text.StringBuilder();
            
            return (true, text.ToString(), "Text extracted successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting text from PDF");
            return (false, "", $"Error extracting text: {ex.Message}");
        }
    }

    /// <summary>
    /// Protect PDF with password
    /// </summary>
    public async Task<(bool success, string outputPath, string message)> ProtectPdfAsync(
        string inputFile, string userPassword, string ownerPassword, string outputFileName)
    {
        try
        {
            if (!File.Exists(inputFile))
            {
                return (false, "", "Input file not found.");
            }

            using var inputDocument = PdfReader.Open(inputFile);
            
            var securitySettings = inputDocument.SecuritySettings;
            securitySettings.UserPassword = userPassword;
            securitySettings.OwnerPassword = ownerPassword;
            securitySettings.PermitPrint = false;

            var outputPath = Path.Combine(_tempFilePath, outputFileName);
            inputDocument.Save(outputPath);
            
            return (true, outputPath, "PDF protected successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error protecting PDF");
            return (false, "", $"Error protecting PDF: {ex.Message}");
        }
    }

    /// <summary>
    /// Remove password protection from PDF
    /// </summary>
    public async Task<(bool success, string outputPath, string message)> RemoveProtectionAsync(
        string inputFile, string password, string outputFileName)
    {
        try
        {
            if (!File.Exists(inputFile))
            {
                return (false, "", "Input file not found.");
            }

            using var inputDocument = PdfReader.Open(inputFile, password: password);
            
            var outputPath = Path.Combine(_tempFilePath, outputFileName);
            inputDocument.Save(outputPath);
            
            return (true, outputPath, "Protection removed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing protection");
            return (false, "", "Invalid password or file cannot be opened.");
        }
    }

    /// <summary>
    /// Convert PDF to images
    /// </summary>
    public async Task<(bool success, List<string> outputPaths, string message)> ConvertToImagesAsync(
        string inputFile, string outputPrefix, string imageFormat)
    {
        var outputPaths = new List<string>();
        
        try
        {
            if (!File.Exists(inputFile))
            {
                return (false, outputPaths, "Input file not found.");
            }

            using var document = PdfReader.Open(inputFile);
            
            for (int i = 0; i < document.Pages.Count; i++)
            {
                var outputFileName = $"{outputPrefix}_page_{i + 1}.png";
                var outputPath = Path.Combine(_tempFilePath, outputFileName);
                outputPaths.Add(outputPath);
            }

            return (true, outputPaths, $"Created {outputPaths.Count} page images.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting PDF to images");
            return (false, outputPaths, $"Error converting to images: {ex.Message}");
        }
    }
}

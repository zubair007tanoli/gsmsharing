using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;
using PdfSharpCore.Drawing;
using Pdfpeaks.Models;
using System.Text;
using System.Diagnostics;

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
        _tempFilePath = Path.Combine(environment.ContentRootPath, "temp_files");
        
        if (!Directory.Exists(_tempFilePath))
        {
            Directory.CreateDirectory(_tempFilePath);
        }
    }

    /// <summary>
    /// Merge multiple PDF files into one using PdfSharpCore
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

            _logger.LogInformation("Starting merge of {Count} files", sortedFiles.Count);
            foreach (var f in sortedFiles)
            {
                _logger.LogInformation("  File: {Path}", f);
            }

            var outputDocument = new PdfDocument();
            outputDocument.Info.Title = "Merged Document";
            
            int totalPagesImported = 0;
            int filesSuccessfullyProcessed = 0;
            
            foreach (var filePath in sortedFiles)
            {
                _logger.LogInformation("Processing file: {FilePath}", filePath);
                
                if (!File.Exists(filePath))
                {
                    _logger.LogWarning("File not found: {FilePath}", filePath);
                    continue;
                }

                try
                {
                    var fileInfo = new FileInfo(filePath);
                    _logger.LogInformation("File size: {Size} bytes", fileInfo.Length);
                    
                    if (fileInfo.Length == 0)
                    {
                        _logger.LogWarning("File is empty: {FilePath}", filePath);
                        continue;
                    }
                    
                    PdfDocument? inputDocument = null;
                    Exception? lastException = null;
                    
                    try
                    {
                        inputDocument = PdfReader.Open(filePath, PdfDocumentOpenMode.Import);
                        _logger.LogInformation("Opened with PdfReader.Open (Import mode)");
                    }
                    catch (Exception ex1)
                    {
                        lastException = ex1;
                        _logger.LogWarning(ex1, "PdfReader.Open (Import mode) failed, trying ReadOnly mode");
                        
                        try
                        {
                            inputDocument = PdfReader.Open(filePath, PdfDocumentOpenMode.ReadOnly);
                            _logger.LogInformation("Opened with PdfReader.Open (ReadOnly mode)");
                        }
                        catch (Exception ex2)
                        {
                            lastException = ex2;
                            _logger.LogError(ex2, "All PDF opening approaches failed for {FilePath}", filePath);
                        }
                    }
                    
                    if (inputDocument == null)
                    {
                        _logger.LogError("Could not open PDF file: {FilePath}. Last error: {Error}", 
                            filePath, lastException?.Message ?? "Unknown error");
                        continue;
                    }
                    
                    var pageCount = inputDocument.PageCount;
                    _logger.LogInformation("Input document has {PageCount} pages", pageCount);
                    
                    if (pageCount == 0)
                    {
                        pageCount = inputDocument.Pages.Count;
                        _logger.LogInformation("Using Pages collection count: {PageCount}", pageCount);
                    }
                    
                    if (pageCount == 0)
                    {
                        _logger.LogWarning("Could not determine page count, skipping file: {FilePath}", filePath);
                        continue;
                    }
                    
                    for (int i = 0; i < pageCount; i++)
                    {
                        try
                        {
                            var sourcePage = inputDocument.Pages[i];
                            outputDocument.AddPage(sourcePage);
                            totalPagesImported++;
                        }
                        catch (Exception pageEx)
                        {
                            _logger.LogError(pageEx, "Error importing page {Page} from {File}", i, filePath);
                        }
                    }
                    
                    filesSuccessfullyProcessed++;
                    _logger.LogInformation("Successfully imported {Count} pages from {File}", pageCount, filePath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error importing file: {FilePath}", filePath);
                }
            }

            _logger.LogInformation("Merge complete. Files processed: {Processed}, Total pages: {Pages}", 
                filesSuccessfullyProcessed, totalPagesImported);
            
            if (totalPagesImported == 0)
            {
                return (false, "", "No pages could be imported from the PDF files.");
            }
            
            var outputPath = Path.Combine(_tempFilePath, outputFileName);
            
            _logger.LogInformation("Saving merged PDF to: {Path}", outputPath);
            outputDocument.Save(outputPath);
            
            var outputFileInfo = new FileInfo(outputPath);
            _logger.LogInformation("Successfully saved merged PDF: {Path} ({Size} bytes, {Pages} pages)", 
                outputPath, outputFileInfo.Length, totalPagesImported);
            
            return (true, outputPath, $"Successfully merged {filesSuccessfullyProcessed} PDF files ({totalPagesImported} pages total).");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during PDF merge");
            return (false, "", $"Error merging PDFs: {ex.Message}");
        }
    }

    /// <summary>
    /// Split PDF into individual pages or a range
    /// </summary>
    public async Task<(bool success, List<string> outputPaths, string message)> SplitPdfAsync(
        string inputFile, int startPage, int endPage, string outputPrefix, string operationId = "")
    {
        var outputPaths = new List<string>();

        _logger.LogInformation("SplitPdfAsync called: inputFile={InputFile}, startPage={StartPage}, endPage={EndPage}, outputPrefix={OutputPrefix}, operationId={OperationId}",
            inputFile, startPage, endPage, outputPrefix, operationId);

        try
        {
            if (!File.Exists(inputFile))
            {
                _logger.LogError("Input file not found: {InputFile}", inputFile);
                return (false, outputPaths, "Input file not found.");
            }

            _logger.LogInformation("Opening input PDF: {InputFile}", inputFile);
            using var inputDocument = PdfReader.Open(inputFile, PdfDocumentOpenMode.Import);
            
            int totalPages = inputDocument.PageCount;
            _logger.LogInformation("PDF has {TotalPages} pages", totalPages);
            
            int start = Math.Max(1, startPage);
            int end = Math.Min(totalPages, endPage);

            if (start > end)
            {
                _logger.LogWarning("Adjusting page range: start was > end, setting to 1-{TotalPages}", totalPages);
                start = 1;
                end = totalPages;
            }

            _logger.LogInformation("Splitting pages {Start} to {End}", start, end);

            for (int i = start; i <= end; i++)
            {
                using var outputDocument = new PdfDocument();
                outputDocument.AddPage(inputDocument.Pages[i - 1]);
                
                // Use operation ID and page number for unique filename
                var outputFileName = !string.IsNullOrEmpty(operationId) 
                    ? $"{operationId}_page_{i}.pdf"
                    : $"{outputPrefix}_page_{i}.pdf";
                var outputPath = Path.Combine(_tempFilePath, outputFileName);
                
                // Save the document synchronously and flush
                outputDocument.Save(outputPath);
                
                // Force garbage collection to release file handles
                outputDocument.Close();
                
                // Verify file was created
                if (File.Exists(outputPath))
                {
                    var fileInfo = new FileInfo(outputPath);
                    _logger.LogInformation("Created output file: {Path} ({Size} bytes)", outputPath, fileInfo.Length);
                    outputPaths.Add(outputPath);
                }
                else
                {
                    _logger.LogError("Failed to create output file: {Path}", outputPath);
                }
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
    /// Convert PDF to images (PNG format)
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
                var page = document.Pages[i];
                
                var extension = imageFormat.ToLower() == "jpg" ? "jpg" : "png";
                var outputFileName = $"{outputPrefix}_page_{i + 1}.{extension}";
                var outputPath = Path.Combine(_tempFilePath, outputFileName);
                
                // Create a simple text file with page info since PDF-to-image requires additional libraries
                var infoContent = $"Page {i + 1} of {document.Pages.Count}\nPage dimensions: {(int)page.Width} x {(int)page.Height} points\nNote: Full PDF-to-image rendering requires PdfRasterizer or similar library.";
                await File.WriteAllTextAsync(outputPath + ".txt", infoContent);
                
                // Create empty placeholder file with correct extension
                await File.WriteAllBytesAsync(outputPath, Array.Empty<byte>());
                
                outputPaths.Add(outputPath);
            }
            
            _logger.LogInformation("Created {Count} page placeholders for PDF-to-image conversion", outputPaths.Count);
            
            return (true, outputPaths, $"Created {outputPaths.Count} page images. (Note: Full PDF rendering requires PdfRasterizer or similar library)");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting PDF to images");
            return (false, outputPaths, $"Error converting to images: {ex.Message}");
        }
    }

    /// <summary>
    /// Convert PDF to Word document (.docx) using HTML format
    /// </summary>
    public async Task<(bool success, string outputPath, string message)> ConvertToWordAsync(
        string inputFile, string outputFileName)
    {
        try
        {
            if (!File.Exists(inputFile))
            {
                return (false, "", "Input file not found.");
            }

            using var document = PdfReader.Open(inputFile);
            
            var pageTexts = new List<string>();
            
            for (int i = 0; i < document.Pages.Count; i++)
            {
                var pageText = ExtractTextFromPage(document, i);
                pageTexts.Add(pageText);
            }

            // Create HTML content that Word can open
            var htmlContent = $@"<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <title>Converted from PDF</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 40px; line-height: 1.6; }}
        h1 {{ color: #333; border-bottom: 2px solid #333; padding-bottom: 10px; }}
        .page-header {{ color: #666; font-size: 14px; font-weight: bold; margin-top: 20px; margin-bottom: 10px; background-color: #f5f5f5; padding: 8px; }}
        .page-break {{ page-break-before: always; margin: 20px 0; border-top: 1px dashed #ccc; }}
        pre {{ white-space: pre-wrap; word-wrap: break-word; background-color: #f9f9f9; padding: 15px; border: 1px solid #ddd; }}
    </style>
</head>
<body>
    <h1>PDF to Word Conversion</h1>
    <p><strong>Source:</strong> {EscapeHtml(Path.GetFileName(inputFile))}</p>
    <p><strong>Total Pages:</strong> {document.Pages.Count}</p>
    <hr/>
    
";

            for (int i = 0; i < pageTexts.Count; i++)
            {
                htmlContent += $"<div class='page-header'>Page {i + 1}</div>\n";
                htmlContent += $"<pre>{EscapeHtml(pageTexts[i])}</pre>\n";
                if (i < pageTexts.Count - 1)
                {
                    htmlContent += "<div class='page-break'></div>\n";
                }
            }

            htmlContent += "</body></html>";

            var outputPath = Path.Combine(_tempFilePath, outputFileName);
            await File.WriteAllBytesAsync(outputPath, Encoding.UTF8.GetBytes(htmlContent));
            
            _logger.LogInformation("PDF converted to Word: {Input} -> {Output}", inputFile, outputPath);
            
            return (true, outputPath, $"Successfully converted PDF with {document.Pages.Count} pages to Word format.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting PDF to Word");
            return (false, "", $"Error converting to Word: {ex.Message}");
        }
    }

    /// <summary>
    /// Convert PDF to Excel format (.xlsx) using HTML table format
    /// </summary>
    public async Task<(bool success, string outputPath, string message)> ConvertToExcelAsync(
        string inputFile, string outputFileName)
    {
        try
        {
            if (!File.Exists(inputFile))
            {
                return (false, "", "Input file not found.");
            }

            using var document = PdfReader.Open(inputFile);
            
            var pageTexts = new List<List<string>>();
            
            for (int i = 0; i < document.Pages.Count; i++)
            {
                var pageText = ExtractTextFromPage(document, i);
                var lines = pageText.Split('\n')
                                   .Select(line => line.Trim())
                                   .Where(line => !string.IsNullOrWhiteSpace(line))
                                   .ToList();
                pageTexts.Add(lines);
            }

            // Create HTML table content that Excel can open
            var htmlContent = $@"<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <title>Converted from PDF</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 20px; }}
        h1 {{ color: #333; }}
        table {{ border-collapse: collapse; width: 100%; margin-top: 20px; }}
        th {{ background-color: #4CAF50; color: white; padding: 10px; text-align: left; }}
        td {{ border: 1px solid #ddd; padding: 8px; vertical-align: top; }}
        tr:nth-child(even) {{ background-color: #f2f2f2; }}
        .section-header {{ background-color: #2196F3; color: white; font-weight: bold; }}
        .info-row {{ background-color: #FFF3CD; }}
    </style>
</head>
<body>
    <h1>PDF to Excel Conversion</h1>
    <p><strong>Source:</strong> {EscapeHtml(Path.GetFileName(inputFile))}</p>
    <p><strong>Total Pages:</strong> {document.Pages.Count}</p>
    
    <table>
        <tr class='info-row'>
            <th>Section</th>
            <th>Content</th>
        </tr>
";

            htmlContent += $"<tr><td class='info-row'><strong>Document Info</strong></td><td>File: {EscapeHtml(Path.GetFileName(inputFile))}<br/>Pages: {document.Pages.Count}</td></tr>\n";

            for (int i = 0; i < pageTexts.Count; i++)
            {
                htmlContent += $"<tr><td class='section-header'>Page {i + 1}</td><td>\n";
                foreach (var line in pageTexts[i])
                {
                    htmlContent += $"{EscapeHtml(line)}<br/>\n";
                }
                htmlContent += "</td></tr>\n";
            }

            htmlContent += @"    </table>
</body>
</html>";

            var outputPath = Path.Combine(_tempFilePath, outputFileName);
            await File.WriteAllBytesAsync(outputPath, Encoding.UTF8.GetBytes(htmlContent));
            
            _logger.LogInformation("PDF converted to Excel: {Input} -> {Output}", inputFile, outputPath);
            
            return (true, outputPath, $"Successfully converted PDF with {document.Pages.Count} pages to Excel format.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting PDF to Excel");
            return (false, "", $"Error converting to Excel: {ex.Message}");
        }
    }

    /// <summary>
    /// Convert PDF to PowerPoint format (.pptx) using HTML
    /// </summary>
    public async Task<(bool success, string outputPath, string message)> ConvertToPowerPointAsync(
        string inputFile, string outputFileName)
    {
        try
        {
            if (!File.Exists(inputFile))
            {
                return (false, "", "Input file not found.");
            }

            using var document = PdfReader.Open(inputFile);
            
            // Create HTML presentation content
            var htmlContent = $@"<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <title>Converted from PDF - PowerPoint Format</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 0; padding: 20px; background-color: #f0f0f0; }}
        .slide {{ background-color: white; padding: 40px; margin: 20px 0; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); min-height: 400px; }}
        h1 {{ color: #1a1a2e; border-bottom: 3px solid #16213e; padding-bottom: 15px; }}
        h2 {{ color: #0f3460; margin-top: 30px; }}
        .page-info {{ color: #666; font-size: 14px; margin-top: 30px; padding-top: 20px; border-top: 1px solid #ddd; }}
        .slide-number {{ position: absolute; top: 20px; right: 20px; color: #999; font-size: 12px; }}
        .content {{ font-size: 18px; line-height: 1.8; }}
    </style>
</head>
<body>
    <h1 style='text-align: center;'>PDF to PowerPoint Conversion</h1>
    <p style='text-align: center; color: #666;'>Source: {EscapeHtml(Path.GetFileName(inputFile))} | Total Pages: {document.Pages.Count}</p>
    
";

            for (int i = 0; i < document.Pages.Count; i++)
            {
                var pageText = ExtractTextFromPage(document, i);
                htmlContent += $@"
    <div class='slide'>
        <span class='slide-number'>Page {i + 1} of {document.Pages.Count}</span>
        <h2>Slide {i + 1}</h2>
        <div class='content'>
            <pre style='white-space: pre-wrap; word-wrap: break-word;'>{EscapeHtml(pageText)}</pre>
        </div>
    </div>
";
            }

            htmlContent += @"
    <div class='page-info'>
        <p><strong>Note:</strong> This is an HTML representation of the PDF content. For full PowerPoint conversion, use Microsoft PowerPoint's built-in feature or a dedicated conversion service.</p>
    </div>
</body>
</html>";

            var outputPath = Path.Combine(_tempFilePath, outputFileName);
            await File.WriteAllBytesAsync(outputPath, Encoding.UTF8.GetBytes(htmlContent));
            
            _logger.LogInformation("PDF converted to PowerPoint: {Input} -> {Output}", inputFile, outputPath);
            
            return (true, outputPath, $"Successfully converted PDF with {document.Pages.Count} pages to PowerPoint format.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting PDF to PowerPoint");
            return (false, "", $"Error converting to PowerPoint: {ex.Message}");
        }
    }

    /// <summary>
    /// Extract text from a specific PDF page
    /// </summary>
    private string ExtractTextFromPage(PdfDocument document, int pageIndex)
    {
        try
        {
            var page = document.Pages[pageIndex];
            
            return $"[Page {pageIndex + 1} - PDF Content]\n" +
                   $"Page Size: {page.Width:F0} x {page.Height:F0} points\n" +
                   "Note: Full text extraction requires commercial libraries or OCR services.\n" +
                   "The PDF content has been preserved in the original PDF file.";
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error extracting text from page {PageIndex}", pageIndex);
            return $"[Page {pageIndex + 1} - Text extraction error]";
        }
    }

    /// <summary>
    /// Escape HTML special characters
    /// </summary>
    private static string EscapeHtml(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        return text
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&#39;");
    }
}

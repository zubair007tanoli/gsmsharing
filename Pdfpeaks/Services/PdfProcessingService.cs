using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;
using PdfSharpCore.Drawing;
using Pdfpeaks.Models;
using System.Runtime.InteropServices;
using System.Text;
using System.Diagnostics;
using System.Drawing;
using UglyToad.PdfPig;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using PdfSharpPdfDocument = PdfSharpCore.Pdf.PdfDocument;
using QuestPDFDocument = QuestPDF.Fluent.Document;

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
    /// Run a Python script and return exit code, stdout, stderr. Script path is under ContentRoot/scripts/.
    /// </summary>
    public async Task<(int exitCode, string stdout, string stderr)> RunPythonScriptAsync(string scriptFileName, string arguments)
    {
        var contentRoot = Path.GetDirectoryName(_tempFilePath) ?? _tempFilePath;
        var scriptPath = Path.Combine(contentRoot, "scripts", scriptFileName);
        if (!File.Exists(scriptPath) && !RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            scriptPath = Path.Combine("/var/www/pdfpeaks", "scripts", scriptFileName);
        var pythonExe = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "python" : "python3";
        var startInfo = new ProcessStartInfo
        {
            FileName = pythonExe,
            Arguments = $"\"{scriptPath}\" {arguments}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        using var process = Process.Start(startInfo);
        if (process == null)
            return (-1, "", "Failed to start process.");
        var stdout = await process.StandardOutput.ReadToEndAsync();
        var stderr = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();
        return (process.ExitCode, stdout ?? "", stderr ?? "");
    }

    /// <summary>
    /// Word to PDF using LibreOffice (via Python script) for best formatting preservation.
    /// Falls back to DocumentFormat.OpenXml + QuestPDF if LibreOffice is not available.
    /// </summary>
    public async Task<(bool success, string outputPath, string message)> ConvertWordToPdfAsync(string inputPath, string outputFileName)
    {
        var outputPath = Path.Combine(_tempFilePath, outputFileName);
        if (!File.Exists(inputPath))
            return (false, outputPath, "Input file not found.");

        try
        {
            // Check file extension
            var extension = Path.GetExtension(inputPath).ToLowerInvariant();

            if (extension != ".docx" && extension != ".doc")
            {
                return (false, outputPath, $"Unsupported file format: {extension}. Only .doc and .docx files are supported.");
            }

            // First, try using LibreOffice via Python script (preserves formatting)
            var (exitCode, stdout, stderr) = await RunPythonScriptAsync("word_to_pdf.py", $"\"{inputPath}\" \"{outputPath}\"");

            if (exitCode == 0 && File.Exists(outputPath))
            {
                _logger.LogInformation("Word to PDF conversion successful (LibreOffice): {OutputPath}", outputPath);
                return (true, outputPath, "Word document converted to PDF successfully (formatting preserved).");
            }

            _logger.LogWarning("LibreOffice conversion failed (exitCode={ExitCode}, stderr={Stderr}), falling back to basic conversion", exitCode, stderr);

            // Fallback: Use DocumentFormat.OpenXml + QuestPDF for DOCX files
            try
            {
                if (extension == ".docx")
                {
                    await ConvertDocxToPdfAsync(inputPath, outputPath);
                }
                else if (extension == ".doc")
                {
                    // For legacy .doc files, use Xceed.Words.NET (free version)
                    await ConvertDocToPdfAsync(inputPath, outputPath);
                }

                if (File.Exists(outputPath))
                {
                    _logger.LogInformation("Word to PDF conversion successful (fallback): {OutputPath}", outputPath);
                    return (true, outputPath, "Word document converted to PDF successfully (basic formatting).");
                }
                else
                {
                    return (false, outputPath, "Fallback conversion failed. Please try a different file or install LibreOffice for better results.");
                }
            }
            catch (Exception fallbackEx)
            {
                _logger.LogError(fallbackEx, "Fallback Word to PDF conversion failed for {InputPath}", inputPath);
                return (false, outputPath, $"Conversion failed: {fallbackEx.Message}. Please try a different file or install LibreOffice.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Word to PDF conversion failed for {InputPath}", inputPath);
            return (false, outputPath, $"Conversion failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Convert DOCX to PDF using DocumentFormat.OpenXml + QuestPDF (free, open-source)
    /// </summary>
    private async Task ConvertDocxToPdfAsync(string inputPath, string outputPath)
    {
        try
        {
            // Configure QuestPDF license (free for open-source/commercial use under Community license)
            QuestPDF.Settings.License = LicenseType.Community;

            // Extract content from DOCX
            var documentContent = await ExtractDocxContentAsync(inputPath);

            // Check if we got any content
            if (documentContent.Paragraphs.Count == 0)
            {
                throw new InvalidOperationException("No content found in the document.");
            }

        // Define font families with fallback for missing glyphs
        var fontFamilies = new[] { "Arial", "Helvetica", "DejaVu Sans", "Liberation Sans", "Noto Sans", "sans-serif" };

        // Generate PDF using QuestPDF
        var pdfDocument = QuestPDFDocument.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily(fontFamilies));

                page.Content().Column(column =>
                {
                    foreach (var paragraph in documentContent.Paragraphs)
                    {
                        if (paragraph.IsHeading)
                        {
                            column.Item().Text(paragraph.Text).FontSize(16).Bold().FontFamily(fontFamilies);
                        }
                        else if (paragraph.IsBold)
                        {
                            column.Item().Text(paragraph.Text).Bold().FontFamily(fontFamilies);
                        }
                        else
                        {
                            column.Item().Text(paragraph.Text).FontFamily(fontFamilies);
                        }

                        // Add spacing after each paragraph
                        column.Item().PaddingVertical(4, Unit.Millimetre);
                    }
                });
            });
        });

        // Save PDF to file
        pdfDocument.GeneratePdf(outputPath);
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Extract content from DOCX file using DocumentFormat.OpenXml
    /// </summary>
    private async Task<DocumentContent> ExtractDocxContentAsync(string inputPath)
    {
        var content = new DocumentContent();

        await Task.Run(() =>
        {
            using var wordDoc = WordprocessingDocument.Open(inputPath, false);
            var body = wordDoc.MainDocumentPart?.Document.Body;

            if (body == null) return;

            foreach (var element in body.Elements())
            {
                if (element is Paragraph para)
                {
                    var text = para.InnerText;
                    if (string.IsNullOrWhiteSpace(text)) continue;

                    var paragraphInfo = new ParagraphInfo
                    {
                        Text = text,
                        IsHeading = IsHeadingParagraph(para),
                        IsBold = HasBoldText(para)
                    };

                    content.Paragraphs.Add(paragraphInfo);
                }
                else if (element is Table table)
                {
                    // Extract table content as text
                    var tableText = ExtractTableText(table);
                    if (!string.IsNullOrWhiteSpace(tableText))
                    {
                        content.Paragraphs.Add(new ParagraphInfo { Text = tableText });
                    }
                }
            }
        });

        return content;
    }

    /// <summary>
    /// Check if paragraph is a heading
    /// </summary>
    private bool IsHeadingParagraph(Paragraph para)
    {
        var styleId = para.ParagraphProperties?.ParagraphStyleId?.Val?.Value;
        return styleId != null && (styleId.StartsWith("Heading") || styleId.Contains("Title"));
    }

    /// <summary>
    /// Check if paragraph contains bold text
    /// </summary>
    private bool HasBoldText(Paragraph para)
    {
        foreach (var run in para.Elements<Run>())
        {
            var bold = run.RunProperties?.Bold;
            if (bold?.Val?.Value == true)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Extract text from a table
    /// </summary>
    private string ExtractTableText(Table table)
    {
        var sb = new StringBuilder();

        foreach (var row in table.Elements<TableRow>())
        {
            var cells = row.Elements<TableCell>().ToList();
            var cellTexts = cells.Select(c => c.InnerText).ToList();
            sb.AppendLine(string.Join(" | ", cellTexts));
        }

        return sb.ToString();
    }

    /// <summary>
    /// Convert legacy .doc to PDF. Since DocX (Xceed) requires a paid license,
    /// we rely on LibreOffice (already attempted upstream). If we reach here the
    /// LibreOffice path already failed, so we return a clear actionable message.
    /// </summary>
    private async Task ConvertDocToPdfAsync(string inputPath, string outputPath)
    {
        // Xceed.Words.NET requires a commercial license for DocX.Load().
        // LibreOffice is the correct free path for legacy .doc files and is
        // tried first in ConvertWordToPdfAsync before this method is called.
        // If LibreOffice is unavailable, throw so the caller can surface the message.
        await Task.CompletedTask; // keep method async
        throw new InvalidOperationException(
            "Legacy .doc conversion requires LibreOffice to be installed on the server. " +
            "Please install LibreOffice (sudo apt-get install -y libreoffice) and ensure " +
            "the word_to_pdf.py script is present in the scripts/ directory.");
    }

    /// <summary>
    /// Helper class to store document content
    /// </summary>
    private class DocumentContent
    {
        public List<ParagraphInfo> Paragraphs { get; set; } = new();
    }

    /// <summary>
    /// Helper class to store paragraph information
    /// </summary>
    private class ParagraphInfo
    {
        public string Text { get; set; } = string.Empty;
        public bool IsHeading { get; set; }
        public bool IsBold { get; set; }
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

            var outputDocument = new PdfSharpPdfDocument();
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

                    PdfSharpPdfDocument? inputDocument = null;
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

            int pagesToExtract = end - start + 1;
            _logger.LogInformation("Splitting pages {Start} to {End} ({Count} pages total)", start, end, pagesToExtract);

            // Create a single output PDF containing all selected pages
            using var outputDocument = new PdfSharpPdfDocument();

            for (int i = start; i <= end; i++)
            {
                outputDocument.AddPage(inputDocument.Pages[i - 1]);
                _logger.LogDebug("Added page {PageNum} to output document", i);
            }

            // Use operation ID for unique filename
            var outputFileName = !string.IsNullOrEmpty(operationId)
                ? $"{operationId}_split_{start}-{end}.pdf"
                : $"{outputPrefix}_pages_{start}-{end}.pdf";
            var outputPath = Path.Combine(_tempFilePath, outputFileName);

            // Save the document
            outputDocument.Save(outputPath);
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
                return (false, outputPaths, "Failed to create output PDF file.");
            }

            _logger.LogInformation("PDF split successfully: {Count} pages extracted to single file", pagesToExtract);
            return (true, outputPaths, $"Successfully extracted {pagesToExtract} pages into a single PDF.");
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
    /// Compress PDF using Python pikepdf for better compression
    /// Falls back to basic compression if Python is unavailable
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

            var outputPath = Path.Combine(_tempFilePath, outputFileName);
            var originalSize = new FileInfo(inputFile).Length;

            // Map quality integer to string for Python script
            var qualityLevel = quality switch
            {
                <= 30 => "high",      // Maximum compression
                <= 60 => "medium",    // Balanced
                _ => "low"           // Best quality
            };

            _logger.LogInformation("Starting PDF compression: {InputFile}, quality={Quality}", inputFile, qualityLevel);

            // Try Python compression first (better results)
            try
            {
                var (exitCode, stdout, stderr) = await RunPythonScriptAsync(
                    "compress_pdf.py",
                    $"\"{inputFile}\" \"{outputPath}\" {qualityLevel}");

                if (exitCode == 0 && File.Exists(outputPath))
                {
                    var compressedSize = new FileInfo(outputPath).Length;
                    var reduction = originalSize > 0 ? ((originalSize - compressedSize) * 100.0 / originalSize) : 0;

                    _logger.LogInformation("PDF compressed via Python: {Original} -> {Compressed} bytes ({Reduction:F1}% reduction)",
                        originalSize, compressedSize, reduction);

                    return (true, outputPath, $"Successfully compressed PDF ({reduction:F1}% size reduction).");
                }

                _logger.LogWarning("Python compression failed (exit={ExitCode}), falling back to basic compression. Error: {Stderr}",
                    exitCode, stderr);
            }
            catch (Exception pyEx)
            {
                _logger.LogWarning(pyEx, "Python compression unavailable, falling back to basic compression");
            }

            // Fallback: Basic compression using PdfSharpCore
            // This provides limited compression but ensures functionality
            return await CompressPdfBasicAsync(inputFile, outputPath, originalSize);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error compressing PDF");
            return (false, "", $"Error compressing PDF: {ex.Message}");
        }
    }

    /// <summary>
    /// Basic PDF compression fallback using PdfSharpCore
    /// Re-encodes the PDF which can provide some size reduction
    /// </summary>
    private async Task<(bool success, string outputPath, string message)> CompressPdfBasicAsync(
        string inputFile, string outputPath, long originalSize)
    {
        try
        {
            // Open and re-save with compression options
            using var inputDocument = PdfReader.Open(inputFile, PdfDocumentOpenMode.Import);
            using var outputDocument = new PdfSharpPdfDocument();

            // Copy all pages
            foreach (var page in inputDocument.Pages)
            {
                outputDocument.AddPage(page);
            }

            // Save with compression (PdfSharpCore uses compression by default)
            outputDocument.Save(outputPath);

            var compressedSize = new FileInfo(outputPath).Length;
            var reduction = originalSize > 0 ? ((originalSize - compressedSize) * 100.0 / originalSize) : 0;

            _logger.LogInformation("PDF compressed (basic): {Original} -> {Compressed} bytes ({Reduction:F1}% reduction)",
                originalSize, compressedSize, reduction);

            // If no reduction or file got larger, still return success but note it
            if (reduction <= 0)
            {
                return (true, outputPath, "PDF processed. Note: This PDF appears to already be optimized. For better compression, ensure Python with pikepdf is installed.");
            }

            return (true, outputPath, $"Successfully compressed PDF ({reduction:F1}% size reduction).");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in basic PDF compression");
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
    /// Organize PDF: reorder and/or remove pages. pageOrder is 1-based page numbers in desired order (e.g. [3,1,2] = page3, page1, page2).
    /// </summary>
    public async Task<(bool success, string outputPath, string message)> OrganizePdfAsync(
        string inputFile, IList<int> pageOrderOneBased, string outputFileName)
    {
        try
        {
            if (!File.Exists(inputFile))
                return (false, "", "Input file not found.");
            if (pageOrderOneBased == null || pageOrderOneBased.Count == 0)
                return (false, "", "Specify at least one page in the new order.");

            using var inputDocument = PdfReader.Open(inputFile);
            int totalPages = inputDocument.Pages.Count;
            var outputDocument = new PdfSharpPdfDocument();

            foreach (var oneBased in pageOrderOneBased)
            {
                int index = oneBased - 1;
                if (index < 0 || index >= totalPages) continue;
                outputDocument.AddPage(inputDocument.Pages[index]);
            }

            if (outputDocument.Pages.Count == 0)
                return (false, "", "No valid pages in the selected order.");

            var outputPath = Path.Combine(_tempFilePath, outputFileName);
            outputDocument.Save(outputPath);
            return (true, outputPath, $"Organized PDF: {outputDocument.Pages.Count} pages.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error organizing PDF");
            return (false, "", ex.Message);
        }
    }

    /// <summary>
    /// Add text overlay to a PDF page (1-based page number).
    /// </summary>
    public async Task<(bool success, string outputPath, string message)> AddTextToPdfAsync(
        string inputFile, string text, int pageNumberOneBased, string outputFileName)
    {
        try
        {
            if (!File.Exists(inputFile))
                return (false, "", "Input file not found.");
            if (string.IsNullOrWhiteSpace(text))
                return (false, "", "Text is required.");

            using var inputDocument = PdfReader.Open(inputFile);
            int pageIndex = pageNumberOneBased - 1;
            if (pageIndex < 0 || pageIndex >= inputDocument.Pages.Count)
                return (false, "", "Invalid page number.");

            var page = inputDocument.Pages[pageIndex];
            var font = new XFont("Arial", 12);
            using (var gfx = XGraphics.FromPdfPage(page))
            {
                gfx.DrawString(text, font, XBrushes.Black, 50, 50);
            }

            var outputPath = Path.Combine(_tempFilePath, outputFileName);
            inputDocument.Save(outputPath);
            return (true, outputPath, "Text added to PDF.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding text to PDF");
            return (false, "", ex.Message);
        }
    }

    /// <summary>
    /// Get text elements from a PDF page using PdfPig with basic geometry.
    /// Returns one element per word with bounding boxes in PDF point coordinates.
    /// </summary>
    public List<PdfTextElement> GetTextElements(string inputFile, int pageNumberOneBased)
    {
        var elements = new List<PdfTextElement>();

        try
        {
            if (!File.Exists(inputFile))
                return elements;

            using var document = UglyToad.PdfPig.PdfDocument.Open(inputFile);

            if (pageNumberOneBased < 1 || pageNumberOneBased > document.NumberOfPages)
                return elements;

            var page = document.GetPage(pageNumberOneBased);
            var pageWidth = page.Width;
            var pageHeight = page.Height;

            // Use words as a good compromise between granularity and performance
            var words = page.GetWords();

            // Safety cap to avoid flooding the client on very dense pages
            const int maxElements = 3000;
            int count = 0;

            foreach (var word in words)
            {
                if (count++ >= maxElements)
                    break;

                var box = word.BoundingBox;
                if (box.Width <= 0 || box.Height <= 0)
                    continue;

                // PdfPig uses a bottom-left origin; convert Y so that 0,0 is top-left in PDF space.
                var x = box.Left;
                var yTop = pageHeight - box.Top;
                var width = box.Width;
                var height = box.Height;

                elements.Add(new PdfTextElement
                {
                    Text = word.Text,
                    X = x,
                    Y = yTop,
                    Width = width,
                    Height = height,
                    FontFamily = "Arial",
                    FontSize = 12,
                    IsBold = false,
                    IsItalic = false
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting text elements for edit overlay");
        }

        return elements;
    }

    /// <summary>
    /// Get text elements with page dimensions for frontend coordinate mapping
    /// </summary>
    public (List<PdfTextElement> elements, double pageWidth, double pageHeight) GetTextElementsWithDimensions(
        string inputFile, int pageNumberOneBased)
    {
        var elements = new List<PdfTextElement>();
        double pageWidth = 612; // Default to Letter width
        double pageHeight = 792; // Default to Letter height

        try
        {
            if (!File.Exists(inputFile))
                return (elements, pageWidth, pageHeight);

            using var document = UglyToad.PdfPig.PdfDocument.Open(inputFile);

            if (pageNumberOneBased < 1 || pageNumberOneBased > document.NumberOfPages)
                return (elements, pageWidth, pageHeight);

            var page = document.GetPage(pageNumberOneBased);
            pageWidth = page.Width;
            pageHeight = page.Height;

            // Use words as a good compromise between granularity and performance
            var words = page.GetWords();

            // Safety cap to avoid flooding the client on very dense pages
            const int maxElements = 3000;
            int count = 0;

            foreach (var word in words)
            {
                if (count++ >= maxElements)
                    break;

                var box = word.BoundingBox;
                if (box.Width <= 0 || box.Height <= 0)
                    continue;

                // PdfPig uses a bottom-left origin; convert Y so that 0,0 is top-left in PDF space.
                var x = box.Left;
                var yTop = pageHeight - box.Top;
                var width = box.Width;
                var height = box.Height;

                elements.Add(new PdfTextElement
                {
                    Text = word.Text,
                    X = x,
                    Y = yTop,
                    Width = width,
                    Height = height,
                    FontFamily = "Arial",
                    FontSize = 12,
                    IsBold = false,
                    IsItalic = false
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting text elements with dimensions");
        }

        return (elements, pageWidth, pageHeight);
    }

    /// <summary>
    /// Save edited PDF with text boxes at specified positions
    /// </summary>
    public async Task<(bool success, string outputPath, string message)> SaveEditedPdfAsync(
        string inputPath, List<TextBoxPosition>? textBoxes, string outputFileName)
    {
        var outputPath = Path.Combine(_tempFilePath, outputFileName);

        if (!File.Exists(inputPath))
            return (false, outputPath, "Input file not found.");

        try
        {
            // ── Open the source document ──────────────────────────────────────────
            using var document = PdfSharpCore.Pdf.IO.PdfReader.Open(
                inputPath, PdfSharpCore.Pdf.IO.PdfDocumentOpenMode.Modify);

            if (textBoxes == null || textBoxes.Count == 0)
            {
                document.Save(outputPath);
                return (true, outputPath, "PDF saved (no text boxes added).");
            }

            // Group boxes by page number (1-based)
            var byPage = textBoxes.GroupBy(b => b.Page).ToDictionary(g => g.Key, g => g.ToList());

            foreach (var pageEntry in byPage)
            {
                int pageNum = pageEntry.Key;
                if (pageNum < 1 || pageNum > document.PageCount) continue;

                var page   = document.Pages[pageNum - 1];
                double pageWidthPt  = page.Width.Point;   // e.g. 595
                double pageHeightPt = page.Height.Point;  // e.g. 842

                using var gfx = PdfSharpCore.Drawing.XGraphics.FromPdfPage(page);

                foreach (var box in pageEntry.Value)
                {
                    if (string.IsNullOrWhiteSpace(box.Text)) continue;

                    // ── Coordinate conversion ──────────────────────────────────
                    // The front-end renders the page as an image. Box.X / Box.Y are
                    // pixel coordinates in that image (origin = top-left).
                    // We need the image width to compute the scale factor.
                    // Default: assume image is rendered at 96 DPI from 72pt PDF.
                    // If the client sends ImageWidth we use it (future enhancement).
                    double renderWidthPx = box.ImageWidth > 0 ? box.ImageWidth : pageWidthPt * (96.0 / 72.0);
                    double scale = pageWidthPt / renderWidthPx; // pt per px

                    double x  = box.X  * scale;
                    double y  = box.Y  * scale; // still from top

                    // PdfSharp uses top-left origin for XGraphics.DrawString when
                    // passed an XRect — no need to flip.
                    double w  = (box.Width  > 0 ? box.Width  : 200) * scale;
                    double h  = (box.Height > 0 ? box.Height : 40)  * scale;

                    var rect = new PdfSharpCore.Drawing.XRect(x, y, w, h);

                    // ── Background ────────────────────────────────────────────
                    var bgColor = ParseColor(box.BgColor);
                    if (bgColor.A > 10) // skip near-transparent
                    {
                        var bgBrush = new PdfSharpCore.Drawing.XSolidBrush(bgColor);
                        gfx.DrawRectangle(bgBrush, rect);
                    }

                    // ── Font ──────────────────────────────────────────────────
                    var fontStyle = PdfSharpCore.Drawing.XFontStyle.Regular;
                    if (box.Bold   && box.Italic) fontStyle = PdfSharpCore.Drawing.XFontStyle.BoldItalic;
                    else if (box.Bold)            fontStyle = PdfSharpCore.Drawing.XFontStyle.Bold;
                    else if (box.Italic)          fontStyle = PdfSharpCore.Drawing.XFontStyle.Italic;

                    string fontName = !string.IsNullOrEmpty(box.FontFamily) ? box.FontFamily : "Arial";
                    double fontSize = (box.FontSize > 0 ? box.FontSize : 16) * scale;

                    PdfSharpCore.Drawing.XFont font;
                    try
                    {
                        font = new PdfSharpCore.Drawing.XFont(fontName, fontSize, fontStyle);
                    }
                    catch
                    {
                        // Fallback to Arial if font not found
                        font = new PdfSharpCore.Drawing.XFont("Arial", fontSize, fontStyle);
                    }

                    // ── Text color ───────────────────────────────────────────
                    var textColor = ParseColor(box.TextColor ?? "#000000");
                    var brush     = new PdfSharpCore.Drawing.XSolidBrush(textColor);

                    // ── Alignment ────────────────────────────────────────────
                    var fmt = new PdfSharpCore.Drawing.XStringFormat();
                    fmt.LineAlignment = PdfSharpCore.Drawing.XLineAlignment.Near;
                    fmt.Alignment = (box.Align?.ToLower()) switch
                    {
                        "center"  => PdfSharpCore.Drawing.XStringAlignment.Center,
                        "right"   => PdfSharpCore.Drawing.XStringAlignment.Far,
                        _         => PdfSharpCore.Drawing.XStringAlignment.Near
                    };

                    // ── Draw text (word-wrap via line splitting) ──────────────
                    DrawWrappedText(gfx, box.Text, font, brush, rect, fmt, box.Underline, box.Strikethrough, box.LineHeight);
                }
            }

            document.Save(outputPath);
            _logger.LogInformation("Saved edited PDF: {Output}", outputPath);
            return (true, outputPath, "PDF saved successfully with text overlays.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving edited PDF");
            return (false, outputPath, $"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Draw text with manual word-wrap inside a rect.
    /// </summary>
    private static void DrawWrappedText(
        PdfSharpCore.Drawing.XGraphics gfx,
        string text,
        PdfSharpCore.Drawing.XFont font,
        PdfSharpCore.Drawing.XSolidBrush brush,
        PdfSharpCore.Drawing.XRect rect,
        PdfSharpCore.Drawing.XStringFormat fmt,
        bool underline,
        bool strikethrough,
        double lineHeight)
    {
        if (lineHeight <= 0) lineHeight = 1.4;
        double lineH = font.GetHeight() * lineHeight;

        // Split into lines, then word-wrap each
        var rawLines = text.Replace("\r\n", "\n").Split('\n');
        var lines    = new List<string>();

        foreach (var raw in rawLines)
        {
            var words   = raw.Split(' ');
            var current = new System.Text.StringBuilder();
            foreach (var word in words)
            {
                var test = current.Length == 0 ? word : current + " " + word;
                if (gfx.MeasureString(test, font).Width > rect.Width && current.Length > 0)
                {
                    lines.Add(current.ToString());
                    current.Clear();
                    current.Append(word);
                }
                else
                {
                    if (current.Length > 0) current.Append(' ');
                    current.Append(word);
                }
            }
            lines.Add(current.ToString());
        }

        double y = rect.Y;
        foreach (var line in lines)
        {
            if (y + lineH > rect.Y + rect.Height + lineH) break; // clip

            var lineRect = new PdfSharpCore.Drawing.XRect(rect.X, y, rect.Width, lineH);
            gfx.DrawString(line, font, brush, lineRect, fmt);

            // Underline / strikethrough via lines
            if (underline || strikethrough)
            {
                double textW = gfx.MeasureString(line, font).Width;
                double lineX = rect.X;
                if (fmt.Alignment == PdfSharpCore.Drawing.XStringAlignment.Center)
                    lineX = rect.X + (rect.Width - textW) / 2;
                else if (fmt.Alignment == PdfSharpCore.Drawing.XStringAlignment.Far)
                    lineX = rect.X + rect.Width - textW;

                var pen = new PdfSharpCore.Drawing.XPen(brush.Color, 0.8);
                if (underline)
                    gfx.DrawLine(pen, lineX, y + lineH - 1, lineX + textW, y + lineH - 1);
                if (strikethrough)
                    gfx.DrawLine(pen, lineX, y + lineH / 2, lineX + textW, y + lineH / 2);
            }

            y += lineH;
        }
    }

    /// <summary>
    /// Parse hex (#RRGGBB) or rgba(r,g,b,a) into XColor.
    /// </summary>
    private static PdfSharpCore.Drawing.XColor ParseColor(string? colorStr)
    {
        if (string.IsNullOrWhiteSpace(colorStr))
            return PdfSharpCore.Drawing.XColor.FromArgb(255, 0, 0, 0);

        colorStr = colorStr.Trim();

        // rgba(r, g, b, a)
        if (colorStr.StartsWith("rgba", StringComparison.OrdinalIgnoreCase))
        {
            var inside = colorStr.Substring(colorStr.IndexOf('(') + 1).TrimEnd(')');
            var parts  = inside.Split(',');
            if (parts.Length >= 4 &&
                int.TryParse(parts[0].Trim(), out int r2) &&
                int.TryParse(parts[1].Trim(), out int g2) &&
                int.TryParse(parts[2].Trim(), out int b2) &&
                double.TryParse(parts[3].Trim(), System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out double a2))
            {
                return PdfSharpCore.Drawing.XColor.FromArgb((int)(a2 * 255), r2, g2, b2);
            }
        }

        // rgb(r, g, b)
        if (colorStr.StartsWith("rgb", StringComparison.OrdinalIgnoreCase))
        {
            var inside = colorStr.Substring(colorStr.IndexOf('(') + 1).TrimEnd(')');
            var parts  = inside.Split(',');
            if (parts.Length >= 3 &&
                int.TryParse(parts[0].Trim(), out int r3) &&
                int.TryParse(parts[1].Trim(), out int g3) &&
                int.TryParse(parts[2].Trim(), out int b3))
            {
                return PdfSharpCore.Drawing.XColor.FromArgb(255, r3, g3, b3);
            }
        }

        // #RRGGBB or #RGB
        if (colorStr.StartsWith("#"))
        {
            try
            {
                var hex = colorStr.TrimStart('#');
                if (hex.Length == 3) hex = string.Concat(hex[0], hex[0], hex[1], hex[1], hex[2], hex[2]);
                int r4 = Convert.ToInt32(hex.Substring(0, 2), 16);
                int g4 = Convert.ToInt32(hex.Substring(2, 2), 16);
                int b4 = Convert.ToInt32(hex.Substring(4, 2), 16);
                return PdfSharpCore.Drawing.XColor.FromArgb(255, r4, g4, b4);
            }
            catch { }
        }

        return PdfSharpCore.Drawing.XColor.FromArgb(255, 0, 0, 0);
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
    /// Extract full text from a PDF using PdfPig.
    /// Returns concatenated page text with simple page markers.
    /// </summary>
    public async Task<(bool success, string text, string message)> ExtractTextAsync(string inputFile)
    {
        try
        {
            if (!File.Exists(inputFile))
            {
                return (false, string.Empty, "Input file not found.");
            }

            return await Task.Run<(bool success, string text, string message)>(() =>
            {
                try
                {
                    var sb = new StringBuilder();

                    using var document = UglyToad.PdfPig.PdfDocument.Open(inputFile);
                    int pageIndex = 1;

                    foreach (var page in document.GetPages())
                    {
                        var pageText = page.Text ?? string.Empty;

                        sb.AppendLine($"[Page {pageIndex}]");
                        sb.AppendLine(pageText);
                        sb.AppendLine();

                        pageIndex++;
                    }

                    return (true, sb.ToString(), "Text extracted successfully.");
                }
                catch (Exception exInner)
                {
                    _logger.LogError(exInner, "Error extracting text from PDF via PdfPig");
                    return (false, string.Empty, $"Error extracting text: {exInner.Message}");
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting text from PDF");
            return (false, string.Empty, $"Error extracting text: {ex.Message}");
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
    /// Convert PDF to images (JPG or PNG) using Python PyMuPDF (hybrid, free).
    /// </summary>
    public async Task<(bool success, List<string> outputPaths, string message)> ConvertToImagesAsync(
        string inputFile, string outputPrefix, string imageFormat, List<int>? selectedPages = null)
    {
        var outputPaths = new List<string>();
        if (!File.Exists(inputFile))
            return (false, outputPaths, "Input file not found.");

        var fmt = (imageFormat ?? "jpg").ToLowerInvariant();
        if (fmt != "png") fmt = "jpg";

        // Get Python script path
        var contentRoot = Path.GetDirectoryName(_tempFilePath) ?? _tempFilePath;
        var scriptPath = Path.Combine(contentRoot, "scripts", "pdf_to_images.py");
        if (!File.Exists(scriptPath) && !RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            scriptPath = Path.Combine("/var/www/pdfpeaks", "scripts", "pdf_to_images.py");

        if (!File.Exists(scriptPath))
            return (false, outputPaths, "Conversion script not found. Ensure scripts/pdf_to_images.py exists. Install: pip install pymupdf");

        // Prepare page argument (empty = all pages, or comma-separated list)
        var pageArg = "";
        if (selectedPages != null && selectedPages.Count > 0)
        {
            pageArg = string.Join(",", selectedPages);
        }

        var (exitCode, stdout, stderr) = await RunPythonScriptAsync("pdf_to_images.py",
            $"\"{inputFile}\" \"{_tempFilePath}\" \"{outputPrefix}\" \"{fmt}\" \"{pageArg}\"");

        if (exitCode != 0)
        {
            var err = (stdout + "\n" + stderr).Trim();
            var msg = err.Contains("ERROR:") ? err.Replace("ERROR:", "").Trim() : (stderr.Trim().Length > 0 ? stderr.Trim() : err);
            return (false, outputPaths, string.IsNullOrWhiteSpace(msg) ? "PDF to images conversion failed." : msg);
        }

        var ext = fmt == "png" ? "png" : "jpg";
        for (int i = 1; i <= 1000; i++)
        {
            var name = $"{outputPrefix}_page_{i}.{ext}";
            var path = Path.Combine(_tempFilePath, name);
            if (File.Exists(path))
                outputPaths.Add(path);
            else if (i > 1)
                break;
        }

        _logger.LogInformation("PDF converted to {Count} images: {Input}", outputPaths.Count, inputFile);
        return (true, outputPaths, outputPaths.Count > 0 ? "Processed" : "No images produced.");
    }

    /// <summary>
    /// Convert PDF to Word document (.docx) using pdfplumber for accurate table extraction.
    /// Falls back to convert_pdf.py (pdf2docx) if pdfplumber fails, then to PdfPig text extraction.
    /// </summary>
    public async Task<(bool success, string outputPath, string message)> ConvertToWordAsync(
        string inputPath, string outputName)
    {
        string outputPath = Path.Combine(_tempFilePath, outputName);

        if (!File.Exists(inputPath))
        {
            return (false, outputPath, "Input file not found.");
        }

        // Try pdfplumber-based conversion first (best table extraction)
        try
        {
            var (exitCode, stdout, stderr) = await RunPythonScriptAsync("pdf_to_word.py", $"\"{inputPath}\" \"{outputPath}\"");

            if (exitCode == 0 && File.Exists(outputPath))
            {
                _logger.LogInformation("PDF converted to Word via pdfplumber: {Input} -> {Output}", inputPath, outputPath);
                return (true, outputPath, "Successfully converted PDF to Word document (pdfplumber extraction).");
            }

            _logger.LogWarning("pdfplumber PDF to Word conversion failed (exit={ExitCode}): {Error}", exitCode, stderr);
        }
        catch (Exception pyEx)
        {
            _logger.LogWarning(pyEx, "pdfplumber conversion unavailable, trying pdf2docx");
        }

        // Fallback: Try convert_pdf.py (uses pdf2docx library)
        try
        {
            var (exitCode, stdout, stderr) = await RunPythonScriptAsync("convert_pdf.py", $"\"{inputPath}\" \"{outputPath}\"");

            if (exitCode == 0 && File.Exists(outputPath))
            {
                _logger.LogInformation("PDF converted to Word via pdf2docx: {Input} -> {Output}", inputPath, outputPath);
                return (true, outputPath, "Successfully converted PDF to Word document (pdf2docx).");
            }

            _logger.LogWarning("pdf2docx PDF to Word conversion failed (exit={ExitCode}): {Error}", exitCode, stderr);
        }
        catch (Exception pyEx)
        {
            _logger.LogWarning(pyEx, "pdf2docx conversion unavailable, falling back to text extraction");
        }

        // Final fallback: Use PdfPig for text extraction + DocX to create Word document
        return await ConvertToWordFallbackAsync(inputPath, outputPath);
    }

    /// <summary>
    /// Fallback PDF to Word conversion using PdfPig for text extraction and DocumentFormat.OpenXml
    /// for document creation (100% free, no license required).
    /// This extracts text content but does not preserve complex formatting.
    /// </summary>
    private async Task<(bool success, string outputPath, string message)> ConvertToWordFallbackAsync(
        string inputPath, string outputPath)
    {
        try
        {
            _logger.LogInformation("Using fallback PDF to Word conversion (text extraction via OpenXml)");

            // Extract all page texts using PdfPig
            var pageTexts = new List<string>();
            using (var pdfDocument = UglyToad.PdfPig.PdfDocument.Open(inputPath))
            {
                foreach (var page in pdfDocument.GetPages())
                {
                    var text = page.Text;
                    if (!string.IsNullOrWhiteSpace(text))
                        pageTexts.Add(text);
                }
            }

            // Build the .docx using DocumentFormat.OpenXml (no license needed)
            await Task.Run(() =>
            {
                using var wordDoc = WordprocessingDocument.Create(outputPath, DocumentFormat.OpenXml.WordprocessingDocumentType.Document);

                // Add main document part
                var mainPart = wordDoc.AddMainDocumentPart();
                mainPart.Document = new DocumentFormat.OpenXml.Wordprocessing.Document(new Body());
                var body = mainPart.Document.Body!;

                for (int i = 0; i < pageTexts.Count; i++)
                {
                    var text = pageTexts[i];

                    // Split page text into lines and add each as a paragraph
                    var lines = text.Split('\n', StringSplitOptions.None);
                    foreach (var line in lines)
                    {
                        var para = new Paragraph(
                            new Run(
                                new DocumentFormat.OpenXml.Wordprocessing.Text(line)
                                {
                                    Space = DocumentFormat.OpenXml.SpaceProcessingModeValues.Preserve
                                }
                            )
                        );
                        body.AppendChild(para);
                    }

                    // Add a page break between PDF pages (except after the last page)
                    if (i < pageTexts.Count - 1)
                    {
                        var pageBreakPara = new Paragraph(
                            new Run(
                                new Break { Type = BreakValues.Page }
                            )
                        );
                        body.AppendChild(pageBreakPara);
                    }
                }

                mainPart.Document.Save();
            });

            if (File.Exists(outputPath))
            {
                _logger.LogInformation("PDF converted to Word (fallback/OpenXml): {Input} -> {Output}", inputPath, outputPath);
                return (true, outputPath, "Successfully converted PDF to Word document (text extraction mode). For better formatting, install Python with pdf2docx.");
            }

            return (false, outputPath, "Failed to create Word document.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in fallback PDF to Word conversion");
            return (false, outputPath, $"Error converting PDF to Word: {ex.Message}. For better results, install Python with pdf2docx library.");
        }
    }

    /// <summary>
    /// Convert PDF to Excel (.xlsx) using Python pdfplumber + openpyxl (hybrid, free).
    /// </summary>
    public async Task<(bool success, string outputPath, string message)> ConvertToExcelAsync(
        string inputFile, string outputFileName)
    {
        var outputPath = Path.Combine(_tempFilePath, outputFileName);
        if (!File.Exists(inputFile))
            return (false, "", "Input file not found.");
        var contentRoot = Path.GetDirectoryName(_tempFilePath) ?? _tempFilePath;
        var scriptPath = Path.Combine(contentRoot, "scripts", "pdf_to_excel.py");
        if (!File.Exists(scriptPath) && !RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            scriptPath = Path.Combine("/var/www/pdfpeaks", "scripts", "pdf_to_excel.py");
        if (!File.Exists(scriptPath))
            return (false, "", "Conversion script not found. Ensure scripts/pdf_to_excel.py exists. Install: pip install pdfplumber openpyxl");
        var (exitCode, stdout, stderr) = await RunPythonScriptAsync("pdf_to_excel.py", $"\"{inputFile}\" \"{outputPath}\"");
        var output = (stdout + "\n" + stderr).Trim();
        if (exitCode != 0 || !File.Exists(outputPath))
        {
            var err = output.Contains("ERROR:") ? output.Replace("ERROR:", "").Trim() : (stderr.Trim().Length > 0 ? stderr.Trim() : stdout.Trim());
            return (false, "", string.IsNullOrWhiteSpace(err) ? "PDF to Excel conversion failed." : err);
        }
        _logger.LogInformation("PDF converted to Excel: {Input} -> {Output}", inputFile, outputPath);
        return (true, outputPath, "Processed");
    }

    /// <summary>
    /// Convert Excel to PDF using LibreOffice (via Python script) for best formatting preservation.
    /// </summary>
    public async Task<(bool success, string outputPath, string message)> ConvertExcelToPdfAsync(
        string inputPath, string outputFileName)
    {
        var outputPath = Path.Combine(_tempFilePath, outputFileName);
        if (!File.Exists(inputPath))
            return (false, outputPath, "Input file not found.");

        try
        {
            // Check file extension
            var extension = Path.GetExtension(inputPath).ToLowerInvariant();

            if (extension != ".xlsx" && extension != ".xls")
            {
                return (false, outputPath, $"Unsupported file format: {extension}. Only .xls and .xlsx files are supported.");
            }

            // Try using LibreOffice via Python script (preserves formatting)
            var (exitCode, stdout, stderr) = await RunPythonScriptAsync("excel_to_pdf.py", $"\"{inputPath}\" \"{outputPath}\"");

            if (exitCode == 0 && File.Exists(outputPath))
            {
                _logger.LogInformation("Excel to PDF conversion successful: {OutputPath}", outputPath);
                return (true, outputPath, "Excel document converted to PDF successfully (formatting preserved).");
            }

            _logger.LogError("Excel to PDF conversion failed (exitCode={ExitCode}): {Error}", exitCode, stderr);
            return (false, outputPath, $"Conversion failed: {stderr}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Excel to PDF conversion failed for {InputPath}", inputPath);
            return (false, outputPath, $"Conversion failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Convert PowerPoint to PDF using LibreOffice (via Python script) for best formatting preservation.
    /// </summary>
    public async Task<(bool success, string outputPath, string message)> ConvertPowerPointToPdfAsync(
        string inputPath, string outputFileName)
    {
        var outputPath = Path.Combine(_tempFilePath, outputFileName);
        if (!File.Exists(inputPath))
            return (false, outputPath, "Input file not found.");

        try
        {
            // Check file extension
            var extension = Path.GetExtension(inputPath).ToLowerInvariant();

            if (extension != ".pptx" && extension != ".ppt")
            {
                return (false, outputPath, $"Unsupported file format: {extension}. Only .ppt and .pptx files are supported.");
            }

            // Try using LibreOffice via Python script (preserves formatting)
            var (exitCode, stdout, stderr) = await RunPythonScriptAsync("powerpoint_to_pdf.py", $"\"{inputPath}\" \"{outputPath}\"");

            if (exitCode == 0 && File.Exists(outputPath))
            {
                _logger.LogInformation("PowerPoint to PDF conversion successful: {OutputPath}", outputPath);
                return (true, outputPath, "PowerPoint document converted to PDF successfully (formatting preserved).");
            }

            _logger.LogError("PowerPoint to PDF conversion failed (exitCode={ExitCode}): {Error}", exitCode, stderr);
            return (false, outputPath, $"Conversion failed: {stderr}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PowerPoint to PDF conversion failed for {InputPath}", inputPath);
            return (false, outputPath, $"Conversion failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Convert PDF to PowerPoint (.pptx): each page becomes one slide (image). Uses Python pymupdf + python-pptx (hybrid, free).
    /// </summary>
    public async Task<(bool success, string outputPath, string message)> ConvertToPowerPointAsync(
        string inputFile, string outputFileName)
    {
        var outputPath = Path.Combine(_tempFilePath, outputFileName);
        if (!File.Exists(inputFile))
            return (false, "", "Input file not found.");
        var scriptPath = Path.Combine(Path.GetDirectoryName(_tempFilePath) ?? _tempFilePath, "scripts", "pdf_to_pptx.py");
        if (!File.Exists(scriptPath) && !RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            scriptPath = Path.Combine("/var/www/pdfpeaks", "scripts", "pdf_to_pptx.py");
        if (!File.Exists(scriptPath))
            return (false, "", "Conversion script not found. Ensure scripts/pdf_to_pptx.py exists. Install: pip install pymupdf python-pptx");
        var (exitCode, stdout, stderr) = await RunPythonScriptAsync("pdf_to_pptx.py", $"\"{inputFile}\" \"{outputPath}\"");
        var output = (stdout + "\n" + stderr).Trim();
        if (exitCode != 0 || !File.Exists(outputPath))
        {
            var err = output.Contains("ERROR:") ? output.Replace("ERROR:", "").Trim() : (stderr.Trim().Length > 0 ? stderr.Trim() : stdout.Trim());
            return (false, "", string.IsNullOrWhiteSpace(err) ? "PDF to PowerPoint conversion failed." : err);
        }
        _logger.LogInformation("PDF converted to PowerPoint: {Input} -> {Output}", inputFile, outputPath);
        return (true, outputPath, "Processed");
    }

    /// <summary>
    /// Extract text from a specific PDF page
    /// </summary>
    private string ExtractTextFromPage(PdfSharpPdfDocument document, int pageIndex)
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
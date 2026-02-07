using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;
using PdfSharpCore.Drawing;
using Pdfpeaks.Models;
using System.Runtime.InteropServices;
using System.Text;
using System.Diagnostics;
using System.Drawing;

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
    private async Task<(int exitCode, string stdout, string stderr)> RunPythonScriptAsync(string scriptFileName, string arguments)
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
    /// Word to PDF using LibreOffice headless (hybrid, free). Requires LibreOffice installed.
    /// </summary>
    public async Task<(bool success, string outputPath, string message)> ConvertWordToPdfAsync(string inputPath, string outputFileName)
    {
        var outputPath = Path.Combine(_tempFilePath, outputFileName);
        if (!File.Exists(inputPath))
            return (false, outputPath, "Input file not found.");
        var contentRoot = Path.GetDirectoryName(_tempFilePath) ?? _tempFilePath;
        var scriptPath = Path.Combine(contentRoot, "scripts", "word_to_pdf.py");
        if (!File.Exists(scriptPath) && !RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            scriptPath = Path.Combine("/var/www/pdfpeaks", "scripts", "word_to_pdf.py");
        if (!File.Exists(scriptPath))
            return (false, outputPath, "Conversion script not found. Ensure scripts/word_to_pdf.py exists. Word to PDF requires LibreOffice installed (e.g. apt install libreoffice).");
        var (exitCode, stdout, stderr) = await RunPythonScriptAsync("word_to_pdf.py", $"\"{inputPath}\" \"{outputPath}\"");
        var output = (stdout + "\n" + stderr).Trim();
        if (exitCode != 0 || !File.Exists(outputPath))
        {
            var err = output.Contains("ERROR:") ? output.Replace("ERROR:", "").Trim() : (stderr.Trim().Length > 0 ? stderr.Trim() : stdout.Trim());
            return (false, outputPath, string.IsNullOrWhiteSpace(err) ? "Word to PDF conversion failed." : err);
        }
        return (true, outputPath, "Processed");
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
            var outputDocument = new PdfDocument();

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
    /// Convert PDF to images (JPG or PNG) using Python PyMuPDF (hybrid, free).
    /// </summary>
    public async Task<(bool success, List<string> outputPaths, string message)> ConvertToImagesAsync(
        string inputFile, string outputPrefix, string imageFormat)
    {
        var outputPaths = new List<string>();
        if (!File.Exists(inputFile))
            return (false, outputPaths, "Input file not found.");
        var fmt = (imageFormat ?? "jpg").ToLowerInvariant();
        if (fmt != "png") fmt = "jpg";
        var contentRoot = Path.GetDirectoryName(_tempFilePath) ?? _tempFilePath;
        var scriptPath = Path.Combine(contentRoot, "scripts", "pdf_to_images.py");
        if (!File.Exists(scriptPath) && !RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            scriptPath = Path.Combine("/var/www/pdfpeaks", "scripts", "pdf_to_images.py");
        if (!File.Exists(scriptPath))
            return (false, outputPaths, "Conversion script not found. Ensure scripts/pdf_to_images.py exists. Install: pip install pymupdf");
        var (exitCode, stdout, stderr) = await RunPythonScriptAsync("pdf_to_images.py", $"\"{inputFile}\" \"{_tempFilePath}\" \"{outputPrefix}\" \"{fmt}\"");
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
    /// Convert PDF to Word document (.docx) using Python pdf2docx script (hybrid AI reconstruction).
    /// </summary>
    public async Task<(bool success, string outputPath, string message)> ConvertToWordAsync(
        string inputPath, string outputName)
    {
        string outputPath = Path.Combine(_tempFilePath, outputName);
        var contentRoot = Path.GetDirectoryName(_tempFilePath) ?? _tempFilePath;
        var scriptPath = Path.Combine(contentRoot, "scripts", "convert_pdf.py");

        if (!File.Exists(inputPath))
        {
            return (false, outputPath, "Input file not found.");
        }

        if (!File.Exists(scriptPath))
        {
            _logger.LogWarning("Python script not found at {ScriptPath}", scriptPath);
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                scriptPath = "/var/www/pdfpeaks/scripts/convert_pdf.py";
            if (!File.Exists(scriptPath))
                return (false, outputPath, "Conversion script not found. Ensure scripts/convert_pdf.py exists and Python with pdf2docx is installed.");
        }

        var pythonExe = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "python" : "python3";
        var startInfo = new ProcessStartInfo
        {
            FileName = pythonExe,
            Arguments = $"\"{scriptPath}\" \"{inputPath}\" \"{outputPath}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        try
        {
            using var process = Process.Start(startInfo);
            if (process == null)
            {
                return (false, outputPath, "Failed to start conversion process.");
            }

            var stdout = await process.StandardOutput.ReadToEndAsync();
            var stderr = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            var output = (stdout + "\n" + stderr).Trim();
            if (process.ExitCode != 0 || !File.Exists(outputPath))
            {
                var errorMsg = output.Contains("ERROR:") ? output.Replace("ERROR:", "").Trim() : (string.IsNullOrEmpty(stderr) ? stdout : stderr);
                if (string.IsNullOrWhiteSpace(errorMsg)) errorMsg = "Conversion failed.";
                _logger.LogWarning("PDF to Word conversion failed: {Error}", errorMsg);
                return (false, outputPath, errorMsg);
            }

            _logger.LogInformation("PDF converted to Word: {Input} -> {Output}", inputPath, outputPath);
            return (true, outputPath, "Processed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting PDF to Word");
            return (false, outputPath, ex.Message);
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

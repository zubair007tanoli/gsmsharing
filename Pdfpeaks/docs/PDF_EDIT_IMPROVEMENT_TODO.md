# PDF Edit & Image Enhancement – Improvement TODO

## Overview
Improve the [PdfPeaks Edit PDF](https://pdfpeaks.com/Pdf/Edit) tool to support:
1. **Edit existing text** (click-to-edit like forms)
2. **Image optimization** (SkiaSharp, CamScanner-style enhancement)
3. **Form-fill experience** (click in boxes and fill online)

---

## ✅ Completed (Implemented)

### Phase 1: Critical Fixes
- ✅ **1.1 Fix GetPageForEdit Bug** – `GetTextElements` now called before file deletion
- ✅ **1.2 Fix SaveEditedPdf TextBox Structure** – Frontend flattens `textBoxes` array correctly
- ✅ **1.3 GetPdfThumbnails Download Path** – Verified `/Pdf/Download` serves from `temp_files`

### Phase 2: PDF Editor – Click-to-Edit
- ✅ **2.1 Extract Text with Bounding Boxes (PdfPig)** – `GetTextElements` returns per-word positions
- ✅ **2.2 Render Existing Text as Editable Overlays** – `loadPageForEditing()` converts text elements to overlays with coordinate mapping
- ✅ **2.2b GetTextElementsWithDimensions** – Returns `pageWidth`/`pageHeight` for proper scaling

### Phase 3: Image Enhancement (SkiaSharp)
- ✅ **3.1 ImageEnhancementService** – Created `Services/ImageEnhancementService.cs` with:
  - Document/photo enhancement modes
  - Auto-contrast (unsharp mask)
  - Sharpen (Gaussian blur-based)
  - Deskew (rotation correction)
  - Binarization (black/white)
  - Shadow removal
- ✅ **3.2 PDF Edit Flow Enhancement** – `PdfController.EnhancePageForEdit()` endpoint
- ✅ **3.3 Image/Edit Backend** – New SkiaSharp endpoints in `ImageController`:
  - `POST /Image/EnhanceWithSkiaSharp`
  - `POST /Image/AutoContrastSkia`
  - `POST /Image/SharpenSkia`
  - `POST /Image/DeskewSkia`
  - `POST /Image/BinarizeSkia`
  - `POST /Image/RemoveShadowSkia`

---

## 🔴 CRITICAL (Must Fix)

### BUG-001: Temp Files Not Cleaned Up
- **Issue**: `temp_files/` directory accumulates files indefinitely (100+ files visible in directory listing)
- **Impact**: Disk space exhaustion, potential data leakage between users
- **Fix**: Add background job (Hangfire) to delete files older than 1 hour
- **File**: `Services/FileProcessingService.cs` – add `CleanupOldFilesAsync()`
- **Priority**: CRITICAL

### BUG-002: AIService Uses Non-Injected HttpClient
- **Issue**: `AIService` creates `new HttpClient()` directly instead of using `IHttpClientFactory`
- **Impact**: Socket exhaustion under load, no retry policies
- **Fix**: Register `IHttpClientFactory` in DI, inject into `AIService`
- **File**: `Services/AIService.cs` line 21, `Program.cs`
- **Priority**: CRITICAL

### BUG-003: Hardcoded Database Credentials in appsettings.json
- **Issue**: `appsettings.json` contains real DB password `1nsp1r0N@321` and JWT secret
- **Impact**: Security vulnerability – credentials exposed in source control
- **Fix**: Move to environment variables or Azure Key Vault; use `dotnet user-secrets` for dev
- **File**: `appsettings.json`
- **Priority**: CRITICAL

### BUG-004: CS8625 Null Literal Warning in FilesController
- **Issue**: `Controllers/Api/FilesController.cs` line 554 passes `null` for non-nullable `operationId`
- **Fix**: Change `operationId: null` to use nullable parameter or provide a default value
- **File**: `Controllers/Api/FilesController.cs:554`
- **Priority**: CRITICAL (warning that should be error)

---

## 🟠 HIGH Priority

### PERF-001: PdfProcessingService is 71KB – Split into Smaller Services
- **Issue**: `PdfProcessingService.cs` is 71,069 chars with 40+ methods – violates SRP
- **Fix**: Split into: `PdfConversionService`, `PdfEditService`, `PdfCompressionService`, `PdfSplitMergeService`
- **Priority**: HIGH

### PERF-002: Python Script Execution Has No Timeout
- **Issue**: `RunPythonScriptAsync` has no timeout – a hung Python process blocks the request indefinitely
- **Fix**: Add `CancellationToken` with timeout to `Process.WaitForExitAsync()`
- **File**: `Services/PdfProcessingService.cs:42`
- **Priority**: HIGH

### PERF-003: ImageProcessingService Pixel-by-Pixel Operations Are Slow
- **Issue**: `ImageEnhancementService` uses `GetPixel`/`SetPixel` in nested loops – O(W×H) per operation
- **Fix**: Use `SKBitmap.GetPixelSpan()` or `SKPixmap` for bulk pixel access; or use `SKImageFilter` pipeline
- **File**: `Services/ImageEnhancementService.cs`
- **Priority**: HIGH

### PERF-004: No Caching for PDF Text Extraction
- **Issue**: `GetTextElements` re-opens and re-parses the PDF on every page load
- **Fix**: Cache text elements in `RedisCacheService` keyed by file hash + page number
- **File**: `Services/PdfProcessingService.cs:834`
- **Priority**: HIGH

### SEC-001: JWT Secret Key is Weak and Hardcoded
- **Issue**: `appsettings.json` has `"SecretKey": "YourSuperSecretKeyForJwtTokens2024"` – weak and hardcoded
- **Fix**: Generate a 256-bit random key; store in environment variable `JWT__SecretKey`
- **File**: `appsettings.json`, `Services/Auth/JwtTokenService.cs`
- **Priority**: HIGH

### SEC-002: No CSRF Protection on File Upload Endpoints
- **Issue**: PDF/Image upload endpoints lack anti-forgery token validation
- **Fix**: Add `[ValidateAntiForgeryToken]` or use `[AutoValidateAntiforgeryToken]` globally
- **File**: `Controllers/PdfController.cs`, `Controllers/ImageController.cs`
- **Priority**: HIGH

### SEC-003: File Extension Validation Missing for Some Endpoints
- **Issue**: Some upload endpoints only check MIME type, not file extension; path traversal possible
- **Fix**: Validate both extension and magic bytes; sanitize filenames with `Path.GetFileName()`
- **File**: `Services/FileProcessingService.cs`
- **Priority**: HIGH

---

## 🟡 MEDIUM Priority

### Phase 2.3: AcroForm Fields (Form Fill)
- **Goal**: Detect PDF form fields (AcroForm) and make them clickable/fillable
- **Library**: PdfPig – `document.GetForm()` or similar for form field positions
- **Backend**: New endpoint or extend `GetPageForEdit` to return `formFields: [{name, type, x, y, width, height, value}]`
- **Frontend**: Render form fields as input overlays; on save, flatten filled values into PDF
- **Priority**: MEDIUM

### FEAT-001: OCR for Scanned PDFs (Tesseract)
- **Goal**: Extract text from image-only PDFs for editing
- **Tool**: Tesseract OCR via Python script (`pytesseract`) or `Tesseract.NET`
- **Flow**: PDF page → image → OCR → text + bounding boxes → editable overlays
- **File**: New `scripts/ocr_pdf.py`, extend `PdfProcessingService`
- **Priority**: MEDIUM

### FEAT-002: Batch Processing / Queue System
- **Issue**: Large file operations block HTTP requests
- **Fix**: Use Hangfire background jobs for heavy operations (compress, convert, OCR)
- **File**: `Program.cs` – Hangfire is already referenced but not configured
- **Priority**: MEDIUM

### FEAT-003: Stripe Payment Integration
- **Issue**: `appsettings.json` has Stripe keys but they're empty; payment flow not implemented
- **Fix**: Implement `StripeService` with checkout sessions, webhooks, subscription management
- **File**: New `Services/StripeService.cs`
- **Priority**: MEDIUM

### FEAT-004: Azure Blob Storage for File Storage
- **Issue**: Files stored in `temp_files/` on local disk – not scalable, lost on restart
- **Fix**: Implement `IBlobStorageService` backed by Azure Blob Storage or S3
- **File**: New `Services/BlobStorageService.cs`; `appsettings.json` has `AzureStorage` config
- **Priority**: MEDIUM

### FEAT-005: PDF Watermark Tool
- **Goal**: Add text or image watermarks to PDFs
- **Library**: PdfSharpCore (already in project)
- **File**: New endpoint in `PdfController`, new method in `PdfProcessingService`
- **Priority**: MEDIUM

### FEAT-006: PDF Page Numbering Tool
- **Goal**: Add page numbers to PDFs
- **Library**: PdfSharpCore
- **File**: `PdfController`, `PdfProcessingService`
- **Priority**: MEDIUM

### FEAT-007: PDF Metadata Editor
- **Goal**: Edit PDF title, author, subject, keywords
- **Library**: PdfSharpCore – `document.Info`
- **File**: New endpoint in `PdfController`
- **Priority**: MEDIUM

### FEAT-008: Image Watermark Tool
- **Goal**: Add text/image watermarks to images
- **Library**: SkiaSharp (already in project)
- **File**: `ImageController`, `ImageEnhancementService`
- **Priority**: MEDIUM

### CODE-001: Obsolete SkiaSharp API Usage
- **Issue**: `SKPaint.TextSize` and `SKCanvas.DrawText(string, float, float, SKPaint)` are obsolete
- **Fix**: Use `SKFont.Size` and `SKCanvas.DrawText(string, float, float, SKTextAlign, SKFont, SKPaint)`
- **File**: `Services/ImageProcessingService.cs:562,567,568`
- **Priority**: MEDIUM

### CODE-002: Obsolete QuestPDF API Usage
- **Issue**: `TextStyleExtensions.Fallback()` is obsolete since QuestPDF 2024.3
- **Fix**: Use `FontFamilyFallback()` method instead
- **File**: `Services/PdfProcessingService.cs:164`
- **Priority**: MEDIUM

### CODE-003: MediatR Version Conflict
- **Issue**: `MediatR.Extensions.Microsoft.DependencyInjection 11.1.0` requires MediatR < 12.0.0 but 14.0.0 is installed
- **Fix**: Remove `MediatR.Extensions.Microsoft.DependencyInjection` (merged into MediatR 12+) or downgrade MediatR
- **File**: `Pdfpeaks.csproj`
- **Priority**: MEDIUM

### CODE-004: RedisCacheService Creates Its Own Connection
- **Issue**: `RedisCacheService` creates its own `ConnectionMultiplexer` instead of using the singleton registered in `Program.cs`
- **Fix**: Inject `IConnectionMultiplexer` from DI instead of creating a new one
- **File**: `Services/Caching/RedisCacheService.cs`
- **Priority**: MEDIUM

---

## 🟢 LOW Priority

### Phase 4: AI & Advanced Features

#### 4.1 Smart Form Detection (AI)
- **Goal**: Detect form-like regions (boxes, lines) in non-AcroForm PDFs
- **Approach**: Use layout analysis or simple heuristics (white rectangles, aligned lines)

#### 4.2 Auto-Enhance with AI (ONNX)
- **Goal**: One-click "enhance like CamScanner" using ML models
- **Option**: ONNX model for document enhancement (`Microsoft.ML.OnnxRuntime` already referenced)

#### 4.3 Document Q&A with RAG
- **Goal**: Ask questions about PDF content using vector search
- **Tool**: Qdrant (already in docker-compose), Python AI service

### Phase 5: UX & Polish

#### 5.1 Undo/Redo in PDF Editor
- Add history stack for text box changes in `Views/Pdf/Edit.cshtml`

#### 5.2 Keyboard Shortcuts
- Already partially done; extend for delete, escape, copy/paste text boxes

#### 5.3 Mobile-Friendly PDF Editor
- Touch-friendly overlays; responsive toolbar

#### 5.4 Loading & Error States
- Clear feedback when `GetPageForEdit` or `SaveEditedPdf` fails
- Progress indicator for long operations

#### 5.5 Dark Mode for Editor
- CSS variables for dark/light theme toggle

### FEAT-009: PDF Comparison Tool
- **Goal**: Compare two PDFs and highlight differences
- **Library**: PdfPig for text extraction, diff algorithm
- **Priority**: LOW

### FEAT-010: PDF to HTML Conversion
- **Goal**: Convert PDF to HTML for web embedding
- **Library**: Python `pdf2htmlEX` or `pdfminer.six`
- **Priority**: LOW

### FEAT-011: Batch File Processing
- **Goal**: Process multiple files at once (bulk compress, bulk convert)
- **Frontend**: Multi-file upload with progress tracking
- **Priority**: LOW

### TEST-001: Unit Tests for PdfProcessingService
- **Goal**: Test merge, split, compress, rotate operations
- **Framework**: xUnit + Moq
- **Priority**: LOW

### TEST-002: Integration Tests for API Endpoints
- **Goal**: Test file upload/download flow end-to-end
- **Framework**: `WebApplicationFactory<Program>` + TestContainers
- **Priority**: LOW

### TEST-003: Frontend JavaScript Tests
- **Goal**: Test `savePdf()`, `loadPageForEditing()`, text box coordinate mapping
- **Framework**: Jest or Vitest
- **Priority**: LOW

### INFRA-001: Add Nginx Configuration File
- **Issue**: `docker-compose.yml` references `./nginx.conf` but file doesn't exist
- **Fix**: Create `nginx.conf` with reverse proxy, SSL, and rate limiting config
- **Priority**: LOW

### INFRA-002: Add .env.example File
- **Issue**: No `.env.example` for Docker deployment
- **Fix**: Create `.env.example` with all required environment variables
- **Priority**: LOW

### INFRA-003: Add GitHub Actions CI/CD
- **Goal**: Automated build, test, and deploy pipeline
- **File**: `.github/workflows/ci.yml`
- **Priority**: LOW

---

## Tech Stack Summary

| Area | Tools |
|------|-------|
| PDF | PdfSharpCore, PdfPig, QuestPDF |
| Image | SixLabors.ImageSharp, **SkiaSharp** |
| OCR | Tesseract (Python) or Tesseract.NET |
| AI | ONNX Runtime (already referenced), Python FastAPI |
| Backend | C# / ASP.NET Core .NET 10 |
| Frontend | JavaScript, Bootstrap, contentEditable overlays |
| Cache | Redis (StackExchange.Redis) |
| DB | SQL Server + EF Core |
| Queue | Hangfire (configured but not used) |
---

## PDF↔Word Conversion Roadmap

### ✅ Completed

- ✅ **C-001: Basic free PDF↔Word / Word↔PDF pipeline**
  - ASP.NET Core controllers (`PdfController`, `FilesController`) integrated with `PdfProcessingService`.
  - Python scripts (`pdf_to_word.py`, `convert_pdf.py`, `word_to_pdf.py`) wired via `RunPythonScriptAsync`.
  - LibreOffice-based Word→PDF path in place with C# fallbacks (OpenXml + QuestPDF).

- ✅ **C-002: Remove extra filename heading from PDF→Word output**
  - `scripts/pdf_to_word.py` no longer injects the source filename as a heading at the top of the converted `.docx` file.
  - Keeps converted documents cleaner and closer to the original layout.

- ✅ **C-010: Make pdf2docx the primary PDF→Word path**
  - `PdfProcessingService.ConvertToWordAsync` now prefers `convert_pdf.py` (pdf2docx) by default and falls back to `pdf_to_word.py` (pdfplumber) when needed.
  - The preferred engine can be configured via `Conversion:PdfToWordPrimaryEngine` in `appsettings.json`.

- ✅ **C-011: Add timeouts and better error handling for conversion scripts**
  - `RunPythonScriptAsync` now applies a configurable timeout (default 180 seconds via `Conversion:PythonScriptTimeoutSeconds`) and terminates hung Python processes.
  - Returns structured error messages when a script exceeds the timeout, improving reliability under load.

### 🟠 Planned / In Progress

- 🟠 **C-012: Background jobs for heavy conversions**
  - Move large PDF↔Word conversions to Hangfire background jobs instead of blocking HTTP requests.
  - Expose job status endpoints / SignalR notifications to the frontend.

### 🟡 Future Enhancements

- 🟡 **C-020: Per-plan limits and metrics for conversions**
  - Integrate with `RateLimitService` and planned Stripe billing to enforce per‑plan limits on conversion size, page count, and frequency.

- 🟡 **C-021: Storage abstraction for converted files**
  - Move from local `temp_files/` storage to pluggable `IFileStorageService` (Azure Blob/S3) for better scalability and durability.

## Implementation Order (Recommended)

1. **BUG-001** – Temp file cleanup (disk safety)
2. **BUG-003** – Move credentials to env vars (security)
3. **BUG-002** – Fix HttpClient injection (reliability)
4. **Phase 2.3** – AcroForm support
5. **FEAT-001** – OCR for scanned PDFs
6. **FEAT-002** – Hangfire background jobs
7. **PERF-001** – Split PdfProcessingService
8. **FEAT-003** – Stripe payments
9. **Phase 4–5** – AI/OCR and UX polish

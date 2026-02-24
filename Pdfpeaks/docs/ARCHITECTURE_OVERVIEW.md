## Pdfpeaks Architecture Overview

### 1. High-Level System

- **Web app (C# / ASP.NET Core MVC)**: Main `Pdfpeaks` project, hosts all PDF/image tools, AI UI, auth, health, and public APIs.
- **Python AI microservice (FastAPI)**: `services-python-ai` project, provides PDF text extraction, summarization, Q&A, classification, and entity extraction over HTTP.
- **Python CLI tools**: Scripts under `scripts/` (e.g. `pdf_to_word.py`, `pdf_to_excel.py`, `pdf_to_pptx.py`, `pdf_to_images.py`, `compress_pdf.py`) used by the C# app for heavy PDF conversions.
- **Shared infrastructure**: SQL Server, Redis, optional Qdrant vector DB and other services defined in `docker-compose.yml`.

The system can run:

- **Locally (developer mode)**: `dotnet run` for the ASP.NET app, `python main.py` in `services-python-ai`, and Python CLI tools available on the host.
- **Docker (full stack)**: `docker-compose up` builds and runs the .NET app, Python AI, SQL Server, Redis, Qdrant, etc., as described in `MODERNIZATION_README.md`.

### 2. ASP.NET Core Application (Pdfpeaks)

- **Entry point & hosting**
  - `Program.cs`: configures DI, logging (Serilog), Identity, EF Core, SignalR, Redis, Swagger, rate limiting, and health checks.
  - `Startup-style` registration via extension services in `Services/Infrastructure` (e.g. `HealthCheckService`).

- **Core services**
  - `Services/PdfProcessingService.cs`:
    - Handles all PDF-centric operations: merge, split, rotate, organize, compress, protect/unlock, page numbering, PDF→Word/Excel/PowerPoint/images, Word/Excel/PowerPoint→PDF.
    - Orchestrates **Python CLI tools** via `RunPythonScriptAsync` for heavy conversions (`pdf_to_word.py`, `pdf_to_excel.py`, `pdf_to_pptx.py`, `pdf_to_images.py`, `compress_pdf.py`, etc.).
    - Uses **PdfSharpCore** for structural operations and **PdfPig** for text inspection/extraction and future editor geometry.
    - Writes outputs to `temp_files/` under the content root and returns paths to controllers for download.
  - `Services/ImageProcessingService.cs` (and related image enhancement services if present):
    - Performs image conversions and basic edits (rotate, resize, format changes) and acts as the backend for `Views/Image` tools.
  - `Services/AIService.cs`:
    - Gateway to the Python AI microservice.
    - Sends PDFs via multipart/form-data to FastAPI endpoints (`/api/v1/process`, `/api/v1/qa`, `/api/v1/summarize`, `/api/v1/classify`, etc.).
    - Wraps results into typed C# models (`AIProcessResult`, `AISummaryResult`, `AIClassifyResult`, `AIQnAResult`).
  - `Services/AI/*`:
    - C# AI services for Azure OpenAI (`OpenAIDocumentAnalysisService`), Semantic Kernel (`SemanticKernelService`), and ML.NET classification (`MLDocumentClassificationService`) for richer analysis and on-device models.

- **Infrastructure & cross-cutting**
  - `Services/Caching/RedisCacheService.cs`: distributed caching for sessions, results, rate limits.
  - `Services/Auth/JwtTokenService.cs`: JWT generation/validation, role-based claims.
  - `Services/Infrastructure/RateLimitService.cs`: tiered rate limiting (Free/Pro/Enterprise).
  - `Services/Infrastructure/HealthCheckService.cs` and `HealthCheckAggregator`: aggregates SQL, Redis, Python AI, and system metrics into health endpoints and structured responses.
  - `Services/Realtime/ProcessingHub.cs`: SignalR hub for progress updates and notifications.

- **Controllers & views**
  - `Controllers/PdfController.cs`:
    - Endpoints for all PDF tools (merge, split, compress, convert, rotate, organize, protect/unlock, edit, thumbnails, fonts).
    - All file IO is funneled through `FileProcessingService` (upload, temp path resolution, download tracking).
  - `Controllers/ImageController.cs`:
    - Endpoints backing `Views/Image/*` (document/photo enhancement, AI-based improve, image→PDF).
  - `Controllers/Api/PublicApiController.cs`:
    - Public REST API v2 for external clients: token issuance, AI document analysis, summarization, Q&A, classification, status.
  - `Views/*`:
    - `Views/Pdf/*.cshtml`: Razor pages for each PDF tool (Edit, Merge, Split, Compress, Organize, Rotate, ConvertToWord, ConvertFromWord, etc.).
    - `Views/Image/Edit.cshtml`: full-screen canvas-based image editor (CamScanner-style flow).
    - `Views/AI/Index.cshtml`: UI for AI analysis and Q&A.
    - `Views/Shared/_Layout.cshtml`: main Bootstrap-based layout used by all tools.

- **Static assets**
  - `wwwroot/js/site.js`: shared JS for interactive images, share buttons, file downloads, and drag-and-drop previews.
  - `wwwroot/css/site.css`: site-wide styling and layout helpers.
  - `wwwroot/lib/*`: vendor libraries (jQuery, Bootstrap, validation).

### 3. Python AI Microservice (`services-python-ai`)

- **Framework & entrypoint**
  - `services-python-ai/main.py`: FastAPI app `Pdfpeaks AI Service` with OpenAPI docs at `/docs`, CORS enabled, and an `/health` endpoint.

- **Responsibilities**
  - **Text extraction**:
    - Uses `pdfplumber` to extract per-page text and normalized full-text.
    - Returned as a JSON structure with page map and concatenated text.
  - **Summarization**:
    - `POST /api/v1/summarize` and `process_type="summarize"` in `/api/v1/process`.
    - Currently uses a simple extractive summarization over sentences and word counts, with styles: `concise`, `detailed`, `bullet`.
  - **Q&A**:
    - `POST /api/v1/qa`:
      - Extracts text from PDF and runs keyword-based relevance scoring over sentences to construct an answer and ranked sources.
  - **Classification**:
    - `POST /api/v1/classify` and `process_type="classify"`:
      - Rule-based classification into Invoice/Contract/Report/Form/Manual/Letter/Other via keyword hits.
  - **Entity extraction**:
    - `POST /api/v1/extract-entities` and `process_type="extract_entities"`:
      - Regex-based detection of emails, phone numbers, dates, and currencies.
  - **Full analysis**:
    - `process_type="full_analysis"` in `/api/v1/process`:
      - Bundles summary, classification, entities, text length, and page count into a single result payload.

- **Security & configuration**
  - Uses HTTP Bearer auth with `AI_API_KEY` environment variable.
  - Accessible by `AIService` in C# via configured base URL (`AIService:Url`) and API key.

### 4. Python CLI Conversion Tools (`scripts/`)

The C# `PdfProcessingService` calls these scripts via `RunPythonScriptAsync`:

- **Format conversions**
  - `word_to_pdf.py`, `excel_to_pdf.py`, `powerpoint_to_pdf.py`: use LibreOffice for high-fidelity Office→PDF.
  - `pdf_to_word.py`, `convert_pdf.py`: use `pdfplumber` / `pdf2docx` for PDF→DOCX (text + tables).
  - `pdf_to_excel.py`: `pdfplumber` + `openpyxl` for PDF→Excel table extraction.
  - `pdf_to_pptx.py`: PyMuPDF + `python-pptx` for PDF→PowerPoint (page-per-slide images).
- **PDF→images and compression**
  - `pdf_to_images.py`: PyMuPDF-based page rendering to JPG/PNG used by `/Pdf/GetPdfThumbnails` and `/Pdf/GetPageForEdit`.
  - `compress_pdf.py`: `pikepdf`-based size optimization with quality levels (high/medium/low).
- **Utilities**
  - Scripts like `pdf_fonts.py`, `pdf_preview.py`, `pdf_fonts` listing, etc., used for font detection and previews.

All scripts expect an input path and output directory/filename under the ASP.NET app’s `temp_files/` root and are OS-agnostic with Windows/Linux paths handled in `PdfProcessingService`.

### 5. Temp Files, Downloads, and Health

- **Temp file storage**
  - `PdfProcessingService` creates and manages `temp_files/` under `ContentRootPath` for all intermediate and final outputs.
  - `FileProcessingService` is responsible for:
    - Saving uploaded files with unique prefixes (e.g. `merge_`, `split_`, `edit_`).
    - Resolving file paths by name for downloads.
    - Recording downloads and quotas per user.

- **Download flow**
  - Controllers write outputs into `temp_files/`, then return JSON with a `/Pdf/Download?fileName=...` URL.
  - `PdfController.Download`:
    - Uses `FileProcessingService.GetFilePath` to resolve the full path.
    - Streams the file with the correct content type and records the download.

- **Health and observability**
  - `HealthCheckService.AddPdfpeaksHealthChecks` wires:
    - SQL Server, Redis, and Python AI `/health` as health check contributors.
    - Custom memory and disk health checks for resource monitoring.
  - `HealthCheckAggregator`:
    - Aggregates DB, cache, and Python AI status plus metrics (`HealthMetrics`) into a structured response for dashboards.

### 6. Editor Flows (PDF & Image)

- **PDF edit flow**
  - Frontend: `Views/Pdf/Edit.cshtml` (JS inline script):
    - Uploads a PDF to `/Pdf/GetPdfThumbnails` to get per-page image URLs.
    - For a given page, calls `/Pdf/GetPageForEdit` to retrieve a page image and `textElements` (future: bounding boxes, fonts, etc.).
    - Maintains a per-page `textBoxes` array (positions + formatting) and lets users add and format overlays.
    - On save, flattens text boxes to a `List<TextBoxPosition>`-compatible JSON and posts to `/Pdf/SaveEditedPdf`.
  - Backend: `PdfController.GetPageForEdit` and `PdfProcessingService.GetTextElements`:
    - Generate PNG images for requested pages via `ConvertToImagesAsync` and returns a download URL plus text metadata.
    - `SaveEditedPdfAsync` reopens the source PDF and draws overlay text using PdfSharpCore based on `TextBoxPosition`.

- **Image editor flow**
  - Frontend: `Views/Image/Edit.cshtml`:
    - Canvas-based editor with document/photo modes, crop, draw, erase, undo/redo, zoom, AI-assisted enhancement, and export to PDF/JPG.
    - Integrates with backend endpoints (`/Image/EnhanceWithAI`, `/Image/EnhanceDocument`, `/Image/StraightenImage`, `/Image/ConvertToPdf`) and has rich local fallbacks.
  - Backend: `ImageController`:
    - Uses `ImageProcessingService` and planned SkiaSharp-based `ImageEnhancementService` to implement CamScanner-style operations and conversions.

This overview is intentionally concise and focuses on **how the pieces fit together** so it can be used as a reference when implementing the detailed TODOs for the PDF editor, AI integration, and reliability improvements.


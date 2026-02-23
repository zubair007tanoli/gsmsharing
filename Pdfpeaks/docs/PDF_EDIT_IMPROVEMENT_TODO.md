# PDF Edit & Image Enhancement – Improvement TODO

## Overview
Improve the [PdfPeaks Edit PDF](https://pdfpeaks.com/Pdf/Edit) tool to support:
1. **Edit existing text** (click-to-edit like forms)
2. **Image optimization** (SkiaSharp, CamScanner-style enhancement)
3. **Form-fill experience** (click in boxes and fill online)

---

## Phase 1: Critical Fixes (Immediate)

### 1.1 Fix GetPageForEdit Bug
- **Issue**: Controller deletes `filePath` before calling `GetTextElements(filePath, page)` → file not found.
- **Fix**: Call `GetTextElements` **before** deleting the input file.
- **File**: `Controllers/PdfController.cs`

### 1.2 Fix SaveEditedPdf – TextBox Structure Mismatch
- **Issue**: Frontend sends `textBoxes` as `[[{x,y,width,height,text,format:{...}}], [...]]` (array of arrays with nested format).
- **Backend expects**: `List<TextBoxPosition>` with flat `Page, X, Y, Width, Height, Text, FontFamily, FontSize`, etc.
- **Fix**: Transform in frontend before `JSON.stringify` – flatten and map `format` to top-level properties, add `Page`.
- **File**: `Views/Pdf/Edit.cshtml` (savePdf function)

### 1.3 Fix GetPdfThumbnails vs Download Path
- **Verify**: `GetPdfThumbnails` returns `url: /Pdf/Download?fileName=...` and that `PdfController.Download` serves files from `temp_files`.
- **Note**: PdfController uses `Download` (not `DownloadFile`). Ensure `FileProcessingService.GetFilePath` and `PdfProcessingService` use same `temp_files` root.

---

## Phase 2: PDF Editor – Click-to-Edit Existing Text

### 2.1 Extract Text with Bounding Boxes (PdfPig)
- **Goal**: Get each text span with `(x, y, width, height)` so we can render editable overlays.
- **Library**: PdfPig (already in project) – use `page.GetWords()` or `page.GetLetters()` for positions.
- **File**: `Services/PdfProcessingService.cs` – enhance `GetTextElements` to return per-word/span positions.
- **Model**: Extend `PdfTextElement` with accurate `X, Y, Width, Height` from PdfPig.

### 2.2 Render Existing Text as Editable Overlays
- **Goal**: When loading a page, show existing text as overlays; user clicks to edit.
- **File**: `Views/Pdf/Edit.cshtml` – in `loadPageForEditing`, use `result.textElements` to create `textBoxes` entries with correct positions.
- **UX**: Click text → becomes contentEditable; blur → save to `textBoxes`.

### 2.3 Support AcroForm Fields (Form Fill)
- **Goal**: Detect PDF form fields (AcroForm) and make them clickable/fillable.
- **Library**: PdfPig – `document.GetForm()` or similar for form field positions.
- **Backend**: New endpoint or extend `GetPageForEdit` to return `formFields: [{name, type, x, y, width, height, value}]`.
- **Frontend**: Render form fields as input overlays; on save, flatten filled values into PDF (or use PdfSharp to set field values).

---

## Phase 3: Image Enhancement (SkiaSharp, CamScanner-Style)

### 3.1 Image Optimization Service (SkiaSharp)
- **Goal**: Enhance scanned documents – improve contrast, sharpen, deskew, remove shadows.
- **Library**: SkiaSharp (already in project).
- **Operations**:
  - Auto-contrast / levels
  - Sharpen
  - Deskew (detect and correct rotation)
  - Binarization (black/white for OCR-friendly output)
  - Shadow removal (optional, more complex)
- **File**: New `Services/ImageEnhancementService.cs` or extend `ImageProcessingService.cs`.

### 3.2 Integrate into PDF Edit Flow
- **Goal**: When loading a PDF page for edit, optionally “enhance” the page image before showing.
- **Flow**: PDF page → render to image → apply SkiaSharp enhancement → show in editor.
- **UI**: Add “Enhance page” button in toolbar; calls backend to return enhanced image.

### 3.3 Image Edit Page (Image/Edit)
- **Goal**: Make `Image/Edit` work – document/photo enhancement with SkiaSharp.
- **File**: `Views/Image/Edit.cshtml` – wire to backend; add enhance, crop, rotate, etc.
- **Backend**: Ensure `ImageController` has endpoints for enhance operations using SkiaSharp.

---

## Phase 4: AI & Advanced Features (Optional)

### 4.1 OCR for Scanned PDFs (Python/Tesseract)
- **Goal**: Extract text from image-only PDFs for editing.
- **Tool**: Tesseract OCR via Python script or .NET wrapper.
- **Flow**: PDF page → image → OCR → text + bounding boxes → editable overlays.

### 4.2 Smart Form Detection (AI)
- **Goal**: Detect form-like regions (boxes, lines) in non-AcroForm PDFs.
- **Approach**: Use layout analysis or simple heuristics (white rectangles, aligned lines).

### 4.3 Auto-Enhance with AI
- **Goal**: One-click “enhance like CamScanner” using ML models.
- **Option**: ONNX model for document enhancement (project has `Microsoft.ML.OnnxRuntime`).

---

## Phase 5: UX & Polish

### 5.1 Undo/Redo
- Add history stack for text box changes.

### 5.2 Keyboard Shortcuts
- Already partially done; extend for delete, escape, etc.

### 5.3 Mobile-Friendly
- Touch-friendly overlays; responsive toolbar.

### 5.4 Loading & Error States
- Clear feedback when GetPageForEdit or SaveEditedPdf fails.

---

## Tech Stack Summary

| Area | Tools |
|------|-------|
| PDF | PdfSharpCore, PdfPig, QuestPDF |
| Image | SixLabors.ImageSharp, **SkiaSharp** |
| OCR | Tesseract (Python) or Tesseract.NET |
| AI | ONNX Runtime (already referenced) |
| Backend | C# / ASP.NET Core |
| Frontend | JavaScript, Bootstrap, contentEditable overlays |

---

## Implementation Order

1. **Phase 1** – Fix bugs so current Edit works (GetPageForEdit, SaveEditedPdf structure).
2. **Phase 2.1–2.2** – Extract and render existing text as editable.
3. **Phase 3.1** – SkiaSharp image enhancement service.
4. **Phase 2.3** – AcroForm support (if needed).
5. **Phase 3.2–3.3** – Wire enhancement into PDF Edit and Image Edit.
6. **Phase 4–5** – AI/OCR and UX polish as time allows.

# Hybrid conversion scripts (free, no commercial libraries)

The Pdfpeaks app uses these Python scripts for high-quality conversions. The C# backend calls them via `python` / `python3`.

## Scripts and dependencies

| Script | Purpose | Pip packages | System (optional) |
|--------|---------|--------------|--------------------|
| `convert_pdf.py` | PDF → Word (.docx) | `pdf2docx` | — |
| `word_to_pdf.py` | Word (.doc/.docx) → PDF | — | **LibreOffice** |
| `pdf_to_excel.py` | PDF → Excel (.xlsx) | `pdfplumber`, `openpyxl` | — |
| `pdf_to_pptx.py` | PDF → PowerPoint (.pptx) | `pymupdf`, `python-pptx` | — |
| `pdf_to_images.py` | PDF → JPG/PNG (per page) | `pymupdf` | — |

## One-time install (development / Windows)

```bash
python -m pip install pdf2docx pdfplumber openpyxl pymupdf python-pptx
```

For **Word to PDF** you also need LibreOffice installed (e.g. [download](https://www.libreoffice.org/download/download/)). The script looks for `soffice` in PATH or in standard install paths.

## One-time install (production Linux)

```bash
# Python packages
sudo apt update && sudo apt install -y python3-pip
pip3 install pdf2docx pdfplumber openpyxl pymupdf python-pptx
# or: sudo pip3 install pdf2docx pdfplumber openpyxl pymupdf python-pptx

# LibreOffice (required for Word → PDF)
sudo apt install -y libreoffice

# Permissions (so the web server can run the scripts)
sudo chown -R www-data:www-data /var/www/pdfpeaks/scripts
```

## Quick reference

- **PDF to Word:** `convert_pdf.py <input.pdf> <output.docx>` — needs `pdf2docx`
- **Word to PDF:** `word_to_pdf.py <input.docx> <output.pdf>` — needs LibreOffice
- **PDF to Excel:** `pdf_to_excel.py <input.pdf> <output.xlsx>` — needs `pdfplumber`, `openpyxl`
- **PDF to PowerPoint:** `pdf_to_pptx.py <input.pdf> <output.pptx>` — needs `pymupdf`, `python-pptx`
- **PDF to images:** `pdf_to_images.py <input.pdf> <output_dir> <prefix> <jpg|png>` — needs `pymupdf`

#!/usr/bin/env python3
"""
PDF to images (JPG or PNG) using PyMuPDF (fitz). pip install pymupdf
Usage: pdf_to_images.py <input_pdf> <output_dir> <prefix> <format> [pages]
  format: jpg or png
  pages: empty (all pages) or comma-separated (e.g. "1,3,5")
  Output: <output_dir>/<prefix>_page_1.<ext>, <prefix>_page_2.<ext>, ...
"""
import sys
import os

def main(input_pdf, output_dir, prefix, img_format, pages_arg=""):
    try:
        import fitz  # PyMuPDF
    except ImportError:
        print("ERROR: Install PyMuPDF: pip install pymupdf", file=sys.stderr)
        sys.exit(1)

    try:
        if not os.path.exists(input_pdf):
            print(f"ERROR: Input file not found: {input_pdf}", file=sys.stderr)
            sys.exit(1)
        
        img_format = (img_format or "jpg").lower()
        if img_format not in ("jpg", "jpeg", "png"):
            img_format = "jpg"
        ext = "png" if img_format == "png" else "jpg"
        
        if not os.path.isdir(output_dir):
            os.makedirs(output_dir, exist_ok=True)

        doc = fitz.open(input_pdf)
        total_pages = len(doc)
        
        # Parse pages argument
        pages_to_convert = []
        if pages_arg and pages_arg.strip():
            try:
                pages_to_convert = [int(p.strip()) for p in pages_arg.split(',') if p.strip()]
                # Filter to valid page numbers (1-based)
                pages_to_convert = [p for p in pages_to_convert if 1 <= p <= total_pages]
            except:
                # If parsing fails, convert all pages
                pages_to_convert = list(range(1, total_pages + 1))
        else:
            # Convert all pages
            pages_to_convert = list(range(1, total_pages + 1))
        
        if not pages_to_convert:
            print("ERROR: No valid pages to convert", file=sys.stderr)
            doc.close()
            sys.exit(1)
        
        # Convert selected pages
        for page_num in pages_to_convert:
            page = doc[page_num - 1]  # 0-based indexing
            pix = page.get_pixmap(matrix=fitz.Matrix(2, 2), alpha=(img_format == "png"))
            out_name = f"{prefix}_page_{page_num}.{ext}"
            out_path = os.path.join(output_dir, out_name)
            pix.save(out_path)
        
        doc.close()
        print("SUCCESS")
        sys.exit(0)
    except Exception as e:
        print(f"ERROR: {str(e)}", file=sys.stderr)
        sys.exit(1)

if __name__ == "__main__":
    if len(sys.argv) < 5:
        print("ERROR: Usage: pdf_to_images.py <input_pdf> <output_dir> <prefix> <format> [pages]", file=sys.stderr)
        sys.exit(1)
    
    pages_arg = sys.argv[5] if len(sys.argv) > 5 else ""
    main(sys.argv[1], sys.argv[2], sys.argv[3], sys.argv[4], pages_arg)

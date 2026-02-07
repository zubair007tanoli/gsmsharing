#!/usr/bin/env python3
"""
PDF to images (JPG or PNG) using PyMuPDF (fitz). pip install pymupdf
Usage: pdf_to_images.py <input_pdf> <output_dir> <prefix> <format>
  format: jpg or png
  Output: <output_dir>/<prefix>_page_1.<ext>, <prefix>_page_2.<ext>, ...
"""
import sys
import os

def main(input_pdf, output_dir, prefix, img_format):
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
        for i in range(len(doc)):
            page = doc[i]
            pix = page.get_pixmap(matrix=fitz.Matrix(2, 2), alpha=(img_format == "png"))
            out_name = f"{prefix}_page_{i + 1}.{ext}"
            out_path = os.path.join(output_dir, out_name)
            pix.save(out_path)
        doc.close()
        print("SUCCESS")
        sys.exit(0)
    except Exception as e:
        print(f"ERROR: {str(e)}", file=sys.stderr)
        sys.exit(1)

if __name__ == "__main__":
    if len(sys.argv) != 5:
        print("ERROR: Usage: pdf_to_images.py <input_pdf> <output_dir> <prefix> <format>", file=sys.stderr)
        sys.exit(1)
    main(sys.argv[1], sys.argv[2], sys.argv[3], sys.argv[4])

#!/usr/bin/env python3
"""
Generate PDF page previews (thumbnails) as base64 encoded images.
Usage: pdf_preview.py <input_pdf> <max_pages>
  Output: JSON with preview data for each page
"""
import sys
import os
import json
import base64
from io import BytesIO

def main(input_pdf, max_pages_str="5"):
    try:
        import fitz  # PyMuPDF
    except ImportError:
        print(json.dumps({"success": False, "error": "Install PyMuPDF: pip install pymupdf"}))
        sys.exit(1)

    try:
        if not os.path.exists(input_pdf):
            print(json.dumps({"success": False, "error": f"File not found: {input_pdf}"}))
            sys.exit(1)
        
        try:
            max_pages = int(max_pages_str)
        except:
            max_pages = 5

        doc = fitz.open(input_pdf)
        total_pages = len(doc)
        pages_to_preview = min(total_pages, max_pages)
        
        previews = []
        for i in range(pages_to_preview):
            try:
                page = doc[i]
                # Render at smaller size for thumbnail (150 width)
                pix = page.get_pixmap(matrix=fitz.Matrix(0.5, 0.5))
                img_bytes = pix.tobytes("png")
                b64_image = base64.b64encode(img_bytes).decode('utf-8')
                previews.append({
                    "pageNumber": i + 1,
                    "preview": f"data:image/png;base64,{b64_image}",
                    "width": pix.width,
                    "height": pix.height
                })
            except Exception as e:
                previews.append({
                    "pageNumber": i + 1,
                    "preview": None,
                    "error": str(e)
                })
        
        doc.close()
        print(json.dumps({
            "success": True,
            "totalPages": total_pages,
            "previewsGenerated": len(previews),
            "previews": previews
        }))
        sys.exit(0)
    except Exception as e:
        print(json.dumps({"success": False, "error": str(e)}))
        sys.exit(1)

if __name__ == "__main__":
    input_pdf = sys.argv[1] if len(sys.argv) > 1 else ""
    max_pages = sys.argv[2] if len(sys.argv) > 2 else "5"
    
    if not input_pdf:
        print(json.dumps({"success": False, "error": "Usage: pdf_preview.py <input_pdf> [max_pages]"}))
        sys.exit(1)
    
    main(input_pdf, max_pages)

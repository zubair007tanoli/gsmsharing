#!/usr/bin/env python3
"""
PDF to Excel conversion using pdfplumber + openpyxl (free).
pip install pdfplumber openpyxl
"""
import sys
import os

def main(input_pdf, output_xlsx):
    try:
        import pdfplumber
        from openpyxl import Workbook
    except ImportError as e:
        print(f"ERROR: Install required packages: pip install pdfplumber openpyxl. {e}", file=sys.stderr)
        sys.exit(1)

    try:
        if not os.path.exists(input_pdf):
            print(f"ERROR: Input file not found: {input_pdf}", file=sys.stderr)
            sys.exit(1)

        wb = Workbook()
        ws = wb.active
        ws.title = "PDF Content"
        row = 1
        with pdfplumber.open(input_pdf) as pdf:
            for page_num, page in enumerate(pdf.pages, start=1):
                ws.cell(row=row, column=1, value=f"Page {page_num}")
                ws.cell(row=row, column=1).font = __bold_font()
                row += 1
                tables = page.extract_tables()
                if tables:
                    for table in tables:
                        for r in table:
                            for c_idx, cell in enumerate(r, start=1):
                                ws.cell(row=row, column=c_idx, value=cell if cell else "")
                            row += 1
                        row += 1
                text = page.extract_text()
                if text:
                    for line in text.splitlines():
                        line = line.strip()
                        if line:
                            ws.cell(row=row, column=1, value=line)
                            row += 1
                row += 1
        out_dir = os.path.dirname(output_xlsx)
        if out_dir and not os.path.isdir(out_dir):
            os.makedirs(out_dir, exist_ok=True)
        wb.save(output_xlsx)
        if os.path.exists(output_xlsx):
            print("SUCCESS")
            sys.exit(0)
        print("ERROR: Output file was not created.", file=sys.stderr)
        sys.exit(1)
    except Exception as e:
        print(f"ERROR: {str(e)}", file=sys.stderr)
        sys.exit(1)

def __bold_font():
    try:
        from openpyxl.styles import Font
        return Font(bold=True)
    except Exception:
        return None

if __name__ == "__main__":
    if len(sys.argv) != 3:
        print("ERROR: Usage: pdf_to_excel.py <input_pdf> <output_xlsx>", file=sys.stderr)
        sys.exit(1)
    main(sys.argv[1], sys.argv[2])

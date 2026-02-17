#!/usr/bin/env python3
"""
Excel to PDF conversion using LibreOffice headless (free, no commercial libraries).
Requires: LibreOffice installed (e.g. apt install libreoffice or download from libreoffice.org)
"""
import sys
import os
import subprocess
import shutil

def find_libreoffice():
    """Find soffice/libreoffice executable."""
    paths_to_check = [
        "/usr/bin/soffice",
        "/usr/bin/libreoffice",
        "/usr/local/bin/soffice",
        "/usr/local/bin/libreoffice",
    ]
    for path in paths_to_check:
        if os.path.isfile(path):
            return path
    
    path = shutil.which("soffice") or shutil.which("libreoffice")
    if path:
        return path
    if sys.platform == "win32":
        for base in [os.environ.get("ProgramFiles", "C:\\Program Files"),
                     os.environ.get("ProgramFiles(x86)", "C:\\Program Files (x86)")]:
            for name in ["LibreOffice", "LibreOffice 24", "LibreOffice 7", "OpenOffice 4"]:
                exe = os.path.join(base, name, "program", "soffice.exe")
                if os.path.isfile(exe):
                    return exe
    return None

def main(input_excel, output_pdf):
    try:
        if not os.path.exists(input_excel):
            print(f"ERROR: Input file not found: {input_excel}", file=sys.stderr)
            sys.exit(1)
        
        out_dir = os.path.dirname(output_pdf)
        out_dir = os.path.abspath(out_dir)
        if not os.path.isdir(out_dir):
            os.makedirs(out_dir, exist_ok=True)
        
        input_abs = os.path.abspath(input_excel)
        
        lo = find_libreoffice()
        if not lo:
            print("ERROR: LibreOffice not found. Install LibreOffice (e.g. apt install libreoffice or download from libreoffice.org).", file=sys.stderr)
            sys.exit(1)
        
        # Set environment variables for better font handling
        env = os.environ.copy()
        env['HOME'] = '/tmp'
        env['SAL_USE_VCLPLUGIN'] = 'svp'
        
        # Convert Excel to PDF using LibreOffice
        cmd = [lo, "--headless", "--convert-to", "pdf", "--outdir", out_dir, input_abs]
        result = subprocess.run(cmd, capture_output=True, text=True, timeout=180, env=env)
        
        base = os.path.splitext(os.path.basename(input_excel))[0]
        expected = os.path.join(out_dir, base + ".pdf")
        
        if os.path.isfile(expected):
            if os.path.abspath(expected) != os.path.abspath(output_pdf):
                shutil.move(expected, output_pdf)
            print("SUCCESS")
            sys.exit(0)
        
        # Check for any PDF in output dir
        pdf_files = [f for f in os.listdir(out_dir) if f.endswith('.pdf')]
        if pdf_files:
            pdf_path = os.path.join(out_dir, pdf_files[0])
            if os.path.abspath(pdf_path) != os.path.abspath(output_pdf):
                shutil.move(pdf_path, output_pdf)
            print("SUCCESS")
            sys.exit(0)
        
        err = (result.stderr or "").strip() or (result.stdout or "").strip()
        print(f"ERROR: LibreOffice conversion failed. {err}", file=sys.stderr)
        sys.exit(1)
        
    except subprocess.TimeoutExpired:
        print("ERROR: Conversion timed out.", file=sys.stderr)
        sys.exit(1)
    except Exception as e:
        print(f"ERROR: {str(e)}", file=sys.stderr)
        sys.exit(1)

if __name__ == "__main__":
    if len(sys.argv) != 3:
        print("ERROR: Usage: excel_to_pdf.py <input_xlsx> <output_pdf>", file=sys.stderr)
        sys.exit(1)
    main(sys.argv[1], sys.argv[2])

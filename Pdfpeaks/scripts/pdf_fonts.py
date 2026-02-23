#!/usr/bin/env python3
"""
pdf_fonts.py — Extract font names used in a PDF.
Usage: python3 pdf_fonts.py <input.pdf>
Output: JSON array of unique font display names, e.g. ["Arial", "Times New Roman"]
"""
import sys
import json

def main():
    if len(sys.argv) < 2:
        print(json.dumps({"error": "Usage: pdf_fonts.py <input.pdf>"}))
        sys.exit(1)

    pdf_path = sys.argv[1]

    try:
        import fitz  # PyMuPDF
    except ImportError:
        # Fallback: try pdfplumber
        try:
            import pdfplumber
            fonts = set()
            with pdfplumber.open(pdf_path) as pdf:
                for page in pdf.pages:
                    for char in (page.chars or []):
                        fname = char.get('fontname', '')
                        if fname:
                            fonts.add(clean_font_name(fname))
            print(json.dumps(sorted(fonts)))
            return
        except Exception as e:
            print(json.dumps({"error": str(e)}))
            sys.exit(1)

    try:
        doc = fitz.open(pdf_path)
        fonts = set()

        for page_num in range(len(doc)):
            page = doc[page_num]
            font_list = page.get_fonts(full=True)
            for font_info in font_list:
                # font_info: (xref, ext, type, basefont, name, encoding, referencer)
                base_font = font_info[3]  # basefont name
                name      = font_info[4]  # alias / subset name
                display   = clean_font_name(base_font or name or '')
                if display:
                    fonts.add(display)

        doc.close()
        print(json.dumps(sorted(fonts)))

    except Exception as e:
        print(json.dumps({"error": str(e)}))
        sys.exit(1)


def clean_font_name(raw: str) -> str:
    """
    Strip PDF subset prefix (e.g. 'ABCDEF+Arial' → 'Arial')
    and normalize common variants.
    """
    if not raw:
        return ''
    # Remove subset prefix like "ABCDEF+"
    if '+' in raw:
        raw = raw.split('+', 1)[1]
    # Replace hyphens/commas
    raw = raw.replace(',', ' ').strip()
    # Map common internal names to display names
    ALIASES = {
        'TimesNewRoman':    'Times New Roman',
        'TimesNewRomanPS':  'Times New Roman',
        'CourierNew':       'Courier New',
        'CourierNewPS':     'Courier New',
        'Helvetica-Bold':   'Helvetica',
        'Arial-Bold':       'Arial',
        'Arial-BoldMT':     'Arial',
        'Arial-ItalicMT':   'Arial',
        'ArialMT':          'Arial',
        'TimesNewRomanPSMT':'Times New Roman',
        'ArialNarrow':      'Arial Narrow',
    }
    return ALIASES.get(raw, raw)


if __name__ == '__main__':
    main()

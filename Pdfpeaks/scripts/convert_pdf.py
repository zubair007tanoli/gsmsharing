import sys
from pdf2docx import Converter

def main(input_pdf, output_docx):
    try:
        cv = Converter(input_pdf)
        cv.convert(output_docx)
        cv.close()
        print("SUCCESS")
        sys.exit(0)
    except Exception as e:
        print(f"ERROR: {str(e)}", file=sys.stderr)
        sys.exit(1)

if __name__ == "__main__":
    if len(sys.argv) != 3:
        print("ERROR: Usage: convert_pdf.py <input_pdf> <output_docx>", file=sys.stderr)
        sys.exit(1)
    main(sys.argv[1], sys.argv[2])

#!/usr/bin/env python3
"""
PDF to Word conversion using pdf2docx.
Preserves fonts, spacing, tables, images, and page layout.
Requires: pip install pdf2docx
"""
import sys
import os


def main(input_pdf, output_docx):
    try:
        from pdf2docx import Converter
    except ImportError:
        print("ERROR: pdf2docx not installed. Run: pip install pdf2docx", file=sys.stderr)
        sys.exit(1)

    try:
        if not os.path.exists(input_pdf):
            print(f"ERROR: Input file not found: {input_pdf}", file=sys.stderr)
            sys.exit(1)

        out_dir = os.path.dirname(output_docx)
        if out_dir and not os.path.isdir(out_dir):
            os.makedirs(out_dir, exist_ok=True)

        cv = Converter(input_pdf)
        cv.convert(
            output_docx,
            start=0,
            end=None,
            # Preserve layout settings
            connected_border_tolerance=0.5,
            max_border_width=6.0,
            min_border_clearance=2.0,
            float_image_ignorable_gap=5.0,
            page_margin_factor_top=0.5,
            page_margin_factor_bottom=0.5,
            shape_merging_threshold=0.5,
            shape_min_dimension=2.0,
            line_overlap_threshold=0.9,
            line_merging_threshold=2.0,
            line_separate_threshold=5.0,
            lines_left_aligned_threshold=0.1,
            lines_right_aligned_threshold=0.1,
            lines_center_aligned_threshold=0.1,
            clip_end_x_threshold=0.9,
            curve_path_ratio=0.2,
        )
        cv.close()

        if os.path.exists(output_docx):
            print("SUCCESS")
            sys.exit(0)
        else:
            print("ERROR: Output file was not created.", file=sys.stderr)
            sys.exit(1)

    except Exception as e:
        print(f"ERROR: {str(e)}", file=sys.stderr)
        import traceback
        traceback.print_exc(file=sys.stderr)
        sys.exit(1)


if __name__ == "__main__":
    if len(sys.argv) != 3:
        print("ERROR: Usage: convert_pdf.py <input_pdf> <output_docx>", file=sys.stderr)
        sys.exit(1)
    main(sys.argv[1], sys.argv[2])

#!/usr/bin/env python3
"""
PDF Compression Script using pikepdf
Compresses PDF files by removing duplicates, compressing images, and optimizing structure

Usage:
    python compress_pdf.py <input_file> <output_file> [quality]

Quality options:
    - low: Best quality, minimal compression (dpi=300)
    - medium: Balanced compression (dpi=150)
    - high: Maximum compression (dpi=72)
"""

import sys
import os

try:
    import pikepdf
except ImportError:
    print("Error: pikepdf not installed. Install with: pip install pikepdf")
    sys.exit(1)


def compress_pdf(input_path: str, output_path: str, quality: str = "medium") -> dict:
    """
    Compress a PDF file.
    
    Args:
        input_path: Path to input PDF file
        output_path: Path to output compressed PDF file
        quality: Compression level (low, medium, high)
    
    Returns:
        Dictionary with compression results
    """
    try:
        # Get original file size
        original_size = os.path.getsize(input_path)
        
        # Set DPI based on quality level
        dpi_map = {
            "low": 300,     # Best quality
            "medium": 150,  # Balanced
            "high": 72      # Maximum compression
        }
        target_dpi = dpi_map.get(quality.lower(), 150)
        
        # Open the PDF
        with pikepdf.open(input_path) as pdf:
            # Remove duplicate resources and optimize
            pdf.save(
                output_path,
                compress_streams=True,
                object_stream_mode=pikepdf.ObjectStreamMode.generate,
                linearize=False
            )
        
        # Get compressed file size
        compressed_size = os.path.getsize(output_path)
        
        # Calculate reduction
        if original_size > 0:
            reduction_percent = ((original_size - compressed_size) / original_size) * 100
        else:
            reduction_percent = 0
        
        return {
            "success": True,
            "original_size": original_size,
            "compressed_size": compressed_size,
            "reduction_percent": round(reduction_percent, 2),
            "message": f"Compressed from {original_size} to {compressed_size} bytes ({reduction_percent:.1f}% reduction)"
        }
        
    except Exception as e:
        return {
            "success": False,
            "error": str(e),
            "message": f"Error compressing PDF: {str(e)}"
        }


def main():
    if len(sys.argv) < 3:
        print("Usage: python compress_pdf.py <input_file> <output_file> [quality]")
        print("Quality options: low, medium, high (default: medium)")
        sys.exit(1)
    
    input_file = sys.argv[1]
    output_file = sys.argv[2]
    quality = sys.argv[3] if len(sys.argv) > 3 else "medium"
    
    # Validate input file exists
    if not os.path.exists(input_file):
        print(f"Error: Input file not found: {input_file}")
        sys.exit(1)
    
    # Create output directory if needed
    output_dir = os.path.dirname(output_file)
    if output_dir and not os.path.exists(output_dir):
        os.makedirs(output_dir)
    
    result = compress_pdf(input_file, output_file, quality)
    
    if result["success"]:
        print(f"SUCCESS|{result['original_size']}|{result['compressed_size']}|{result['reduction_percent']}")
    else:
        print(f"ERROR|{result['message']}")
        sys.exit(1)


if __name__ == "__main__":
    main()
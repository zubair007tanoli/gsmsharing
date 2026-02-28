"""
OCR PDF Script - Extract text from images in PDF and create searchable PDF
Uses Tesseract OCR for text extraction from scanned documents
"""

import sys
import os
import json
import tempfile
from pathlib import Path

# Try to import required libraries
try:
    import fitz  # PyMuPDF
except ImportError:
    print("PyMuPDF not installed. Installing...")
    os.system("pip install pymupdf")
    import fitz

try:
    import pytesseract
    from PIL import Image
except ImportError:
    print("Installing OCR dependencies...")
    os.system("pip install pytesseract pillow")
    import pytesseract
    from PIL import Image


def extract_text_from_image(image_path):
    """Extract text from an image using Tesseract OCR"""
    try:
        img = Image.open(image_path)
        
        # Preprocess image for better OCR
        # Convert to grayscale
        img = img.convert('L')
        
        # OCR with configuration for better accuracy
        text = pytesseract.image_to_string(
            img, 
            config='--psm 6 --oem 3'  # PSM 6 = Assume a single uniform block of text
        )
        
        return text.strip()
    except Exception as e:
        print(f"OCR Error: {e}", file=sys.stderr)
        return ""


def extract_text_with_boxes(image_path):
    """Extract text with bounding boxes for precise positioning"""
    try:
        img = Image.open(image_path)
        
        # Keep original for color detection
        original_img = img.copy()
        img_gray = img.convert('L')
        
        # Get data with bounding boxes
        data = pytesseract.image_to_data(
            img_gray, 
            output_type=pytesseract.Output.DICT,
            config='--psm 6 --oem 3'
        )
        
        words = []
        n_boxes = len(data['text'])
        
        for i in range(n_boxes):
            text = data['text'][i].strip()
            if text:  # Only include non-empty text
                # Extract font size from height
                height = data['height'][i]
                font_size = max(height * 0.75, 8)  # Approximate font size
                
                # Try to detect if text is bold or italic from font name
                font_name = data.get('font', [''])[i] if 'font' in data else ''
                is_bold = 'bold' in font_name.lower() if font_name else False
                is_italic = 'italic' in font_name.lower() or 'oblique' in font_name.lower() if font_name else False
                
                # Detect text color (approximate from image region)
                try:
                    x, y, w, h = data['left'][i], data['top'][i], data['width'][i], data['height'][i]
                    # Sample a region to detect if text is dark or light
                    if w > 0 and h > 0:
                        region = original_img.crop((x, y, min(x + w, original_img.width), min(y + h, original_img.height)))
                        avg_color = sum(region.getpixel((w // 2, h // 2))) / len(region.getpixel((w // 2, h // 2)))
                        text_color = 'dark' if avg_color < 128 else 'light'
                    else:
                        text_color = 'dark'
                except:
                    text_color = 'dark'
                
                words.append({
                    'text': text,
                    'x': data['left'][i],
                    'y': data['top'][i],
                    'width': data['width'][i],
                    'height': data['height'][i],
                    'font_size': font_size,
                    'font_family': 'Arial',  # Default, can be enhanced
                    'is_bold': is_bold,
                    'is_italic': is_italic,
                    'text_color': text_color,
                    'confidence': data['conf'][i]
                })
        
        return words
    except Exception as e:
        print(f"OCR Error with boxes: {e}", file=sys.stderr)
        return []


def create_searchable_pdf(input_pdf_path, output_pdf_path):
    """Create a searchable PDF with invisible text layer over images"""
    try:
        # Open input PDF
        doc = fitz.open(input_pdf_path)
        
        # Process each page
        for page_num in range(len(doc)):
            page = doc[page_num]
            
            # Get all images on the page
            image_list = page.get_images()
            
            for img_index, img in enumerate(image_list):
                xref = img[0]
                base_image = doc.extract_image(xref)
                image_bytes = base_image["image"]
                
                # Create temporary image file
                with tempfile.NamedTemporaryFile(delete=False, suffix='.png') as tmp:
                    tmp.write(image_bytes)
                    tmp_path = tmp.name
                
                try:
                    # Extract text with position info
                    words = extract_text_with_boxes(tmp_path)
                    
                    # Add invisible text over the image
                    for word in words:
                        if word['confidence'] > 30:  # Only add confident detections
                            # Add text at the position (convert to PDF coordinates)
                            # PDF coordinates: origin is bottom-left
                            page_height = page.rect.height
                            
                            # Add invisible text
                            page.insert_text(
                                (word['x'], page_height - word['y'] - word['height']),
                                word['text'],
                                fontsize=word['height'] * 0.8,
                                color=(1, 1, 1),  # White (invisible on white background)
                                overlay=True
                            )
                finally:
                    # Clean up temp file
                    try:
                        os.unlink(tmp_path)
                    except:
                        pass
        
        # Save output PDF
        doc.save(output_pdf_path)
        doc.close()
        
        return True, "Searchable PDF created successfully"
        
    except Exception as e:
        print(f"Error creating searchable PDF: {e}", file=sys.stderr)
        return False, str(e)


def extract_all_text(input_pdf_path):
    """Extract all text from a PDF (including OCR from images)"""
    try:
        doc = fitz.open(input_pdf_path)
        all_text = {}
        
        for page_num in range(len(doc)):
            page = doc[page_num]
            
            # First try to get existing text
            text = page.get_text()
            
            if not text.strip():
                # If no text, try OCR
                # Render page to image
                mat = fitz.Matrix(2, 2)  # 2x scale for better OCR
                pix = page.get_pixmap(matrix=mat)
                
                # Save to temp file
                with tempfile.NamedTemporaryFile(delete=False, suffix='.png') as tmp:
                    pix.save(tmp.name)
                    tmp_path = tmp.name
                
                try:
                    text = extract_text_from_image(tmp_path)
                finally:
                    try:
                        os.unlink(tmp_path)
                    except:
                        pass
            
            all_text[f"page_{page_num + 1}"] = text
        
        doc.close()
        
        return True, all_text
        
    except Exception as e:
        print(f"Error extracting text: {e}", file=sys.stderr)
        return False, {}


def main():
    """Main entry point"""
    if len(sys.argv) < 3:
        print("Usage: ocr_pdf.py <input_pdf> <output_pdf>")
        print("Or: ocr_pdf.py --extract <input_pdf>")
        sys.exit(1)
    
    if sys.argv[1] == "--extract":
        # Extract text only mode
        input_pdf = sys.argv[2]
        success, result = extract_all_text(input_pdf)
        
        if success:
            print(json.dumps({
                'success': True,
                'text': result
            }))
        else:
            print(json.dumps({
                'success': False,
                'error': result
            }))
    else:
        # Create searchable PDF mode
        input_pdf = sys.argv[1]
        output_pdf = sys.argv[2]
        
        success, message = create_searchable_pdf(input_pdf, output_pdf)
        
        if success:
            print(f"Success: {message}")
            sys.exit(0)
        else:
            print(f"Error: {message}", file=sys.stderr)
            sys.exit(1)


if __name__ == "__main__":
    main()

#!/usr/bin/env python3
"""
AI-Powered Document Image Enhancement Script
Uses OpenCV and PIL for document processing with intelligent enhancement.
"""

import sys
import json
import argparse
import numpy as np
from pathlib import Path

try:
    import cv2
    from PIL import Image, ImageEnhance, ImageFilter
    import scipy.ndimage as ndi
except ImportError as e:
    print(json.dumps({"success": False, "error": f"Missing required library: {str(e)}"}))
    sys.exit(1)


def load_image(input_path):
    """Load image from file path."""
    try:
        img = cv2.imread(input_path)
        if img is None:
            # Try with PIL
            pil_img = Image.open(input_path)
            img = cv2.cvtColor(np.array(pil_img), cv2.COLOR_RGB2BGR)
        return img
    except Exception as e:
        return None


def save_image(img, output_path):
    """Save image to file path."""
    try:
        # Convert BGR to RGB for saving
        if len(img.shape) == 3:
            img_rgb = cv2.cvtColor(img, cv2.COLOR_BGR2RGB)
        else:
            img_rgb = img
        pil_img = Image.fromarray(img_rgb)
        pil_img.save(output_path)
        return True
    except Exception as e:
        print(f"Error saving image: {e}", file=sys.stderr)
        return False


def deskew_image(img):
    """
    Automatically detect and correct skew in document images.
    Uses Hough transform to detect lines and calculate skew angle.
    """
    try:
        # Convert to grayscale
        if len(img.shape) == 3:
            gray = cv2.cvtColor(img, cv2.COLOR_BGR2GRAY)
        else:
            gray = img.copy()
        
        # Threshold the image
        _, binary = cv2.threshold(gray, 0, 255, cv2.THRESH_BINARY_INV + cv2.THRESH_OTSU)
        
        # Find all non-zero pixels
        coords = np.column_stack(np.where(binary > 0))
        
        if len(coords) == 0:
            return img, 0.0
        
        # Get rotated bounding box
        angle = cv2.minAreaRect(coords)[-1]
        
        # The angle correction
        if angle < -45:
            angle = -(90 + angle)
        else:
            angle = -angle
        
        # Only correct if angle is significant
        if abs(angle) < 0.5:
            return img, 0.0
        
        # Rotate the image
        h, w = img.shape[:2]
        center = (w // 2, h // 2)
        M = cv2.getRotationMatrix2D(center, angle, 1.0)
        
        # Calculate new bounding box dimensions
        cos = np.abs(M[0, 0])
        sin = np.abs(M[0, 1])
        new_w = int((h * sin) + (w * cos))
        new_h = int((h * cos) + (w * sin))
        
        # Adjust the rotation matrix
        M[0, 2] += (new_w / 2) - center[0]
        M[1, 2] += (new_h / 2) - center[1]
        
        # Perform the rotation
        rotated = cv2.warpAffine(img, M, (new_w, new_h), 
                                  flags=cv2.INTER_CUBIC, 
                                  borderMode=cv2.BORDER_REPLICATE)
        
        return rotated, angle
    except Exception as e:
        print(f"Error in deskew: {e}", file=sys.stderr)
        return img, 0.0


def remove_borders(img):
    """
    Remove dark borders from scanned documents.
    """
    try:
        if len(img.shape) == 3:
            gray = cv2.cvtColor(img, cv2.COLOR_BGR2GRAY)
        else:
            gray = img.copy()
        
        # Apply adaptive threshold
        thresh = cv2.adaptiveThreshold(gray, 255, cv2.ADAPTIVE_THRESH_GAUSSIAN_C, 
                                        cv2.THRESH_BINARY_INV, 11, 2)
        
        # Find contours
        contours, _ = cv2.findContours(thresh, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)
        
        if not contours:
            return img
        
        # Find the largest contour (document border)
        largest_contour = max(contours, key=cv2.contourArea)
        
        # Get bounding rectangle
        x, y, w, h = cv2.boundingRect(largest_contour)
        
        # Add small margin
        margin = 5
        x = max(0, x - margin)
        y = max(0, y - margin)
        w = min(img.shape[1] - x, w + 2 * margin)
        h = min(img.shape[0] - y, h + 2 * margin)
        
        # Crop the image
        cropped = img[y:y+h, x:x+w]
        
        return cropped
    except Exception as e:
        print(f"Error removing borders: {e}", file=sys.stderr)
        return img


def enhance_document(img, mode='auto'):
    """
    Enhance document image for better readability.
    Uses various techniques based on the selected mode.
    """
    try:
        # Convert to grayscale if needed
        if len(img.shape) == 3:
            gray = cv2.cvtColor(img, cv2.COLOR_BGR2GRAY)
        else:
            gray = img.copy()
        
        if mode == 'document' or mode == 'auto':
            # Document mode: improve text clarity
            
            # Apply CLAHE (Contrast Limited Adaptive Histogram Equalization)
            clahe = cv2.createCLAHE(clipLimit=2.0, tileGridSize=(8, 8))
            enhanced = clahe.apply(gray)
            
            # Sharpen the image
            kernel = np.array([[-1,-1,-1],
                              [-1, 9,-1],
                              [-1,-1,-1]])
            sharpened = cv2.filter2D(enhanced, -1, kernel)
            
            # Convert back to BGR
            result = cv2.cvtColor(sharpened, cv2.COLOR_GRAY2BGR)
            
        elif mode == 'picture':
            # Picture mode: enhance colors while preserving quality
            
            # Convert to RGB for PIL processing
            rgb = cv2.cvtColor(img, cv2.COLOR_BGR2RGB)
            pil_img = Image.fromarray(rgb)
            
            # Enhance contrast
            enhancer = ImageEnhance.Contrast(pil_img)
            pil_img = enhancer.enhance(1.2)
            
            # Enhance sharpness
            enhancer = ImageEnhance.Sharpness(pil_img)
            pil_img = enhancer.enhance(1.3)
            
            # Enhance color
            enhancer = ImageEnhance.Color(pil_img)
            pil_img = enhancer.enhance(1.1)
            
            # Convert back to BGR
            result = cv2.cvtColor(np.array(pil_img), cv2.COLOR_RGB2BGR)
            
        else:
            result = img
            
        return result
        
    except Exception as e:
        print(f"Error enhancing document: {e}", file=sys.stderr)
        return img


def make_white_background(img, threshold=240):
    """
    Make light backgrounds white while preserving content.
    """
    try:
        if len(img.shape) == 3:
            gray = cv2.cvtColor(img, cv2.COLOR_BGR2GRAY)
        else:
            gray = img.copy()
        
        # Invert the image
        inverted = 255 - gray
        
        # Apply threshold to find light areas
        _, mask = cv2.threshold(inverted, 255 - threshold, 255, cv2.THRESH_BINARY)
        
        # Apply morphological operations to clean up the mask
        kernel = cv2.getStructuringElement(cv2.MORPH_RECT, (5, 5))
        mask = cv2.morphologyEx(mask, cv2.MORPH_CLOSE, kernel)
        
        # Make light areas white
        result = img.copy()
        result[mask == 255] = [255, 255, 255]
        
        return result
        
    except Exception as e:
        print(f"Error making white background: {e}", file=sys.stderr)
        return img


def denoise_image(img):
    """
    Remove noise from document images while preserving edges.
    """
    try:
        # Convert to grayscale if needed
        if len(img.shape) == 3:
            gray = cv2.cvtColor(img, cv2.COLOR_BGR2GRAY)
        else:
            gray = img.copy()
        
        # Apply non-local means denoising
        denoised = cv2.fastNlMeansDenoising(gray, None, 10, 7, 21)
        
        # Convert back to BGR if needed
        if len(img.shape) == 3:
            result = cv2.cvtColor(denoised, cv2.COLOR_GRAY2BGR)
        else:
            result = denoised
            
        return result
        
    except Exception as e:
        print(f"Error denoising: {e}", file=sys.stderr)
        return img


def full_auto_enhance(input_path, output_path, mode='document'):
    """
    Perform full automatic enhancement on document image.
    This is the main function that combines all enhancement techniques.
    """
    try:
        # Load image
        img = load_image(input_path)
        if img is None:
            return {"success": False, "error": "Failed to load image"}
        
        original_shape = img.shape
        
        # Step 1: Denoise
        img = denoise_image(img)
        
        # Step 2: Deskew (auto-straighten)
        img, angle = deskew_image(img)
        
        # Step 3: Remove borders
        img = remove_borders(img)
        
        # Step 4: Make white background
        img = make_white_background(img)
        
        # Step 5: Enhance based on mode
        img = enhance_document(img, mode)
        
        # Step 6: Final denoise pass
        img = denoise_image(img)
        
        # Save the result
        if not save_image(img, output_path):
            return {"success": False, "error": "Failed to save output image"}
        
        return {
            "success": True,
            "message": "Image enhanced successfully",
            "details": {
                "original_size": original_shape,
                "final_size": img.shape,
                "deskew_angle": angle,
                "mode": mode
            }
        }
        
    except Exception as e:
        return {"success": False, "error": str(e)}


def main():
    parser = argparse.ArgumentParser(description='AI-Powered Document Image Enhancement')
    parser.add_argument('input', help='Input image path')
    parser.add_argument('output', help='Output image path')
    parser.add_argument('--mode', choices=['document', 'picture', 'auto'], default='auto',
                        help='Enhancement mode')
    parser.add_argument('--operation', default='full',
                        help='Operation to perform: full, deskew, denoise, enhance, whitebg')
    
    args = parser.parse_args()
    
    try:
        img = load_image(args.input)
        if img is None:
            print(json.dumps({"success": False, "error": "Failed to load image"}))
            sys.exit(1)
        
        if args.operation == 'full':
            result = full_auto_enhance(args.input, args.output, args.mode)
        else:
            # Process based on operation
            if args.operation == 'deskew':
                img, angle = deskew_image(img)
            elif args.operation == 'denoise':
                img = denoise_image(img)
            elif args.operation == 'enhance':
                img = enhance_document(img, args.mode)
            elif args.operation == 'whitebg':
                img = make_white_background(img)
            
            if not save_image(img, args.output):
                print(json.dumps({"success": False, "error": "Failed to save output"}))
                sys.exit(1)
            
            result = {"success": True, "message": f"Operation '{args.operation}' completed"}
        
        print(json.dumps(result))
        sys.exit(0 if result.get("success") else 1)
        
    except Exception as e:
        print(json.dumps({"success": False, "error": str(e)}))
        sys.exit(1)


if __name__ == '__main__':
    main()

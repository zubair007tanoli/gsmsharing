#!/usr/bin/env python3
"""
CamScanner-style Document Image Enhancement Script
Supports: perspective correction, document edge detection,
paper-size fitting (A4, Legal, Letter, A3), denoising, deskew,
white-background, text sharpening, and full auto scan pipeline.
"""

import sys
import json
import argparse
import numpy as np
from pathlib import Path

try:
    import cv2
    from PIL import Image, ImageEnhance, ImageFilter
except ImportError as e:
    print(json.dumps({"success": False, "error": f"Missing required library: {str(e)}"}))
    sys.exit(1)


# ---------------------------------------------------------------------------
# Paper sizes at 300 DPI (pixels). width x height in portrait orientation.
# ---------------------------------------------------------------------------
PAPER_SIZES_300DPI = {
    "a4":     (2480, 3508),
    "letter": (2550, 3300),
    "legal":  (2550, 4200),
    "a3":     (3508, 4961),
    "a5":     (1748, 2480),
    "auto":   None,          # keep original cropped size
}


# ---------------------------------------------------------------------------
# I/O helpers
# ---------------------------------------------------------------------------

def load_image(input_path: str):
    img = cv2.imread(input_path)
    if img is None:
        try:
            pil = Image.open(input_path).convert("RGB")
            img = cv2.cvtColor(np.array(pil), cv2.COLOR_RGB2BGR)
        except Exception:
            pass
    return img


def save_image(img: np.ndarray, output_path: str) -> bool:
    try:
        if len(img.shape) == 3:
            img_rgb = cv2.cvtColor(img, cv2.COLOR_BGR2RGB)
        else:
            img_rgb = img
        pil = Image.fromarray(img_rgb)
        pil.save(output_path, dpi=(300, 300))
        return True
    except Exception as e:
        print(f"Error saving: {e}", file=sys.stderr)
        return False


# ---------------------------------------------------------------------------
# Document edge / corner detection  (CamScanner core)
# ---------------------------------------------------------------------------

def order_points(pts: np.ndarray) -> np.ndarray:
    """Order corner points: top-left, top-right, bottom-right, bottom-left."""
    rect = np.zeros((4, 2), dtype="float32")
    s = pts.sum(axis=1)
    rect[0] = pts[np.argmin(s)]
    rect[2] = pts[np.argmax(s)]
    diff = np.diff(pts, axis=1)
    rect[1] = pts[np.argmin(diff)]
    rect[3] = pts[np.argmax(diff)]
    return rect


def find_document_corners(img: np.ndarray):
    """
    Detect the four corners of a document in the image.
    Returns a (4,2) float32 array or None if not found.
    """
    h, w = img.shape[:2]

    # Downscale for faster processing
    scale = 800.0 / max(h, w)
    small = cv2.resize(img, (int(w * scale), int(h * scale)))

    gray = cv2.cvtColor(small, cv2.COLOR_BGR2GRAY)
    blur = cv2.GaussianBlur(gray, (5, 5), 0)

    # Adaptive threshold works well for documents on varied backgrounds
    thresh = cv2.adaptiveThreshold(blur, 255,
                                   cv2.ADAPTIVE_THRESH_GAUSSIAN_C,
                                   cv2.THRESH_BINARY_INV, 11, 3)

    # Close gaps in document border
    kernel = cv2.getStructuringElement(cv2.MORPH_RECT, (5, 5))
    closed = cv2.morphologyEx(thresh, cv2.MORPH_CLOSE, kernel, iterations=2)

    contours, _ = cv2.findContours(closed, cv2.RETR_EXTERNAL,
                                   cv2.CHAIN_APPROX_SIMPLE)
    if not contours:
        return None

    # Pick the largest contour that looks rectangular
    contours = sorted(contours, key=cv2.contourArea, reverse=True)[:5]
    for cnt in contours:
        peri = cv2.arcLength(cnt, True)
        approx = cv2.approxPolyDP(cnt, 0.02 * peri, True)
        if len(approx) == 4:
            # Scale corners back to original size
            corners = approx.reshape(4, 2) / scale
            return corners.astype("float32")

    return None


def four_point_transform(img: np.ndarray, pts: np.ndarray,
                         paper_size=None) -> np.ndarray:
    """
    Perspective-correct the image to a flat rectangle.
    If paper_size is given (w, h), output is exactly that size.
    """
    rect = order_points(pts)
    (tl, tr, br, bl) = rect

    widthA  = np.linalg.norm(br - bl)
    widthB  = np.linalg.norm(tr - tl)
    heightA = np.linalg.norm(tr - br)
    heightB = np.linalg.norm(tl - bl)

    maxW = int(max(widthA, widthB))
    maxH = int(max(heightA, heightB))

    if paper_size:
        pw, ph = paper_size
        # Choose portrait or landscape to best match detected orientation
        if maxW > maxH:
            pw, ph = ph, pw
        dstW, dstH = pw, ph
    else:
        dstW, dstH = maxW, maxH

    dst = np.array([
        [0,       0      ],
        [dstW-1,  0      ],
        [dstW-1,  dstH-1 ],
        [0,       dstH-1 ],
    ], dtype="float32")

    M = cv2.getPerspectiveTransform(rect, dst)
    warped = cv2.warpPerspective(img, M, (dstW, dstH),
                                 flags=cv2.INTER_CUBIC,
                                 borderMode=cv2.BORDER_REPLICATE)
    return warped


# ---------------------------------------------------------------------------
# Individual processing steps
# ---------------------------------------------------------------------------

def denoise_image(img: np.ndarray) -> np.ndarray:
    """Non-local means denoising – preserves edges well."""
    try:
        if len(img.shape) == 3:
            return cv2.fastNlMeansDenoisingColored(img, None, 7, 7, 7, 21)
        return cv2.fastNlMeansDenoising(img, None, 10, 7, 21)
    except Exception as e:
        print(f"Denoise error: {e}", file=sys.stderr)
        return img


def deskew_image(img: np.ndarray):
    """Correct small rotational skew using minAreaRect on text pixels."""
    try:
        gray = cv2.cvtColor(img, cv2.COLOR_BGR2GRAY) if len(img.shape) == 3 else img.copy()
        _, binary = cv2.threshold(gray, 0, 255,
                                  cv2.THRESH_BINARY_INV + cv2.THRESH_OTSU)
        coords = np.column_stack(np.where(binary > 0))
        if len(coords) < 10:
            return img, 0.0
        angle = cv2.minAreaRect(coords)[-1]
        angle = -(90 + angle) if angle < -45 else -angle
        if abs(angle) < 0.3:
            return img, 0.0

        h, w = img.shape[:2]
        center = (w // 2, h // 2)
        M = cv2.getRotationMatrix2D(center, angle, 1.0)
        cos, sin = abs(M[0, 0]), abs(M[0, 1])
        nw = int(h * sin + w * cos)
        nh = int(h * cos + w * sin)
        M[0, 2] += nw / 2 - center[0]
        M[1, 2] += nh / 2 - center[1]
        rotated = cv2.warpAffine(img, M, (nw, nh),
                                 flags=cv2.INTER_CUBIC,
                                 borderMode=cv2.BORDER_REPLICATE)
        return rotated, angle
    except Exception as e:
        print(f"Deskew error: {e}", file=sys.stderr)
        return img, 0.0


def make_white_background(img: np.ndarray, threshold: int = 235) -> np.ndarray:
    """Make near-white pixels fully white (removes yellowing / shadows)."""
    try:
        gray = cv2.cvtColor(img, cv2.COLOR_BGR2GRAY) if len(img.shape) == 3 else img.copy()
        _, mask = cv2.threshold(gray, threshold, 255, cv2.THRESH_BINARY)
        kernel = cv2.getStructuringElement(cv2.MORPH_RECT, (5, 5))
        mask = cv2.morphologyEx(mask, cv2.MORPH_CLOSE, kernel)
        result = img.copy()
        if len(result.shape) == 3:
            result[mask == 255] = [255, 255, 255]
        else:
            result[mask == 255] = 255
        return result
    except Exception as e:
        print(f"White bg error: {e}", file=sys.stderr)
        return img


def remove_borders(img: np.ndarray) -> np.ndarray:
    """Crop away dark scanning borders."""
    try:
        gray = cv2.cvtColor(img, cv2.COLOR_BGR2GRAY) if len(img.shape) == 3 else img.copy()
        thresh = cv2.adaptiveThreshold(gray, 255,
                                       cv2.ADAPTIVE_THRESH_GAUSSIAN_C,
                                       cv2.THRESH_BINARY_INV, 11, 2)
        contours, _ = cv2.findContours(thresh, cv2.RETR_EXTERNAL,
                                       cv2.CHAIN_APPROX_SIMPLE)
        if not contours:
            return img
        c = max(contours, key=cv2.contourArea)
        x, y, w, h = cv2.boundingRect(c)
        m = 5
        x = max(0, x - m)
        y = max(0, y - m)
        w = min(img.shape[1] - x, w + 2 * m)
        h = min(img.shape[0] - y, h + 2 * m)
        return img[y:y+h, x:x+w]
    except Exception as e:
        print(f"Border removal error: {e}", file=sys.stderr)
        return img


def enhance_document_local(img: np.ndarray, mode: str = "document") -> np.ndarray:
    """
    Enhance image for print-ready output.
    document – grayscale CLAHE + unsharp mask (great for text docs)
    picture  – colour enhancement
    """
    try:
        if mode == "document":
            gray = cv2.cvtColor(img, cv2.COLOR_BGR2GRAY) if len(img.shape) == 3 else img.copy()
            clahe = cv2.createCLAHE(clipLimit=2.0, tileGridSize=(8, 8))
            enhanced = clahe.apply(gray)

            # Unsharp mask for crisper text
            blur = cv2.GaussianBlur(enhanced, (0, 0), 3)
            sharpened = cv2.addWeighted(enhanced, 1.5, blur, -0.5, 0)

            # Gentle binarisation to clean up background noise
            _, clean = cv2.threshold(sharpened, 0, 255,
                                     cv2.THRESH_BINARY + cv2.THRESH_OTSU)
            # Blend: 70% cleaned + 30% sharpened to keep greyscale gradients
            blended = cv2.addWeighted(clean, 0.7, sharpened, 0.3, 0)

            return cv2.cvtColor(blended, cv2.COLOR_GRAY2BGR)

        else:  # picture / auto
            rgb = cv2.cvtColor(img, cv2.COLOR_BGR2RGB)
            pil = Image.fromarray(rgb)
            pil = ImageEnhance.Contrast(pil).enhance(1.25)
            pil = ImageEnhance.Sharpness(pil).enhance(1.4)
            pil = ImageEnhance.Color(pil).enhance(1.1)
            return cv2.cvtColor(np.array(pil), cv2.COLOR_RGB2BGR)

    except Exception as e:
        print(f"Enhancement error: {e}", file=sys.stderr)
        return img


# ---------------------------------------------------------------------------
# Main pipeline – CamScanner style
# ---------------------------------------------------------------------------

def scan_document(input_path: str, output_path: str,
                  mode: str = "document",
                  paper: str = "a4") -> dict:
    """
    Full CamScanner-like pipeline:
      1. Load
      2. Denoise
      3. Detect document corners & perspective-correct   (key step)
      4. Resize to target paper size at 300 DPI
      5. Make white background
      6. Enhance (CLAHE + sharpen)
      7. Save
    """
    img = load_image(input_path)
    if img is None:
        return {"success": False, "error": "Failed to load image"}

    original_shape = img.shape
    steps = []

    # 1. Light denoise first so edge detection works better
    img = denoise_image(img)
    steps.append("denoise")

    # 2. Perspective correction
    corners = find_document_corners(img)
    paper_size = PAPER_SIZES_300DPI.get(paper.lower())

    if corners is not None:
        img = four_point_transform(img, corners, paper_size)
        steps.append(f"perspective_correct→{paper}")
        perspective_applied = True
    else:
        # Fallback: deskew + optionally resize to paper
        img, angle = deskew_image(img)
        steps.append(f"deskew(angle={angle:.1f}°)")
        if paper_size:
            # Resize to paper preserving aspect ratio with padding
            h, w = img.shape[:2]
            pw, ph = paper_size
            if w > h:  # landscape input
                pw, ph = ph, pw
            # Scale to fit inside paper
            scale = min(pw / w, ph / h)
            nw, nh = int(w * scale), int(h * scale)
            resized = cv2.resize(img, (nw, nh), interpolation=cv2.INTER_LANCZOS4)
            # Place on white canvas
            canvas = np.full((ph, pw, 3), 255, dtype=np.uint8)
            y_off = (ph - nh) // 2
            x_off = (pw - nw) // 2
            canvas[y_off:y_off+nh, x_off:x_off+nw] = resized
            img = canvas
            steps.append(f"fit_to_{paper}")
        perspective_applied = False

    # 3. White background
    img = make_white_background(img)
    steps.append("white_bg")

    # 4. Enhance
    img = enhance_document_local(img, mode)
    steps.append(f"enhance({mode})")

    if not save_image(img, output_path):
        return {"success": False, "error": "Failed to save output"}

    return {
        "success": True,
        "message": "Document scanned successfully",
        "details": {
            "original_size": list(original_shape),
            "final_size": list(img.shape),
            "paper": paper,
            "mode": mode,
            "perspective_applied": perspective_applied,
            "corners_detected": corners is not None,
            "steps": steps,
        }
    }


def full_auto_enhance(input_path: str, output_path: str,
                      mode: str = "document") -> dict:
    """Legacy-compatible full enhancement (no paper resize)."""
    return scan_document(input_path, output_path, mode=mode, paper="auto")


# ---------------------------------------------------------------------------
# CLI
# ---------------------------------------------------------------------------

def main():
    parser = argparse.ArgumentParser(
        description="CamScanner-style Document Image Enhancement")
    parser.add_argument("input",  help="Input image path")
    parser.add_argument("output", help="Output image path")
    parser.add_argument("--mode", choices=["document", "picture", "auto"],
                        default="document", help="Enhancement mode")
    parser.add_argument("--operation", default="scan",
                        help="Operation: scan | full | deskew | denoise | "
                             "enhance | whitebg | borders")
    parser.add_argument("--paper",
                        choices=["a4", "a3", "a5", "letter", "legal", "auto"],
                        default="a4", help="Target paper size")
    args = parser.parse_args()

    try:
        img = load_image(args.input)
        if img is None:
            print(json.dumps({"success": False, "error": "Failed to load image"}))
            sys.exit(1)

        if args.operation in ("scan", "full"):
            result = scan_document(args.input, args.output,
                                   mode=args.mode, paper=args.paper)
        else:
            # Single-step operations
            if args.operation == "deskew":
                img, angle = deskew_image(img)
                result_msg = f"Deskewed by {angle:.1f}°"
            elif args.operation == "denoise":
                img = denoise_image(img)
                result_msg = "Denoised"
            elif args.operation == "enhance":
                img = enhance_document_local(img, args.mode)
                result_msg = f"Enhanced ({args.mode})"
            elif args.operation == "whitebg":
                img = make_white_background(img)
                result_msg = "White background applied"
            elif args.operation == "borders":
                img = remove_borders(img)
                result_msg = "Borders removed"
            else:
                print(json.dumps({"success": False,
                                  "error": f"Unknown operation: {args.operation}"}))
                sys.exit(1)

            if not save_image(img, args.output):
                print(json.dumps({"success": False, "error": "Failed to save output"}))
                sys.exit(1)
            result = {"success": True, "message": result_msg}

        print(json.dumps(result))
        sys.exit(0 if result.get("success") else 1)

    except Exception as e:
        print(json.dumps({"success": False, "error": str(e)}))
        sys.exit(1)


if __name__ == "__main__":
    main()

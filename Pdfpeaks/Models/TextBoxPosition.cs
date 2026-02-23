namespace Pdfpeaks.Models;

/// <summary>
/// Represents a text box position and formatting in the PDF editor
/// </summary>
public class TextBoxPosition
{
    /// <summary>
    /// Page number (1-based)
    /// </summary>
    public int Page { get; set; }
    
    /// <summary>
    /// X position on the page
    /// </summary>
    public double X { get; set; }
    
    /// <summary>
    /// Y position on the page
    /// </summary>
    public double Y { get; set; }
    
    /// <summary>
    /// Width of the text box
    /// </summary>
    public double Width { get; set; } = 200;
    
    /// <summary>
    /// Height of the text box
    /// </summary>
    public double Height { get; set; } = 40;
    
    /// <summary>
    /// Width of the rendered page image in pixels (used for coordinate scaling).
    /// Frontend should send this for pixel-perfect placement.
    /// Defaults to 0, in which case 96dpi render from 72pt PDF is assumed.
    /// </summary>
    public double ImageWidth { get; set; } = 0;
    
    /// <summary>
    /// Text content
    /// </summary>
    public string Text { get; set; } = string.Empty;
    
    /// <summary>
    /// Font family name
    /// </summary>
    public string FontFamily { get; set; } = "Arial";
    
    /// <summary>
    /// Font size in points
    /// </summary>
    public double FontSize { get; set; } = 16;
    
    /// <summary>
    /// Text color in hex format
    /// </summary>
    public string TextColor { get; set; } = "#000000";
    
    /// <summary>
    /// Background color (supports rgba for opacity)
    /// </summary>
    public string BgColor { get; set; } = "rgba(255,255,255,0)";
    
    /// <summary>
    /// Is text bold
    /// </summary>
    public bool Bold { get; set; }
    
    /// <summary>
    /// Is text italic
    /// </summary>
    public bool Italic { get; set; }
    
    /// <summary>
    /// Is text underlined
    /// </summary>
    public bool Underline { get; set; }
    
    /// <summary>
    /// Is text strikethrough
    /// </summary>
    public bool Strikethrough { get; set; }
    
    /// <summary>
    /// Text alignment: left, center, right, justify
    /// </summary>
    public string Align { get; set; } = "left";
    
    /// <summary>
    /// Line height multiplier
    /// </summary>
    public double LineHeight { get; set; } = 1.5;
    
    /// <summary>
    /// Letter spacing in points
    /// </summary>
    public double LetterSpacing { get; set; }
}

/// <summary>
/// Represents a text element extracted from a PDF
/// </summary>
public class PdfTextElement
{
    /// <summary>
    /// Text content
    /// </summary>
    public string Text { get; set; } = "";
    
    /// <summary>
    /// X position on the page
    /// </summary>
    public double X { get; set; }
    
    /// <summary>
    /// Y position on the page
    /// </summary>
    public double Y { get; set; }
    
    /// <summary>
    /// Width of the text element
    /// </summary>
    public double Width { get; set; }
    
    /// <summary>
    /// Height of the text element
    /// </summary>
    public double Height { get; set; }
    
    /// <summary>
    /// Font family name
    /// </summary>
    public string FontFamily { get; set; } = "Arial";
    
    /// <summary>
    /// Font size in points
    /// </summary>
    public double FontSize { get; set; }
    
    /// <summary>
    /// Is text bold
    /// </summary>
    public bool IsBold { get; set; }
    
    /// <summary>
    /// Is text italic
    /// </summary>
    public bool IsItalic { get; set; }
}

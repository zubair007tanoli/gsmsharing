namespace Pdfpeaks.Models;

/// <summary>
/// Represents a text element extracted from PDF
/// </summary>
public class TextElement
{
    public string Text { get; set; } = string.Empty;
    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public double FontSize { get; set; }
    public string FontFamily { get; set; } = "Arial";
    public string Color { get; set; } = "#000000";
}

/// <summary>
/// Response model for OCR operations
/// </summary>
public class OcrResponse
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public List<TextElement>? TextElements { get; set; }
    public int PageWidth { get; set; }
    public int PageHeight { get; set; }
}

/// <summary>
/// Represents a text box to be added to PDF
/// </summary>
public class TextBoxData
{
    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public string Text { get; set; } = string.Empty;
    public int FontSize { get; set; } = 14;
    public string FontFamily { get; set; } = "Arial";
    public string Color { get; set; } = "#000000";
    public string BgColor { get; set; } = "transparent";
    public bool Bold { get; set; }
    public bool Italic { get; set; }
    public bool Underline { get; set; }
}

/// <summary>
/// Page text boxes for saving edited PDF
/// </summary>
public class PageTextBox
{
    public int Page { get; set; }
    public List<TextBoxData> Boxes { get; set; } = new();
}

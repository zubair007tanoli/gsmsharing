using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace PdfPeaksApp
{
    public partial class MainPage : ContentPage
    {
        public ObservableCollection<QuickAction> QuickActions { get; } = new();
        public ObservableCollection<AIWorkflow> AIWorkflows { get; } = new();
        public ObservableCollection<DocumentCard> RecentDocuments { get; } = new();

        public MainPage()
        {
            InitializeComponent();
            BindingContext = this;

            SeedQuickActions();
            SeedWorkflows();
            SeedDocuments();
        }

        private void SeedQuickActions()
        {
            QuickActions.Add(new QuickAction("Smart Scan", "Auto edge detection, deskew and batch capture", "scan", BrushFrom("#12D5B5"), BrushFrom("#E9FFFA"), "📄"));
            QuickActions.Add(new QuickAction("ID / Passport", "AI forgery checks, MRZ capture, glare handling", "identity", BrushFrom("#00B483"), BrushFrom("#E6FFF6"), "🪪"));
            QuickActions.Add(new QuickAction("Photo → PDF", "Clean up photos and export as crystal clear PDFs", "photo-to-pdf", BrushFrom("#28C5E5"), BrushFrom("#E6F7FF"), "🖼️"));
            QuickActions.Add(new QuickAction("Import from Gallery", "Grab existing images and enhance instantly", "import", BrushFrom("#7C6CF8"), BrushFrom("#EFE9FF"), "🗂️"));
        }

        private void SeedWorkflows()
        {
            AIWorkflows.Add(new AIWorkflow("Passport Detection", "Crop, straighten, detect MRZ lines", 0.86, "ai-passport", BrushFrom("#00B483"), "🛂"));
            AIWorkflows.Add(new AIWorkflow("ID Anti‑Fraud", "Glare + tamper signals in real time", 0.72, "ai-idfraud", BrushFrom("#12D5B5"), "🛡️"));
            AIWorkflows.Add(new AIWorkflow("Form → PDF + Text", "Align forms, OCR and export to PDF", 0.64, "ai-forms", BrushFrom("#28C5E5"), "📑"));
            AIWorkflows.Add(new AIWorkflow("Tables → Excel", "Detect tables and export to XLSX", 0.58, "ai-tables", BrushFrom("#7C6CF8"), "📊"));
        }

        private void SeedDocuments()
        {
            RecentDocuments.Add(new DocumentCard(
                "Client Passport Verification",
                "2 pages • crop + enhance",
                "AI Verified",
                0.92,
                "Today · 10:12 AM",
                "Confidence 92% • Auto-edge + anti-glare",
                "🛂"));

            RecentDocuments.Add(new DocumentCard(
                "Invoice_Jan24.pdf",
                "5 pages • OCR text embedded",
                "OCR",
                0.81,
                "Yesterday · 6:48 PM",
                "Confidence 81% • Searchable PDF ready",
                "🧾"));

            RecentDocuments.Add(new DocumentCard(
                "Lecture Notes",
                "8 photos • Magic color + cleanup",
                "Multi-page",
                0.75,
                "Feb 28 · 3:05 PM",
                "Confidence 75% • Auto deskew applied",
                "📚"));
        }

        private static SolidColorBrush BrushFrom(string hex) => new(Color.FromArgb(hex));

        private async void OnScanClicked(object? sender, EventArgs e)
        {
            await DisplayAlertAsync("Scanner", "Launching AI scanner with auto-edge detection and OCR.", "OK");
        }

        private async void OnQuickActionTapped(object? sender, TappedEventArgs e)
        {
            if (e.Parameter is string key)
            {
                await DisplayAlertAsync("Quick Action", $"Starting {key} workflow with AI enhancements.", "OK");
            }
        }

        private async void OnWorkflowTapped(object? sender, TappedEventArgs e)
        {
            if (e.Parameter is string key)
            {
                await DisplayAlertAsync("AI Desk", $"Opening {key} detector with real-time checks.", "OK");
            }
        }

        private async void OnDocumentActionClicked(object? sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is string title)
            {
                var action = button.Text?.ToLowerInvariant() ?? "open";
                await DisplayAlertAsync("Documents", $"{action} → {title}", "OK");
            }
        }
    }

    public record QuickAction(string Title, string Subtitle, string ActionKey, Brush AccentBrush, Brush SurfaceBrush, string Icon);

    public record AIWorkflow(string Title, string Subtitle, double Confidence, string ActionKey, Brush AccentBrush, string Icon)
    {
        public string ConfidenceText => $"Confidence {(int)(Confidence * 100)}%";
    }

    public record DocumentCard(string Title, string Subtitle, string Badge, double Confidence, string Timestamp, string ConfidenceLabel, string Icon);
}

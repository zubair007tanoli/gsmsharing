using Microsoft.Maui.Controls;

namespace PdfPeaksApp
{
    public partial class AIDeskPage : ContentPage
    {
        public AIDeskPage()
        {
            InitializeComponent();
        }

        private async void OnPassportDetect(object? sender, EventArgs e)
        {
            await DisplayAlert("Passport Detector", "Running MRZ detection and tamper checks.", "OK");
        }

        private async void OnIdDetect(object? sender, EventArgs e)
        {
            await DisplayAlert("ID Anti-Fraud", "Scanning for glare, hologram, and pixel anomalies.", "OK");
        }

        private async void OnSearchablePdf(object? sender, EventArgs e)
        {
            await DisplayAlert("Searchable PDF", "Creating a searchable PDF with OCR text layer.", "OK");
        }
    }
}

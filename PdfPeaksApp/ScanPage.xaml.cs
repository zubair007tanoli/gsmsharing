using Microsoft.Maui.Controls;

namespace PdfPeaksApp
{
    public partial class ScanPage : ContentPage
    {
        public ScanPage()
        {
            InitializeComponent();
        }

        private async void OnStartCamera(object? sender, EventArgs e)
        {
            await DisplayAlert("Camera Scan", "Launching camera with auto-edge detection and AI cleanup.", "OK");
        }

        private async void OnImportImages(object? sender, EventArgs e)
        {
            await DisplayAlert("Gallery Import", "Opening gallery to pick images for conversion.", "OK");
        }
    }
}

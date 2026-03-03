using Microsoft.Maui.Controls;

namespace PdfPeaksApp
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void OnScanClicked(object? sender, EventArgs e)
        {
            // Navigate to Scanner screen
            await DisplayAlertAsync("Scanner", "Opening scanner...", "OK");
            // In a full implementation, this would navigate to the ScannerPage
            // await Navigation.PushAsync(new ScannerPage());
        }

        private async void OnDocumentsClicked(object? sender, EventArgs e)
        {
            // Navigate to Documents screen
            await DisplayAlertAsync("Documents", "Opening documents...", "OK");
            // In a full implementation, this would navigate to the DocumentsPage
            // await Navigation.PushAsync(new DocumentsPage());
        }

        private async void OnSettingsClicked(object? sender, EventArgs e)
        {
            // Navigate to Settings screen
            await DisplayAlertAsync("Settings", "Opening settings...", "OK");
            // In a full implementation, this would navigate to the SettingsPage
            // await Navigation.PushAsync(new SettingsPage());
        }
    }
}

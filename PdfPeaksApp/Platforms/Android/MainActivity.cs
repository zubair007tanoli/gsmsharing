using Android.App;
using Android.Content.PM;
using Android.OS;

namespace PdfPeaksApp
{
    // Android 12+ requires activities with intent filters (like the launcher)
    // to declare Exported = true, otherwise installation fails with a generic
    // "can't install app on this device" message.
    [Activity(
        Theme = "@style/Maui.SplashTheme",
        MainLauncher = true,
        LaunchMode = LaunchMode.SingleTop,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density,
        Exported = true)]
    public class MainActivity : MauiAppCompatActivity
    {
    }
}

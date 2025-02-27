using Android.App;
using Android.Content.PM;
using Android.OS;
using AndroidX.Core.Content;
using Avalonia;
using Avalonia.Android;
using Java.Net;
using Java.Security;

namespace soqba.Android;

[Activity(
    Label = "soqba.Android",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity<App>
{
}

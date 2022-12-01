using Android.App;
using Android.Runtime;

namespace Client.Platforms.Android;

[Application(UsesCleartextTraffic = false)]
public class MainApplication : MauiApplication
{
    public MainApplication(IntPtr handle, JniHandleOwnership ownership) : base(handle, ownership)
    {
    }

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}

using Client.Utility;
using System.Diagnostics;

namespace Client.Pages.DebugOnly;

public partial class LoggingPage : ContentPage
{
	public LoggingPage()
	{
		InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);

		Process process = Process.GetCurrentProcess();

		long total = process.WorkingSet64;

        Logger.Push($"Memory Usage: {toGb(total).ToString().Substring(0,4)}gb", TraceType.Func, LogLevel.Debug);
    }

	double toGb(long bytes) => (bytes / Math.Pow(10, 9));
}
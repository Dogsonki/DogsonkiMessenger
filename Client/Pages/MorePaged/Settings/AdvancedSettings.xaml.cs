using Client.IO;
using Client.Networking.Core;
using Client.Utility;

namespace Client.Pages.Settings;

public partial class AdvancedSettings : ContentPage
{
	public AdvancedSettings()
	{
		InitializeComponent();
		NavigationPage.SetHasNavigationBar(this, false);
	}

    public void ClearCache(object sender, EventArgs e) => Task.Run(() => Cache.ClearAbsoluteCache());

    public void SendLogs(object sender, EventArgs e)
    {
        List<string> json = new List<string>(Logger.LoggerStack.Count);

        foreach (var _ in Logger.LoggerStack)
        {
            json.Add(_.message);
        }

        SocketCore.Send(json);
    }
}
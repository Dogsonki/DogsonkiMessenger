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

    #region __CRASH__TESTS__

    public void Crash1(object sender, EventArgs e)
    {
        int[] G = null;
        Debug.Write(G[1]);
    }

    public void Crash2(object sender, EventArgs e)
    {
        throw new Exception("CRASH_TEST_INVOKED");
    }

    public void Crash3(object sender, EventArgs e)
    {
        int _ = int.MaxValue; _++;
        int[] __ = new int[_];
    }

    #endregion

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
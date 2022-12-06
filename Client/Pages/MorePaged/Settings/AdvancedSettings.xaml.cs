using Client.IO;
using Client.Networking.Core;

/* Unmerged change from project 'Client (net6.0-windows10.0.19041.0)'
Before:
using Client.Utility;
After:
using Client.Utility;
using Client.Utility.Logger;
using Client.Utility.Logger.Logger;
*/

/* Unmerged change from project 'Client (net6.0-android)'
Before:
using Client.Utility;
After:
using Client.Utility;
using Client.Utility.Logger;
*/

/* Unmerged change from project 'Client (net6.0-maccatalyst)'
Before:
using Client.Utility;
After:
using Client.Utility;
using Client.Utility.Logger;
using Client.Utility.Logger.Logger;
using Client.Utility.Logger.Logger.Logger;
*/
using Client.Utility;

/* Unmerged change from project 'Client (net6.0-android)'
Before:
using Client.Utility.Logger.Logger.Logger;
using Client.Utility.Logger.Logger.Logger.Logger;
After:
using Client.Utility.Logger.Logger;
using Client.Utility.Logger.Logger.Logger;
*/

/* Unmerged change from project 'Client (net6.0-maccatalyst)'
Before:
using Client.Utility.Logger.Logger.Logger.Logger;
After:
using Client.Utility.Logger.Logger;
*/

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
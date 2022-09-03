using Client.IO;

namespace Client.Pages.Settings;

public partial class AdvancedSettings : ContentPage
{
	public AdvancedSettings()
	{
		InitializeComponent();
		NavigationPage.SetHasNavigationBar(this, false);
	}

    public void ClearCache(object sender, EventArgs e) => Task.Run(() => Cache.ClearAbsoluteCache());
}
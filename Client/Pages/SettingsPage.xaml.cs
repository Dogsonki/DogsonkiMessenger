using Client.IO.Cache;
using Client.Models.Bindable;
using Client.Pages.Settings;

namespace Client.Pages;

public partial class SettingsPage : ContentPage
{
    public bool DebugSettingsVisible { get; set; } = false;

    public SettingsPage()
    {
        InitializeComponent();
#if DEBUG 
        DebugSettingsVisible = true;
#else
        DebugSettingsVisible = false;
#endif
        NavigationPage.SetHasNavigationBar(this, false);
    }

    private async void ShowProfileSettings(object sender, EventArgs e) => await Navigation.PushAsync(new ProfileSettings());
    private async void ShowAdvancedSettings(object sender, EventArgs e) => await Navigation.PushAsync(new AdvancedSettings());
    private async void ShowCommandsList(object sender, EventArgs e) => await Navigation.PushAsync(new ChatCommands());

    private void Logout(object sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            LocalUser.Logout();
            Navigation.PushAsync(new LoginPage());
        });
    }

    private void ClearCache(object sender, EventArgs e)
    {
        Cache.ClearAbsoluteCache();
    }

    private void UseLoggerSwitched(object sender, EventArgs e)
    {
        Switch sw = (Switch)sender;
        sw.IsToggled = !sw.IsToggled;
    }
}
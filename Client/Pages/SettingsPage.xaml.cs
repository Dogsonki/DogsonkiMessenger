using Client.IO;
using Client.Models.UserType.Bindable;
using Client.Networking.Core;
using Client.Pages.TemporaryPages.GroupChat;
using Client.Utility;

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

    private async void ChangeAvatar(object sender, EventArgs e)
    {
        try
        {
            var image = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
            {
                Title = "Pick avatar"
            });

            if (image == null)
                return;

            byte[] ImageBuffer;

            Stream stream = await image.OpenReadAsync();
            ImageBuffer = stream.StreamToBuffer();

            SocketCore.Send(ImageBuffer, Token.CHANGE_AVATAR);

            LocalUser.Current.Avatar = ImageSource.FromStream(() => new MemoryStream(ImageBuffer));

            Cache.SaveToCache(ImageBuffer, "avatar" + LocalUser.id);

            stream.Close();
        }
        catch (Exception ex)
        {
            Logger.Push(ex, TraceType.Func, LogLevel.Error);
        }
    }

    private async void ShowConsole(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new DebugOnly.LoggingPage());
    }

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
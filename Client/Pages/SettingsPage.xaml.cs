using Client.IO;
using Client.Models.UserType.Bindable;
using Client.Networking.Core;
using Client.Networking.Model;
using Client.Utility;
using System.Drawing;

using Image = System.Drawing.Image;
using Color = System.Drawing.Color;
using System.Drawing.Drawing2D;
using Brush = System.Drawing.Brush;

namespace Client.Pages;

public partial class SettingsPage : ContentPage
{
	public SettingsPage()
	{
		InitializeComponent();
		NavigationPage.SetHasNavigationBar(this, false);
	}

    protected override bool OnBackButtonPressed()
    {
        return base.OnBackButtonPressed();
    }

    private async void ChangeAvatar(object sender, EventArgs e)
    {
        var image = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
        {
            Title = "Pick avatar"
        });

        if (image == null)
            return;

        byte[] ImageBuffer;

        Stream stream = await image.OpenReadAsync();
        ImageBuffer = Essential.StreamToBuffer(stream, stream.Length);

        SocketCore.Send(ImageBuffer, Token.CHANGE_AVATAR);

        LocalUser.Current.Avatar = ImageSource.FromStream(() => new MemoryStream(ImageBuffer));

        Cache.SaveToCache(ImageBuffer, "avatar" + LocalUser.id);

        stream.Close();
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
        foreach(var file in Directory.GetFiles(FileSystem.Current.CacheDirectory + "/temp/"))
        {
            Logger.Push($"[Cache] Deleting file {file}",TraceType.Func,LogLevel.Warning);
            try
            {
                File.Delete(file);
            }
            catch(Exception ex)
            {
                Logger.Push($"Cannot delete cache file: {ex}",TraceType.Func,LogLevel.Error);
            }
            Logger.Push("[Cache] Cache cleared, remember that clearing cache will not have impact on temporary memory", TraceType.Func, LogLevel.Debug);
        }
    }

    private async void CreateGroup(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new CreateGroupChatPage());
    }
}
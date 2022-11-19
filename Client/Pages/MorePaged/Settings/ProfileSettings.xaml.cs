using Client.IO;
using Client.Models.Bindable;
using Client.Networking.Core;
using Client.Utility;

namespace Client.Pages.Settings;

public partial class ProfileSettings : ContentPage
{
	public ProfileSettings()
	{
		InitializeComponent();
		NavigationPage.SetHasNavigationBar(this, false);
	}

    private async void ChangePassword(object sender, EventArgs e)
    {

    }

    private async void ChangeAvatar(object sender, EventArgs e)
    {
        try
        {
            var image = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
            {
                Title = "Pick avatar"
            });

            if (image is null) return;

            byte[] ImageBuffer;

            Cache.RemoveFromCache("user_avatar"+LocalUser.id);

            Stream stream = await image.OpenReadAsync();
            ImageBuffer = stream.StreamToBuffer();

            SocketCore.Send(ImageBuffer, Token.CHANGE_AVATAR);

            MainThread.BeginInvokeOnMainThread(() =>
            {
                LocalUser.Current.Avatar = ImageSource.FromStream(() => new MemoryStream(ImageBuffer));
            });

            Cache.SaveToCache(ImageBuffer, "user_avatar" + LocalUser.id);

            stream.Close();
        }
        catch (Exception ex)
        {
            Logger.Push(ex, TraceType.Func, LogLevel.Error);
        }
    }
}
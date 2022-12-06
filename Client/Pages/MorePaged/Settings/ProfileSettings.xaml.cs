using Client.IO;
using Client.Models.Bindable;
using Client.Networking.Core;
using Client.Networking.Models;

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

            Cache.RemoveFromCache("user_avatar"+LocalUser.Current.Id);

            Stream stream = await image.OpenReadAsync();
            ImageBuffer = stream.StreamToBuffer();

            SocketCore.Send(ImageBuffer, Token.CHANGE_AVATAR);

            MainThread.BeginInvokeOnMainThread(() =>
            {
                LocalUser.Current.Avatar = ImageSource.FromStream(() => new MemoryStream(ImageBuffer));
            });

            Cache.SaveToCache(ImageBuffer, "user_avatar" + LocalUser.Current.Id);

            stream.Close();
        }
        catch (Exception ex)
        {
            Logger.Push(ex, LogLevel.Error);
        }
    }
}
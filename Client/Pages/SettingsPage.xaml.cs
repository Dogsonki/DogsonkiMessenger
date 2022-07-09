using Client.IO;
using Client.Models.UserType.Bindable;
using Client.Networking.Core;
using Client.Networking.Model;
using Client.Utility;

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
            Title = "Pick new avatar"
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

    private void Logout(object sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            LocalUser.Logout();
            Navigation.PushAsync(new LoginPage());
        });
    }

    private async void CreateGroup(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new CreateGroupChatPage());
    }
}
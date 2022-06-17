using Client.Networking.Core;
using Client.Networking.Model;
using Client.Utility;

namespace Client.Pages;

public partial class ProfileSettingsPage : ContentPage
{
	public ProfileSettingsPage()
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
        stream.Close();
    }
}
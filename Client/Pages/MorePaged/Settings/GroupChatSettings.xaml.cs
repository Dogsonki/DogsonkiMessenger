using Client.Models;
using Client.Models.Bindable;
using Client.Networking.Core;
using Client.Utility;

namespace Client.Pages.Settings;

public partial class GroupChatSettings : ContentPage
{
	public GroupChatSettings()
	{
		InitializeComponent();
		NavigationPage.SetHasNavigationBar(this,false);
	}

	private void LeaveGroup()
	{
		SocketCore.Send($"{LocalUser.Id}", Token.GROUP_USER_KICK);
	}

    private async void ChangeGroupIcon(object? sender, EventArgs e)
    {
        try
        {
            var image = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
            {
                Title = "Pick Group Image"
            });

            if (image is null) return;

            Stream stream = await image.OpenReadAsync();
            byte[] imageBuffer = stream.StreamToBuffer();

            Group group = Conversation.Current.GetCurrentGroupChat();

            MainThread.BeginInvokeOnMainThread(() =>
            {
                group.Avatar = ImageSource.FromStream(() => new MemoryStream(imageBuffer));
            });
            
            stream.Close();
        }
        catch (Exception ex)
        {
            Logger.Push(ex, TraceType.Func, LogLevel.Error);
        }
    }

}
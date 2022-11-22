using System.Text;
using Client.Models;
using Client.Models.Bindable;
using Client.Networking.Core;
using Client.Networking.Packets;
using Client.Utility;
using Newtonsoft.Json;

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

    private async void ChangeGroupAvatar(object? sender, EventArgs e)
    {
        try
        {
            var image = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
            {
                Title = "Pick Group Image"
            });

            if (image is null) return;

            Stream stream = await image.OpenReadAsync();
            byte[] avatarBuffer = stream.StreamToBuffer();

            Group group = Conversation.Current.GetCurrentGroupChat();

            GroupImageRequestPacket packet = new GroupImageRequestPacket(avatarBuffer, group.Id);
            Debug.Write($"Sending avatar {avatarBuffer.Length}");
            SocketCore.Send(packet, Token.GROUP_AVATAR_SET, false);

            MainThread.BeginInvokeOnMainThread(() =>
            {
                group.Avatar = ImageSource.FromStream(() => new MemoryStream(avatarBuffer));
            });

            stream.Close();
        }
        catch (Exception ex)
        {
            Logger.Push(ex, TraceType.Func, LogLevel.Error);
        }
    }
}
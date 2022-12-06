using System.Text;
using Client.IO;
using Client.Models;
using Client.Models.Bindable;
using Client.Networking.Core;
using Client.Networking.Models;
using Client.Networking.Packets;
using Client.Utility;

/* Unmerged change from project 'Client (net6.0-windows10.0.19041.0)'
Before:
using Newtonsoft.Json;
After:
using Client.Utility.Logger;
using Client.Utility.Logger.Logger;
using Newtonsoft.Json;
*/

/* Unmerged change from project 'Client (net6.0-android)'
Before:
using Newtonsoft.Json;
After:
using Client.Utility.Logger;
using Newtonsoft.Json;
*/

/* Unmerged change from project 'Client (net6.0-maccatalyst)'
Before:
using Newtonsoft.Json;
After:
using Client.Utility.Logger;
using Client.Utility.Logger.Logger;
using Client.Utility.Logger.Logger.Logger;
using Newtonsoft.Json;
*/

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
		SocketCore.Send($"{LocalUser.Current.Id}", Token.GROUP_USER_KICK);
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

            IViewBindable group = Conversation.Current.GetCurrentConversation;

            GroupImageRequestPacket packet = new GroupImageRequestPacket(avatarBuffer, group.Id);
            SocketCore.Send(packet, Token.GROUP_AVATAR_SET, false);

            AvatarManager.SetAvatar(group, avatarBuffer);

            stream.Close();
        }
        catch (Exception ex)
        {
            Logger.Push(ex, LogLevel.Error);
        }
    }
}
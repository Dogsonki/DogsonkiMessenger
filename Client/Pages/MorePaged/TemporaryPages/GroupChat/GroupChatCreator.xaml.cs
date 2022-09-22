using Client.Models;
using Client.Models.UserType.Bindable;
using Client.Networking.Core;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using Client.Networking.Packets;
using Client.Utility;
using Newtonsoft.Json;
using Client.IO;

namespace Client.Pages.TemporaryPages.GroupChat;

public partial class GroupChatCreator : ContentPage
{
    public static ObservableCollection<AnyListBindable> Invited { get; set; } = new ObservableCollection<AnyListBindable>();
    public static ObservableCollection<AnyListBindable> UsersFound { get; set; } = new ObservableCollection<AnyListBindable>();

    private byte[] GroupAvatarBuffer;

    public GroupChatCreator()
    {
        Invited.Clear();
        UsersFound.Clear();

        if(LocalUser.UserRef != null)
        {
            Invited.Add(new AnyListBindable(LocalUser.UserRef));
        }
      
        LocalUser.isCreatingGroup = true;

        InitializeComponent();

        GroupName.Text = LocalUser.UserRef.Username + "'s Chat Group";

        NavigationPage.SetHasNavigationBar(this, false);
    }

    private void Search(object sender, EventArgs e)
    {
        UsersFound.Clear();

        string input = ((Entry)sender).Text;

        if (!string.IsNullOrEmpty(input))
        {
            SocketCore.Send(input, Token.SEARCH_USER);
        }
    }

    public static void ParseFound(object req)
    {
        List<SearchModel> users = ((JArray)req).ToObject<List<SearchModel>>();

        foreach (var user in users)
        {
            AnyListBindable br = new(User.CreateOrGet(user.Username, user.Id))
            {
                Input = new Command(AddToInvited)
            };

            MainThread.BeginInvokeOnMainThread(() =>
            {
                UsersFound.Add(br);
            });
        }
    }

    public static void AddToInvited(object user)
    {
        AnyListBindable AddedUser = (AnyListBindable)user;

        foreach(AnyListBindable u in Invited)
        {
            if(u.Id == AddedUser.Id)
            {
                return;
            }
        }
        Invited.Add(AddedUser);
    }

    private void CreateGroup(object sender, EventArgs e)
    {
        List<int> users = new(Invited.Count);

        for(int i = 0; i < Invited.Count; i++)
        {
            int id = Invited[i].Id;

            if (id != LocalUser.Id)
            {
                users.Add(id);
            }
        }

        SocketCore.SendCallback(
            CreateGroupCallback,
            new GroupChatCreatePacket(GroupName.Text, int.Parse(LocalUser.id), users.ToArray()),
            Token.GROUP_CHAT_CREATE);
    }

    private void CreateGroupCallback(object data)
    {
        GroupChatCreateCallbackPacket? groupPacket = JsonConvert.DeserializeObject<GroupChatCreateCallbackPacket>((string)data);

        if (groupPacket is null)
        {
            Logger.Push("GroupCreatePacket_NULL",TraceType.Func,LogLevel.Error);
            return;
        }

        Group group = Group.CreateOrGet(groupPacket.GroupName,groupPacket.GroupId);

        SocketCore.Send($"{group.Id}", Token.GROUP_CHAT_INIT);
        MainThread.BeginInvokeOnMainThread(() =>
        {
            StaticNavigator.Push(new MessagePage(group));
        });

        if (GroupAvatarBuffer is not null && GroupAvatarBuffer.Length > 0)
        {
            SocketCore.Send(GroupAvatarBuffer, Token.GROUP_AVATAR_SET);
        }
    }

    private async void GroupImageChange(object? sender, EventArgs e)
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

            GroupAvatarBuffer = new byte[imageBuffer.Length];

            Array.Copy(imageBuffer,GroupAvatarBuffer,imageBuffer.Length);

            GroupImage.Source = ImageSource.FromStream(() => new MemoryStream(imageBuffer));

            stream.Close();
        }
        catch (Exception ex)
        {
            Logger.Push(ex, TraceType.Func, LogLevel.Error);
        }
    }
}
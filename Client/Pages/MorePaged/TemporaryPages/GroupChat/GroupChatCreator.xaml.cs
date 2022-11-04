using Client.Models.Bindable;
using Client.Networking.Core;
using System.Collections.ObjectModel;
using Client.Networking.Packets;
using Client.Networking.Packets.Models;
using Client.Utility;
using Newtonsoft.Json;

namespace Client.Pages.TemporaryPages.GroupChat;

public partial class GroupChatCreator : ContentPage
{
    public ObservableCollection<AnyListBindable> Invited { get; set; } = new ObservableCollection<AnyListBindable>();

    public ObservableCollection<AnyListBindable> PeopleFound { get; set; } = new ObservableCollection<AnyListBindable>(); 

    private byte[] GroupAvatarBuffer;
    private bool IsShowingInvited = true;

    public GroupChatCreator()
    {
        NavigationPage.SetHasNavigationBar(this, false);
        InitializeComponent();
        
        InvitedList.ItemsSource = Invited;
        ListPeopleFound.ItemsSource = PeopleFound;

        UserInvited(LocalUser.UserRef);
    }

    private void CreateGroupCallback(object data)
    {
        GroupChatCreateCallbackPacket? groupPacket = JsonConvert.DeserializeObject<GroupChatCreateCallbackPacket>((string)data);

        if (groupPacket is null)
        {
            Logger.Push("GroupCreatePacket_NULL", TraceType.Func, LogLevel.Error);
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

    private void ShowInvited(object? sender, EventArgs e)
    {
        LbShowInvite.TextDecorations = TextDecorations.None;
        LbShowInvited.TextDecorations = TextDecorations.Underline;

        LbInvitedHint.IsVisible = true;

        SearchInvite.IsVisible = false;
        SearchInvite.IsEnabled = false;

        if (!IsShowingInvited)
        {
            InvitedList.IsVisible = true;
            InvitedList.IsEnabled = true;
            IsShowingInvited = true;
        }
    }

    private void ShowInvite(object? sender, EventArgs e)
    {
        LbShowInvite.TextDecorations = TextDecorations.Underline;
        LbShowInvited.TextDecorations = TextDecorations.None;

        LbInvitedHint.IsVisible = false;

        SearchInvite.IsVisible = true;
        SearchInvite.IsEnabled= true;

        if (IsShowingInvited)
        {
            InvitedList.IsVisible = false;
            InvitedList.IsEnabled = false;

            IsShowingInvited = false;
        }
    }

    private void SearchPressed(object? sender, EventArgs e)
    {
        Entry? entry = (Entry?)sender;
        if (entry is null) return;

        if(string.IsNullOrEmpty(entry.Text) || entry.Text.Length < 1)
        {
            return;
        }

        SocketCore.SendCallback(new SearchPacket(entry.Text,false), Token.SEARCH_USER, SearchCallback);
    }

    private void SearchCallback(object packet)
    {
        SearchModel[]? found = JsonConvert.DeserializeObject<SearchModel[]?>((string)packet);

        if (found is null) return;

        PeopleFound.Clear();

        foreach (SearchModel userFound in found)
        {
            User user = User.CreateOrGet(userFound.Username,userFound.Id);

            AnyListBindable[] f = Invited.Where(x => x.Id == user.UserId).ToArray();

            if (Invited.Count > 0)
            {
                if (f.Length > 0 && Invited.Contains(Invited.First()))
                {
                    continue;
                }
            }


            MainThread.BeginInvokeOnMainThread(() =>
            {
                PeopleFound.Add(new AnyListBindable(user,new Command(() => UserInvited(user))));
            });
        }
    }

    private void UserInvited(User user)
    {
        Invited.Add(new AnyListBindable(user,new Command(() => UserRemovedFromInvited(user))));

        if (PeopleFound.Count > 0)
        {
            AnyListBindable? found = PeopleFound.Where(x => x.Id == user.UserId).First();

            if (found is not null)
            {
                PeopleFound.Remove(found);
            }
        }
    }

    private void UserRemovedFromInvited(User user)
    {
        AnyListBindable invited = Invited.Where(x => x.Id == user.UserId).First();

        if (invited.Id == LocalUser.Id)
        {
            return;
        }

        Invited.Remove(invited);
    }

    private void CreateGroupClicked(object? sender, EventArgs e)
    {
        string name = GroupName.Text;

        if (string.IsNullOrEmpty(name) || name.Length < 1)
        {
            return;
        }

        List<int> Ids = new List<int>(Invited.Count);
        foreach (AnyListBindable user in Invited)
        {
            if (user.Id != LocalUser.UserRef.UserId)
            {
                Ids.Add(user.Id);
            }
        }

        SocketCore.SendCallback( new GroupChatCreatePacket(name,LocalUser.UserRef.UserId, Ids.ToArray()),Token.GROUP_CHAT_CREATE, CreateGroupCallback);
    }
}
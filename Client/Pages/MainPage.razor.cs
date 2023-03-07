using Microsoft.AspNetCore.Components;
using Client.Models;
using Client.Networking.Core;
using Client.Networking.Models;
using Client.Networking.Packets.Models;
using Client.Pages.Components;
using Microsoft.AspNetCore.Components.Routing;
using Client.Models.LastChats;

namespace Client.Pages;

public partial class MainPage
{
    [Parameter]
    public LocalUser currentUser { get; set; } = LocalUser.CurrentUser;

    private LastChatService lastChatService { get; } = new LastChatService();
    
    private static List<IViewBindable> Requests { get; } = new List<IViewBindable>();

    private StateComponentController<IViewBindable> MiniProfileController { get; set; } = new StateComponentController<IViewBindable>();

    public MainPage()
    {
        currentUser.PropertyChanged += async (sender, e) => await InvokeAsync(StateHasChanged);
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            if (lastChatService.ChatsCount == 0)
            {
                GetLastChats();
            }
            else
            {
                LoadingEvents["LastChatsLoading"].State = false;
            }

        }
        base.OnAfterRender(firstRender);
    }

    private bool _shouldRenderLastChats = true;
    public bool ShouldRenderLastChats
    {
        get => _shouldRenderLastChats;
        set
        {
            if(value != _shouldRenderLastChats)
            {
                _shouldRenderLastChats = value;
                StateHasChanged();
            }
        }
    }

    Dictionary<string, StateComponentController> LoadingEvents { get; } = new Dictionary<string, StateComponentController>()
    {
        ["LastChatsLoading"] = new StateComponentController(),
        ["LocalUserLoading"] = new StateComponentController(),
    };

    protected override void OnInitialized()
    {
        currentUser.PropertyChanged += async (sender, e) => { await InvokeAsync(StateHasChanged); };
        navigation.LocationChanged += PreventBack;
    }

    private void PreventBack(object? sender, LocationChangedEventArgs e)
    {
        if(e.Location == "/" && LocalUser.IsLoggedIn)
        {
            navigation.NavigateTo("/MainPage");
        }
    }

    private string? SearchInput;
    private void SearchBarSubmit()
    {
        if (!string.IsNullOrEmpty(SearchInput))
        {
            navigation.NavigateTo("/Search/" + SearchInput);
        }
    }

    private void GetLastChats()
    {
        SocketCore.SendCallback(" ", Token.GET_LAST_CHATS, (SocketPacket packet) =>
        {
            LastChatsPacket[]? lastChats = packet.ModelCast<LastChatsPacket[]>();

            if(lastChats is null || lastChats.Length == 0)
            {
                LoadingEvents["LastChatsLoading"].State = false;    
                return;
            }

            foreach(LastChatsPacket lastChat in lastChats)
            {
                IViewBindable view = IViewBindable.CreateOrGet(lastChat.Name, lastChat.Id, lastChat.isGroup);
                UserStatus status = UserStatus.None;

                if(lastChat.LastOnlineTime is not null)
                {
                    status = UserStatus.Online;
                }
                else
                {
                    status = UserStatus.Offline;
                }

                LastChat chat = new LastChat(view, lastChat.MessageSenderName, lastChat.TypeOfMessage,
                    lastChat.LastMessage, lastChat.LastMessageTime, status, lastChat.IsFriend);

                chat.PropertyChanged += async (sender, e) => await InvokeAsync(StateHasChanged);

                lastChatService.AddLastChat(chat);
            }

            LoadingEvents["LastChatsLoading"].State = false;

            InvokeAsync(StateHasChanged);

            GetInvitesList();
        });
    }

    private void GetInvitesList()
    {
        SocketCore.SendCallback(" ", Token.USER_INVITATIONS, (packet) =>
        {
            UserInvitationPacket[]? invitations = packet.Deserialize<UserInvitationPacket[]>();  

            if(invitations is null)
            {
                return;
            }

            foreach(UserInvitationPacket invitation in invitations)
            {
                User invitationSender = (User)IViewBindable.CreateOrGet(invitation.InvitationSenderName, invitation.InvitationSenderId, false);

                invitationSender.PropertyChanged += async (sender, e) => { await InvokeAsync(StateHasChanged); };

                Requests.Add(invitationSender);
            }

            InvokeAsync(StateHasChanged);
        });
    }

    public void ShowMiniProfileMenu(IViewBindable view)
    {
        MiniProfileController.State = view;
    }

    public void OpenSettings()
    {
        navigation.NavigateTo("/ProfileSettings");
    }

    private void AcceptInviteRequest(IViewBindable view)
    {
        SocketCore.Send(view.Id, Token.ACCEPT_USER_FRIEND_INVITE, false);
        Requests.Remove(view);

        StateHasChanged();
    }
}
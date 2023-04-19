using Client.Models;
using Client.Networking.Core;
using Client.Pages.Components;
using Microsoft.AspNetCore.Components.Routing;
using Client.Models.LastChats;
using Client.Models.Invitations;
using Client.Utility;

namespace Client.Pages;

public partial class MainPage
{
    public LocalUser currentUser { get; set; } = LocalUser.CurrentUser;
    
    private static List<IViewBindable> Requests { get; } = new List<IViewBindable>();

    private static bool _wasInitialized = false;

    private StateComponentController<IViewBindable> MiniProfileController { get; set; } = new StateComponentController<IViewBindable>();

    private bool _shouldRenderLastChats = true;
    public bool ShouldRenderLastChats {
        get => _shouldRenderLastChats;
        set {
            if (value != _shouldRenderLastChats) {
                _shouldRenderLastChats = value;
                StateHasChanged();
            }
        }
    }

    Dictionary<string, StateComponentController> LoadingEvents { get; } = new Dictionary<string, StateComponentController>() {
        ["LastChatsLoading"] = new StateComponentController(),
        ["LocalUserLoading"] = new StateComponentController(),
    };

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender && !_wasInitialized)
        {
            _wasInitialized = true;

            currentUser.SetPropertyChanged(InvokeAsync(StateHasChanged));

            currentUser.Build();

            GetLastChats();
        }

        if (_wasInitialized) 
        {
            LoadingEvents["LastChatsLoading"].State = false;
            LoadingEvents["LocalUserLoading"].State = false;
        }

        base.OnAfterRender(firstRender);
    }

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
        lastChatService.FetchLastChats((lastChats) => 
        {
            LoadingEvents["LastChatsLoading"].State = false;

            foreach (LastChat lastChat in lastChats) 
            {
                lastChat.BindedView.PropertyChanged += (_, _) =>
                {
                    InvokeAsync(StateHasChanged);
                };
            }

            GetInvitesList();

            InvokeAsync(StateHasChanged);
        });
    }

    private void GetInvitesList()
    {
        invitationService.FetchInvitation(invitations => 
        {
            foreach (Invitation lastChat in invitations) 
            {
                lastChat.BindedView.PropertyChanged += (_, _) => {
                    InvokeAsync(StateHasChanged);
                };
            }
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
using Microsoft.AspNetCore.Components;
using Client.Models;
using Client.IO;
using Client.Networking.Core;
using Client.Networking.Models;
using Client.Networking.Packets.Models;
using Client.Pages.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace Client.Pages;

public partial class MainPage
{
    [Parameter]
    public LocalUser currentUser { get; set; } = LocalUser.CurrentUser;

    private static List<LastChat> LastChats { get; } = new List<LastChat>();

    Dictionary<string, LoadingComponentController> LoadingEvents { get; } = new Dictionary<string, LoadingComponentController>()
    {
        ["LastChatsLoading"] = new LoadingComponentController(),
        ["LocalUserLoading"] = new LoadingComponentController(),
    };

    protected override void OnInitialized()
    {
        if(LastChats.Count == 0)
        {
            GetLastChats();
        }
        else
        {
            LoadingEvents["LastChatsLoading"].IsLoading = false;
        }

        currentUser.PropertyChanged += async (sender, e) => { await InvokeAsync(StateHasChanged); };
        navigation.LocationChanged += PreventBack;
    }

    private void PreventBack(object? sender, LocationChangedEventArgs e)
    {
        if(e.Location == "/")
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

    private async void ChangeAvatar()
    {
        await AvatarManager.MediaPickerSet();
    }

    private void GetLastChats()
    {
        SocketCore.SendCallback(" ", Token.GET_LAST_CHATS, (SocketPacket packet) =>
        {
            LastChatsPacket[]? lastChats = packet.ModelCast<LastChatsPacket[]>();

            if(lastChats is null || lastChats.Length == 0)
            {
                return;
            }

            foreach(LastChatsPacket lastChat in lastChats)
            {
                IViewBindable view = IViewBindable.CreateOrGet(lastChat.Name, lastChat.Id, lastChat.isGroup);

                UserStatus status = UserStatus.None;

                if(lastChat.LastOnlineTime != null && Utility.Essential.UnixToDateTime((double)lastChat.LastOnlineTime) == DateTime.Now)
                {
                    status = UserStatus.Online;
                }
                else if(lastChat.LastOnlineTime != null)
                {
                    status = UserStatus.Offline;
                }
                

                LastChat chat = new LastChat(view, lastChat.MessageType, lastChat.LastMessage, lastChat.LastMessageTime, status);

                chat.PropertyChanged += async (sender, e) => await InvokeAsync(StateHasChanged);

                LastChats.Add(chat);
            }

            for(int i = 0; i < 5; i++)
            {
                User Testuser = (User)IViewBindable.CreateTestView($"Test_User {i}", (uint)i + 100, false);
                LastChat lastC = new LastChat(Testuser, "text", "Uwu", 2463781, UserStatus.Offline);

                

                LastChats.Add(lastC);
            }

            LoadingEvents["LastChatsLoading"].IsLoading = false;

            InvokeAsync(StateHasChanged);
        });
    }
}
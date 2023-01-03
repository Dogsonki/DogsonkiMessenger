using Microsoft.AspNetCore.Components;
using Client.Models;
using Client.IO;
using Client.Networking.Core;
using Client.Networking.Models;
using Client.Networking.Packets.Models;

namespace Client.Pages;

public partial class MainPage
{
    [Parameter]
    public LocalUser currentUser { get; set; } = LocalUser.CurrentUser;

    private static List<LastChat> LastChats { get; } = new List<LastChat>();

    private string? LocalUserAvatar;

    protected override void OnInitialized()
    {
        if(LastChats.Count == 0)
        {
            GetLastChats();
        }

        currentUser.PropertyChanged += async (sender, e) => { await InvokeAsync(StateHasChanged); };
        navigation.LocationChanged += PreventBack;
    }

    private void PreventBack(object? sender, Microsoft.AspNetCore.Components.Routing.LocationChangedEventArgs e)
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

                LastChat chat = new LastChat(view, lastChat.MessageType, lastChat.LastMessage, lastChat.LastMessageTime);

                chat.PropertyChanged += async (sender, e) => await InvokeAsync(StateHasChanged);

                LastChats.Add(chat);
            }

            InvokeAsync(StateHasChanged);
        });
    }
}
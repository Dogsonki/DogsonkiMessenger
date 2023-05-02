using Client.Networking.Core;
using Microsoft.AspNetCore.Components;

namespace Client.Models;

public class Conversation
{
    public static bool IsLocalUserInChat { get; set; }

    public static void OpenChat(IViewBindable chat, NavigationManager navigation)
    {
        if (chat.BindType == BindableType.Group)
        {
            SocketCore.Send(chat.Id, Token.GROUP_CHAT_INIT);
        }
        else
        {
            SocketCore.Send(chat.Name, Token.USER_INIT_CHAT);
        }

        IsLocalUserInChat = true;

        navigation.NavigateTo($"/ChatPage/{chat.Id}/{chat.BindType == BindableType.Group}");
    }

    public static void CloseChat()
    {
        IsLocalUserInChat = false;
        SocketCore.SendCallback(" ", Token.END_CHAT, null, false);
    }
}
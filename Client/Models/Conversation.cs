using Client.Networking.Core;
using Client.Utility;
using Microsoft.AspNetCore.Components;

namespace Client.Models;

public class Conversation
{
    public static void OpenChat(IViewBindable view, NavigationManager navigation)
    {
        if (view.BindType == BindableType.Group)
        {
            SocketCore.Send(view.Id, Token.GROUP_CHAT_INIT);
        }
        else
        {
            SocketCore.Send(view.Name, Token.USER_INIT_CHAT);
        }

        navigation.NavigateTo($"/ChatPage/{view.Id}/{view.BindType == BindableType.Group}");
    }
}
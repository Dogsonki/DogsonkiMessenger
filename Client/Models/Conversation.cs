#nullable enable
using Client.Models.Bindable;
using Client.Networking.Models;
using Client.Pages;
using Client.Utility;
using Newtonsoft.Json;
using Client.Networking.Packets.Models;
using Client.Networking.Core;

namespace Client.Models;

public class Conversation
{
    public static Conversation? Current { get; private set; }

    public int Id { get; set; }

    public bool IsGroupConversation => _view.BindType == BindableType.Group;

    private readonly IViewBindable _view;

    public IViewBindable GetCurrentConversation => _view;

    public Conversation(IViewBindable view)
    {
        Debug.ThrowIfNull(view);

        _view = view;
        Current = this;
    }

    public void ExitConversation()
    {
        Current = null;
    }

    private void GroupChatInfoCallback(object data)
    {
        GroupChatUserInfo[]? users = JsonConvert.DeserializeObject<GroupChatUserInfo[]>((string)data);

        Debug.ThrowIfNull(users);

        foreach (var u in users)
        {
            //Don't download users avatars, only when in Member Settings
            User user = User.CreateOrGet(u.UserName, u.UserId, UserCreateFlags.UseDefaultAvatar);

            GroupUser groupUser = new GroupUser(u.IsAdmin, user);

            (_view as Group).AddUser(groupUser);
        }
    }

    public static void OpenChat(IViewBindable view)
    {
        MessagePage.Messages.Clear();

        if (view.BindType == BindableType.Group)
        {
            SocketCore.Send(view.Name, Token.USER_INIT_CHAT);
        }
        else
        {
            SocketCore.Send(view.Id, Token.GROUP_CHAT_INIT);
        }
    
        MessagePage chatPage = new MessagePage(view);

        SocketCore.OnToken(Token.CHAT_MESSAGE, chatPage.RealTimeMessageCallback);
        SocketCore.OnToken(Token.GET_MORE_MESSAGES, chatPage.GetMoreChatMessagesCallback);

        SocketCore.SendCallback(" ", Token.GET_INIT_MESSAGES, chatPage.GetChatMessagesCallback, false);

        MainThread.BeginInvokeOnMainThread(() =>
        {
            StaticNavigator.Push(chatPage);
        });
    }
}
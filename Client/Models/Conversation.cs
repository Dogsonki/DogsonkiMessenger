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
    public static Conversation Current;

    public int Id { get; set; }
    public bool IsGroupConversation { get; set; }

    private readonly User? _user;
    private readonly Group? _group;

    public Conversation(Group group)
    {
        if (group is null)
        {
            throw new Exception("Group can't be null in group conversation");
        }

        _group = group;
        IsGroupConversation = true;

        Current = this;
    }

    public Conversation(User user)
    {
        if (user is null)
        {
            throw new Exception("User can't be null in user conversation");
        }

        _user = user;
        IsGroupConversation = false;

        Current = this;
    }

    public User GetCurrentUserChat() => _user;
    public Group GetCurrentGroupChat() => _group;

    private void GroupChatInfoCallback(object data)
    {
        GroupChatUserInfo[]? users = JsonConvert.DeserializeObject<GroupChatUserInfo[]>((string)data);

        if (users is null)
        {
            throw new Exception("GROUP_CHAT_USERS_INFO_NULL");
        }

        foreach (var u in users)
        {
            //Don't download users avatars, only when in Member Settings
            User user = User.CreateOrGet(u.UserName, u.UserId, UserCreateFlags.UseDefaultAvatar);

            GroupUser groupUser = new GroupUser(u.IsAdmin, user);

            _group.AddUser(groupUser);
        }
    }

    public static void OpenChat(User user)
    {
        SocketCore.Send(user.Username, Token.INIT_CHAT);

        MessagePage.Messages.Clear();

        MessagePage chatPage = new MessagePage(user);

        SocketCore.OnToken(Token.CHAT_MESSAGE, chatPage.RealTimeMessageCallback);
        SocketCore.OnToken(Token.GET_MORE_MESSAGES, chatPage.GetMoreChatMessagesCallback);

        SocketCore.SendCallback(" ", Token.GET_INIT_MESSAGES, chatPage.GetChatMessagesCallback, false);

        MainThread.BeginInvokeOnMainThread(() =>
        {
            StaticNavigator.Push(chatPage);
        });
    }

    public static void OpenChat(Group group)
    {
        SocketCore.Send(group.Id, Token.GROUP_CHAT_INIT);

        MessagePage.Messages.Clear();

        MessagePage chatPage = new MessagePage(group);

        SocketCore.OnToken(Token.CHAT_MESSAGE, chatPage.RealTimeMessageCallback);
        SocketCore.OnToken(Token.GET_MORE_MESSAGES, chatPage.GetMoreChatMessagesCallback);

        SocketCore.SendCallback(" ", Token.GET_INIT_MESSAGES, chatPage.GetChatMessagesCallback, false);

        MainThread.BeginInvokeOnMainThread(() =>
        {
            StaticNavigator.Push(chatPage);
        });
    }
}
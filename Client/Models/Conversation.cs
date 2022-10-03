#nullable enable

using Client.Models.UserType.Bindable;
using Client.Networking.Core;
using Client.Networking.Packets.Models;
using Newtonsoft.Json;

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

        SocketCore.SendCallback(GroupChatInfoCallback, " ", Token.GET_GROUP_INFO, false);

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
}
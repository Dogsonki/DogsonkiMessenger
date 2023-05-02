using Client.Networking.Core;

namespace Client.Models;

public partial class User : ViewBindable
{
    public static readonly List<User> Users = new List<User>();

    public static readonly User SystemBot = new User("System", 0, isBot:true, false, false);

    public UserProperties UserProperties { get; } = new UserProperties();

    public bool IsLocalUser { get; }
    
    public bool IsBot { get; }

    public User(string name, uint id, bool isBot = false, bool loadAvatar = true, bool isLocalUser = false) : base(BindableType.User, name, id)
    {
        if (loadAvatar && !isLocalUser && !isBot)
        {
            LoadAvatar();
        }

        IsBot = isBot;

        IsLocalUser = isLocalUser;

        Users.Add(this);
    }

    public static void ClearUsers() => Users.Clear();

    public static User CreateOrGet(string username, uint id, bool loadAvatar)
    {
        User? user;
        if ((user = Users.Find(x => x.Id == id)) != null)
            return user;

        return new User(username, id, loadAvatar: loadAvatar);
    }

    public static User CreateLocalUser(string username, uint id)
    {
        return new User(username, id, true);
    }

    public static User? GetUser(uint id)
    {
        return Users.Find(x => x.Id == id);
    }

    public void InviteAsFriend()
    {
        if(UserProperties.IsFriend != FriendStatus.Friend && UserProperties.IsFriend != FriendStatus.Invited)
        {
            UserProperties.IsFriend = FriendStatus.Invited;
            SocketCore.Send(Id, Token.SEND_USER_FRIEND_INVITE, true);
        }
    }
}

[Flags]
public enum UserCreateFlags
{
    UseDefaultAvatar,
    IsSystemUser,
    Default
}
using Client.Networking.Core;
using Client.Utility;

namespace Client.Models;

public partial class User : ViewBindable
{
    public UserProperties UserProperties { get; set; } = new UserProperties();

    public static readonly List<User> Users = new List<User>();

    public static readonly User SystemBot = new User("System", 0, isBot:true, false, false);

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

    public static User CreateOrGet(string username, uint id, UserCreateFlags flags = UserCreateFlags.Default)
    {
        User? user;
        if ((user = Users.Find(x => x.Id == id)) != null)
            return user;

        return new User(username, id);
    }

    public static User CreateLocalUser(string username, uint id)
    {
        return new User(username, id, true);
    }

    /// <summary>
    /// User "bot" that is visible only for client, sends messages with errors and warnings to client
    /// </summary>
    public static User CreateSystemBot()
    {
        User? _;
        if ((_ = GetUser(0)) is not null) return _;

        User systemBot = new User("System", 0);
        return systemBot;
    }

    public static User GetSystemBot()
    {
        User? systemBot = GetUser(0);
        return systemBot is not null ? systemBot : CreateSystemBot();
    }

    public static User? GetUser(uint id) => Users.Find(x => x.Id == id);

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
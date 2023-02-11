using Client.IO;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Client.Models;

public partial class User : IViewBindable
{
    public static readonly List<User> Users = new List<User>();

    public UserPropertiesLocal UserProperties { get; set; } = new UserPropertiesLocal();

    public event PropertyChangedEventHandler? PropertyChanged;

    public BindableType BindType { get; } = BindableType.User;

    public uint Id { get; }

    IViewBindable IViewBindable.View => this;

    public string Name { get; }

    public string AvatarPath => $"user_avatar{Id}";

    private string _avatarImageSource;

    public string AvatarImageSource
    {
        get
        {
            if (_avatarImageSource is null)
                return AvatarManager.BlankAvatar;
            return _avatarImageSource;
        }
        set
        {
            _avatarImageSource = value;
            NotifyPropertyChanged();
        }
    }


    public User(string username, uint id, bool isLocalUser = false, bool setAvatar = true)
    {
        Name = username;
        Id = id;

        if (isLocalUser)
        {
            BindType = BindableType.LocalUser;
        }
        else
        {
            if (setAvatar)
            {
                AvatarManager.SetAvatar(this);
            }
        }
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

    private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

[Flags]
public enum UserCreateFlags
{
    UseDefaultAvatar,
    IsSystemUser,
    Default
}
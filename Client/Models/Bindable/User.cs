using Client.IO;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Client.Models.Bindable;

public partial class User : ObservableObject, IViewBindable
{
    public static List<User> Users = new List<User>();

    public BindableType BindType { get; set; }

    [ObservableProperty]
    public string name;

    [ObservableProperty]
    private ImageSource? avatar;

    [ObservableProperty]
    private int dogeCoins;

    public uint Id { get; }

    [ObservableProperty]
    private string? tag;

    public bool isBot { get; set; }
    public bool VisibleTag { get; set; } = false;

    public User(string username, uint id, UserCreateFlags flags = UserCreateFlags.Default)
    {
        name = username;
        Id = id;
           
        Users.Add(this);

        AvatarManager.SetAvatar(this);
    }

    /// <summary>
    /// Sets avatar to user
    /// </summary>
    /// <param name="buffer"></param>
    /// <returns>Returns false if buffer is null or empty</returns>
    public bool SetAvatar(byte[] buffer)
    {
        ImageSource src = ImageSource.FromStream(() => new MemoryStream(buffer));

        if (src.IsEmpty || src is null)
        {
            throw new ArgumentException($"Buffer was not a image buffer len? {buffer?.Length} is src null: {src is null}");
        }

        MainThread.BeginInvokeOnMainThread(() =>
        {
            Avatar = ImageSource.FromStream(() => new MemoryStream(buffer));
        });

        return true;
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
        CreateSystemBot();
        return new User(username, id);
    }

    /// <summary>
    /// User "bot" that is visible only for client, sends messages with errors and warnings to client
    /// </summary>
    public static User CreateSystemBot()
    {
        User? _;
        if ((_ = GetUser(0)) is not null) return _;

        User systemBot = new User("System", 0, flags: UserCreateFlags.IsSystemUser)
        {
            isBot = true,
            Tag = "System",
            VisibleTag = true
        };
        return systemBot;
    }

    public static User GetSystemBot()
    {
        User? systemBot = GetUser(0);    
        return systemBot is not null ? systemBot : CreateSystemBot();
    }

    public static User? GetUser(uint id) => Users.Find(x => x.Id == id);
}

[Flags]
public enum UserCreateFlags
{
    UseDefaultAvatar,
    IsSystemUser,
    Default
}
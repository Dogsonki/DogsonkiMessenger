using Client.IO;
using Client.Networking.Core;
using Client.Pages;
using System.ComponentModel;

namespace Client.Models.UserType.Bindable;

[Bindable(BindableSupport.Yes)]
public class User : BindableObject
{
    public static List<User> Users = new List<User>();
    public List<MessageModel> CachedMessages = new List<MessageModel>();

    private ImageSource avatar;
    public ImageSource Avatar
    {
        get
        {
            if (IsLocalUser)
            {
                return LocalUser.Current.Avatar;
            }
            return avatar;
        }
        set
        {
            if (IsLocalUser)
            {
                LocalUser.Current.Avatar = value;
            }
            else
            {
                avatar = value;
                OnPropertyChanged(nameof(Avatar));
            }
        }
    }

    private int dogeCoins;
    public int DogeCoins
    {
        get
        {
            return dogeCoins;
        }
        set
        {
            dogeCoins = value;
            OnPropertyChanged(nameof(DogeCoins));
        }
    }
    /* As LocalUser have his own binding but User reuse it LocalUser have to be binding as LocalUser.<BindableObject> */
    public bool IsLocalUser = false;

    /* Username and ID will never change then don't make them as OnPropertyChanged */
    public string Username { get; set; }
    public int Id { get; set; }

    public Command OpenChatCommand { get; set; }

    public User(string username, int id, bool isLocalUser = false)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (Users.Find(x => x.Id == id) != null)
                return;

            Username = username;
            Id = id;
            IsLocalUser = isLocalUser;
            OpenChatCommand = new Command(OpenChat);

            Users.Add(this);
        });

        Debug.Write($"Getting avatar: username: {username} id: {Id}");
        byte[] AvatarCacheBuffer = Cache.ReadCache("avatar" + id);

        if (AvatarCacheBuffer is not null)
        {
            ImageSource src = ImageSource.FromStream(() => new MemoryStream(AvatarCacheBuffer));
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Avatar = src;
            });
        }
        else
        {
            Debug.Write("REQUESTING AVATAR");
            SocketCore.Send(id, Token.AVATAR_REQUEST);
        }
    }

    public static void ClearUsers() => Users.Clear();

    private void OpenChat()
    {
        SocketCore.Send($"{Username}", Token.INIT_CHAT);
        MainThread.BeginInvokeOnMainThread(() =>
        {
            StaticNavigator.Push(new MessagePage(this));
        });
    }

    public static User CreateOrGet(string username, int id)
    {
        User user;
        if ((user = Users.Find(x => x.Id == id)) != null)
            return user;

        return new User(username, id, false);
    }

    public static User CreateLocalUser(string username, int id)
    {
        return new User(username, id, true);
    }

    public static User GetUser(int id) => Users.Find(x => x.Id == id);
}

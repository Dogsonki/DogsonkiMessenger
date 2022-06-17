using Client.Models;
using Client.Networking.Core;
using Client.Pages;
using System.ComponentModel;

namespace Client;
[Bindable(BindableSupport.Yes)]
public class LocalUser : BindableObject
{
    //Current have to be binded to property in view class to reuse it 
    public static LocalUser Current { get; set; } 
    public LocalUser User { get { return Current; } }

    protected static bool InstanceCreated = false;

    public static ImageSource avatar;
    public ImageSource Avatar
    {
        get
        {
            return avatar;
        }
        set { avatar = value; OnPropertyChanged(nameof(Avatar)); }
    }

    public static string username;
    public string Username
    {
        get { return username; }
        set { username = value; OnPropertyChanged(nameof(Username)); }
    }

    public static string id;
    public string ID
    {
        get { return id; }
        set { id = value; OnPropertyChanged(nameof(ID)); }
    }

    public static bool isLoggedIn;
    public bool IsLoggedIn
    {
        get { return isLoggedIn; }
        set { isLoggedIn = value; OnPropertyChanged(nameof(IsLoggedIn)); }
    }


    public static void Logout()
    {
        SocketCore.Send(" ", 0);
        StaticNavigator.PopAndPush(new LoginPage());
        ToDefault();
    }
    public static void Login(string _username, string _id)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (!InstanceCreated)
            {
                new LocalUser();
            }
            Current.ID = _id;
            Current.Username = _username;
            isLoggedIn = true;
        });
        UserModel user = UserModel.CreateOrGet(username, uint.Parse(id));
        user.isLocalUser = true;
    }

    public LocalUser()
    {
        if (!InstanceCreated) 
        {
            Current = this;
            ToDefault();
            InstanceCreated = true;
        }
    }
    protected static void ToDefault()
    {
        Current.Username = "NOT_LOGGED_USER";
        Current.ID = 0xffffffff.ToString();
        isLoggedIn = false;
    }
}

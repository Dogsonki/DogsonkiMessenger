using Client.Models;
using Client.Networking.Core;
using Client.Pages;
using System.ComponentModel;

namespace Client;
[Bindable(BindableSupport.Yes)]
public class LocalUser : BindableObject
{
    /*
     TODO: Create properties that provides all informations from UserModel not from localuser that being copied by both classes
     */

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

    public static string email;
    public string Email 
    {
        get { return email; } 
        set { email = value;OnPropertyChanged(nameof(Email)); } 
    }

    public static void Logout()
    {
        SocketCore.Send(" ", 0);
        MainThread.BeginInvokeOnMainThread(() =>
        {
            StaticNavigator.Push(new LoginPage());
        });
        ToDefault();
    }
    public static void Login(string _username, string _id,string _email)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (!InstanceCreated)
            {
                new LocalUser();
            }
            Current.ID = _id;
            Current.Username = _username;
            Current.Email = _email;
            isLoggedIn = true;
            MainThread.BeginInvokeOnMainThread(() => StaticNavigator.Push(new MainPage()));
        });
        UserModel user = UserModel.CreateOrGet(username, uint.Parse(_id));
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
        UserModel.ClearUsers();
        isLoggedIn = false;
        Current.Avatar = null;
    }
}

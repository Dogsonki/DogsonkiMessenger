﻿using Client.Networking.Core;
using Client.Pages;
using System.ComponentModel;

namespace Client.Models.UserType.Bindable;

[Bindable(BindableSupport.Yes)]
public class LocalUser : BindableObject
{
    /* Current have to be binded to property in view class to reuse it  */
    public static LocalUser Current { get; set; }
    public LocalUser User { get { return Current; } }
    public static User UserRef { get; set; }
    public static LocalOptions Settings { get; set; }

    /*  */
    public static bool isCreatingGroup { get; set; } = false;


    protected static bool InstanceCreated = false;

    public static ImageSource avatar { get; private set; }
    public ImageSource Avatar
    {
        get
        {
            return avatar;
        }
        set { avatar = value; OnPropertyChanged(nameof(Avatar)); }
    }

    public static string username { get; private set; }
    public string Username
    {
        get { return username; }
        set { username = value; OnPropertyChanged(nameof(Username)); }
    }

    public static string id { get; private set; }
    public string ID
    {
        get { return id; }
        set { id = value; OnPropertyChanged(nameof(ID)); }
    }

    public static int Id => int.Parse(id);

    public static bool isLoggedIn { get; private set; }
    public bool IsLoggedIn
    {
        get { return isLoggedIn; }
        set { isLoggedIn = value; OnPropertyChanged(nameof(IsLoggedIn)); }
    }

    public static string email { get; private set; }
    public string Email
    {
        get { return email; }
        set { email = value; OnPropertyChanged(nameof(Email)); }
    }

    public static void Logout()
    {
        SocketCore.Send(" ", 0);
        ToDefault();
    }

    public static void Login(string _username, string _id, string _email)
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

            User user = Bindable.User.CreateLocalUser(username, uint.Parse(_id));
            UserRef = user;

            StaticNavigator.Push(new MainPage());
        });
    }

    public LocalUser()
    {
        if (!InstanceCreated)
        {
            Current = this;
            Settings = new LocalOptions();
            ToDefault();
            InstanceCreated = true;
        }
    }

    //Clear datas generated by last user
    private static void ToDefault()
    {
        Current.Username = "NOT_LOGGED_USER";
        Current.ID = 0xffffffff.ToString();
        isLoggedIn = false;
        Current.Avatar = null;

        MainPage.LastChats.Clear();
        Bindable.User.ClearUsers();
    }
}

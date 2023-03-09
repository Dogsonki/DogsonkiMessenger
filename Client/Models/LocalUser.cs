using Client.IO;
using Client.Networking.Core;
using Microsoft.AspNetCore.Components;
using System.ComponentModel;

namespace Client.Models;

public class LocalUser : ViewBindable
{
    public static LocalUser CurrentUser { get; private set; }

    public static bool IsLoggedIn { get => CurrentUser != null; }

    public LocalUser(string name, uint id) : base(BindableType.LocalUser, name, id)
    {
        CurrentUser = this;
    }

    public void Build() 
    {
        AvatarManager.SetAvatar(this);

        User.CreateLocalUser(Name, Id);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public void Logout(NavigationManager navigation)
    {
        CurrentUser = null;

        SocketCore.Send(" ", Token.LOGOUT);
        Session.DeleteSession();

        navigation.NavigateTo("/");
    }
}
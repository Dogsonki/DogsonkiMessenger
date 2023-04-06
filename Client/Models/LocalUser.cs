using Client.IO;
using Client.Networking.Core;
using Microsoft.AspNetCore.Components;

namespace Client.Models;

public class LocalUser : ViewBindable
{
    public static LocalUser CurrentUser { get; private set; }

    public static bool IsLoggedIn { get => CurrentUser != null; }

    public readonly LocalUserProperties UserProperties = new LocalUserProperties();

    public LocalUser(string name, uint id) : base(BindableType.LocalUser, name, id)
    {
        CurrentUser = this;
    }

    public void Build() 
    {
        AvatarManager.SetAvatar(this);
        
        User.CreateLocalUser(Name, Id);
    }

    public void Logout(NavigationManager navigation)
    {
        CurrentUser = null;

        SocketCore.Send(" ", Token.LOGOUT);
        Session.DeleteSession();

        navigation.NavigateTo("/");
    }
}
using Client.IO;
using Client.Networking.Core;
using Microsoft.AspNetCore.Components;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Client.Models;

public class LocalUser : ViewBindable
{
    public static LocalUser CurrentUser { get; private set; }

    public static bool IsLoggedIn { get => CurrentUser != null; }

    public LocalUser(string name, uint id) : base(BindableType.LocalUser, name, id)
    {
        CurrentUser = this;

        AvatarManager.SetAvatar(this);

        User.CreateLocalUser(name, id);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void Logout(NavigationManager navigation)
    {
        CurrentUser = null;

        SocketCore.Send(" ", Token.LOGOUT);
        Session.DeleteSession();

        navigation.NavigateTo("/");
    }
}
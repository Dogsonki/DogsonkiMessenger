using Client.IO;
using Client.Utility;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Client.Models;


public class LocalUser : IViewBindable
{
    [NotNull]
    public static LocalUser CurrentUser { get; private set; }

    public static bool IsLoggedIn { get => CurrentUser != null; }

    public BindableType BindType { get; } = BindableType.LocalUser;

    public string Name { get; }

    public uint Id { get; }

    public string AvatarPath => $"user_avatar" + Id;

    IViewBindable IViewBindable.View => this;

    private string _avatarImageSource;
    public string AvatarImageSource
    {
        get
        {
            return _avatarImageSource;
        }
        set
        {
            _avatarImageSource = value;
            NotifyPropertyChanged();
        }
    }

    public LocalUser(string username, uint id)
    {
        Name = username;
        Id = id;
        CurrentUser = this;

        Debug.Write("Requesting local user avatar");
        AvatarManager.SetAvatar(this);

        User.CreateLocalUser(username, id);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
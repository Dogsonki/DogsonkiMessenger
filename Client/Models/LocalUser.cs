using Client.IO;
using Client.IO.Models.Offline;
using Client.Networking.Core;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;

namespace Client.Models;

public class LocalUser : ViewBindable
{
    private static LocalUser currentUser;
    public static LocalUser CurrentUser
    {
        get
        {
            if(currentUser is null)
            {
                throw new Exception("LocalUser was used before initalization");
            }

            return currentUser;
        }
        private set
        {
            currentUser = value;
        }
    }

    private static bool _isLoggedIn = false;

    public static bool IsLoggedIn { get => _isLoggedIn; private set => _isLoggedIn = value; }

    public static bool IsInOfflineMode = false;

    public readonly LocalUserProperties UserProperties = new LocalUserProperties();

    public LocalUser(string name, uint id, bool enableOfflineMode = false) : base(BindableType.LocalUser, name, id)
    {
        CurrentUser = this;

        IsInOfflineMode = enableOfflineMode;

        IsLoggedIn = !enableOfflineMode;
    }

    public void Build() 
    {
        AvatarManager.SetAvatar(this);
        
        User.CreateLocalUser(Name, Id);

        CacheLocalUser();
    }

    private void CacheLocalUser()
    {
        LocalUserCache localUserCache = new LocalUserCache(Name, Id);

        Cache.SaveToCache(JsonConvert.SerializeObject(localUserCache), nameof(LocalUserCache));
    }

    public void Logout(NavigationManager navigation)
    {
        CurrentUser = null;

        SocketCore.Send(" ", Token.LOGOUT);

        Session.DeleteSession();

        navigation.NavigateTo("/", true);
    }
}
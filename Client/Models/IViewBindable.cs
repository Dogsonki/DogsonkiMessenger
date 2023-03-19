using System.ComponentModel;

namespace Client.Models;

public interface IViewBindable
{
    /// <summary>
    /// Returns view
    /// </summary>
    public IViewBindable View { get; }

    /// <summary>
    /// Indicates if this view is group or user
    /// </summary>
    public BindableType BindType { get; } 

    /// <summary>
    /// Name of this view
    /// </summary>
    public string Name { get; }
   
    /// <summary>
    /// Id of this view
    /// </summary>
    public uint Id { get; }

    /// <summary>
    /// JS type avatar 
    /// </summary>
    public string? AvatarImageSource { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;

    public abstract void SetPropertyChanged(Task task, bool silentNotify = false);

    public static IViewBindable CreateOrGet(string name, uint id, bool isGroup)
    {
        if(isGroup)
        {
            return Group.CreateOrGet(name, id);
        }
        else
        {
            if (LocalUser.CurrentUser.Id == id)
            {
                return LocalUser.CurrentUser;
            }
            return User.CreateOrGet(name, id);
        }
    }

    public static IViewBindable Get(uint id, bool isGroup)
    {
        if (isGroup)
        {
            return Group.GetGroup(id);
        }
        else
        {
            if(LocalUser.CurrentUser.Id == id)
            {
                return LocalUser.CurrentUser;
            }
            return User.GetUser(id);
        }
    }

    public static IViewBindable CreateTestView(string name, uint id, bool isGroup)
    {
#if RELEASE
        throw new Exception("Test view was created in release mode");
#endif
        if (!isGroup)
        {
            return new User(name, id, loadAvatar: false);
        }
        else
        {
            return new Group(name, id,loadAvatar: false);
        }
    }

    public bool IsUser() => BindType == BindableType.User;
}

public enum BindableType
{
    LocalUser,
    User,
    Group,
    Any
}

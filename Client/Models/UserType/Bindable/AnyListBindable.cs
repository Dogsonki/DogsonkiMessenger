namespace Client.Models.UserType.Bindable;

/// <summary>
/// Object used to be used as BindableObject in ObservableCollection T with onPropertyChanged
/// </summary>
public class AnyListBindable : BindableObject
{
    private User BindedUser;

    public string Name
    {
        get
        {
            return BindedUser.Name;
        }
    }

    public Command Input
    {
        get
        {
            return BindedUser.OpenChatCommand;
        }
    }

    public ImageSource Avatar
    {
        get
        {
            return BindedUser.Avatar;
        }
    }

    public AnyListBindable(User user)
    {
        BindedUser = user;
    }
}
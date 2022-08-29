namespace Client.Models.UserType.Bindable;

/// <summary>
/// Object used to be used as BindableObject in ObservableCollection T with onPropertyChanged
/// </summary>

public class AnyListBindable : BindableObject
{
    private readonly User BindedUser;
    private readonly Group BindedGroup;
    private readonly bool UseUserInput;
    private readonly bool UseGroupInput;

    private readonly BindType type;

    public string Username
    {
        get
        {
            if(type == BindType.Group)
            {
                return BindedGroup.Name;
            }

            return BindedUser.Username;
        }
    }
    public int Id 
    {
        get
        {
            if (type == BindType.Group)
            {
                return BindedGroup.Id;
            }

            return BindedUser.UserId;
        }
     }
    private Command input;
    public Command Input
    {
        get
        {
            if (UseUserInput)
            {
                return BindedUser.OpenChatCommand;
            }
            if (UseGroupInput)
            {
                return BindedGroup.OpenChatCommand;
            } 
            return input;
        }
        set
        {
            input = value;
        }
    }

    public ImageSource Avatar
    {
        get
        {
            if(type == BindType.Group)
            {
                return null;
            }
            return BindedUser.Avatar;
        }
    }

    public AnyListBindable(User user, bool useUserInput = false, Command input = null)
    {
        type = BindType.User;
        BindedUser = user;
        UseUserInput = useUserInput;
        Input = input;
    }

    public AnyListBindable(Group group, bool useGroupInput = false, Command input = null)
    {
        BindedGroup = group;
        type = BindType.Group;
        UseGroupInput= useGroupInput;
        Input = input;
    }
}

enum BindType
{
    User,
    Group
}
using System.Collections.Specialized;
using System.ComponentModel;

namespace Client.Models.Bindable;

/// <summary>
/// Object used to be used as BindableObject in ObservableCollection T with onPropertyChanged
/// </summary>
public partial class AnyListBindable : BindableObject
{
    public IViewBindable View { get; }

    private readonly bool UseUserInput;
    private readonly bool UseGroupInput;

    private readonly BindableType BindType;

    public string Username { get => View.Name; }
    public uint Id { get => View.Id; }

    private Command? input;
    public Command? Input
    {
        get
        {
            return input;
        }
        set
        {
            input = value;
        }
    }

    public ImageSource BindedAvatar { get => View.Avatar; }

    public bool IsGroup => View.BindType == BindableType.Group;

    public AnyListBindable(IViewBindable view, Command input = null)
    {
        BindType = view.BindType;
        View = view;
        Input = input;
    }
}
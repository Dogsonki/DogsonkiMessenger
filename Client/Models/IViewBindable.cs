using Client.Models.Bindable;

namespace Client.Models;

/// <summary>
/// Manages user and group models. Proviedes user, avatars etc
/// </summary>
public interface IViewBindable
{
    public BindableType BindType { get; }

    public string Name { get; }

    public uint Id { get; }

    public ImageSource Avatar { get; set; }
}

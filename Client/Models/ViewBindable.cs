using Client.IO;
using Client.Utility;
using System.ComponentModel;

namespace Client.Models;

public abstract class ViewBindable : IViewBindable
{
    /// <summary>
    /// Castable view to User/Group. Before casting check BindType
    /// </summary>
    public IViewBindable View => this;

    public BindableType BindType { get; }

    public string Name { get; }

    public uint Id { get; }

    private string? _avatarImageSource;
    public string? AvatarImageSource
    {
        get => _avatarImageSource;
        set
        {
            _avatarImageSource = value;
            NotifyUI();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ViewBindable(BindableType type, string name, uint id)
    {
        BindType = type;
        Name = name;
        Id = id;
    }

    private void NotifyUI()
    {
        Debug.Write($"NotifyUI invoked by {View.Name} was null? {PropertyChanged is null}");
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
    }

    public void Dispose()
    {
        _avatarImageSource = null;
        PropertyChanged = null;
    }

    public void LoadAvatar()
    {
        AvatarManager.SetAvatar(this);
    }

    public void SetPropertyChanged(Task task, bool silentNotify = false) {
        PropertyChanged += async (sender, e) => { await task; };
        if (silentNotify) {
            NotifyUI();
        }
    }
}
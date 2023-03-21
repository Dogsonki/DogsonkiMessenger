using System.ComponentModel;

namespace Client.Models.Invitations;

internal class Invitation : IViewBindable 
{
    public IViewBindable View => throw new NotImplementedException();

    public BindableType BindType => throw new NotImplementedException();

    public string Name => throw new NotImplementedException();

    public uint Id => throw new NotImplementedException();

    public string? AvatarImageSource { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public event PropertyChangedEventHandler? PropertyChanged;

    public void SetPropertyChanged(Task task, bool silentNotify = false) {
        throw new NotImplementedException();
    }

    public Invitation() : base()
    {
        
    }
}
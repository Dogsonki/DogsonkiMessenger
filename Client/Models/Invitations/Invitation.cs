namespace Client.Models.Invitations;

internal class Invitation : ViewBindable 
{
    public IViewBindable BindedView { get; }

    public Invitation(IViewBindable view) : base(BindableType.Any, view.Name, view.Id)
    {
        BindedView = view;
    }
}
namespace Client.Models.Bindable;

public interface IBindableType
{
    public BindableType Type { get; set; }
    public string Name { get; set; }
    public int Id { get; }
    public ImageSource Avatar { get; set; }
}
using Client.Networking.Core;
using Client.Networking.Model;
using System.ComponentModel;

namespace Client.Models;

[Bindable(BindableSupport.Yes)]
public class UserModel : BindableObject
{
    public static List<UserModel> Users = new List<UserModel>();
    public List<MessageModel> CachedMessages = new List<MessageModel>();

    protected ImageSource avatar;
    public ImageSource Avatar
    {
        get
        {
            return avatar;
        }
        set
        {
            avatar = value;
            OnPropertyChanged(nameof(Avatar));
        }
    }

    public bool isLocalUser { get; set; }
    public string Name { get; set; }
    public uint ID { get; set; }

    public UserModel(string username, uint id)
    {
        if (Users.Find(x => x.ID == id) != null)
            return;
        Name = username;
        ID = id;

        Users.Add(this);

        SocketCore.Send(ID, Token.AVATAR_REQUEST);
    }
    


    public static UserModel CreateOrGet(string username, uint id)
    {
        UserModel user;
        if ((user = Users.Find(x => x.ID == id)) != null)
            return user;
        return new UserModel(username, id);
    }

    public static UserModel GetUser(uint id) => Users.Find(x => x.ID == id);
}

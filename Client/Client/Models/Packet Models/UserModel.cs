using Client.Networking;
using System.Collections.Generic;
using System.ComponentModel;
using Xamarin.Forms;

namespace Client.Models
{
    public class UserModel : INotifyPropertyChanged
    {
        public static List<UserModel> Users = new List<UserModel>();

        public event PropertyChangedEventHandler PropertyChanged;

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
                OnPopertyChanged("Avatar");
            }
        }

        public string Name { get; set; }
        public uint ID { get; set; }

        public UserModel(string username,uint id)
        {
            if (Users.Find(x => x.ID == id) != null)
                return;
            Name = username;
            ID = id;

            Users.Add(this);

            SocketCore.Send(ID, Token.AVATAR_REQUEST);
        }

        void OnPopertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public static UserModel CreateOrGet(string username, uint id) 
        {
            UserModel user;
            if ((user = Users.Find(x => x.ID == id)) != null)
                return user;
            return new UserModel(username, id);
        }

        public static UserModel GetUser(uint id) => Users.Find(x => x.ID == id);
    }
}

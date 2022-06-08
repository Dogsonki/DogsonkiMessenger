using Client.Networking;
using Client.Pages;
using System.ComponentModel;
using Xamarin.Forms;

namespace Client.Models
{
   public class LocalUser : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        //Current have to be binded to property in view class to reuse it 
        public static LocalUser Current { get; } = new LocalUser();

        protected static bool InstanceCreated = false;  

        public static ImageSource avatar;
        public ImageSource Avatar
        {
            get
            {
                return avatar;
            }
            set
            {
                avatar = value;
                OnPropertyChanges("Avatar");
            }
        }

        public static string username;
        public string Username
        {
            get { return username; }  
            set { Username = value; OnPropertyChanges("username"); }
        }

        public static string id;
        public string ID
        {
            get { return id; }
            set { id = value; OnPropertyChanges("ID"); }
        }

        public static bool isLoggedIn;
        public bool IsLoggedIn
        {
            get { return isLoggedIn; }
            set { isLoggedIn = value; OnPropertyChanges("IsLoggedIn"); }
        }


        public static void Logout()
        {
            SocketCore.Send(" ", 0);
            StaticNavigator.PopAndPush(new AppEntry(false));
            ToDefault();
        }
        public static void Login(string _username,string _id)
        {
            id = _id;
            username = _username;
            isLoggedIn=true;
        }

        public LocalUser()
        {
            if (!InstanceCreated)
                ToDefault();
            InstanceCreated = true;
        }

        protected static void ToDefault()
        {
            username = "NOT_LOGGED_USER";
            id = 0xffffffff.ToString();
            isLoggedIn = false;
        }

        public void OnPropertyChanges(string prop) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}

using Client.Networking;
using Client.Utility;
using Client.Views;
using System.IO;
using System.Reflection;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Client.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainAfterLoginPage : ContentPage
    {
        public static MainAfterLoginPage MainInstance { get; set; }
        public static Image d;

        public MainAfterLoginPage(bool redirectedFormLogin = false)
        {
            NavigationPage.SetHasNavigationBar(this, false);
            InitializeComponent();

            MainInstance = this;
            if (redirectedFormLogin)
                MainAfterLoginPageView.Clear();
            d = here;
            return;
            var assembly = IntrospectionExtensions.GetTypeInfo(typeof(MainAfterLoginPage)).Assembly;
            byte[] b;
            using (Stream stream = assembly.GetManifestResourceStream("Client.Pages.B.png"))
            {
                byte[] bf = new byte[16 * 1024];
                using (MemoryStream ms = new MemoryStream())
                {
                    int read;
                    while ((read = stream.Read(bf, 0, bf.Length)) > 0)
                    {
                        ms.Write(bf, 0, read);
                    }
                    b = ms.ToArray();
                }
            }
            ImageSource xd = ImageSource.FromStream(() => new MemoryStream(b));
           // here.Source = xd;
            SocketCore.SendFile(b);
        }

        private void FindPeople_Clicked(object sender, System.EventArgs e) => Navigation.PushAsync(new SearchPage()); //Don't pop cuz you can get back to "main menu"

        private void FriendList_Clicked(object sender, System.EventArgs e)
        {
            return;
            Navigation.PushAsync(null); //TODO: Friend list ....
        }

        protected override bool OnBackButtonPressed()
        {
            SocketCore.Send(" ", 0);
            LocalUser.Logout();
            return true;
        }

        private void LastChatsOpened_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var _temp = (ListView)sender;
            _temp.SelectedItem = null;
        }
    }
}
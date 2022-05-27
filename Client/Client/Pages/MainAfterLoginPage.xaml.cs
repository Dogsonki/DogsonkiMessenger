using Client.Networking;
using Client.Views;
using System.IO;
using System.Reflection;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Client.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainAfterLoginPage : ContentPage
    {
        public static MainAfterLoginPage MainInstance { get; set; }

        public MainAfterLoginPage(bool redirectedFormLogin = false)
        {
            NavigationPage.SetHasNavigationBar(this, false);
            InitializeComponent();

            MainInstance = this;
            if (redirectedFormLogin)
                MainAfterLoginViewModel.Clear();
            return;
            #region test png buffer

            if (false)
            {
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
            }
            #endregion
        }

        private void FindPeople_Clicked(object sender, System.EventArgs e)
        {
            Navigation.PushAsync(new PeopleFinder()); //Don't pop cuz you can get back to "main menu"
        }

        private void FriendList_Clicked(object sender, System.EventArgs e)
        {
            return;
            Navigation.PushAsync(null); //TODO: Friend list ....
        }

        protected override bool OnBackButtonPressed()
        {
            SocketCore.SendRaw(" ", 0);
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
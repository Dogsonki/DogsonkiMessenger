#define l
using Client.Networking;
using Client.Utility.Services;
using Client.Views;
using System;
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
        public static Image f;
        public MainAfterLoginPage(bool redirectedFormLogin = false)
        {
            NavigationPage.SetHasNavigationBar(this, false);
            InitializeComponent();
            f = here;

          
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

            return;
            IFileService aa = DependencyService.Get<IFileService>();
            byte[] buffer = aa.ReadFileFromStorage("B.png");

            using (MemoryStream s = new MemoryStream(buffer))
            {
                if (s == null)
                {
                    Console.WriteLine("Buffer was null");
                }
                SocketCore.SendFile(s.ToArray());
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
            SocketCore.SendRaw("2");
            LogOut("");
            return base.OnBackButtonPressed();
        }

        private void LogOut(string rev)
        {
            LocalUser.IsLoggedIn = false;
            LocalUser.ActualChatWith = "";
            LocalUser.Username = "";
        }

        private void LastChatsOpened_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var _temp = (ListView)sender;
            _temp.SelectedItem = null;
        }
    }
}
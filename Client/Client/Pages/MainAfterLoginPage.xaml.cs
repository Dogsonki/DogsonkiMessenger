using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Client.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainAfterLoginPage : ContentPage
    {
        public MainAfterLoginPage()
        {
            NavigationPage.SetHasNavigationBar(this,false); 
            InitializeComponent();
        }
    }
}
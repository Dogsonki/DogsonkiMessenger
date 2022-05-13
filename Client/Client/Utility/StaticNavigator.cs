using Xamarin.Forms;

namespace Client.Utility
{
    public class StaticNavigator //TODO: make it async
    {
        public static void PopAndPush(Page page)
        {
            Application.Current.MainPage.Navigation.PopAsync();
            Application.Current.MainPage.Navigation.PushAsync(page);
        }

        public static void Push(Page page)
        {
            Application.Current.MainPage.Navigation.PushAsync(page);
        }
    }
}

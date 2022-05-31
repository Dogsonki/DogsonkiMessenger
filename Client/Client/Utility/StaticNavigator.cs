using Client.Pages;
using Xamarin.Forms;

namespace Client.Utility
{
    public enum MainPages
    {
        MainAfterLogin,
        Entry
    }
    public class StaticNavigator //TODO: make it async
    {
        public static void PopAndPush(Page page)
        {
            Application.Current.MainPage.Navigation.PopAsync();
            Application.Current.MainPage.Navigation.PushAsync(page);
        }

        public static void Change(MainPages page)
        {
            switch (page)
            {
                case MainPages.MainAfterLogin:
                    Push(MainAfterLoginPage.MainInstance ?? MainAfterLoginPage.MainInstance ?? new MainAfterLoginPage());
                    break;
            }
        }

        public static void Push(Page page)
        {
            Application.Current.MainPage.Navigation.PushAsync(page);
        }
    }
}

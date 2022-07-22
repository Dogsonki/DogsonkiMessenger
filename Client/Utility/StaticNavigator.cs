namespace Client
{
    public class StaticNavigator //TODO: make it async
    {
        public static void PopAndPush(Page page)
        {
            Application.Current.MainPage.Navigation.PopAsync();
            Application.Current.MainPage.Navigation.PushAsync(page);
        }

        public static void PushOnTop(Page page) => Application.Current.MainPage.Navigation.PushModalAsync(page);

        public static void Push(Page page)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Application.Current.MainPage.Navigation.PushAsync(page, false);
            });
        }
    }
}
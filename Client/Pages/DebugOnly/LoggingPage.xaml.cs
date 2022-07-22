namespace Client.Pages.DebugOnly;

public partial class LoggingPage : ContentPage
{
    public LoggingPage()
    {
        InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);
    }
}
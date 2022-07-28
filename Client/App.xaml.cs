using Client.Networking.Core;
using Client.Pages;
using Client.IO;
using Client.Models.UserType.Bindable;

namespace Client;

public partial class App : Application
{
    private static LoginPage CurrentLoginPage;

    public App()
    {
        InitializeComponent();
        try
        {
            Connection.AddOnConnection(Session.ReadSession);
            SocketCore.Init();
            CurrentLoginPage = new LoginPage();
            MainPage = new NavigationPage(CurrentLoginPage);
        }
        catch(Exception ex)
        {
            SocketCore.Send(ex);
        }
    }
}
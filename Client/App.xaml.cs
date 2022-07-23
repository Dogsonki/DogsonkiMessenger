using Client.Networking.Core;
using Client.Pages;
using Client.IO;

namespace Client;

public partial class App : Application
{
    private static LoginPage CurrentLoginPage;

    public App()
    {
        InitializeComponent();
        try
        {
            CurrentLoginPage = new LoginPage();

            Connection.AddOnConnection(Session.ReadSession);
            SocketCore.Init(); //TODO: if throw redirect login with error 

            MainPage = new NavigationPage(CurrentLoginPage);

        }
        catch(Exception ex)
        {
            SocketCore.Send(ex);
        }
    }


}

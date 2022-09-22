using Client.Networking.Core;
using Client.Pages;
using Client.IO;

namespace Client;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        Connection.AddOnConnection(Session.ReadSession);

        Task.Run(SocketCore.Start);
#if DEBUG   
        try
        {
            MainPage = new NavigationPage(new LoginPage());
        }
        catch (Exception ex)
        {
            SocketCore.Send(ex.ToString(), Token.EMPTY);
        }
#else
        MainPage = new NavigationPage(new LoginPage());
#endif
    }
}
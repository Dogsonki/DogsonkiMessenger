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
        try
        {
            MainPage = new NavigationPage(new LoginPage());
        }
        catch (Exception ex)
        {
            SocketCore.Send(ex, Token.EMPTY);
        }
    }
}
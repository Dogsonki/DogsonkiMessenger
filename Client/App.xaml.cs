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

        MainPage = new NavigationPage(new LoginPage());
    }
}
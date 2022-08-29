using Client.Networking.Core;
using Client.Pages;
using Client.IO;

namespace Client;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        try
        {
            Connection.AddOnConnection(Session.ReadSession);
            Task.Factory.StartNew(SocketCore.Start);

            MainPage = new NavigationPage(new LoginPage());
        }
        catch(Exception ex)
        {
            SocketCore.Send(ex);
        }
    }
}
using Client.Networking.Core;
using Client.Pages;
using Client.IO;
using Client.Utility;

namespace Client;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        Connection.AddOnConnection(Session.Init);

        Task.Run(SocketCore.Start);
#if DEBUG   
        try
        {
            MainPage = new NavigationPage(new LoginPage());
        }
        catch (Exception ex)
        {
            Logger.Push(ex,TraceType.Func,LogLevel.Error);
        }
#else
        MainPage = new NavigationPage(new LoginPage());
#endif
    }
}
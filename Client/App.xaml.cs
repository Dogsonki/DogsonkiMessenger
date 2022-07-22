using Client.Models.Session;
using Client.Networking.Core;
using Client.Pages;
using Newtonsoft.Json;

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

            MainPage = new NavigationPage(CurrentLoginPage);

            Connection.AddOnConnection(ReadSession);

            SocketCore.Init();
        }
        catch(Exception ex)
        {
            SocketCore.Send(ex);
        }
    }

    private static void ReadSession()
    {
#if ANDROID
        AndroidFileService wr = new();
        bool SessionExist = wr.CreateFileIfNotExist("session.json");
        if (SessionExist)
        {
            string ses = File.ReadAllText(AndroidFileService.GetPersonalDir("session.json"));
            Session session = JsonConvert.DeserializeObject<Session>(ses);
            if (session != null && !string.IsNullOrEmpty(ses))
            {
                if (session.SessionKey != null)
                {
                    SocketCore.Send(session, Token.SESSION_INFO);
                }
            }
        }
#endif
    }
}

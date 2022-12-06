using Client.IO.Interfaces;
using Client.Models.Bindable;
using Client.Networking.Core;
using Client.Networking.Packets;
using Client.Pages;
using Client.Utility;
using Newtonsoft.Json;

namespace Client.IO;

[Serializable]
public class Session : IStorage
{
    private const string FileName = "session.json";

    [JsonProperty("session_key")] public string SessionKey { get; set; } = string.Empty;

    [JsonProperty("login_id")] public string LoginId { get; set; } = string.Empty;

    [JsonConstructor]
    public Session(string session_key, string login_id)
    {
        SessionKey = session_key;
        LoginId = login_id;
    }

    private static void OverwriteSession(object session)
    {
        Cache.SaveToCache(JsonConvert.SerializeObject(session), "session.json");
        Logger.Push("Overwriting session to cache", LogLevel.Warning);
    }

    public static void Init()
    {
        SocketCore.OnToken(Token.SESSION_INFO, GetSessionInfoCallback);
        SocketCore.OnToken(Token.LOGIN_SESSION, LoginBySessionCallback);

        ReadSession();
    }

    private static void ReadSession()
    {
        string cache = Cache.ReadFileCache("session.json");

        if (string.IsNullOrWhiteSpace(cache))
        {
            Logger.Push("Session info was null", LogLevel.Warning);
            return;
        }

        Session? session = JsonConvert.DeserializeObject<Session>(cache);

        if (session is null || string.IsNullOrEmpty(session.SessionKey)) return;

        SocketCore.Send(session, Token.SESSION_INFO);
    }

    private static void GetSessionInfoCallback(object packet)
    {
        Session? session = JsonConvert.DeserializeObject<Session?>((string)packet);

        if (session is null)
            return;

        OverwriteSession(session);
    }

    private static void LoginBySessionCallback(object packet)
    {
        LoginCallbackPacket? login = JsonConvert.DeserializeObject<LoginCallbackPacket>((string)packet);

        if (login is null)
            return;

        if (login.Token == "1")
        {
            LocalUser.Login(login.Username, login.ID, login.Email);
        }
        else if (login.Token == "-1")
        {
            LoginPage.Current.message.ShowError("User is banned");
        }
    }
}
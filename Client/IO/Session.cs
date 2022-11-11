using Client.IO.Interfaces;
using Client.Models.Bindable;
using Client.Networking.Core;
using Client.Networking.Packets;
using Client.Pages;
using Client.Utility;
using Newtonsoft.Json;
using System.Security;

namespace Client.IO;

[Serializable]
public class Session : IStorage
{
    private const string FileName = "session.json";

    [JsonProperty("session_key")] public string SessionKey { get; set; } = "";

    [JsonProperty("login_id")] public string LoginId { get; set; } = "";

    [JsonConstructor]
    public Session(string session_key, string login_id)
    {
        SessionKey = session_key;
        LoginId = login_id;
    }

    public static void OverwriteSession(Session session)
    {
        Cache.Cache.SaveToCache(JsonConvert.SerializeObject(session), "session.json");
        Logger.Push("Overwriting session to cache", TraceType.Func, LogLevel.Warning);
    }

    public static void ReadSession()
    {
        string cache = Cache.Cache.ReadFileCache("session.json");

        if (cache == null || cache.Length == 0)
        {
            Logger.Push("Session info was null", TraceType.Func, LogLevel.Warning);
            return;
        }

        Session? session = JsonConvert.DeserializeObject<Session>(cache);

        if (session is null || string.IsNullOrEmpty(session.SessionKey)) return;

        SocketCore.OnToken(Token.LOGIN_SESSION, LoginBySessionCallback);
        SocketCore.SendCallback(session, Token.SESSION_INFO, GetSessionInfoCallback);
#if ANDROID 
        
#endif
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
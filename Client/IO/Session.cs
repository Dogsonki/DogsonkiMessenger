using Client.IO.Interfaces;
using Client.Models;
using Client.Networking.Core;
using Client.Networking.Models;
using Client.Networking.Packets;
using Client.Pages;
using Client.Utility;
using Microsoft.AspNetCore.Components;
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
        Cache.SaveToCache(JsonConvert.SerializeObject(session), FileName);
        Logger.Push("Overwriting session to cache", LogLevel.Warning);
    }

    public static void Init(Action<SocketPacket> loginCallback)
    {
        SocketCore.OnToken(Token.SESSION_INFO, GetSessionInfoCallback);
        SocketCore.OnToken(Token.LOGIN_SESSION, loginCallback);

        ReadSession(loginCallback);
    }

    private static void ReadSession(Action<SocketPacket> callback)
    {
        string cache = Cache.ReadFileCache(FileName);

        if (string.IsNullOrWhiteSpace(cache))
        {
            Logger.Push("Session info was null", LogLevel.Warning);
            return;
        }

        Session? session = JsonConvert.DeserializeObject<Session>(cache);

        if (session is null || string.IsNullOrEmpty(session.SessionKey)) return;

        SocketCore.Send(session, Token.SESSION_INFO);
    }

    public static void DeleteSession()
    {
        Cache.RemoveFromCache(FileName);
    }

    private static void GetSessionInfoCallback(SocketPacket packet)
    {
        Session? session = packet.Deserialize<Session>(); 

        if (session is null)
            return;

        OverwriteSession(session);
    }
}
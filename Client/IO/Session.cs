using Client.Networking.Core;
using Newtonsoft.Json;

namespace Client.IO;

[Serializable]
public class Session : IStorage
{
    [JsonProperty("session_key")]
    public string SessionKey { get; set; } = string.Empty;

    [JsonProperty("login_id")]
    public string LoginID { get; set; } = string.Empty;

    public static void OverwriteSession(Session session)
    {
#if ANDROID
        AndroidFileService wr = new AndroidFileService();
        wr.WriteToFile(JsonConvert.SerializeObject(session), "session.json");
#endif
    }

    [JsonConstructor]
    public Session(string session_key, string login_id)
    {
        SessionKey = session_key;
        LoginID = login_id;
    }

    public static void ReadSession()
    {
#if ANDROID
        AndroidFileService wr = new();
        bool SessionExist = wr.CreateFileIfNotExist("session.json");
        if (SessionExist)
        {
            string ses = File.ReadAllText(AndroidFileService.GetPersonalDir("session.json"));
            Session? session = JsonConvert.DeserializeObject<Session>(ses);
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
using Client.Networking.Core;
using Newtonsoft.Json;

namespace Client.IO;

[Serializable]
public class Session : IStorage
{
    [JsonProperty("session_key")]
    public string SessionKey { get; set; }

    [JsonProperty("login_id")]
    public string LoginId { get; set; }

    [JsonConstructor]
    public Session(string session_key, string login_id)
    {
        SessionKey = session_key;
        LoginId = login_id;
    }

    public static void OverwriteSession(Session session)
    {
#if ANDROID
        AndroidFileService wr = new AndroidFileService();
        wr.WriteToFile(JsonConvert.SerializeObject(session), "session.json");
#endif
    }

    public static void ReadSession()
    {
#if ANDROID 
        AndroidFileService wr = new();
        bool sessionExists = wr.CreateFileIfNotExist("session.json");
        if (sessionExists)
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
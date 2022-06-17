using Newtonsoft.Json;
using Client;

namespace Client.IO;

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

    public Session(string session_key, string login_id)
    {
        SessionKey = session_key;
        LoginID = login_id;
    }
}
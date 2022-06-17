using Client.IO;
using Newtonsoft.Json;
using System.Text;

namespace Client.Model.Session;

public class Session : IStorage
{
    [JsonProperty("session_key")]
    public string SessionKey { get; set; } = string.Empty;

    [JsonProperty("login_id")]
    public string LoginID { get; set; } = string.Empty;

    public static void OverwriteSession(Session session)
    {
        IFileService file = DependencyService.Get<IFileService>();
        file.WriteToFile(new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(session))), "session.json");
    }

    public Session(string session_key, string login_id)
    {
        SessionKey = session_key;
        LoginID = login_id;
    }
}
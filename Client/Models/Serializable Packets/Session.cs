﻿using Client.IO;
using Newtonsoft.Json;
using System.Text;

namespace Client.Models.Session;

[Serializable]
public class Session : IStorage
{
    [JsonProperty("session_key")]
    public string SessionKey { get; set; } = string.Empty;

    [JsonProperty("login_id")]
    public string LoginID { get; set; } = string.Empty;

    [JsonConstructor]
    public Session(string session_key, string login_id)
    {
        SessionKey = session_key;
        LoginID = login_id;
    }

    public static void OverwriteSession(Session session)
    {
        IFileService file = DependencyService.Get<IFileService>();
        file.WriteToFile(new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(session))), "session.json");
    }
}
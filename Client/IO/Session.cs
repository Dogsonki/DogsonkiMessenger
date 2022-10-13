﻿using Client.IO.Interfaces;
using Client.Models.Bindable;
using Client.Networking.Core;
using Client.Networking.Packets;
using Client.Pages;
using Client.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
#if ANDROID
        AndroidFileService wr = new AndroidFileService();
        wr.WriteToFile(JsonConvert.SerializeObject(session), "session.json");
#endif
    }

    public static void ReadSession()
    {
#if ANDROID 
        AndroidFileService wr = new();
        bool sessionExists = wr.CreateFileIfNotExist(FileName);
        if (sessionExists)
        {
            string ses = File.ReadAllText(AndroidFileService.GetPersonalDir(FileName));
            Session? session = JsonConvert.DeserializeObject<Session>(ses);
            if (session is not null && !string.IsNullOrEmpty(ses))
            {
                if (session.SessionKey != string.Empty)
                {
                    SocketCore.OnToken(Token.LOGIN_SESSION, LoginBySessionCallback);
                    SocketCore.SendCallback(GetSessionInfoCallback,session, Token.SESSION_INFO);
                }
            }
        }
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
        Debug.Write("LOGINN ?");
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
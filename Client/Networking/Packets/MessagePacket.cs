﻿using System.Text;
using Client.Utility;
using Newtonsoft.Json;

namespace Client.Networking.Packets;

public class MessagePacket
{
    [JsonProperty("message")]
    public byte[] Content;

    [JsonProperty("message_type")]
    public string MessageType;

    [JsonIgnore]
    public string ContentString;

    [JsonIgnore]
    public bool IsGroup;

    [JsonIgnore]
    public int GroupId;

    [JsonIgnore]
    public int UserId;

    [JsonIgnore]
    public string Username;

    [JsonIgnore]
    public DateTime Time;

    [JsonConstructor]
    public MessagePacket(string username, byte[] message, string message_type, double time, int user_id, bool is_group, int group_id)
    {
        ContentString = Encoding.UTF8.GetString(message);
        MessageType = message_type;
        Time = Essential.UnixToDateTime(time);
        UserId = user_id;
        IsGroup = is_group;
        GroupId = group_id;
        Username = username;
    }

    public MessagePacket(byte[] imageBuffer, string extension)
    {
        MessageType = extension;
        Content = imageBuffer;
    }

    public MessagePacket(string message)
    {
        MessageType = "text";
        Content = Encoding.UTF8.GetBytes(message);
    }
}
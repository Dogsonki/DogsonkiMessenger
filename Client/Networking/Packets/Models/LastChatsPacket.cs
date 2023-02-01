#nullable enable

namespace Client.Networking.Packets.Models;

[Serializable]
partial class LastChatsPacket
{
    public string Name{ get; }
    public double? LastMessageTime { get; set; }
    public byte[]? LastMessage { get; }
    public string? MessageType { get; }
    public uint Id { get; }
    public string? Type { get; }
    public string? Sender { get; }
    public double? LastOnlineTime { get; }

    public bool isGroup => Type != "user";

    public LastChatsPacket(string name, uint id, double? last_online_time, double? last_message_time,
        string? type, string? message_type, byte[]? message, string? sender)
    {
        Name = name;

        if (last_message_time != null)
        {
            LastMessageTime = last_message_time;
        }

        LastOnlineTime = last_online_time;
        Type = type;
        LastMessage = message;
        MessageType = message_type;
        Id = id;
        Sender = sender;
    }
}
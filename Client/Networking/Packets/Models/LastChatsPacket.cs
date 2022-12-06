using Client.Models.Bindable;

namespace Client.Networking.Packets.Models;

[Serializable]
partial class LastChatsPacket
{
    public readonly User? BindedUser;
    public readonly Group? BindedGroup;

    public string Name{ get; set; }
    public double? LastMessageTime { get; set; }
    public byte[]? LastMessage { get; set; }
    public string? MessageType { get; set; }
    public uint Id { get; set; }
    public string? Type { get; set; }
    public string? Sender { get; set; }

    public bool isGroup => Type != "user";

    public LastChatsPacket(string name, uint id, double? last_message_time, string? type, string? message_type, byte[]? message, string? sender)
    {
        Name = name;
        if (last_message_time != null)
        {
            LastMessageTime = last_message_time;
        }
        Type = type;
        LastMessage = message;
        MessageType = message_type;
        Id = id;
        Sender = sender;
    }
}
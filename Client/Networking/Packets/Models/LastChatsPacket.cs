#nullable enable

using Client.Models;
using Client.Models.Chat;

namespace Client.Networking.Packets.Models;

[Serializable]
partial class LastChatsPacket
{
    public string Name{ get; }
    public double? LastMessageTime { get; set; }
    public byte[]? LastMessage { get; }
    public MessageType? TypeOfMessage { get; }
    public uint Id { get; }
    public string? Type { get; }
    public string? MessageSenderName { get; }
    public double? LastOnlineTime { get; }
    public FriendStatus IsFriend { get; }
    public bool isGroup => Type != "user";

    public LastChatsPacket(string name, uint id, double? last_online_time, double? last_message_time,
        string? type, string message_type, byte[]? message, string? sender, int is_friend)
    {
        Name = name;

        if (last_message_time != null)
        {
            LastMessageTime = last_message_time;
        }

        LastOnlineTime = last_online_time;
        Type = type;
        LastMessage = message;
        TypeOfMessage = GetMessageType(message_type);
        Id = id;
        MessageSenderName= sender;
        IsFriend = (FriendStatus)is_friend;
    }

    private MessageType GetMessageType(string messageType)
    {
        if(messageType == "text")
        {
            return MessageType.Text;
        }
        return MessageType.Image;
    }
}
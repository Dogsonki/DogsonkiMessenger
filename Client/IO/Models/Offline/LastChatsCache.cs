using Client.Models.LastChats;
using Newtonsoft.Json;

namespace Client.IO.Models.Offline;

[Serializable]
public class LastChatCache
{
    public uint ChatId { get; set; }
    public bool IsGroup { get; set; }
    public string ChatName { get; set; }
    public double? LastMessageTime { get; set; }
    public string? LastMessageAuthorName { get; set; }
    public string? LastMessage { get; set; }

    [JsonConstructor]
    public LastChatCache(uint chatId, bool isGroup, string chatName, double? lastMessageTime, 
        string? lastMessageAuthorName, string? lastMessage)
    {
        ChatId = chatId;
        ChatName = chatName;
        LastMessageTime = lastMessageTime;
        LastMessageAuthorName = lastMessageAuthorName;
        LastMessage = lastMessage;
        IsGroup = isGroup;
    }

    public LastChatCache(LastChat lastChat)
    {
        ChatId = lastChat.Id;
        IsGroup = lastChat.View.IsUser();
        ChatName = lastChat.Name;
        LastMessageTime = lastChat.LastMessageTimestamp;
        LastMessageAuthorName = lastChat.MessageSenderName;
        LastMessage = lastChat.Message;
    }
}
using Newtonsoft.Json;
using System.Text;
using Client.Utility;
using Client.IO.Models;
using Client.Models.Bindable;

namespace Client.IO;

internal class ChatCache
{
    public ChatCache(ChatMessage[] messages, User user)
    {
        Task.Run(() =>
        {
            ChatMessage[] _messages = new ChatMessage[30];
            int len = 30;

            if (messages.Length > 30)
            {
                for (int i = 0; i < 30; i++)
                {
                    _messages[i] = messages[messages.Length - i];
                }
            }
            else
            {
                _messages = messages;
                len = _messages.Length;
            }

            List<ChatMessageCacheModel> cacheMessages = new List<ChatMessageCacheModel>(len);

            foreach (ChatMessage m in _messages)
            {
                cacheMessages.Add(new ChatMessageCacheModel(m.BindedUser.UserId, m.TextContent, m.Time.Ticks, m.IsText));
            }

            _messages.ToList().Sort((x, y) => DateTime.Compare(x.Time, y.Time));

            DateTime lastMessageTime = _messages[0].Time;

            ChatCacheModel model = new ChatCacheModel(cacheMessages, lastMessageTime.Ticks);

            Cache.SaveToCache(JsonConvert.SerializeObject(cacheMessages), $"cache_chat_{user.UserId}");
        });
    }

    public static ChatMessage[]? ReadCacheChat(User user)
    {
        byte[] cachedChat = Cache.ReadFileBytesCache($"cache_chat_{user.UserId}");

        if (cachedChat is null || cachedChat.Length == 0)
        {
            Debug.Write($"cache null {user.UserId}");
            return null;
        }

        string messagesJson = Encoding.UTF8.GetString(cachedChat);

        ChatMessageCacheModel[]? deserializedMessages = JsonConvert.DeserializeObject<ChatMessageCacheModel[]>(messagesJson);

        if (deserializedMessages is null || deserializedMessages?.Length == 0)
        {
            return null;
        }

        List<ChatMessage> messages = new List<ChatMessage>(deserializedMessages.Length);

        foreach (ChatMessageCacheModel model in deserializedMessages)
        {
            messages.Add(new ChatMessage(model));
        }

        return messages.ToArray();
    }
}
using Newtonsoft.Json;

namespace Client.IO.Models;

[Serializable]
public class ChatMessageCacheModel
{
    [JsonProperty("userId")]
    public int UserId { get; set; }
    [JsonProperty("message")]
    public string Message { get; set; }
    [JsonProperty("date")]
    public double Date { get; set; }
    [JsonProperty("isText")]
    public bool IsText { get; set; }

    [JsonConstructor]
    public ChatMessageCacheModel(int userId, string message, double date, bool isText)
    {
        UserId = userId;
        Message = message;
        Date = date;
        IsText = isText;
    }
}
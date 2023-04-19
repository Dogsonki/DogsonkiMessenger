namespace Client.IO.Models.Offline;

[Serializable]
public class LocalUserCache
{
    public string UserName { get; set; }
    public uint UserId { get; set; }

    public LocalUserCache(string userName, uint userId)
    {
        UserName = userName;
        UserId = userId;
    }
}
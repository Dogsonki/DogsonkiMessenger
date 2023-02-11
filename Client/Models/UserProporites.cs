namespace Client.Models;

/// <summary>
/// User properties with local user
/// </summary>
public class UserPropertiesLocal
{
    public FriendStatus IsFriend { get; set; } = FriendStatus.Unknown;
}

public enum FriendStatus
{
    None = 0,
    Invited = 1,
    Friend = 2,
    Unknown
}
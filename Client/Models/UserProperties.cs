namespace Client.Models;

/// <summary>
/// User properties with local user
/// </summary>
public class UserProperties
{
    public FriendStatus IsFriend { get; set; } = FriendStatus.Unknown;
    public UserStatus Status { get; set; } = UserStatus.Offline;
}

public enum FriendStatus
{
    None = 0,
    Invited = 1,
    Friend = 2,
    Unknown
}
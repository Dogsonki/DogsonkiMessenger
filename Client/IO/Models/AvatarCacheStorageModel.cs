namespace Client.IO.Models;

internal class AvatarCacheStorageModel
{
    public double AvatarTicks { get; set; }
    public uint UserId { get; set; }

    public AvatarCacheStorageModel(double avatarTicks, uint userId)
    {
        AvatarTicks = avatarTicks;
        UserId = userId;
    }
}
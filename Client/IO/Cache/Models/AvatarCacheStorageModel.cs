namespace Client.IO.Cache.Models;

internal class AvatarCacheStorageModel
{
    public double AvatarTicks { get; set; }
    public int UserId { get; set; }

    public AvatarCacheStorageModel(double avatarTicks, int userId)
    {
        AvatarTicks = avatarTicks;
        UserId = userId;
    }
}
using Client.Networking.Core;

namespace Client.Networking.Models;

internal class AsyncRequestedCallbackModel 
{
    public bool IsCompleted { get; set; }
    public Token PacketToken { get; }
    public SocketPacket? AllocatedPacket { get; set; } = null;

    public AsyncRequestedCallbackModel(Token token)
    {
        PacketToken = token;
    }

    public void SetCompleted(SocketPacket packet) 
    {
        AllocatedPacket = packet;
        IsCompleted = true;
    }
}
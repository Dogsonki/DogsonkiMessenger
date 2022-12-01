using Client.Networking.Models;

namespace Client.Networking.Core;

internal static class SocketQueue
{
    public static Queue<SocketPacket> Queue = new Queue<SocketPacket>();

    public static void Add(SocketPacket packet) => Queue.Enqueue(packet);

    public static bool CanSend() => Queue.Count > 0 && !IsSending;
    public static bool IsSending = false;
}

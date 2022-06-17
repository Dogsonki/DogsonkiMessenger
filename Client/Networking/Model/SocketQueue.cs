namespace Client.Networking.Model;

public class SocketQueue
{
    public static List<SocketPacket> SendingPackets = new List<SocketPacket>();
    public static List<SocketPacket> WaitingPackets = new List<SocketPacket>();

    /// <summary>
    /// Adds packet to WaitingPackets 
    /// </summary>
    /// <param name="packet"></param>
    public static void Add(SocketPacket packet) => WaitingPackets.Add(packet);

    /// <summary>
    /// Called every time when SendingPakcets got looped
    /// </summary>
    public static void Renew()
    {
        SendingPackets = new List<SocketPacket>(WaitingPackets);
        WaitingPackets.Clear();
    }

    /// <summary>
    /// If count of SendingPackets is more than 0
    /// </summary>
    /// <returns></returns>
    public static bool AbleToSend() => SendingCount > 0;

    public static int SendingCount => SendingPackets.Count;

    public static SocketPacket[] GetSendingPackets => SendingPackets.ToArray();
}
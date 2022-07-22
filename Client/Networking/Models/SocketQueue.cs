namespace Client.Networking.Model;

/* Works as Stacked Queue */
public class SocketQueue
{
    public static List<SocketPacket> SendingPackets = new List<SocketPacket>();
    public static List<SocketPacket> WaitingPackets = new List<SocketPacket>();

    /// <summary>
    /// Adds packet to WaitingPackets 
    /// </summary>
    public static void Add(SocketPacket packet) => WaitingPackets.Add(packet);

    /// <summary>
    /// Called every time when SendingPakcets got looped
    /// </summary>
    public static void Renew()
    {
        if (WaitingPackets.Count > 0 || SendingPackets.Count > 0)
        {
            SendingPackets = new List<SocketPacket>(WaitingPackets);
        }

        WaitingPackets.Clear();
    }

    /// <summary>
    /// Returns count of SendingPackets is more than 0
    /// </summary>
    public static bool AbleToSend() => SendingCount > 0;

    public static int SendingCount => SendingPackets.Count;

    public static SocketPacket[] GetSendingPackets => SendingPackets.ToArray();
}
using System.Collections.Generic;
using Client.Networking.Model;

namespace Client.Networking
{
    public class SocketQueue
    {
        protected static List<SocketPacketModel> SendingPackets = new List<SocketPacketModel>();
        protected static List<SocketPacketModel> WaitingPackets = new List<SocketPacketModel>();

        /// <summary>
        /// Adds packet to WaitingPackets 
        /// </summary>
        /// <param name="packet"></param>
        public static void Add(SocketPacketModel packet) => WaitingPackets.Add(packet);

        /// <summary>
        /// Called every time when SendingPakcets got looped
        /// </summary>
        public static void Renew()
        {
            if(WaitingPackets.Count > 0)
            {
                SendingPackets = new List<SocketPacketModel>(WaitingPackets);
                WaitingPackets.Clear();
            }  
        }

        /// <summary>
        /// If count of SendingPackets is more than 0
        /// </summary>
        /// <returns></returns>
        public static bool AbleToSend() => SendingCount > 0;

        public static int SendingCount => SendingPackets.Count;
        
        public static List<SocketPacketModel> GetSendingPackets => SendingPackets;
    }
}

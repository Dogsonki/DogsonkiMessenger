namespace Client.Networking.Model
{
    public class SocketPacketModel
    {
        public byte[] Data { get; set; }

        protected int UpdatedIndex;

        public bool IsImage { get; set; } = false;

        public SocketPacketModel() { }

        public SocketPacketModel(byte[] bytes)
        {
            IsImage = true;
        }

        public int GetIndex() => UpdatedIndex;
    }
}

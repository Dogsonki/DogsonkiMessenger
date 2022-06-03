using System;
using System.Collections.Generic;
using System.IO;
using Xamarin.Forms;

namespace Client.Networking
{
    public class SocketImage
    {
        public static bool isReadingImage { get; set; }

        public static List<byte[]> ImageBuffer = new List<byte[]>();

        public static void AddBuffer(byte[] buffer)
        {
            if (buffer.Length < 1)
                return;
            ImageBuffer.Add(buffer);
        }

        public static void SendImage(byte[] image)
        {

        }

        public static ImageSource GetImage()
        {
            List<byte> temp = new List<byte>();
            foreach(byte[] by in ImageBuffer)
                foreach (byte b in by)
                    temp.Add(b);
            ImageSource sr = ImageSource.FromStream(() => new MemoryStream(temp.ToArray()));
            temp.Clear();
            temp = null;
            return sr;
        }
    }
}

using System.IO;

namespace Client.IO
{
    public class Essential
    {
        public static byte[] StreamToBuffer(Stream stream,long maxBuffer = 12*1024)
        {
            byte[] buffer = new byte[maxBuffer];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
    }
}

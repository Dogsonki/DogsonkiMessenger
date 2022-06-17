namespace Client.Networking.Core;

public class SlicedBuffer
{
    protected List<byte[]> Buffer = new List<byte[]>();

    public SlicedBuffer(byte[] arr, int maxbuffer)
    {
        for (var i = 0; i < arr.Length / maxbuffer + 1; i++)
        {
            Buffer.Add(arr.Skip(i * maxbuffer).Take(maxbuffer).ToArray());
        }
    }

    public List<byte[]> GetSlicedBuffer() => Buffer;
}
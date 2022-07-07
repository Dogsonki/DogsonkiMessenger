namespace Client.IO;

internal class Cache
{
    public static void SaveToCache(object obj,string name)//TODO: make it serializing data classes
    {
        if(obj.GetType() == typeof(byte[]))
        {
#if ANDROID
            AndroidFileService wr = new AndroidFileService();
            wr.WriteToFile((byte[])obj, "/temp/"+name);
#endif
        }
    }

    public static T ReadCache<T>(string path)
    {
#if ANDROID
        AndroidFileService wr = new AndroidFileService();
        byte[] Buffer = wr.ReadFileFromStorage(path);

        if (typeof(T) == typeof(ImageSource))
        {
            return (T)(object)ImageSource.FromStream(() => new MemoryStream(Buffer));
        }
#endif
        return default;
    }

    public static byte[] ReadCache(string path) => ReadCache<byte[]>(path);
}

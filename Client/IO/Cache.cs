namespace Client.IO;

internal class Cache
{
    private const string CachePath = "/temp/";

    public static void SaveToCache(object obj,string name)//TODO: make it serializing data classes
    {
        if (obj is null) { Debug.Error("Cached object is null "+name); return; }

        try
        {
            if (obj.GetType() == typeof(byte[]))
            {
#if ANDROID
                Task.Run(async () =>
                {
                    PermissionStatus status = await Permissions.RequestAsync<Permissions.StorageRead>();
                });

                if(!Directory.Exists(FileSystem.Current.AppDataDirectory + "/temp/"))
                {
                    Directory.CreateDirectory(FileSystem.Current.AppDataDirectory + "/temp/");
                }

                File.WriteAllBytes(FileSystem.Current.CacheDirectory + "/temp/"+name, (byte[])obj);
#endif
            }
        }
        catch(Exception ex)
        {
            Debug.Error(ex);
        }
    }

    public static T ReadCache<T>(string name)
    {
#if ANDROID
        string cachePath = FileSystem.Current.CacheDirectory + "/temp/";

        if (!File.Exists(cachePath + name))
        {
            Debug.Error("Cache file dose not exist");
            return default(T);
        }
        byte[] Buffer = File.ReadAllBytes(cachePath + name);

        if (typeof(T) == typeof(ImageSource))
        {
            return (T)(object)ImageSource.FromStream(() => new MemoryStream(Buffer));
        }
#endif
        return default;
    }

    public static byte[] ReadCache(string name)
    {
        try
        {
            string cachePath = FileSystem.Current.CacheDirectory + "/temp/";
            if (!File.Exists(cachePath + name))
            {
                Debug.Error("Cache file dose not exist");
                return new byte[0];
            }
            return File.ReadAllBytes(cachePath + name);
        }
        catch(Exception ex)
        {
            Debug.Error(ex);
            return new byte[0];
        }
    }

    public static void DebugPrintAllCacheFiles()
    {
        foreach(string file in Directory.GetFiles(FileSystem.Current.CacheDirectory + "/temp/"))
        {
            Debug.Write(file);
        }
    }
}

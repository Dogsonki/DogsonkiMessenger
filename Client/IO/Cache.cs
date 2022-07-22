using Client.Utility;

namespace Client.IO;

internal class Cache
{
    private const string CachePath = "/temp/";

    public static void SaveToCache(object obj, string name)//TODO: make it serializing data classes
    {
        if (obj is null) { Logger.Push("Cannot cache null object", TraceType.Func, LogLevel.Error); return; }

        try
        {
            if (obj.GetType() == typeof(byte[]))
            {
#if ANDROID
                Task.Run(async () =>
                {
                    PermissionStatus status = await Permissions.RequestAsync<Permissions.StorageRead>();
                });

                if (!Directory.Exists(FileSystem.Current.CacheDirectory + "/temp/"))
                {
                    Directory.CreateDirectory(FileSystem.Current.CacheDirectory + "/temp/");
                }

                File.WriteAllBytes(FileSystem.Current.CacheDirectory + "/temp/" + name, (byte[])obj);
#endif
            }
        }
        catch (Exception ex)
        {
            Logger.Push(ex, TraceType.Func, LogLevel.Error);
        }
    }

    public static byte[] ReadCache(string name)
    {
        try
        {
            string cachePath = FileSystem.Current.CacheDirectory + "/temp/";
            if (!File.Exists(cachePath + name))
            {
                Logger.Push($"Cache file dose not exist {name}", TraceType.Func, LogLevel.Warning);
                return new byte[0];
            }
            return File.ReadAllBytes(cachePath + name);
        }
        catch (Exception ex)
        {
            Logger.Push(ex, TraceType.Func, LogLevel.Error);
            return new byte[0];
        }
    }

    public static void DebugPrintAllCacheFiles()
    {
        foreach (string file in Directory.GetFiles(FileSystem.Current.CacheDirectory + "/temp/"))
        {
            Debug.Write(file);
        }
    }
}

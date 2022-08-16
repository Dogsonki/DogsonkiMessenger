using Client.Utility;

namespace Client.IO;

internal class Cache
{
    //Max sum of every file size in cache directory
    public const long MAX_CACHE_SIZE = 200_000_000;

    /// <summary>
    /// For now cache only work with avatars: (byte[] avatarCache, avatar+UserId)
    /// </summary>
    public static void SaveToCache(object obj, string name)
    {
        if (!CheckCacheSize())
        {
            Debug.Write("Checking cache");
            ControlCache();
        }

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

                if (!Directory.Exists(CachePath))
                {
                    Directory.CreateDirectory(CachePath);
                }

                File.WriteAllBytes(CachePath + name, (byte[])obj);
#endif
            }
        }
        catch (Exception ex)
        {
            Logger.Push(ex, TraceType.Func, LogLevel.Error);
        }
    }

    public static string CachePath => FileSystem.Current.CacheDirectory + "/temp/";

    public static byte[] ReadCache(string name)
    {
        try
        {
            string cachePath = CachePath;
            if (!File.Exists(cachePath + name))
            {
                Logger.Push($"Cache file dose not exist {name}", TraceType.Func, LogLevel.Warning);
                return null;
            }
            Logger.Push($"Cache file exist {name}", TraceType.Func, LogLevel.Warning);

            return File.ReadAllBytes(cachePath + name);
        }
        catch (Exception ex)
        {
            Logger.Push(ex, TraceType.Func, LogLevel.Error);
            return null;
        }
    }

    //Clears half of cache files
    private static void ControlCache()
    {
        try
        {
            Dictionary<string,long> Files = new Dictionary<string, long>();
            string[] CacheFiles = Directory.GetFiles(CachePath);
            foreach (var file in CacheFiles)
            {
                Debug.Write($"Adding {file} to check");
                Files.Add(file,new FileInfo(file).Length);
            }

            var l = Files.OrderBy(i => i.Value);

            for (int i = Math.Abs(l.Count()/2) - 1; i >= 0; i--)
            {
                Debug.Write($"Deleting {Files.ElementAt(i).Key}");
                File.Delete(l.ElementAt(i).Key);
            }
        }
        catch(Exception ex)
        {
            Logger.Push(ex, TraceType.Func, LogLevel.Error);
        }

        if (!CheckCacheSize())
        {
            ControlCache();
        }
    }

    /// <summary>
    /// Returns true if files in cache is less than max cache size
    /// </summary>
    private static bool CheckCacheSize()
    {
        long sumSize = 0;
        string[] CacheFiles = Directory.GetFiles(CachePath);

        foreach(var file in CacheFiles)
        {
            long size = new FileInfo(file).Length;
            sumSize += size;
        }

        if(sumSize > MAX_CACHE_SIZE)
        {
            return false;
        }
        return true;
    }

    public static void ClearAbsoluteCache()
    {
        try
        {
            string[] CacheFiles = Directory.GetFiles(CachePath);

            foreach (var file in CacheFiles)
            {
                Logger.Push($"Deleting {file} from cache", TraceType.Func, LogLevel.Warning);
                File.Delete(file);
            }
        }
        catch (Exception ex)
        {
            Logger.Push(ex, TraceType.Func, LogLevel.Error);
        }
    }
}

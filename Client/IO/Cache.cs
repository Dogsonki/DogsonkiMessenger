using System.Text;
using Client.Utility;

namespace Client.IO;

internal class Cache
{
    //Max sum of every file size in cache directory
    public const long MAX_CACHE_SIZE = 200_000_000;
    public static string CachePath => FileSystem.Current.CacheDirectory + "/temp/";

    /// <summary>
    /// For now cache only work with avatars: (byte[] avatarCache, avatar+UserId)
    /// </summary>
    public static void SaveToCache(object obj, string name)
    {
        try
        {
            if (!CheckCacheSize())
            {
                ControlCache();
            }

            if (obj is null) { Logger.Push("Cannot cache null object", LogLevel.Error); return; }

            Task.Run(async () =>
            {
                PermissionStatus status = await Permissions.RequestAsync<Permissions.StorageRead>();
            });

            if (obj.GetType() == typeof(byte[]))
            {
                if (!Directory.Exists(CachePath))
                {
                    Directory.CreateDirectory(CachePath);
                }

                Task.Run(async () =>
                {
                    await File.WriteAllBytesAsync(CachePath + name, (byte[])obj);
                });
            }
            else if (obj.GetType() == typeof(string))
            {
                Task.Run(async () =>
                {
                    byte[] encoded = Encoding.UTF8.GetBytes((string)obj);
                    await File.WriteAllBytesAsync(CachePath + name, encoded);
                });
            }
        }
        catch (Exception ex)
        {
            Logger.Push(ex, LogLevel.Error);
        }
    }

    public static void RemoveFromCache(string name)
    {
        File.Delete(CachePath + name);
    }

    public static byte[] ReadFileBytesCache(string name)
    {
        try
        {
            if (!Directory.Exists(CachePath))
            {
                Directory.CreateDirectory(CachePath);
            }

            if (!File.Exists(CachePath + name))
            {
                Logger.Push($"Cache file dose not exist {name}", LogLevel.Warning);
                return null;
            }
            Logger.Push($"Cache file exist {name}", LogLevel.Warning);

            return File.ReadAllBytes(CachePath + name);
        }
        catch (Exception ex)
        {
            Logger.Push(ex, LogLevel.Error);
            return null;
        }
    }

    public static string ReadFileCache(string name)
    {
        try
        {
            if (!Directory.Exists(CachePath))
            {
                Directory.CreateDirectory(CachePath);
            }

            if (!File.Exists(CachePath + name))
            {
                Logger.Push($"Cache file dose not exist {name}", LogLevel.Warning);
                return null;
            }
            Logger.Push($"Cache file exist {name}", LogLevel.Warning);

            return File.ReadAllText(CachePath + name);
        }
        catch (Exception ex)
        {
            Logger.Push(ex, LogLevel.Error);
            return null;
        }
    }

    //Clears half of cache files
    private static void ControlCache()
    {
        try
        {
            Dictionary<string, long> Files = new Dictionary<string, long>();
            string[] CacheFiles = Directory.GetFiles(CachePath);
            foreach (var file in CacheFiles)
            {
                Debug.Write($"Adding {file} to check");
                Files.Add(file, new FileInfo(file).Length);
            }

            var l = Files.OrderBy(i => i.Value);

            for (int i = Math.Abs(l.Count() / 2) - 1; i >= 0; i--)
            {
                Debug.Write($"Deleting {Files.ElementAt(i).Key}");
                File.Delete(l.ElementAt(i).Key);
            }
        }
        catch (Exception ex)
        {
            Logger.Push(ex, LogLevel.Error);
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

        foreach (var file in CacheFiles)
        {
            long size = new FileInfo(file).Length;
            sumSize += size;
        }

        if (sumSize > MAX_CACHE_SIZE)
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
                Logger.Push($"Deleting {file} from cache", LogLevel.Warning);
                File.Delete(file);
            }

            Logger.Push($"DELETED {CacheFiles.Length} CACHE FILES", LogLevel.Warning);
        }
        catch (Exception ex)
        {
            Logger.Push(ex, LogLevel.Error);
        }
    }
}

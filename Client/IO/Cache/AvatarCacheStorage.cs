using System.Text;
using Client.IO.Cache.Models;
using Newtonsoft.Json;

namespace Client.IO.Cache;

internal class AvatarCacheStorage
{
    private const string AvatarCacheFileName = "avatarCacheStorage.json";

    public static void SaveAvatarCache(double ticks, int userId)
    {
        byte[]? avatarCacheStorage = Cache.ReadCache(AvatarCacheFileName);

        if (avatarCacheStorage is null || avatarCacheStorage.Length == 0)
        {
            Debug.Error("AvatarCache is null or empty creating new cache file");

            List<AvatarCacheStorageModel> model = new List<AvatarCacheStorageModel>();
            model.Add(new AvatarCacheStorageModel(ticks, userId));

            string json = JsonConvert.SerializeObject(model);

            Cache.SaveToCache(json, AvatarCacheFileName);
        }
        else
        {
            List<AvatarCacheStorageModel>? models = JsonConvert.DeserializeObject<List<AvatarCacheStorageModel>>(Encoding.UTF8.GetString(avatarCacheStorage));

            if (models is null)
            {
                Debug.Write("Can't deserialize storage avatar cache");
                return;
            }

            int lastModelIndex = models.FindIndex(x => x.UserId == userId);

            if (lastModelIndex != -1)
            {
                AvatarCacheStorageModel lastModel = models[lastModelIndex];
                lastModel.AvatarTicks = ticks;

                models[lastModelIndex] = lastModel;

                string json = JsonConvert.SerializeObject(models);

                Debug.Write("Cache file was present changing index");
                Cache.SaveToCache(json, AvatarCacheFileName);
            }
            else
            {

                Debug.Write("Cache file was present adding new cache model");

                AvatarCacheStorageModel model = new AvatarCacheStorageModel(ticks, userId);
                models.Add(model);

                string json = JsonConvert.SerializeObject(models);
                Cache.SaveToCache(json, AvatarCacheFileName);
            }
        }
    }
}
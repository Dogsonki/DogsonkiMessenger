﻿using Newtonsoft.Json;

namespace Client.Networking.Packets;

[Serializable]
public class UserImageRequestPacket
{
    [JsonProperty("avatar")]
    public string ImageData { get; set; }
    [JsonProperty("login_id")]
    public uint Id { get; set; }

    [JsonConstructor]
    public UserImageRequestPacket(string avatar, uint login_id)
    {
        ImageData = avatar;
        Id = login_id;
    }
}

using System.ComponentModel;
using Client.Models.UserType.Bindable;
using Client.Utility;

namespace Client.Networking.Packets.Models;

[Bindable(BindableSupport.Yes)]
[Serializable]
public class LastChatsPacket : BindableObject
{
    public readonly User BindedUser;
    public readonly Group BindedGroup;

    public string Username { get; set; }
    public DateTime LastMessageTime { get; set; }
    public string LastMessage { get; set; }
    public string Type { get; set; }
    public int Id { get; set; }
    public string Sender { get; set; }

    public ImageSource Avatar
    {
        get
        {
            if (isGroup)
            {
                return BindedGroup.Avatar;
            }

            return BindedUser.Avatar;
        }
    }

    public bool isGroup => Type != "user";

    public LastChatsPacket(string name, string message, double? last_message_time, string message_type, int id, string sender)
    {
        Username = name;
        if (last_message_time != null)
        {
            LastMessageTime = Essential.UnixToDateTime((double)last_message_time);
        }
        
        LastMessage = message;
        Type = message_type;
        Id = id;
        Sender = sender;
    }
}
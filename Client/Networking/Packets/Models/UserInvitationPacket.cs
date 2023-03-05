using Client.Models;

namespace Client.Networking.Packets.Models;

[Serializable]
internal class UserInvitationPacket
{
    public string InvitationSenderName { get; }
    public uint InvitationSenderId { get; }

    public UserInvitationPacket(string name, uint id)
    {
       InvitationSenderId = id;
       InvitationSenderName = name;
    }
}
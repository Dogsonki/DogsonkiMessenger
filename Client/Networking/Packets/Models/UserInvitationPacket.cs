using Client.Models;

namespace Client.Networking.Packets.Models;

[Serializable]
internal class UserInvitationPacket
{
    public IViewBindable view { get; }

    public UserInvitationPacket(string name, uint id)
    {
        view = User.CreateOrGet(name, id);
    }
}
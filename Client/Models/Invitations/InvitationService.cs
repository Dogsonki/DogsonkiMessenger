using Client.Networking.Core;
using Client.Networking.Packets.Models;

namespace Client.Models.Invitations;

internal class InvitationService
{
    private readonly List<Invitation> invitations = new List<Invitation>();

    public IEnumerable<Invitation> GetInvitations() => invitations;

    public void FetchInvitation(Action<IEnumerable<Invitation>> callback) 
    {
        SocketCore.SendCallback(" ", Token.USER_INVITATIONS, (packet) => {

            UserInvitationPacket[]? fetchedInvitations = packet.Deserialize<UserInvitationPacket[]>();

            if (fetchedInvitations is null || fetchedInvitations.Length == 0)
            {
                callback(new Invitation[0]);
                return;
            }

            foreach (UserInvitationPacket invitation in fetchedInvitations) 
            {
                User invitationSender = (User)IViewBindable.CreateOrGet(invitation.InvitationSenderName, invitation.InvitationSenderId, false);

                Invitation _invitation = new Invitation(invitationSender);
                
                invitations.Add(_invitation);
            }
        });
    }
}

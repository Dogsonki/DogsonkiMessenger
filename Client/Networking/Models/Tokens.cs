using Client.IO;
using Client.Models;
using Client.Models.UserType.Bindable;
using Client.Networking.Model;
using Client.Pages;
using Client.Pages.TemporaryPages.GroupChat;
using Client.Utility;
using Client.Pages.PasswordForgot;

namespace Client;

public enum Token : int
{
    EMPTY = -2,
    ERROR = -1,
    LOGOUT = 0,
    LOGIN = 1,
    REGISTER = 2,
    INIT_CHAT = 3,
    SEARCH_USER = 4,
    CHAT_MESSAGE = 5,
    GET_MORE_MESSAGES = 6,
    END_CHAT = 7,
    CHANGE_AVATAR = 8,
    SESSION_INFO = 9,
    LOGIN_SESSION = 10,
    AVATAR_REQUEST = 11,
    LAST_CHATS = 12,
    PASSWORD_FORGOT = 13,
    SEND_MESSAGE = 14,
    GROUP_CHAT_CREATE = 15,
    GROUP_CHAT_INIT = 17,
    BOT_COMMAND = 20
}

/// <summary>
/// Tokens recived / Callbackable tokens
/// </summary>
public enum RToken : int
{
    ACCEPT = 0,
    NULL_BYTE = 1,
    EMAIL_SENT = 2,
    USER_ALREADY_EXISTS = 3,
    CANNOT_SEND_EMAIL = 4,
    EMAIL_WAITING = 6,
    NICKNAME_TAKEN = 7,
    INCORRECT_PASSW_OR_LOGIN = 8,
    WRONG_CODE = 9,
    MAX_CODE_ATTEMPS = 10
}

public static class Tokens
{
    public static RToken CharToRToken(object rev) => (RToken)int.Parse(rev.ToString()); //Get string and get char but don't convert as special

    public static void Process(SocketPacket packet)
    {
        switch ((Token)packet.Token)
        {
            case Token.ERROR:
                Debug.Error(packet.Data + "TOKEN -2");
                break;
            case Token.SEARCH_USER:
                if (!LocalUser.isCreatingGroup)
                    SearchPage.ParseFound(packet.Data);
                else
                    GroupChatCreator.ParseFound(packet.Data);
                break;
            case Token.CHAT_MESSAGE:
                MessagePage.AddMessage(packet);
                break;
            case Token.GET_MORE_MESSAGES:
                MessagePage.PrependNewMessages(packet);
                break;
            case Token.SESSION_INFO:
                Session session = Essential.ModelCast<Session>(packet.Data);
                Session.OverwriteSession(session);
                break;
            case Token.LOGIN_SESSION:
                LoginCallbackModel model = Essential.ModelCast<LoginCallbackModel>(packet.Data);
                if (model.Token == "1")
                {
                    LocalUser.Login(model.Username, model.ID, model.Email);
                }
                else if (model.Token == "-1")
                {
                    LoginPage.Current.message.ShowError("User is banned");
                }
                break;
            case Token.AVATAR_REQUEST:
                UserImageRequestModel.ProcessImage(packet);
                break;
            case Token.LAST_CHATS:
                MainPage.AddLastChats(packet);
                break;
            default:
                Debug.Write("TOKEN_UNRECOGNIZED: " + packet.Data);
                break;
        }
    }
}
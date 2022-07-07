using Client.Models;
using Client.Pages;
using Client.IO;
using Client.Utility;

namespace Client.Networking.Model;

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
    LAST_USERS = 12,
    LAST_GROUP_CHATS = 13,
    SEND_MESSAGE = 14,
    GROUP_CHAT_CREATE = 15,
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
    NULL_BYTE = 5,
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
                Debug.Error(packet.Data+"TOKEN -2");
                break;
            case Token.SEARCH_USER:
                SearchPage.ParseFound(packet.Data);
                break;
            case Token.CHAT_MESSAGE:
                MessagePage.AddMessage(packet);
                break;
            case Token.GET_MORE_MESSAGES:
                break;
            case Token.SESSION_INFO:
                Session session = Essential.ModelCast<Session>(packet.Data);
                Session.OverwriteSession(session);
                break;
            case Token.LOGIN_SESSION:
                LoginCallbackModel model = Essential.ModelCast<LoginCallbackModel>(packet.Data);
                if (model.Token == "1")
                {
                    Debug.Write("LOGGIN BY SESSION SUCCESS");
                    LocalUser.Login(model.Username, model.ID, model.Email);
                }
                break;
            case Token.AVATAR_REQUEST:
                UserImageRequest.ProcessImage(packet);
                break;
            case Token.LAST_USERS:
                MainPage.AddLastUsers(packet);
                break;
            case Token.LAST_GROUP_CHATS:
                MainThread.BeginInvokeOnMainThread(() => StaticNavigator.PopAndPush(new MainPage()));
                break;
            default:
                Debug.Write("TOKEN_UNRECOGNIZED: " + (string)packet.Data);
                break;
        }
    }
}
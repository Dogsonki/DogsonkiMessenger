using Client.Models;
using Client.Pages;
using Newtonsoft.Json.Linq;
using Client.IO;

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
    CONFIRMATION_CODE_ACCEPT = 13,
}

/// <summary>
/// Tokens recived / Callbackable tokens
/// </summary>
public enum RToken : int
{
    ACCEPT = 0,
    EMAIL_SENT = 2,
    USER_ALREADY_EXISTS = 3,
    CANNOT_SEND_EMAIL = 4,
    EMAIL_WAITING = 6,
    NICKNAME_TAKEN = 7,
    INCORRECT_PASSW_OR_LOGIN = 8,
}

public static class Tokens
{
    public static RToken CharToRToken(object rev) => (RToken)int.Parse(rev.ToString()[0].ToString());//Get string and get char but don't convert as special
    public static void Process(SocketPacket packet)
    {
        switch ((Token)packet.Token)
        {
            case Token.ERROR:
                Debug.Error("TOKEN -2");
                break;
            case Token.SEARCH_USER:
                SearchPage.ParseFound(packet.Data);
                break;
            case Token.CHAT_MESSAGE:
                try
                {
                    MainThread.BeginInvokeOnMainThread(() => MessagePage.AddMessage(((JObject)packet.Data).ToObject<MessageModel>()));
                }
                catch (Exception ex)
                {
                    Debug.Error(ex);
                }
                break;
            case Token.GET_MORE_MESSAGES:
                break;
            case Token.SESSION_INFO:
                Session session = ((JObject)packet.Data).ToObject<Session>();
                Session.OverwriteSession(session);
                break;
            case Token.LOGIN_SESSION:
                LoginCallbackModel model = ((JObject)packet.Data).ToObject<LoginCallbackModel>();
                if (model.Token == "1")
                {
                    LocalUser.Login(model.Username, model.ID);
                    MainThread.InvokeOnMainThreadAsync(() => StaticNavigator.PopAndPush(new MainPage()));
                }
                break;
            case Token.AVATAR_REQUEST:
                UserImageRequest img = ((JObject)packet.Data).ToObject<UserImageRequest>();
                if (img.ImageData == " ")
                {
                    return;
                }
                string avat = img.ImageData.Substring(2);
                avat = avat.Substring(0, avat.Length - 1);

                var user = UserModel.GetUser(img.UserID);
                byte[] imgBuffer = Convert.FromBase64String(avat);
                if (user.isLocalUser)
                {
                    MainThread.BeginInvokeOnMainThread(() => LocalUser.Current.Avatar = ImageSource.FromStream(() => new MemoryStream(imgBuffer)));
                }
                else
                {
                    MainThread.BeginInvokeOnMainThread(() => user.Avatar = ImageSource.FromStream(() => new MemoryStream(imgBuffer)));
                }
                break;
            case Token.LAST_USERS:
                SearchModel[] users = ((JArray)packet.Data).ToObject<SearchModel[]>();
                MainPage.AddLastUsers(users);
                break;
            case Token.CONFIRMATION_CODE_ACCEPT:
                MainThread.BeginInvokeOnMainThread(() => StaticNavigator.PopAndPush(new MainPage()));
                break;
            default:
                Debug.Write("TOKEN_UNRECOGNIZED: " + (string)packet.Data);
                break;
        }
    }
}
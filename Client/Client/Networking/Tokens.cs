namespace Client.Networking
{
    public enum Token
    {
        EMPTY = -2,
        ERROR = -1,
        LOGOUT = 0,
        LOGIN = 1,
        REGISTER = 2,
        INIT_CHAT = 3,
        SEARCH_USER = 4,
        CHAT_MESSAGE = 5,
        USER_CHAT = 6,
        END_CHAT = 7,
        CHANGE_AVATAR = 8,
        SESSION_INFO = 9,
        LOGIN_SESSION = 10,
        AVATAR_REQUEST = 11,
        LAST_USERS = 12,
    }
}

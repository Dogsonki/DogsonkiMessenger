﻿namespace Client.Networking.Core;

public enum Token : int
{
    EMPTY = -2,
    ERROR = -1,
    LOGOUT = 0,
    LOGIN = 1,
    REGISTER = 2,
    USER_INIT_CHAT = 3,
    SEARCH_USER = 4,
    CHAT_MESSAGE = 5, //Kind of realtime messages
    GET_MORE_MESSAGES = 6,
    END_CHAT = 7,
    CHANGE_AVATAR = 8,
    SESSION_INFO = 9,
    LOGIN_SESSION = 10,
    USER_AVATAR_REQUEST = 11,
    GET_LAST_CHATS = 12,
    PASSWORD_FORGOT = 13,
    SEND_MESSAGE = 14,
    GROUP_CHAT_CREATE = 15,
    GROUP_INVITE = 16,
    GROUP_CHAT_INIT = 17,
    GROUP_AVATAR_REQUEST = 18,
    GROUP_AVATAR_SET = 19,
    BOT_COMMAND = 20,
    CHAT_IMAGE_REQUEST = 21,
    GET_GROUP_INFO = 22,
    GROUP_USER_KICK = 23,
    GET_USER_AVATAR_TIME = 24,
    GET_GROUP_AVATAR_TIME = 25,
    GET_LAST_MESSAGE_ID = 26,
    GROUP_GET_LAST_MESSAGE_TIME = 27,
    GET_INIT_MESSAGES = 28,
    GET_LAST_TIME_ONLINE = 29,
    SET_MESSAGE_ID = 30
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
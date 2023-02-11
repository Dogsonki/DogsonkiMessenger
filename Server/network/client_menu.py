import time
from datetime import datetime

from Server.sql import handling_sql
from .connection import Client, MessageType, current_connections
from . import functions
from . import group


class NormalChatroom(functions.Chatroom):
    receiver: str
    receiver_id: int
    friends: bool

    def __init__(self, connection: Client, receiver: str):
        super().__init__(connection)
        self.receiver = receiver
        self.friends = True
        self.receiver_id = handling_sql.get_user_id(self.connection, self.receiver)
        self.is_group = False
        self.chat_actions.update({
            MessageType.GET_LAST_CHAT_MESSAGE_ID: self.get_last_message_id
        })

    def get_last_message_id(self, message: str):
        last_message_id = handling_sql.get_last_message_id(self.connection, self.connection.login_id,
                                                           self.receiver_id)
        self.connection.send_message(last_message_id, MessageType.GET_LAST_CHAT_MESSAGE_ID)

    def send_last_messages(self, old: bool = False):
        message_history = handling_sql.get_last_30_messages_from_chatroom(self.connection,
                                                                          self.connection.login_id,
                                                                          self.receiver_id,
                                                                          self.number_of_sent_last_messages)
        if not message_history and not old:
            self.friends = False
        else:
            self._send_last_messages(message_history, old, False)

    def on_new_message(self, message: dict):
        message_ = message["message"].strip()
        message_type = message["message_type"]
        if message_ != "":
            message_id = self.save_message_in_database(message_, message_type)
            self.connection.send_message(message_id, MessageType.NEW_MESSAGE)
            receiver_connection = current_connections.get(self.receiver)
            if receiver_connection:
                data = {"user": self.connection.nick, "message": message_, "id": message_id,
                        "time": time.time(), "user_id": self.connection.login_id,
                        "is_group": False, "group_id": -1, "message_type": message_type}
                receiver_connection.send_message(data, MessageType.CHAT_MESSAGE)

    def save_message_in_database(self, message: str, message_type: str, save: bool = True) -> int:
        is_path = False if message_type == "text" else True
        if is_path and save:
            message = self._save_file(self.receiver_id, message)
        handling_sql.save_message(self.connection, message, self.connection.login_id,
                                  self.receiver_id, message_type, is_path)
        message_id = self.connection.db_cursor.lastrowid
        if not self.friends:
            handling_sql.create_users_link(self.connection, self.connection.login_id, self.receiver_id)
            self.friends = True
        handling_sql.update_last_time_message(self.connection, self.connection.login_id, self.receiver_id,
                                              message_id)
        return message_id


def search_users(client: Client, message_data: dict):
    nick = message_data["nick"]
    search_groups = message_data["search_groups"]
    data = []

    if search_groups:
        groups = handling_sql.get_user_groups(client, client.login_id)
        for i in groups:
            if i.name.startswith(nick):
                data.append({"name": i.name, "id": i.id, "type": "group"})

    first_logins, friends = handling_sql.search_by_nick(client, nick, client.login_id)
    for i in first_logins:
        is_friends = 0
        for u1, u2, is_friend in friends:
            if u1 or u2 == i[0]:
                is_friends = is_friend
                break
        data.append({"name": i[1], "id": i[0], "type": "user", "is_friends": is_friends})
    client.send_message(data, MessageType.SEARCH_USERS)


def logout(client: Client, data: str):
    client.logout()


def change_avatar(client: Client, data: str):
    client.set_avatar(data)


def get_avatar(client: Client, data: str):
    client.get_avatar(data)


def start_chatroom(client: Client, data: str):
    NormalChatroom(client, data).init_chatroom()


def start_group_chatroom(client: Client, data: str):
    group.GroupChatroom(client, data).init_chatroom()


def get_group_avatar(client: Client, data: str):
    group.get_avatar(client, int(data))


def set_group_avatar(client: Client, data: dict):
    group.set_avatar(client, data)


def send_last_chats(client: Client, data: str):
    user_chats = handling_sql.get_user_chats(client, client.login)
    chats = []
    if user_chats:
        for i in user_chats:
            if client.login_id == i.u2_id:
                if i.u1_last_online is not None:
                    i.u1_last_online = datetime.timestamp(i.u1_last_online)
                chats.append({"name": i.u1_nick,
                              "id": i.u1_id,
                              "last_online_time": i.u1_last_online,
                              "last_message_time": datetime.timestamp(i.last_message_time),
                              "type": "user",
                              "message_type": i.message_type,
                              "message": i.message,
                              "sender": i.sender,
                              "is_friend": i.is_friend})
            else:
                if i.u2_last_online is not None:
                    i.u2_last_online = datetime.timestamp(i.u2_last_online)
                chats.append({"name": i.u2_nick,
                              "id": i.u2_id,
                              "last_online_time": i.u2_last_online,
                              "last_message_time": datetime.timestamp(i.last_message_time),
                              "type": "user",
                              "message_type": i.message_type,
                              "message": i.message,
                              "sender": i.sender,
                              "is_friend": i.is_friend})

    user_group_chats = handling_sql.get_user_groups(client, client.login_id)
    if user_group_chats:
        for i in user_group_chats:
            try:
                last_message_time = datetime.timestamp(i.last_message_time)
            except TypeError:
                last_message_time = None
            chats.append({"name": i.name,
                          "id": i.id,
                          "last_message_time": last_message_time,
                          "type": "group",
                          "message_type": i.message_type,
                          "message": i.message,
                          "sender": i.sender})

    client.send_message(chats, MessageType.LAST_CHATS)


def accept_invite(client: Client, data: str):
    handling_sql.accept_invite(client, data, client.login_id)


def send_invite(client: Client, data: str):
    handling_sql.send_invite(client, data, client.login_id)


def get_user_invitations(client: Client, data: str):
    invitations = handling_sql.get_invitations(client, client.login_id)
    invitations_list = []
    if invitations:
        for i in invitations:
            invitations_list.append({
                "nick": i[0],
                "id": i[1],
                "last_online": i[2]
            })
    client.send_message(invitations_list, MessageType.USER_INVITATIONS)


class ClientMenu:
    client: Client
    actions: dict = {
        MessageType.INIT_CHAT: start_chatroom,
        MessageType.SEARCH_USERS: search_users,
        MessageType.LOGOUT: logout,
        MessageType.CHANGE_AVATAR: change_avatar,
        MessageType.GET_AVATAR: get_avatar,
        MessageType.CREATE_GROUP: group.create_group,
        MessageType.ADD_TO_GROUP: group.add_to_group,
        MessageType.INIT_GROUP_CHAT: start_group_chatroom,
        MessageType.GET_GROUP_AVATAR: get_group_avatar,
        MessageType.SET_GROUP_AVATAR: set_group_avatar,
        MessageType.LAST_CHATS: send_last_chats,
        MessageType.GET_USER_AVATAR_TIME: functions.get_user_avatar_time,
        MessageType.GET_GROUP_AVATAR_TIME: functions.get_group_avatar_time,
        MessageType.SEND_INVITE: send_invite,
        MessageType.ACCEPT_INVITE: accept_invite,
        MessageType.USER_INVITATIONS: get_user_invitations
    }

    def __init__(self, client: Client):
        self.client = client

    def listening(self):
        while True:
            message = self.client.receive_message()
            self.actions[message.token](self.client, message.data)

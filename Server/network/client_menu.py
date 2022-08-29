import time
from datetime import datetime

from Server.sql import handling_sql
from .connection import Client, MessageType, current_connections, Message
from . import functions
from . import group
from . import bot


class NormalChatroom(functions.Chatroom):
    receiver: str
    receiver_id: int
    friends: bool

    def __init__(self, connection: Client, receiver: str):
        super().__init__(connection)
        self.receiver = receiver
        self.friends = True
        self.receiver_id = handling_sql.get_user_id(self.connection.db_cursor, self.receiver)

    def send_last_messages(self, old: bool = False):
        message_history = handling_sql.get_last_30_messages_from_chatroom(self.connection.db_cursor,
                                                                          self.connection.login_id,
                                                                          self.receiver_id,
                                                                          self.number_of_sent_last_messages)
        if not message_history and not old:
            self.friends = False
        else:
            self._send_last_messages(message_history, old, False)

    def receive_messages(self):
        while True:
            message = self.connection.receive_message()
            if message.token == MessageType.END_CHAT:
                break
            elif message.token == MessageType.GET_OLD_MESSAGES:
                self.send_last_messages(True)
            elif message.token == MessageType.NEW_MESSAGE:
                self.on_new_message(message)
            elif message.token == MessageType.BOT_COMMAND:
                bot.check_command(self.connection, message.data)
            else:
                self.connection.send_message("", MessageType.ERROR)

    def on_new_message(self, message: Message):
        message_ = message.data["message"].strip()
        message_type = message.data["message_type"]
        if message_ != "":
            receiver_connection = current_connections.get(self.receiver)
            if receiver_connection:
                data = [{"user": self.connection.nick, "message": message_,
                        "time": time.time(), "user_id": self.connection.login_id,
                         "is_group": False, "group_id": -1, "message_type": message_type}]
                receiver_connection.send_message(data, MessageType.CHAT_MESSAGE)
            self.save_message_in_database(message_, message_type)

    def save_message_in_database(self, message: str, message_type: str):
        if not self.friends:
            handling_sql.create_users_link(self.connection.db_cursor, self.connection.login_id, self.receiver_id)
            self.friends = True
        else:
            handling_sql.update_last_time_message(self.connection.db_cursor, self.connection.login_id, self.receiver_id)

        is_path = False if message_type == "text" else True
        if is_path:
            functions.save_file(f"{int(time.time())}{self.receiver_id}{self.connection.login_id}", message)
        handling_sql.save_message(self.connection.db_cursor, message, self.connection.login_id,
                                  self.receiver_id, message_type, is_path)


def search_users(client: Client, nick: str):
    data = []

    groups = handling_sql.get_user_groups(client.db_cursor, client.login_id)
    for i in groups:
        if i[0].startswith(nick):
            data.append({"name": i[0], "id": i[1], "type": "group"})

    first_logins = handling_sql.search_by_nick(client.db_cursor, nick)
    for i in first_logins:
        data.append({"name": i[1], "id": i[0], "type": "user"})
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
    user_chats = handling_sql.get_user_chats(client.db_cursor, client.login)
    chats = []
    if user_chats:
        for i in user_chats:
            if client.login_id == i[2]:
                chats.append({"name": i[1], "id": i[0], "last_message_time": datetime.timestamp(i[4]), "type": "user"})
            else:
                chats.append({"name": i[3], "id": i[2], "last_message_time": datetime.timestamp(i[4]), "type": "user"})

    user_group_chats = handling_sql.get_user_groups(client.db_cursor, client.login_id)
    if user_group_chats:
        for i in user_group_chats:
            chats.append({"name": i[0], "id": i[1], "last_message_time": datetime.timestamp(i[2]), "type": "group"})

    client.send_message(chats, MessageType.LAST_CHATS)


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
        MessageType.LAST_CHATS: send_last_chats
    }

    def __init__(self, client: Client):
        self.client = client

    def listening(self):
        while True:
            message = self.client.receive_message()
            self.actions[message.token](self.client, message.data)

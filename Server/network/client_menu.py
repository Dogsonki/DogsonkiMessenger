import time

from Server.sql import handling_sql
from .connection import Client, MessageType, current_connections, Message
from . import functions
from . import group


class NormalChatroom(functions.Chatroom):
    receiver: str
    receiver_id: int

    def __init__(self, connection: Client, receiver: str):
        super().__init__(connection)
        self.receiver = receiver
        self.receiver_id = handling_sql.get_user_id(self.connection.db_cursor, self.receiver)

    def send_last_messages(self, old: bool = False):
        message_history = handling_sql.get_last_30_messages_from_chatroom(self.connection.db_cursor,
                                                                          self.connection.login_id,
                                                                          self.receiver_id,
                                                                          self.number_of_sent_last_messages)
        self._send_last_messages(message_history, old)

    def receive_messages(self):
        while True:
            message = self.connection.receive_message()
            if message.token == MessageType.END_CHAT:
                break
            elif message.token == MessageType.GET_OLD_MESSAGES:
                self.send_last_messages(True)
            elif message.token == MessageType.NEW_MESSAGE:
                self.on_new_message(message)
            else:
                self.connection.send_message("", MessageType.ERROR)

    def on_new_message(self, message: Message):
        message_ = message.data.strip()
        if message_ != "":
            receiver_connection = current_connections.get(self.receiver)
            if receiver_connection:
                data = {"user": self.connection.nick, "message": message_,
                        "time": time.time(), "user_id": self.connection.login_id}
                receiver_connection.send_message(data, MessageType.CHAT_MESSAGE)
            self.save_message_in_database(message_)

    def save_message_in_database(self, message):
        handling_sql.save_message(self.connection.db_cursor, message, self.connection.login_id, self.receiver_id)


def search_users(client: Client, nick: str):
    first_logins = handling_sql.search_by_nick(client.db_cursor, nick)
    first_logins_parsed = []
    for i in first_logins:
        first_logins_parsed.append({"login": i[1], "id": i[0]})
    client.send_message(first_logins_parsed, MessageType.SEARCH_USERS)


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
        MessageType.INIT_GROUP_CHAT: start_group_chatroom
    }

    def __init__(self, client: Client):
        self.client = client

    def listening(self):
        while True:
            message = self.client.receive_message()
            self.actions[message.token](self.client, message.data)

import time

from Server.sql import handling_sql
from .connection import Client, MessageType, current_connections, Message
from . import functions
from . import group


class ClientMenu:
    client: Client

    def __init__(self, client: Client):
        self.client = client

    def listening(self):
        while True:
            message = self.client.receive_message()
            if message.token == MessageType.INIT_CHAT:
                NormalChatroom(self.client, message.data).init_chatroom()
            elif message.token == MessageType.SEARCH_USERS:
                search_users(self.client, message.data)
            elif message.token == MessageType.LOGOUT:
                self.client.logout()
            elif message.token == MessageType.CHANGE_AVATAR:
                self.client.set_avatar(message.data)
            elif message.token == MessageType.GET_AVATAR:
                self.client.get_avatar(message.data)
            elif message.token == MessageType.CREATE_GROUP:
                group.create_group(self.client, message.data)
            elif message.token == MessageType.ADD_TO_GROUP:
                group.add_to_group(self.client, message.data)
            elif message.token == MessageType.INIT_GROUP_CHAT:
                group.GroupChatroom(self.client, int(message.data)).init_chatroom()


class NormalChatroom(functions.Chatroom):
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

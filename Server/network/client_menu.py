import time
from datetime import datetime

from Server.sql import handling_sql
from .connection import Client, MessageType, current_connections

INSERT_INTO_DB = handling_sql.InsertIntoDatabase()
GET_INFO_FROM_DB = handling_sql.GetInfoFromDatabase()


class ClientMenu:
    def __init__(self, client: Client):
        self.client = client

    def listening(self):
        while True:
            message = self.client.receive_message()
            if message.token == MessageType.INIT_CHAT:
                Chatroom(self.client, message.data).init_chatroom()
            elif message.token == MessageType.SEARCH_USERS:
                first_logins = GET_INFO_FROM_DB.search_by_nick(self.client.db_cursor, message.data)
                first_logins_parsed = []
                for i in first_logins:
                    first_logins_parsed.append({"login": i[1], "id": i[0]})
                self.client.send_message(first_logins_parsed, MessageType.SEARCH_USERS)
            elif message.token == MessageType.LOGOUT:
                self.client.logout()
            elif message.token == MessageType.CHANGE_AVATAR:
                self.client.set_avatar(message.data)
            elif message.token == MessageType.GET_AVATAR:
                self.client.get_avatar(message.data)


class Chatroom:
    def __init__(self, connection: Client, receiver: str):
        self.connection = connection
        self.receiver = receiver
        self.receiver_id, = GET_INFO_FROM_DB.get_user_id(self.connection.db_cursor, receiver)
        self.number_of_sent_last_messages = 0

    def init_chatroom(self):
        self.send_last_messages()
        self.receive_messages()

    def send_last_messages(self, old: bool = False):
        message_history = GET_INFO_FROM_DB.get_last_30_messages_from_chatroom(self.connection.db_cursor,
                                                                              self.connection.login_id,
                                                                              self.receiver_id,
                                                                              self.number_of_sent_last_messages)
        self.number_of_sent_last_messages += 30
        if message_history:
            message_history.reverse()
            for i in message_history:
                data = {"user": i[1], "message": i[0], "time": datetime.timestamp(i[3])}
                self.send_message(data, old)

    def send_message(self, message_data: dict, old: bool):
        if old:
            token = MessageType.GET_OLD_MESSAGES
        else:
            token = MessageType.CHAT_MESSAGE
        self.connection.send_message(message_data, token)

    def receive_messages(self):
        while True:
            message = self.connection.receive_message()
            if message.token == MessageType.END_CHAT:
                break
            if message.token == MessageType.GET_OLD_MESSAGES:
                self.send_last_messages(True)
                break

            message.data = message.data.strip()
            if message.data != "":
                receiver_connection = current_connections.get(self.receiver)
                if receiver_connection:
                    data = {"user": self.connection.login, "message": message.data, "time": time.time()}
                    receiver_connection.send_message(data, MessageType.CHAT_MESSAGE)
                self.save_message_in_database(message.data)

    def save_message_in_database(self, message):
        INSERT_INTO_DB.save_message(self.connection.db_cursor, message, self.connection.login_id, self.receiver_id)

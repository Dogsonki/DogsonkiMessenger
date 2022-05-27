import time

from Server.sql import handling_sql
from .connection import Client, MessageType, current_connections

INSERT_INTO_DB = handling_sql.InsertIntoDatabase()
GET_INFO_FROM_DB = handling_sql.GetInfoFromDatabase()


class ClientMenu:
    def __init__(self, client: Client):
        self.client = client

    def listening(self):
        while True:
            message, _ = self.client.receive_message()
            if message.token == MessageType.INIT_CHAT:
                Chatroom(self.client, message.data).init_chatroom()
            elif message.token == MessageType.SEARCH_USERS:
                first_logins = GET_INFO_FROM_DB.search_by_login(self.client.db_cursor, message.data)
                self.client.send_message(first_logins, MessageType.SEARCH_USERS)
            elif message.token == MessageType.LOGOUT:
                self.client.logout()
            elif message.token == MessageType.CHANGE_AVATAR:
                self.client.set_avatar()


class Chatroom:
    def __init__(self, connection: Client, receiver):
        self.connection = connection
        self.receiver = receiver
        self.receiver_id = GET_INFO_FROM_DB.get_user_id(self.connection.db_cursor, self.connection.login)
        self.number_of_sent_last_messages = 0

    def init_chatroom(self):
        self.send_last_messages()
        self.receive_messages()

    def send_last_messages(self):
        message_history = GET_INFO_FROM_DB.get_last_30_messages_from_chatroom(self.connection.db_cursor,
                                                                              self.connection.login, self.receiver_id,
                                                                              self.number_of_sent_last_messages)
        self.number_of_sent_last_messages += 30
        if message_history:
            for i in message_history:
                data = {"user": i[1], "message": i[0], "time": str(i[3])}
                self.send_message(data)

    def send_message(self, message_data):
        self.connection.send_message(message_data, MessageType.CHAT_MESSAGE)

    def receive_messages(self):
        in_chat = True
        while in_chat:
            message = self.connection.receive_message()
            for i in message:
                if i.token == MessageType.END_CHAT:
                    in_chat = False
                    break

                if i.data != "" or i.data != " ":
                    receiver_connection = current_connections.get(self.receiver)
                    if receiver_connection:
                        data = {"user": self.receiver, "message": i.data, "time": time.time()}
                        receiver_connection.send_message(data, MessageType.CHAT_MESSAGE)
                    self.save_message_in_database(i.data)

    def save_message_in_database(self, message):
        INSERT_INTO_DB.save_message(self.connection.db_cursor, message, self.connection.login_id, self.receiver_id)

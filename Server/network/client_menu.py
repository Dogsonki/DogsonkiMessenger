import json
import time

from Server.sql import handling_sql
from .connection import Client, current_connections

INSERT_INTO_DB = handling_sql.InsertIntoDatabase()
GET_INFO_FROM_DB = handling_sql.GetInfoFromDatabase()


class ClientMenu:
    def __init__(self, connection: Client):
        self.search_code = "0004"
        self.connection = connection

    def listening(self):
        # 0 --> go to chatroom with given login
        # 1 --> search people
        while True:
            message = self.connection.receive_message()
            code = message.split("-")[0]
            arg = " ".join(message.split("-")[1:])
            if code == "0":
                Chatroom(self.connection, arg).init_chatroom()
            elif code == "1":
                logins_all = GET_INFO_FROM_DB.search_by_login(arg)
                first_logins = []
                # can send only 1024 bytes, 1024/8 ~ 120
                logins_len = 0
                for i in logins_all:
                    logins_len += len(i)+2
                    if logins_len >= 120:
                        break
                    else:
                        first_logins.append(i)
                self.connection.send_message(f"{self.search_code}-{str(first_logins)}")


class Chatroom:
    def __init__(self, connection: Client, receiver):
        self.connection = connection
        self.receiver = receiver
        self.init_chatroom_code = "0003"
        self.message_chatroom_code = "0005"
        self.number_of_sent_last_messages = 0

    def init_chatroom(self):
        self.connection.send_message(self.init_chatroom_code)
        self.send_last_messages()
        self.receive_messages()

    def send_last_messages(self):
        message_history = GET_INFO_FROM_DB.get_last_30_messages_from_chatroom(self.connection.login, self.receiver, self.number_of_sent_last_messages)
        self.number_of_sent_last_messages += 30
        for i in message_history:
            data = json.dumps({"user": i[1], "message": i[0], "time": str(i[3])})
            self.send_message(data)

    def send_message(self, message_data):
        message = f"{self.message_chatroom_code}-{message_data}"
        self.connection.send_message(message)
        time.sleep(0.2)  # without sleep app is broken

    def receive_messages(self):
        while True:
            message = self.connection.receive_message()
            if message == "$:}{#@$#@%":  # todo find better exit code
                break

            receiver_connection = current_connections.get(self.receiver)
            if receiver_connection:
                data = json.dumps({"user": self.receiver, "message": message, "time": time.time()})
                message_data = f"{self.message_chatroom_code}-{data}"
                receiver_connection.send_message(message_data)

            self.save_message_in_database(message)

    def save_message_in_database(self, message):
        INSERT_INTO_DB.save_message(message, self.connection.login, self.receiver)

import json

from Server.sql import handling_sql
from .connection import Client, current_connections

INSERT_INTO_DB = handling_sql.InsertIntoDatabase()
GET_INFO_FROM_DB = handling_sql.GetInfoFromDatabase()


class ClientMenu:
    def __init__(self, client: Client):
        self.search_token = 4
        self.client = client

    def listening(self):
        # 0 --> go to chatroom with given login
        # 1 --> search people
        # 2 --> logout
        # 3 --> change avatar
        while True:
            message, _ = self.client.receive_message()
            code = message.split("-")[0]
            arg = " ".join(message.split("-")[1:])
            if code == "0":
                Chatroom(self.client, arg).init_chatroom()
            elif code == "1":
                first_logins = GET_INFO_FROM_DB.search_by_login(arg)
                self.client.send_message(str(first_logins), self.search_token)
            elif code == "2":
                self.client.logout()
            elif code == "3":
                self.client.set_avatar()


class Chatroom:
    def __init__(self, connection: Client, receiver):
        self.connection = connection
        self.receiver = receiver
        self.init_chatroom_token = 3
        self.message_chatroom_token = 5
        self.number_of_sent_last_messages = 0

    def init_chatroom(self):
        self.connection.send_message("0", self.init_chatroom_token)
        self.send_last_messages()
        self.receive_messages()

    def send_last_messages(self):
        message_history = GET_INFO_FROM_DB.get_last_30_messages_from_chatroom(self.connection.login, self.receiver, self.number_of_sent_last_messages)
        self.number_of_sent_last_messages += 30
        for i in message_history:
            data = {"user": i[1], "message": i[0], "time": str(i[3])}
            self.send_message(data)

    def send_message(self, message_data):
        self.connection.send_message(message_data, self.message_chatroom_token)

    def receive_messages(self):
        in_chat = True
        while in_chat:
            message = self.connection.receive_message()
            for i in message:
                if i == "ENDCHAT":  # todo find better exit code
                    in_chat = False
                    break
                #receiver_connection = current_connections.get(self.receiver)
                #if receiver_connection:
                #    data = json.dumps({"user": self.receiver, "message": i, "time": time.time()})
                #    message_data = f"{self.message_chatroom_code}-{data}"
                #    receiver_connection.send(bytes(message_data, "UTF-8"))
                self.save_message_in_database(i)

    def save_message_in_database(self, message):
        INSERT_INTO_DB.save_message(message, self.connection.login, self.receiver)

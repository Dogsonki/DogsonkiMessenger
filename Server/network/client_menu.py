from Server.sql import handling_sql
from .connection import Connection, current_connections

INSERT_INTO_DB = handling_sql.InsertIntoDatabase()
GET_INFO_FROM_DB = handling_sql.GetInfoFromDatabase()


class ClientMenu:
    def __init__(self, connection: Connection):
        self.search_code = "0004"
        self.connection = connection
        self.chatroom = Chatroom(self.connection)

    def listening(self):
        # 0 --> go to chatroom with given login
        # 1 --> search people
        while True:
            message = self.connection.receive_message()
            code = message.split("-")[0]
            arg = " ".join(message.split("-")[1:])
            if code == "0":
                self.chatroom.init_chatroom(arg)
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
    def __init__(self, connection: Connection):
        self.connection = connection
        self.init_chatroom_code = "0003"
        self.message_chatroom_code = "0005"
        self.number_of_sent_last_messages = 0

    def init_chatroom(self, receiver):
        self.connection.send_message(self.init_chatroom_code)
        self.send_last_messages(receiver)
        self.receive_messages(receiver)

    def send_last_messages(self, receiver):
        message_history = GET_INFO_FROM_DB.get_last_30_messages_from_chatroom(self.connection.login, receiver, self.number_of_sent_last_messages)
        self.number_of_sent_last_messages += 30
        for i in message_history:
            message = f"{self.message_chatroom_code}-{i[1]} {i[0]}\0"
            self.connection.send_message(message)

    def receive_messages(self, receiver):
        while True:
            message = self.connection.receive_message()
            if not message:
                self.connection.send_message(f"{self.message_chatroom_code}-\0")
                break
            self.save_message_in_database(message, receiver)

    def save_message_in_database(self, message, receiver):
        INSERT_INTO_DB.save_message(message, self.connection.login, receiver)

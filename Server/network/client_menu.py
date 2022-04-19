from Server.sql import handling_sql
from .connection import Connection, current_connections

INSERT_INTO_DB = handling_sql.InsertIntoDatabase()
GET_INFO_FROM_DB = handling_sql.GetInfoFromDatabase()


class ClientMenu:
    def __init__(self, connection: Connection):
        self.connection = connection
        self.chatroom = Chatroom(self.connection)

    def listening(self):
        while True:
            receiver = self.connection.receive_message()
            if receiver == "Q":
                self.connection.send_message("\0")
                break
            self.chatroom.init_chatroom(receiver)


class Chatroom:
    def __init__(self, connection: Connection):
        self.connection = connection

    def init_chatroom(self, receiver):
        messages_history = GET_INFO_FROM_DB.get_last_30_messages_from_chatroom(self.connection.login, receiver)
        if messages_history is False:
            self.connection.send_message("0")
        else:
            self.connection.send_message("1")
            self.send_last_messages(messages_history)
            self.receive_messages(receiver)

    def send_last_messages(self, message_history):
        message_history.reverse()
        for i in message_history:
            message = f"{i[1]} {i[0]}\0"
            print(message)
            self.connection.send_message(message)

    def receive_messages(self, receiver):
        while True:
            message = self.connection.receive_message()
            if message == "Q":
                self.connection.send_message("\0")
                break

            try:
                # send notification to receiver if receiver is online
                current_connections[receiver].send(bytes("2", "UTF-8"))
            except KeyError:
                pass
            self.save_message_in_database(message, receiver)

    def save_message_in_database(self, message, receiver):
        INSERT_INTO_DB.save_message(message, self.connection.login, receiver)

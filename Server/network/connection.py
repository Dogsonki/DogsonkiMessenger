import socket
import time

from Server.sql import handling_sql

current_connections = {}
INSERT_SQL = handling_sql.InsertIntoDatabase()
SELECT_SQL = handling_sql.GetInfoFromDatabase()


class Connection:
    def __init__(self, connection, address):
        self.client = connection
        self.address = address
        self.login = False
        self.password = False
        self.closed = False

    def send_message(self, message):
        print(f"sent: {message}")
        try:
            self.client.send(bytes(message, "UTF-8"))
        except socket.error:
            self.close_connection()

    def receive_message(self):
        while True:
            try:
                received_message = self.client.recv(1024).decode("UTF-8")
                print(f"recv: {received_message}")
                if not received_message:
                    self.close_connection()
                    return False
                if received_message.startswith == "0005":
                    continue
                return received_message
            except socket.error:
                self.close_connection()
                return False

    def close_connection(self):
        print(current_connections)
        if not self.closed:
            self.closed = True
            if self.login:
                del current_connections[self.login]
            #print(f"Closing connection with {self.client}")
            #self.connection.shutdown(socket.SHUT_RDWR)
            raise Exception(f"Closing connection with {self.client}")   # todo find better way to close connection


class Client(Connection):
    def __init__(self, connection, address):
        super().__init__(connection, address)
        self.login_app_code = "0001"
        self.registering_app_code = "0002"
        self.last_user_chats = "0006"

    def get_login_action(self):
        while True:
            action_info = self.receive_message()
            if action_info == "logging":
                is_logged_correctly = self.login_user()
                if is_logged_correctly:
                    break
            elif action_info == "registering":
                is_registered_correctly = self.register_user()
                if is_registered_correctly:
                    break
            else:
                self.send_message(f"Error - should receive 'logging' or 'registering'")

    def login_user(self):
        self.login, self.password = self.get_login_data()
        if SELECT_SQL.login_user(self.login, self.password):
            self.send_message(f"{self.login_app_code}-1")  # 1 --> user has been logged
            time.sleep(0.2)
            user_chats = SELECT_SQL.get_user_chats(self.login)
            self.send_message(f"{self.last_user_chats}-{user_chats}")
            current_connections[self.login] = self.client
            return True
        else:
            self.send_message(f"{self.login_app_code}-0")  # 0 --> wrong login or password
        return False

    def register_user(self):
        self.login, self.password = self.get_register_data()
        if self.validate_login_data():
            if INSERT_SQL.register_user(self.login, self.password):
                self.send_message(f"{self.registering_app_code}-1")  # 1 --> user has been registered
                return True
            else:
                self.send_message(f"{self.registering_app_code}-01")  # 01 --> user with given login exists
        else:
            self.send_message(f"{self.registering_app_code}-00")  # 00 --> incorrect password
        return False

    def get_login_data(self):
        login_data = []
        for _ in range(2):
            data_from_user = self.receive_message()
            if data_from_user == "Q":
                return "", ""
            login_data.append(data_from_user)
        return login_data

    def get_register_data(self):
        registering_data = []
        for _ in range(2):
            data_from_user = self.receive_message()
            if data_from_user == "Q":
                return "", ""
            registering_data.append(data_from_user)
        return registering_data

    def validate_login_data(self):
        if 2 <= len(self.login) <= 50 and 2 <= len(self.password) <= 50:
            return True
        return False

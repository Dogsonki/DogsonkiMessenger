import socket

from Server.sql import handling_sql

current_connections = {}
INSERT_SQL = handling_sql.InsertIntoDatabase()
SELECT_SQL = handling_sql.GetInfoFromDatabase()


class Connection:
    def __init__(self, connection, address):
        self.connection = connection
        self.address = address
        self.login = False
        self.password = False
        self.closed = False

    def send_message(self, message):
        print(f"sent: {message}")
        try:
            self.connection.send(bytes(message, "UTF-8"))
        except socket.error:
            self.close_connection()

    def receive_message(self):
        while True:
            try:
                received_message = self.connection.recv(1024).decode("UTF-8")
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
            #print(f"Closing connection with {self.connection}")
            #self.connection.shutdown(socket.SHUT_RDWR)
            raise Exception(f"Closing connection with {self.connection}")   # todo find better way to close connection


class LoginUser:
    def __init__(self, connection: Connection):
        self.connection = connection
        self.login_app_code = "0001"
        self.registering_app_code = "0002"

    def get_action(self):
        while True:
            action_info = self.connection.receive_message()
            if action_info == "logging":
                is_logged_correctly = self.login_user()
                if is_logged_correctly:
                    break
            elif action_info == "registering":
                is_registered_correctly = self.register_user()
                if is_registered_correctly:
                    break
            else:
                self.connection.send_message(f"Error - should receive 'logging' or 'registering'")

    def login_user(self):
        self.connection.login, self.connection.password = self.get_login_data()
        if SELECT_SQL.login_user(self.connection.login, self.connection.password):
            self.connection.send_message(f"{self.login_app_code}-1")  # 1 --> user has been logged
            current_connections[self.connection.login] = self.connection
            return True
        else:
            self.connection.send_message(f"{self.login_app_code}-0")  # 0 --> wrong login or password
        return False

    def register_user(self):
        self.connection.login, self.connection.password = self.get_register_data()
        if self.validate_login_data():
            if INSERT_SQL.register_user(self.connection.login, self.connection.password):
                self.connection.send_message(f"{self.registering_app_code}-1")  # 1 --> user has been registered
                return True
            else:
                self.connection.send_message(f"{self.registering_app_code}-01")  # 01 --> user with given login exists
        else:
            self.connection.send_message(f"{self.registering_app_code}-00")  # 00 --> incorrect password
        return False

    def get_login_data(self):
        login_data = []
        for _ in range(2):
            data_from_user = self.connection.receive_message()
            if data_from_user == "Q":
                return "", ""
            login_data.append(data_from_user)
        return login_data

    def get_register_data(self):
        registering_data = []
        for _ in range(2):
            data_from_user = self.connection.receive_message()
            if data_from_user == "Q":
                return "", ""
            registering_data.append(data_from_user)
        return registering_data

    def validate_login_data(self):
        if 2 <= len(self.connection.login) <= 50 and 2 <= len(self.connection.password) <= 50:
            return True
        return False

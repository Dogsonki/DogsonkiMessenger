import socket
import base64

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
            self.client.send(bytes(f"{message}$", "UTF-8"))
        except socket.error:
            self.close_connection()

    def receive_message(self):
        try:
            received_message = self.client.recv(1024).decode("UTF-8")
            print(f"recv {received_message}")
            received_message = received_message.split("$")
            if not received_message[0]:
                self.close_connection()
                return False
            received_message.remove("")
            return received_message
        except socket.error:
            self.close_connection()
            return False

    def send_image(self, image_raw):
        try:
            print(f"sent {image_raw}")
            self.client.sendall(bytes(image_raw, "UTF-8"))
        except socket.error:
            self.close_connection()

    def receive_image(self):
        image_data = b""
        while True:
            data = self.client.recv(1024)
            print(f"recv: {data}")
            if data.decode("UTF-8", "ignore") == "ENDFILE":
                break
            if data == b"":
                self.close_connection()
            image_data += data
        image_data = base64.b64encode(image_data)
        return image_data

    def close_connection(self):
        if not self.closed:
            self.closed = True
            if self.login:
                del current_connections[self.login]
            raise Exception(f"Closing connection with {self.client}")  # todo find better way to close connection


class Client(Connection):
    def __init__(self, connection, address):
        super().__init__(connection, address)
        self.login_app_code = "0001"
        self.registering_app_code = "0002"
        self.last_user_chats = "0006"
        self.avatar_code = "0007"

    def get_login_action(self):
        while True:
            action_info, = self.receive_message()
            if action_info == "logging":
                is_logged_correctly = self.login_user()
                if is_logged_correctly:
                    break
            elif action_info == "registering":
                self.register_user()
            else:
                self.send_message(f"Error - should receive 'logging' or 'registering'")

    def login_user(self):
        self.login, self.password = self.get_login_data()
        if SELECT_SQL.login_user(self.login, self.password):
            self.send_message(f"{self.login_app_code}-1")  # 1 --> user has been logged
            user_chats = SELECT_SQL.get_user_chats(self.login)
            self.send_message(f"{self.last_user_chats}-{user_chats}")
            avatar, = SELECT_SQL.get_user_avatar(self.login)
            #self.send_message(self.avatar_code)
            #self.send_image(str(avatar))
            #self.send_message(self.avatar_code)
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
            else:
                self.send_message(f"{self.registering_app_code}-01")  # 01 --> user with given login exists
        else:
            self.send_message(f"{self.registering_app_code}-00")  # 00 --> incorrect password

    def logout(self):
        del current_connections[self.login]
        self.login = False
        self.password = False
        self.get_login_action()

    def set_avatar(self):
        avatar = self.receive_image()
        INSERT_SQL.set_user_avatar(self.login, avatar)

    def get_login_data(self):
        login_data = []
        while len(login_data) != 2:
            data_from_user = self.receive_message()
            login_data.extend(data_from_user)
        return login_data

    def get_register_data(self):
        registering_data = []
        while len(registering_data) != 2:
            data_from_user = self.receive_message()
            registering_data.extend(data_from_user)
        return registering_data

    def validate_login_data(self):
        if 2 <= len(self.login) <= 50 and 2 <= len(self.password) <= 50:
            return True
        return False

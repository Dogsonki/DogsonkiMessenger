import base64
import json
import socket
from dataclasses import dataclass
from enum import Enum
from typing import Any, Optional

from Server.sql import handling_sql

current_connections = {}
INSERT_SQL = handling_sql.InsertIntoDatabase()
SELECT_SQL = handling_sql.GetInfoFromDatabase()


class MessageType(Enum):
    LOGIN = 0
    CONTINUE = 5


@dataclass
class Message:
    type: MessageType
    data: dict[str, Any]

    @classmethod
    def deserialize(cls, message: dict[str, Any]):
        return cls(
            MessageType(message["type"]),
            message["data"]
        )


class Buffer:
    socket: socket.socket
    buffer: bytes
    delimiter: bytes

    def __init__(self, sock, delimiter=b"\0"):
        self.socket = sock
        self.buffer = b""
        self.delimiter = delimiter

    def read(self) -> Optional[str]:
        while b'\0' not in self.buffer:
            data = self.socket.recv(1024)
            if not data:
                return None
            self.buffer += data

        line, _, self.buffer = self.buffer.partition(b'\r\n')
        return line.decode("UTF-8")


class Connection:
    delimiter = b"\0"
    connection: socket.socket
    buffer: Buffer
    
    def __init__(self, connection: socket.socket, address):
        self.client = connection
        self.address = address
        self.buffer = Buffer(connection, Connection.delimiter)

        self.login = False
        self.password = False
        self.closed = False

    def send_message(self, message: Message):
        print(f"sent: {message}")
        try:
            self.client.send(json.dumps(message).encode("UTF-8") + Connection.delimiter)
        except socket.error:
            self.close_connection()

    def receive_message(self) -> Optional[Message]:
        while True:
            try:
                received_message = self.buffer.read()
                print(f"buffer.read: {received_message}")
                if not received_message:
                    self.close_connection()
                    return None
                message = json.loads(received_message, object_hook=Message.deserialize)  # FIXME: handle incorrect data
                if message.type == MessageType.CONTINUE:
                    continue
                return message
            except socket.error:
                self.close_connection()
                return None

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

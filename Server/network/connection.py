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
    def deserialize(cls, message: str):
        message = json.loads(message)
        return cls(
            MessageType.LOGIN,  # todo
            #MessageType(message["token"]),
            message["data"]
        )


class Buffer:
    socket: socket.socket
    buffer: bytes
    delimiter: bytes

    def __init__(self, sock, delimiter=b"$"):
        self.socket = sock
        self.buffer = b""
        self.delimiter = delimiter

    def read(self) -> Optional[str]:
        while self.delimiter not in self.buffer:
            data = self.socket.recv(1024)
            if not data:
                return None
            self.buffer += data
        line, _, self.buffer = self.buffer.partition(b'\r\n')
        return line.decode("UTF-8")


class Connection:
    delimiter = b"$"
    connection: socket.socket
    buffer: Buffer
    
    def __init__(self, connection: socket.socket, address):
        self.client = connection
        self.address = address
        self.buffer = Buffer(connection, Connection.delimiter)

        self.login = False
        self.password = False
        self.closed = False

    def send_message(self, message: Message, token: int):
        message = json.dumps({"data": message, "token": token}).encode("UTF-8") + Connection.delimiter
        print(f"sent: {message}")
        try:
            self.client.send(message)
        except socket.error:
            self.close_connection()

    def receive_message(self) -> Optional[Message]:
        while True:
            try:
                received_message = self.buffer.read()
                print(f"recv: {received_message}")
                if not received_message:
                    self.close_connection()
                    return None
                received_message = received_message.split("$")
                data = []
                for i in received_message:
                    if not i:
                        data.append("")
                        break
                    message = Message.deserialize(i)  # FIXME: handle incorrect data
                    data.append(message.data)
                #if message.type == MessageType.CONTINUE:
                #    continue
                return data
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
        self.error_token = -1
        self.login_app_token = 1
        self.registering_token = 2
        self.last_user_chats_token = 6

    def get_login_action(self):
        while True:
            action_info, _ = self.receive_message()
            if action_info == "logging":
                is_logged_correctly = self.login_user()
                if is_logged_correctly:
                    break
            elif action_info == "registering":
                self.register_user()
            else:
                self.send_message(f"Error - should receive 'logging' or 'registering'", self.error_token)

    def login_user(self):
        login_data = self.get_login_data()
        self.login = login_data["login"]
        self.password = login_data["password"]
        if SELECT_SQL.login_user(self.login, self.password):
            self.send_message("1", self.login_app_token)  # 1 --> user has been logged
            user_chats = SELECT_SQL.get_user_chats(self.login)
            self.send_message(user_chats, self.last_user_chats_token)
            avatar, = SELECT_SQL.get_user_avatar(self.login)
            #self.send_image(str(avatar))
            current_connections[self.login] = self.client
            return True
        else:
            self.send_message("0", self.login_app_token)  # 0 --> wrong login or password
        return False

    def register_user(self):
        register_data = self.get_login_data()
        self.login = register_data["login"]
        self.password = register_data["password"]
        if self.validate_login_data():
            if INSERT_SQL.register_user(self.login, self.password):
                self.send_message("1", self.registering_token)  # 1 --> user has been registered
            else:
                self.send_message("01", self.registering_token)  # 01 --> user with given login exists
        else:
            self.send_message(f"00", self.registering_token)  # 00 --> incorrect password

    def logout(self):
        del current_connections[self.login]
        self.login = False
        self.password = False
        self.get_login_action()

    def set_avatar(self):
        avatar = self.receive_image()
        INSERT_SQL.set_user_avatar(self.login, avatar)

    def get_login_data(self):
        data_from_user, _ = self.receive_message()
        return data_from_user

    def validate_login_data(self):
        if 2 <= len(self.login) <= 50 and 2 <= len(self.password) <= 50:
            return True
        return False

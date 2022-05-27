import base64
import json
import socket
from dataclasses import dataclass
from enum import Enum
from mysql.connector.cursor_cext import CMySQLCursor
from typing import Optional

from Server.sql import handling_sql
from Server.sql.connection import get_cursor

current_connections = {}
INSERT_SQL = handling_sql.InsertIntoDatabase()
SELECT_SQL = handling_sql.GetInfoFromDatabase()


class MessageType(Enum):
    EMPTY = -2
    ERROR = -1
    LOGOUT = 0
    LOGIN = 1
    REGISTER = 2
    INIT_CHAT = 3
    SEARCH_USERS = 4
    CHAT_MESSAGE = 5
    USER_CHAT = 6
    END_CHAT = 7
    CHANGE_AVATAR = 8
    SESSION_INFO = 9
    AUTOMATICALLY_LOGGED = 10


@dataclass
class Message:
    token: MessageType
    data: str

    @classmethod
    def deserialize(cls, message: str):
        message = json.loads(message)
        return cls(
            MessageType(message["token"]),
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
    db_cursor = CMySQLCursor
    
    def __init__(self, connection: socket.socket, address):
        self.client = connection
        self.address = address
        self.buffer = Buffer(connection)
        self.login_id = 0
        self.login = ""
        self.password = ""
        self.closed = False
        self.db_cursor = get_cursor()

    def send_message(self, message, token: MessageType):
        message = json.dumps({"data": message, "token": token.value}).encode("UTF-8") + Connection.delimiter
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
                        message = Message
                        message.data = " "
                        message.token = MessageType.EMPTY
                        data.append(Message)
                        break
                    message = Message.deserialize(i)  # FIXME: handle incorrect data
                    data.append(message)
                # if message.type == MessageType.CONTINUE:
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
            self.db_cursor.close()
            self.closed = True
            if self.login:
                del current_connections[self.login]
            raise ConnectionAbortedError(f"Closing connection with {self.client}")  # todo find better way to close connection


class Client(Connection):
    def __init__(self, connection: socket.socket, address: str):
        super().__init__(connection, address)

    def get_login_action(self):
        while True:
            action_data, _ = self.receive_message()
            if action_data.token == MessageType.LOGIN:
                is_logged_correctly = self.login_user(action_data)
                if is_logged_correctly:
                    break
            elif action_data.token == MessageType.REGISTER:
                self.register_user(action_data)
            elif action_data.token == MessageType.SESSION_INFO:
                is_logged_correctly = self.login_by_session(action_data)
                if is_logged_correctly:
                    break
            else:
                self.send_message("Error - should receive 'logging' or 'registering' code", MessageType.ERROR)

    def login_by_session(self, session_data) -> bool:
        self.login_id = session_data.data["login_id"]
        session_key = session_data.data["session_key"]
        self.login_id = SELECT_SQL.check_session(self.db_cursor, self.login_id, session_key)
        if not self.login_id:
            return False
        self.login, self.password = SELECT_SQL.get_user(self.db_cursor, self.login_id)
        self.send_message(self.login, MessageType.AUTOMATICALLY_LOGGED)
        current_connections[self.login] = self
        return True

    def login_user(self, login_data):
        self.login = login_data.data["login"]
        self.password = login_data.data["password"]
        remember = login_data.data["remember"]
        self.login_id = SELECT_SQL.login_user(self.db_cursor, self.login, self.password)
        if self.login_id:
            self.send_message("1", MessageType.LOGIN)  # 1 --> user has been logged
            if remember:
                session_key = INSERT_SQL.create_session(self.db_cursor, self.login_id)
                self.send_message({"login_id": self.login_id, "session_key": session_key}, MessageType.SESSION_INFO)
            user_chats = SELECT_SQL.get_user_chats(self.db_cursor, self.login)
            self.send_message(user_chats, MessageType.SEARCH_USERS)
            avatar, = SELECT_SQL.get_user_avatar(self.db_cursor, self.login)
            # self.send_image(str(avatar))
            current_connections[self.login] = self
            return True
        else:
            self.send_message("0", MessageType.LOGIN)  # 0 --> wrong login or password
        return False

    def register_user(self, register_data) -> bool:
        self.login = register_data.data["login"]
        self.password = register_data.data["password"]
        if self.validate_login_data():
            if INSERT_SQL.register_user(self.db_cursor, self.login, self.password):
                self.send_message("1", MessageType.REGISTER)  # 1 --> user has been registered
            else:
                self.send_message("01", MessageType.REGISTER)  # 01 --> user with given login exists
        else:
            self.send_message("00", MessageType.REGISTER)  # 00 --> incorrect password

    def logout(self):
        del current_connections[self.login]
        self.login = False
        self.password = False
        INSERT_SQL.delete_session(self.db_cursor, self.login_id)
        self.login_id = 0
        self.get_login_action()

    def set_avatar(self):
        avatar = self.receive_image()
        INSERT_SQL.set_user_avatar(self.db_cursor, self.login, avatar)

    def validate_login_data(self):
        if 2 <= len(self.login) <= 50 and 2 <= len(self.password) <= 50:
            return True
        return False

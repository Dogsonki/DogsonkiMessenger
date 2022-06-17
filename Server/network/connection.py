import base64
import json
import socket
from dataclasses import dataclass
from enum import Enum
from typing import Optional
import json

from mysql.connector.cursor_cext import CMySQLCursor
from email_validator import validate_email, EmailNotValidError

from Server.sql import handling_sql
from Server.sql.connection import get_cursor
from Server.network.email_handling import SmptConnection, get_confirmation_code

current_connections = {}
INSERT_SQL = handling_sql.InsertIntoDatabase()
SELECT_SQL = handling_sql.GetInfoFromDatabase()

smpt_connection = SmptConnection()


class MessageType(Enum):
    EMPTY = -2
    ERROR = -1
    LOGOUT = 0
    LOGIN = 1
    REGISTER = 2
    INIT_CHAT = 3
    SEARCH_USERS = 4
    CHAT_MESSAGE = 5
    GET_OLD_MESSAGES = 6
    END_CHAT = 7
    CHANGE_AVATAR = 8
    SESSION_INFO = 9
    AUTOMATICALLY_LOGGED = 10
    GET_AVATAR = 11
    LAST_CHATS = 12


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
        line, _, self.buffer = self.buffer.partition(self.delimiter)
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
        print(f"sent: {message}")
        message = json.dumps({"data": message, "token": token.value}, ensure_ascii=False).encode("UTF-8") + Connection.delimiter
        try:
            self.client.send(message)
        except socket.error:
            self.close_connection()

    def receive_message(self):
        while True:
            try:
                received_message = self.buffer.read()
                if not received_message:
                    self.close_connection()
                    return None
                message = Message.deserialize(received_message)
                print(f"recv: {received_message}")
                return message
            except socket.error:
                self.close_connection()
                return None

    def close_connection(self):
        if not self.closed:
            self.db_cursor.close()
            self.closed = True
            if self.login:
                del current_connections[self.login]
            raise ConnectionAbortedError(f"Closing connection with {self.client}")  # todo find better way to close connection


class Client(Connection):

    def __init__(self, connection: socket.socket, address):
        super().__init__(connection, address)

    def get_login_action(self):
        while True:
            action_data = self.receive_message()
            if action_data.token == MessageType.LOGIN:
                is_logged_correctly = self.login_user(action_data)
                if is_logged_correctly:
                    self.after_login()
                    break
            elif action_data.token == MessageType.REGISTER:
                self.register_user(action_data)
            elif action_data.token == MessageType.SESSION_INFO:
                is_logged_correctly = self.login_by_session(action_data)
                if is_logged_correctly:
                    self.after_login()
                    break
            else:
                self.send_message("Error - should receive 'logging' or 'registering' code", MessageType.ERROR)

    def after_login(self):
        user_chats = SELECT_SQL.get_user_chats(self.db_cursor, self.login)
        chats = []
        if user_chats:
            for i in user_chats:
                chats.append({"login": i[1], "id": i[0]})
        self.send_message(chats, MessageType.LAST_CHATS)
        current_connections[self.login] = self

    def login_by_session(self, session_data) -> bool:
        self.login_id = session_data.data["login_id"]
        session_key = session_data.data["session_key"]
        self.login_id = SELECT_SQL.check_session(self.db_cursor, self.login_id, session_key)
        if not self.login_id:
            self.send_message({"token": 0, "login": None, "login_id": None}, MessageType.AUTOMATICALLY_LOGGED)
            return False
        self.login, self.password = SELECT_SQL.get_user(self.db_cursor, self.login_id)
        self.send_message({"token": 1, "login": self.login, "login_id": self.login_id}, MessageType.AUTOMATICALLY_LOGGED)
        return True

    def login_user(self, login_data):
        self.login = login_data.data["login"]
        self.password = login_data.data["password"]
        remember = login_data.data["remember"]
        self.login_id = SELECT_SQL.login_user(self.db_cursor, self.login, self.password)
        if self.login_id:
            self.send_message({"token": 1, "login": self.login, "login_id": self.login_id}, MessageType.LOGIN)  # 1 --> user has been logged
            if remember:
                session_key = INSERT_SQL.create_session(self.db_cursor, self.login_id)
                self.send_message({"login_id": self.login_id, "session_key": session_key}, MessageType.SESSION_INFO)
            return True
        else:
            self.send_message({"token": 0, "login": None, "login_id": None}, MessageType.LOGIN)  # 0 --> wrong login or password
        return False

    def register_user(self, register_data):
        nick = register_data.data["login"]
        self.login = register_data.data["email"]
        self.password = register_data.data["password"]
        if not self.validate_login_data():
            self.send_message("8", MessageType.REGISTER)  # 8 --> incorrect password or mail
            return

        if handling_sql.check_if_nick_exist(self.db_cursor, nick):
            self.send_message("7", MessageType.REGISTER)  # 7 -> nickname is taken
            return

        code = get_confirmation_code()
        if not INSERT_SQL.create_confirmation_code(self.db_cursor, self.login, code):
            self.send_message("6", MessageType.REGISTER)  # 6 --> this email is waiting for confirmation
            confirmed = self.confirm_email(code)
        else:
            if not self.send_confirmation_email(code):
                self.send_message("4", MessageType.REGISTER)  # 4 --> cannot send email
                return
            self.send_message("2", MessageType.REGISTER)  # -> 2 email sent
            confirmed = self.confirm_email(code)

        if not confirmed:
            return

        if INSERT_SQL.register_user(self.db_cursor, self.login, self.password, nick):
            self.send_message("0", MessageType.REGISTER)  # 1 --> user has been registered
        else:
            self.send_message("3", MessageType.REGISTER)  # 3 --> user with given email exists

    def confirm_email(self, code: int) -> bool:
        while True:
            code_from_user = self.receive_message()
            if code_from_user.data == "a":
                self.send_confirmation_email(code)
                continue
            if code_from_user.data == "b":
                return False

            if SELECT_SQL.check_email_confirmation(self.db_cursor, self.login, int(code_from_user.data)):
                return True
            else:
                self.send_message("9", MessageType.REGISTER)  # 9 --> wrong confirmation email

    def send_confirmation_email(self, code: int):
        message = smpt_connection.create_message(self.login, code)
        if not smpt_connection.send_mail(self.login, message):
            return False
        return True

    def logout(self):
        del current_connections[self.login]
        self.login = False
        self.password = False
        INSERT_SQL.delete_session(self.db_cursor, self.login_id)
        self.login_id = 0
        self.get_login_action()

    def set_avatar(self, avatar: str):
        avatar = base64.b64encode(bytes(avatar, "UTF-8"))
        INSERT_SQL.set_user_avatar(self.db_cursor, self.login, avatar)

    def get_avatar(self, login_id: str):
        avatar = SELECT_SQL.get_user_avatar(self.db_cursor, login_id)
        if avatar:
            avatar = str(base64.b64decode(avatar[0]))
        else:
            avatar = " "
        self.send_message({"avatar": avatar, "login_id": login_id}, MessageType.GET_AVATAR)

    def validate_login_data(self):
        if 2 <= len(self.login) <= 50 and 2 <= len(self.password) <= 50:
            try:
                validate_email(self.login).email
            except EmailNotValidError:
                return False
            return True
        return False

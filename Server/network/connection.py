import sys
import base64
import json
import socket
from dataclasses import dataclass
from enum import Enum
from typing import Optional, Union, Tuple
from datetime import datetime
import abc

from mysql.connector.cursor_cext import CMySQLCursor
from email_validator import validate_email, EmailNotValidError

from Server.sql import handling_sql
from Server.sql.connection import sql_con
from Server.network.email_handling import SmptConnection, get_confirmation_code

current_connections = {}
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
    FORGOT_PASSWORD = 13
    NEW_MESSAGE = 14
    CREATE_GROUP = 15
    ADD_TO_GROUP = 16
    INIT_GROUP_CHAT = 17
    GET_GROUP_AVATAR = 18
    SET_GROUP_AVATAR = 19
    BOT_COMMAND = 20
    GET_CHAT_FILE = 21
    GET_GROUP_MEMBERS = 22
    DELETE_FROM_GROUP = 23
    GET_USER_AVATAR_TIME = 24
    GET_GROUP_AVATAR_TIME = 25
    GET_LAST_CHAT_MESSAGE_ID = 26
    GET_LAST_GROUP_CHAT_MESSAGE_ID = 27
    GET_FIRST_MESSAGES = 28
    GET_LAST_TIME_ONLINE = 29


@dataclass
class Message:
    token: MessageType
    data: Union[str, dict]

    @classmethod
    def deserialize(cls, message: str):
        message = json.loads(message)
        return cls(
            MessageType(message["token"]),
            message["data"]
        )


class Buffer:
    sock: socket.socket
    buffer_: bytes
    delimiter: bytes

    def __init__(self, sock: socket.socket):
        self.sock = sock
        self.buffer_ = b""
        self.delimiter = b"$"

    def read(self) -> Optional[str]:
        while self.delimiter not in self.buffer_:
            data = self.sock.recv(1024)
            if not data:
                return None
            self.buffer_ += data
        line, _, self.buffer_ = self.buffer_.partition(self.delimiter)
        return line.decode("UTF-8")


class Connection:
    client: socket.socket
    address: Tuple[str, int]
    buffer: Buffer
    login_id: int
    login: str
    password: str
    nick: str
    closed: bool
    db_cursor: CMySQLCursor
    delimiter: bytes
    
    def __init__(self, client: socket.socket, address: Tuple[str, int]):
        self.client = client
        self.address = address
        self.buffer = Buffer(client)
        self.login_id = 0
        self.login = ""
        self.password = ""
        self.nick = ""
        self.db_cursor = sql_con.get_cursor()
        self.delimiter = b"$"

    def send_message(self, message, token: MessageType):
        message = json.dumps({"data": message, "token": token.value}, ensure_ascii=False).encode("UTF-8")+self.delimiter
        try:
            self.client.send(message)
        except socket.error as e:
            print(e)
            self.close_connection()

    def receive_message(self) -> Message:
        while True:
            try:
                received_message = self.buffer.read()
                if not received_message:
                    self.close_connection()
                else:
                    message = Message.deserialize(received_message)
                    if message.token == MessageType.ERROR:
                        print(message.data)
                    return message
            except socket.error as e:
                print(e)
                self.close_connection()

    def close_connection(self):
        self.set_last_time_online(datetime.now())
        self.db_cursor.close()
        if self.login:
            del current_connections[self.nick]
        print(f"Closing connection with {self.client}")
        sys.exit()  # close the thread that was responsible for the connection with the given client

    @abc.abstractmethod
    def set_last_time_online(self, date: datetime):
        pass


class Client(Connection):
    def __init__(self, connection: socket.socket, address: Tuple[str, int]):
        super().__init__(connection, address)

    def get_login_action(self):
        while True:
            action_data = self.receive_message()
            if action_data.token == MessageType.LOGIN:
                is_logged_correctly = self.login_user(action_data)
            elif action_data.token == MessageType.REGISTER:
                self.register_user(action_data)
                is_logged_correctly = False
            elif action_data.token == MessageType.SESSION_INFO:
                is_logged_correctly = self.login_by_session(action_data)
            elif action_data.token == MessageType.FORGOT_PASSWORD:
                self.forgot_password(action_data.data)
                is_logged_correctly = False
            else:
                self.send_message("Error - should receive 'logging' or 'registering' code", MessageType.ERROR)
                is_logged_correctly = False

            if is_logged_correctly:
                self.after_login()
                break

    def after_login(self):
        current_connections[self.nick] = self
        self.set_last_time_online(None)

    def login_by_session(self, session_data) -> bool:
        """
        token:
            0 - wrong session
            1 - logged
            -1 - banned
        """
        self.login_id = session_data.data["login_id"]
        session_key = session_data.data["session_key"]
        self.login_id = handling_sql.check_session(self, self.login_id, session_key)
        if not self.login_id:
            self.send_message({"token": 0, "email": None, "login_id": None, "nick": None},
                              MessageType.AUTOMATICALLY_LOGGED)
            return False
        self.login, self.password, self.nick, is_banned = handling_sql.get_user(self, self.login_id)
        token, callback = [-1, False] if is_banned else [1, True]
        self.send_message({"token": token, "email": self.login, "login_id": self.login_id, "nick": self.nick},
                          MessageType.AUTOMATICALLY_LOGGED)
        return callback

    def login_user(self, login_data) -> bool:
        """
        token:
            0 - wrong email/password
            1 - logged
            -1 - banned
        """
        self.login = login_data.data["login"]
        self.password = login_data.data["password"]
        remember = login_data.data["remember"]
        self.login_id, is_banned = handling_sql.login_user(self, self.login, self.password)
        if self.login_id:
            self.nick = handling_sql.get_nick(self, self.login_id)
            token, callback = [-1, False] if is_banned else [1, True]
            self.send_message({"token": token, "email": self.login, "login_id": self.login_id, "nick": self.nick},
                              MessageType.LOGIN)
            if remember:
                self.create_session()
            return callback
        else:
            self.send_message({"token": 0, "email": None, "login_id": None, "nick": None}, MessageType.LOGIN)
        return False

    def create_session(self):
        session_key = handling_sql.create_session(self, self.login_id)
        self.send_message({"login_id": self.login_id, "session_key": session_key}, MessageType.SESSION_INFO)

    def register_user(self, register_data):
        """
        codes from server:
            0 - user has been registered
            2 - email sent
            3 - user with given email exists
            4 - cannot send email
            6 - this email is waiting for confirmation
            7 - nickname is taken
            8 - password or mail failed validation
            9 - wrong confirmation code
            10 - max attempts during writing code from email, try again
        """
        nick = register_data.data["login"]
        self.login = register_data.data["email"]
        self.password = register_data.data["password"]
        if not self.validate_login_data():
            self.send_message(8, MessageType.REGISTER)
            return
        if handling_sql.check_if_nick_exist(self, nick):
            self.send_message(7, MessageType.REGISTER)
            return
        if handling_sql.check_if_login_exist(self, self.login):
            self.send_message(3, MessageType.REGISTER)

        code = get_confirmation_code()
        created = handling_sql.create_confirmation_code(self, self.login, code)
        if created is not True:
            code = created
            self.send_message(6, MessageType.REGISTER)
        else:
            if not self.send_confirmation_email(code):
                self.send_message(4, MessageType.REGISTER)
                return
            self.send_message(2, MessageType.REGISTER)
        confirmed = self.confirm_email(code, MessageType.REGISTER)

        if not confirmed:
            return
        handling_sql.register_user(self, self.login, self.password, nick)
        self.send_message(0, MessageType.REGISTER)

    def confirm_email(self, code: int, type_: MessageType) -> bool:
        while True:
            code_from_user = self.receive_message()
            if code_from_user.data == "a":
                self.send_confirmation_email(code)
                continue
            if code_from_user.data == "b":
                return False

            confirmed = handling_sql.check_email_confirmation(self, self.login, int(code_from_user.data))
            if confirmed is True:
                return True
            else:
                if confirmed > 5:
                    self.send_message(10, type_)
                    return False
                else:
                    self.send_message(9, type_)

    def send_confirmation_email(self, code: int) -> bool:
        message = smpt_connection.create_message(self.login, code)
        if not smpt_connection.send_mail(self.login, message):
            return False
        return True

    def forgot_password(self, login: str):
        """
        info from server:
            0 - changed password
            1 - cannot send email
            2 - user with this email doesn't exist
            3 - password doesn't match requirements
            4 - email sent
            5 - correct code
            9 - wrong confirmation code
            10 - max attempts during writing code from email, try again
        """
        if not handling_sql.check_if_login_exist(self, login):
            self.send_message(2, MessageType.FORGOT_PASSWORD)
            return

        code = get_confirmation_code()
        self.login = login
        created = handling_sql.create_confirmation_code(self, login, code)
        if created is not True:
            code = created
        else:
            if not self.send_confirmation_email(code):
                self.send_message(1, MessageType.FORGOT_PASSWORD)
                return

        self.send_message(4, MessageType.FORGOT_PASSWORD)
        confirmed = self.confirm_email(code, MessageType.FORGOT_PASSWORD)
        if confirmed:
            self.send_message(5, MessageType.FORGOT_PASSWORD)
            while True:
                new_password = self.receive_message().data
                if 2 <= len(new_password) <= 50:
                    break
                else:
                    self.send_message(3, MessageType.FORGOT_PASSWORD)
            handling_sql.change_password(self, login, new_password)
            handling_sql.delete_session(self, self.login_id)
            self.send_message(0, MessageType.FORGOT_PASSWORD)

    def logout(self):
        del current_connections[self.nick]
        self.login = ""
        self.password = ""
        handling_sql.delete_session(self, self.login_id)
        self.login_id = -1
        self.get_login_action()

    def set_avatar(self, avatar: str):
        if (len(avatar) * 3) / 4 - avatar.count("=", -2) < 2000000:
            avatar = base64.b64encode(bytes(avatar, "UTF-8"))
            handling_sql.set_user_avatar(self, self.login, avatar)
            changed = 1
        else:
            changed = 0
        self.send_message(changed, MessageType.CHANGE_AVATAR)

    def get_avatar(self, login_id: str):
        avatar = handling_sql.get_user_avatar(self, login_id)
        try:
            if avatar[0]:
                avatar = str(base64.b64decode(avatar[0]))[2:-1]
                avatar_time = avatar[1]
            else:
                avatar = " "
                avatar_time = " "
        except (IndexError, TypeError):
            avatar = " "
            avatar_time = " "
        self.send_message({"avatar": avatar, "login_id": login_id, "avatar_time": avatar_time}, MessageType.GET_AVATAR)

    def validate_login_data(self) -> bool:
        if 2 <= len(self.login) <= 50 and 2 <= len(self.password) <= 50:
            try:
                validate_email(self.login).email
            except EmailNotValidError:
                return False
            return True
        return False
    
    def set_last_time_online(self, date: datetime):
        handling_sql.set_last_time_online(self, self.login_id, date)

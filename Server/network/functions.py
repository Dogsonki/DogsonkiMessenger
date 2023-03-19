import abc
from datetime import datetime
from io import BytesIO
import base64
import time

from PIL import Image
import bcrypt

from .connection import Client, MessageType, handling_sql, current_connections
from . import bot


def get_user_avatar_time(client: Client, data: int):
    u_time = handling_sql.get_user_avatar_time(client, data)
    client.send_message(datetime.timestamp(u_time), MessageType.GET_USER_AVATAR_TIME)


def get_group_avatar_time(client: Client, data: int):
    u_time = handling_sql.get_group_avatar_time(client, data)
    client.send_message(datetime.timestamp(u_time), MessageType.GET_GROUP_AVATAR_TIME)


def last_time_online(client: Client, data: int):
    u_time = handling_sql.get_last_time_online(client, data)
    if u_time is not None: 
        u_time = datetime.timestamp(u_time)
    client.send_message(u_time, MessageType.GET_LAST_TIME_ONLINE)


class Chatroom(metaclass=abc.ABCMeta):
    connection: Client
    number_of_sent_last_messages: int
    listening: bool

    def __init__(self, connection: Client):
        self.is_group: bool
        self.connection = connection
        self.number_of_sent_last_messages = 0
        self.listening = True
        self.chat_actions = {
            MessageType.END_CHAT: self.stop_listening,
            MessageType.GET_OLD_MESSAGES: self.send_last_old_messages,
            MessageType.NEW_MESSAGE: self.on_new_message,
            MessageType.BOT_COMMAND: self.bot_command,
            MessageType.GET_CHAT_FILE: self.send_image,
            MessageType.GET_USER_AVATAR_TIME: get_user_avatar_time,
            MessageType.GET_GROUP_AVATAR_TIME: get_group_avatar_time,
            MessageType.GET_FIRST_MESSAGES: self.send_first_messages,
            MessageType.GET_LAST_TIME_ONLINE: self.last_time_online
        }

    def last_time_online(self, data):
        last_time_online(self.connection, data)

    def init_chatroom(self):
        self.receive_messages()

    @abc.abstractmethod
    def on_new_message(self, message: dict):
        pass

    @abc.abstractmethod
    def send_last_messages(self, old: bool = False):
        pass

    @abc.abstractmethod
    def save_message_in_database(self, message: str, message_type: str, save: bool, bot: bool):
        pass

    def receive_messages(self):
        while self.listening:
            message = self.connection.receive_message()
            self.chat_actions[message.token](message.data)

    def bot_command(self, message: dict):
        message = bot.check_command(self.connection, message, self.is_group)
        message_id = self.save_message_in_database(message["message"], message["message_type"], False, True)
        message["id"] = message_id
        if self.is_group:
            for i in self.group_members:
                receiver_connection = current_connections.get(i.nick)
                if receiver_connection:
                    receiver_connection.send_message(message, MessageType.CHAT_MESSAGE)
        else:
            self.connection.send_message(message, MessageType.CHAT_MESSAGE)

    def _send_last_messages(self, message_history: list, old: bool, is_group: bool, group_id: int = -1):
        self.number_of_sent_last_messages += 30
        if message_history:
            message_list = []
            for i in message_history:
                data = {"user": i.sender, "message": i.content, "time": datetime.timestamp(i.time),
                        "user_id": i.sender_id, "is_group": is_group, "group_id": group_id,
                        "message_type": i.message_type, "id": i.id, "is_bot": i.is_bot}
                message_list.append(data)
            self.send_message(message_list, old)

    def send_message(self, message_data: list, old: bool):
        token = MessageType.GET_OLD_MESSAGES if old else MessageType.GET_FIRST_MESSAGES
        self.connection.send_message(message_data, token)

    def send_image(self, message: dict):
        image = get_file(message["path"], message["file_format"])
        self.connection.send_message(image, MessageType.GET_CHAT_FILE)

    def stop_listening(self, *args):
        self.connection.send_message("", MessageType.END_CHAT)
        self.listening = False

    def send_last_old_messages(self, *args):
        self.send_last_messages(True)
        
    def send_first_messages(self, *args):
        self.send_last_messages(False)

    def _save_file(self, second_id: int, message: str):
        filename = f"{int(time.time())}{second_id}{self.connection.login_id}"
        save_file(filename, message)
        message = base64.b64encode(f"./media/{filename}.webp".encode("UTF_8"))
        return message


def save_file(name: str, image_data: str):
    if (len(image_data)*3) / 4 - image_data.count("=", -2) < 4000000:
        image = Image.open(BytesIO(base64.b64decode(image_data))).convert("RGB")
        image.save(f"./media/{name}.webp", format="webp")


def get_file(path: str, file_format: str) -> str:
    image = Image.open(path)
    buffer = BytesIO()
    image.save(buffer, file_format)
    data = base64.b64encode(buffer.getvalue())
    return str(data)[2:-1]


def hash_password(password: str) -> str:
    salt = bcrypt.gensalt()
    hashed_password = str(bcrypt.hashpw(password.encode("UTF-8"), salt), "UTF-8")
    return hashed_password


def check_password(password: str, hashed_password: str) -> bool:
    is_correct = bcrypt.checkpw(password.encode("UTF-8"), hashed_password.encode("UTF-8"))
    return is_correct

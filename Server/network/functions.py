import abc
from datetime import datetime
from io import BytesIO
import base64
import time

from PIL import Image
import bcrypt

from .connection import Client, MessageType
from . import bot


class Chatroom(metaclass=abc.ABCMeta):
    connection: Client
    number_of_sent_last_messages: int
    listening: bool

    def __init__(self, connection: Client):
        self.connection = connection
        self.number_of_sent_last_messages = 0
        self.listening = True
        self.chat_actions = {
            MessageType.END_CHAT: self.stop_listening,
            MessageType.GET_OLD_MESSAGES: self.send_last_old_messages,
            MessageType.NEW_MESSAGE: self.on_new_message,
            MessageType.BOT_COMMAND: self.bot_command,
            MessageType.GET_CHAT_FILE: self.send_image
        }

    def init_chatroom(self):
        self.send_last_messages()
        self.receive_messages()

    @abc.abstractmethod
    def on_new_message(self, message: dict):
        pass

    @abc.abstractmethod
    def send_last_messages(self, old: bool = False):
        pass

    def receive_messages(self):
        while self.listening:
            message = self.connection.receive_message()
            self.chat_actions[message.token](message.data)

    def bot_command(self, message: dict):
        bot.check_command(self.connection, message)

    def _send_last_messages(self, message_history: list, old: bool, is_group: bool, group_id: int = -1):
        self.number_of_sent_last_messages += 30
        if message_history:
            message_list = []
            for i in message_history:
                data = {"user": i.sender, "message": i.content, "time": datetime.timestamp(i.time),
                        "user_id": i.sender_id, "is_group": is_group, "group_id": group_id,
                        "message_type": i.message_type}
                message_list.append(data)
            self.send_message(message_list, old)

    def send_message(self, message_data: list, old: bool):
        token = MessageType.GET_OLD_MESSAGES if old else MessageType.CHAT_MESSAGE
        self.connection.send_message(message_data, token)

    def send_image(self, message: dict):
        image = get_file(message["path"], message["file_format"])
        self.connection.send_message(image, MessageType.GET_CHAT_FILE)

    def stop_listening(self, *args):
        self.listening = False

    def send_last_old_messages(self, *args):
        self.send_last_messages(True)

    def _save_file(self, second_id: int, message: str):
        filename = f"{int(time.time())}{second_id}{self.connection.login_id}"
        save_file(filename, message)
        message = base64.b64encode(f"./media/{filename}.webp".encode("UTF_8"))
        return message


def save_file(name: str, image_data: str):
    if (len(image_data)*3) / 4 - image_data.count("=", -2) < 4000000:  # todo test, file can have max 4mb
        image = Image.open(BytesIO(base64.b64decode(image_data)))
        image.save(f"./media/{name}.webp", format="webp")


def get_file(path: str, file_format: str) -> str:
    image = Image.open(path)
    buffer = BytesIO()
    image.save(buffer, file_format)
    data = base64.b64encode(buffer.getvalue())
    return str(data)


def hash_password(password: str) -> str:
    salt = bcrypt.gensalt()
    hashed_password = str(bcrypt.hashpw(password.encode("UTF-8"), salt), "UTF-8")
    return hashed_password


def check_password(password: str, hashed_password: str) -> bool:
    is_correct = bcrypt.checkpw(password.encode("UTF-8"), hashed_password.encode("UTF-8"))
    return is_correct

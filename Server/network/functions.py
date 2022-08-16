import abc
from datetime import datetime
from io import BytesIO
import base64

from PIL import Image

from .connection import Client, MessageType


class Chatroom(metaclass=abc.ABCMeta):
    connection: Client
    number_of_sent_last_messages: int

    def __init__(self, connection: Client):
        self.connection = connection
        self.number_of_sent_last_messages = 0

    def init_chatroom(self):
        self.send_last_messages()
        self.receive_messages()

    @abc.abstractmethod
    def send_last_messages(self):
        pass

    @abc.abstractmethod
    def receive_messages(self):
        pass

    def _send_last_messages(self, message_history: list, old: bool, is_group: bool, group_id: int = -1):
        self.number_of_sent_last_messages += 30
        if message_history:
            message_list = []
            for i in message_history:
                if i.is_path:
                    i.content = get_file(str(i.content), i.message_type)
                data = {"user": i.sender, "message": i.content, "time": datetime.timestamp(i.time),
                        "user_id": i.sender_id, "is_group": is_group, "group_id": group_id,
                        "message_type": i.message_type}
                message_list.append(data)
            self.send_message(message_list, old)

    def send_message(self, message_data: list, old: bool):
        token = MessageType.GET_OLD_MESSAGES if old else MessageType.CHAT_MESSAGE
        self.connection.send_message(message_data, token)


def save_file(name: str, image_data: str):
    if (len(image_data)*3) / 4 - image_data.count("=", -2) < 4000000:  # todo test, file can have max 4mb
        image = Image.open(BytesIO(base64.b64decode(image_data)))
        image.save(f"./media/{name}.webp", format="webp")


def get_file(path: str, file_format: str) -> bytes:
    image = Image.open(path)
    buffer = BytesIO()
    image.save(buffer, file_format)
    data = base64.b64encode(buffer.getvalue())
    return data

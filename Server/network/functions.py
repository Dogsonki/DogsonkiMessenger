import abc
from datetime import datetime

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
                data = {"user": i.sender, "message": i.content, "time": datetime.timestamp(i.time),
                        "user_id": i.sender_id, "is_group": is_group, "group_id": group_id}
                message_list.append(data)
            self.send_message(message_list, old)

    def send_message(self, message_data: list, old: bool):
        if old:
            token = MessageType.GET_OLD_MESSAGES
        else:
            token = MessageType.CHAT_MESSAGE
        self.connection.send_message(message_data, token)

import abc
from datetime import datetime

from .connection import Client, MessageType


class Chatroom(metaclass=abc.ABCMeta):
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

    def _send_last_messages(self, message_history: list, old: bool):
        self.number_of_sent_last_messages += 30
        if message_history:
            message_history.reverse()
            for i in message_history:
                data = {"user": i[1], "message": i[0], "time": datetime.timestamp(i[3]), "user_id": i[4]}
                self.send_message(data, old)

    def send_message(self, message_data: dict, old: bool):
        if old:
            token = MessageType.GET_OLD_MESSAGES
        else:
            token = MessageType.CHAT_MESSAGE
        self.connection.send_message(message_data, token)

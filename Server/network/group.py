import time

from Server.sql import handling_sql
from .connection import Client, MessageType, current_connections, Message
from . import functions


def create_group(client: Client, data: dict):
    group_name = data["group_name"]
    creator = data["creator"]
    group_id = handling_sql.create_group(client.db_cursor, group_name)
    handling_sql.add_to_group(client.db_cursor, creator, group_id)
    handling_sql.make_admin(client.db_cursor, creator, group_id)
    client.send_message(group_id, MessageType.CREATE_GROUP)


def add_to_group(client: Client, data: dict):
    group_id = data["group_id"]
    added_user_id = data["added_person_id"]
    if handling_sql.get_is_admin(client.db_cursor, client.login_id, group_id):
        handling_sql.add_to_group(client.db_cursor, added_user_id, group_id)
        message = "0"  # 0 -> user has been added
    else:
        message = "1"  # 1 -> adding person is not a group admin
    client.send_message(message, MessageType.ADD_TO_GROUP)


class GroupChatroom(functions.Chatroom):
    group_id: str
    group_members: list

    def __init__(self, connection: Client, group_id: str):
        super().__init__(connection)
        self.group_members = handling_sql.get_group_members(self.connection.db_cursor, int(group_id))

    def send_last_messages(self, old: bool = False):
        message_history = handling_sql.get_last_30_messages_from_group_chatroom(self.connection.db_cursor,
                                                                                int(self.group_id),
                                                                                self.number_of_sent_last_messages)
        self._send_last_messages(message_history, old)

    def receive_messages(self):
        while True:
            message = self.connection.receive_message()
            if message.token == MessageType.END_CHAT:
                break
            elif message.token == MessageType.GET_OLD_MESSAGES:
                self.send_last_messages(True)
            elif message.token == MessageType.NEW_MESSAGE:
                self.on_new_message(message)
            else:
                self.connection.send_message("", MessageType.ERROR)

    def on_new_message(self, message: Message):
        message_ = message.data.strip()
        if message_ != "":
            for i in self.group_members:
                receiver_connection = current_connections.get(i)
                if receiver_connection:
                    data = {"user": self.connection.nick, "message": message_,
                            "time": time.time(), "user_id": self.connection.login_id}
                    receiver_connection.send_message(data, MessageType.CHAT_MESSAGE)
            self.save_message_in_database(message_)

    def save_message_in_database(self, message):
        handling_sql.save_group_message(self.connection.db_cursor, message, self.connection.login_id, self.group_id)

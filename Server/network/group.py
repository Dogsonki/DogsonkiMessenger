import time

from Server.sql import handling_sql
from .connection import Client, MessageType, current_connections
from . import functions

INSERT_INTO_DB = handling_sql.InsertIntoDatabase()
GET_INFO_FROM_DB = handling_sql.GetInfoFromDatabase()


def create_group(client: Client, data: dict):
    group_name = data["group_name"]
    creator = data["creator"]
    group_id = INSERT_INTO_DB.create_group(client.db_cursor, group_name)
    INSERT_INTO_DB.add_to_group(client.db_cursor, creator, group_id)
    INSERT_INTO_DB.make_admin(client.db_cursor, creator, group_id)
    client.send_message(group_id, MessageType.CREATE_GROUP)


def add_to_group(client: Client, data: dict):
    group_id = data["group_id"]
    added_user_id = data["added_person_id"]
    if GET_INFO_FROM_DB.get_is_admin(client.db_cursor, client.login_id, group_id):
        INSERT_INTO_DB.add_to_group(client.db_cursor, added_user_id, group_id)
        message = "0"  # 0 -> user has been added
    else:
        message = "1"  # 1 -> adding person is not a group admin
    client.send_message(message, MessageType.ADD_TO_GROUP)


class GroupChatroom(functions.Chatroom):
    def __init__(self, connection: Client, group_id: int):
        super().__init__(connection)
        self.group_id = group_id
        self.group_members = GET_INFO_FROM_DB.get_group_members(self.connection.db_cursor, group_id)

    def send_last_messages(self, old: bool = False):
        message_history = GET_INFO_FROM_DB.get_last_30_messages_from_group_chatroom(self.connection.db_cursor,
                                                                                    self.group_id,
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
                message_ = message.data.strip()
                if message_ != "":
                    for i in self.group_members:
                        receiver_connection = current_connections.get(i)
                        if receiver_connection:
                            data = {"user": self.connection.nick, "message": message_,
                                    "time": time.time(), "user_id": self.connection.login_id}
                            receiver_connection.send_message(data, MessageType.CHAT_MESSAGE)
                        self.save_message_in_database(message_)
            else:
                self.connection.send_message("", MessageType.ERROR)

    def save_message_in_database(self, message):
        INSERT_INTO_DB.save_group_message(self.connection.db_cursor, message, self.connection.login_id, self.group_id)

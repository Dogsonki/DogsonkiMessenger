import time
import base64
import dataclasses

from Server.sql import handling_sql
from .connection import Client, MessageType, current_connections
from . import functions


def create_group(client: Client, data: dict):
    group_name = data["group_name"]
    creator = data["creator"]
    users = data["users"]
    group_id = handling_sql.create_group(client.db_cursor, group_name)
    handling_sql.add_to_group(client.db_cursor, creator, group_id)
    handling_sql.make_admin(client.db_cursor, creator, group_id)
    for i in users:
        add_to_group(client, {"group_id": group_id, "added_person_id": i})
    client.send_message({"group_id": group_id, "group_name": group_name}, MessageType.CREATE_GROUP)


def add_to_group(client: Client, data: dict):
    group_id = data["group_id"]
    added_user_id = data["added_person_id"]
    if handling_sql.get_is_admin(client.db_cursor, client.login_id, group_id):
        handling_sql.add_to_group(client.db_cursor, added_user_id, group_id)
        message = "0"  # 0 -> user has been added
    else:
        message = "1"  # 1 -> adding person is not a group admin
    client.send_message(message, MessageType.ADD_TO_GROUP)


def delete_from_group(client: Client, data: dict):
    group_id = data["group_id"]
    removed_user_id = data["removed_person_id"]
    if handling_sql.get_is_admin(client.db_cursor, client.login_id, group_id):
        handling_sql.delete_from_group(client.db_cursor, removed_user_id, group_id)
        message = "0"  # 0 -> user has been removed from group
    else:
        message = "1"  # 1 -> removing person is not a group admin
    client.send_message(message, MessageType.DELETE_FROM_GROUP)


def get_avatar(client: Client, group_id: int):
    avatar = handling_sql.get_group_avatar(client.db_cursor, group_id)
    try:
        if avatar[0]:
            avatar = str(base64.b64decode(avatar[0]))
            avatar_time = avatar[1]
        else:
            avatar = " "
            avatar_time = ""
    except (IndexError, TypeError):
        avatar = " "
        avatar_time = ""
    client.send_message({"avatar": avatar, "group_id": group_id, "avatar_time": avatar_time},
                        MessageType.GET_GROUP_AVATAR)


def set_avatar(client: Client, data: dict):
    avatar = data["avatar"]
    if (len(avatar) * 3) / 4 - avatar.count("=", -2) < 2000000:
        avatar = base64.b64encode(bytes(avatar, "UTF-8"))
        handling_sql.set_group_avatar(client.db_cursor, data["group_id"], avatar)


class GroupChatroom(functions.Chatroom):
    group_id: int
    group_members: list

    def __init__(self, connection: Client, group_id: str):
        super().__init__(connection)
        self.group_id = int(group_id)
        self.group_members = handling_sql.get_group_members(self.connection.db_cursor, self.group_id)
        self.is_group = False
        self.chat_actions.update({
            MessageType.ADD_TO_GROUP: self.add_to_group,
            MessageType.DELETE_FROM_GROUP: self.delete_from_group,
            MessageType.GET_GROUP_MEMBERS: self.send_group_members,
            MessageType.GET_GROUP_AVATAR: self.get_avatar,
            MessageType.GET_LAST_GROUP_CHAT_MESSAGE_ID: self.get_last_chat_message_id,
            MessageType.SET_GROUP_AVATAR: self.set_avatar
        })

    def get_last_chat_message_id(self, message: str):
        last_message_id = handling_sql.get_last_group_message_id(self.connection.db_cursor, self.group_id)
        self.connection.send_message(last_message_id, MessageType.GET_LAST_GROUP_CHAT_MESSAGE_ID)

    def delete_from_group(self, message: dict):
        delete_from_group(self.connection, message)

    def get_avatar(self, message: str):
        self.connection.get_avatar(message)
        
    def set_avatar(self, message: dict):
        set_avatar(self.connection, message)

    def add_to_group(self, message: dict):
        add_to_group(self.connection, message)

    def send_group_members(self, *args):
        self.connection.send_message([dataclasses.asdict(i) for i in self.group_members], MessageType.GET_GROUP_MEMBERS)

    def send_last_messages(self, old: bool = False):
        message_history = handling_sql.get_last_30_messages_from_group_chatroom(self.connection.db_cursor,
                                                                                self.group_id,
                                                                                self.number_of_sent_last_messages)
        self._send_last_messages(message_history, old, True, self.group_id)

    def on_new_message(self, message: dict):
        message_ = message["message"].strip()
        message_type = message["message_type"]
        if message_ != "":
            message_id = self.save_message_in_database(message_, message_type)
            data = [{"user": self.connection.nick, "message": message_, "id": message_id,
                     "time": time.time(), "user_id": self.connection.login_id,
                     "is_group": True, "group_id": self.group_id, "message_type": message_type}]
            for i in self.group_members:
                receiver_connection = current_connections.get(i.nick)
                if receiver_connection:
                    receiver_connection.send_message(data, MessageType.CHAT_MESSAGE)

    def save_message_in_database(self, message: str, message_type: str, save: bool = True) -> int:
        is_path = False if message_type == "text" else True
        if is_path and save:
            message = self._save_file(self.group_id, message)
        handling_sql.save_group_message(self.connection.db_cursor, message, self.connection.login_id,
                                        int(self.group_id), message_type, is_path)
        message_id = self.connection.db_cursor.lastrowid
        handling_sql.update_last_time_message_group(self.connection.db_cursor, self.group_id, message_id)
        return message_id

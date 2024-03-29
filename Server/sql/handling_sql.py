import os
import binascii
from datetime import datetime
from dataclasses import dataclass
import requests
from typing import List, Tuple, Union

from mysql.connector.errors import IntegrityError
from mysql.connector.cursor_cext import CMySQLCursor

from ..network.config import config
from ..network import functions, connection
from .connection import sql_con


def connection_checking(function):
    def check_cursor_connection(client: connection.Client, *args):
        try:
            data = function(client.db_cursor, *args)
        except AttributeError:
            client.db_cursor = sql_con.get_cursor()
            data = function(client.db_cursor, *args)
        return data
    return check_cursor_connection


@dataclass
class ChatMessage:
    id: int
    content: str
    sender: str
    time: datetime
    sender_id: int
    message_type: str
    is_path: bool
    is_bot: bool

@connection_checking
def get_last_30_messages_from_chatroom(cursor: CMySQLCursor, sender_id: int, receiver_id: int,
                                       number_of_sent_last_messages: int) -> List[ChatMessage]:

    cursor.execute("""SELECT messages.id, content, u1.nick, time, sender_id, message_type, is_path, is_bot FROM ((messages
                      INNER JOIN users AS u1 ON messages.sender_id = u1.id)
                      INNER JOIN users AS u2 ON messages.receiver_id = u2.id)
                      WHERE (sender_id = %s AND receiver_id = %s) OR (sender_id = %s AND receiver_id = %s)
                      ORDER BY messages.id DESC LIMIT %s,30;""", (sender_id, receiver_id, receiver_id, sender_id,
                                                                  number_of_sent_last_messages))
    sql_data = cursor.fetchall()
    messages = []
    for i in sql_data:
        messages.append(ChatMessage(*i))
    return messages


@connection_checking
def get_last_30_messages_from_group_chatroom(cursor: CMySQLCursor, group_id: int,
                                             number_of_sent_last_messages: int) -> List[ChatMessage]:

    cursor.execute("""SELECT groups_messages.id, content, u.nick, time, sender_id, message_type, is_path, is_bot FROM ((groups_messages
                      INNER JOIN users AS u ON groups_messages.sender_id = u.id)
                      INNER JOIN groups_ AS g ON groups_messages.group_id = g.id)
                      WHERE group_id = %s
                      ORDER BY groups_messages.id DESC LIMIT %s,30;""", (group_id, number_of_sent_last_messages))
    sql_data = cursor.fetchall()
    messages = []
    for i in sql_data:
        messages.append(ChatMessage(*i))
    return messages


@connection_checking
def login_user(cursor: CMySQLCursor, login: str, password: str) -> Tuple:
    cursor.execute("""SELECT id, is_banned, password FROM users
                      WHERE login = %s;""", (login,))
    sql_data = cursor.fetchone()
    if sql_data is not None:
        if functions.check_password(password, sql_data[2]):
            return sql_data[0:2]
    return False, None


@connection_checking
def check_session(cursor: CMySQLCursor, login_id: int, session_key: str) -> int:
    cursor.execute("""SELECT login_id FROM sessions
                      WHERE login_id=%s AND session_key=%s;""", (login_id, session_key))
    sql_data = cursor.fetchone()
    if sql_data is None:
        return 0
    return sql_data[0]


@connection_checking
def search_by_nick(cursor: CMySQLCursor, nick: str, client_id: int) -> Union[bool, Tuple]:
    cursor.execute("""SELECT id, nick FROM users
                      WHERE nick LIKE %s;""", ("%" + nick + "%",))
    logins = cursor.fetchall()
    if logins is None:
        return False
    cursor.execute("""SELECT user1_id, user2_id, is_friend FROM users_link_table
                      WHERE user1_id = %s  OR user2_id = %s;""",
                   (client_id, client_id))
    friends = cursor.fetchall()
    return logins, friends


@dataclass
class UserChats:
    u1_id: int
    u1_nick: str
    u1_last_online: datetime
    u2_id: int
    u2_nick: str
    u2_last_online: datetime
    last_message_time: datetime
    message: str
    message_type: str
    sender: str
    is_friend: bool

@connection_checking
def get_user_chats(cursor: CMySQLCursor, login: str) -> Union[bool, List[UserChats]]:
    cursor.execute("""SELECT u1.id, u1.nick, u1.last_online, u2.id, u2.nick, u2.last_online, m.time, m.content, m.message_type, s.nick, is_friend FROM ((((users_link_table
                      INNER JOIN users AS u1 ON users_link_table.user1_id = u1.id)
                      INNER JOIN users AS u2 ON users_link_table.user2_id = u2.id)
                      LEFT JOIN messages as m ON users_link_table.message_id = m.id)
                      LEFT JOIN users AS s ON m.sender_id = s.id)
                      WHERE u1.login=%s OR u2.login=%s;""", (login, login))
    chats = cursor.fetchall()
    if chats is None:
        return False
    user_chats = []
    for i in chats:
        user_chats.append(UserChats(*i))
    return user_chats


@connection_checking
def get_user_avatar(cursor: CMySQLCursor, login_id: str) -> Union[Tuple, None]:
    cursor.execute("""SELECT avatar, avatar_time FROM users
                      WHERE id=%s""", (login_id, ))
    avatar = cursor.fetchone()
    return avatar


@connection_checking
def get_user(cursor: CMySQLCursor, login_id: int) -> Tuple[str, str, str, bool]:
    cursor.execute("""SELECT login, password, nick, is_banned FROM users
                      WHERE id=%s;""", (login_id,))
    sql_data = cursor.fetchone()
    return sql_data


@connection_checking
def get_user_id(cursor: CMySQLCursor, login: str) -> int:
    cursor.execute("""SELECT id FROM users
                      WHERE nick=%s""", (login,))
    sql_data = cursor.fetchone()
    return sql_data[0]


@connection_checking
def check_email_confirmation(cursor: CMySQLCursor, login: str, code: int) -> Union[bool, int]:
    delete = False
    cursor.execute("""SELECT id FROM email_confirmation
                      WHERE mail=%s AND code=%s;""", (login, code))
    sql_data = cursor.fetchone()
    if sql_data:
        sql_data = True
        delete = True
    else:
        update_email_confirmation_attempts(cursor, login)
        cursor.execute("""SELECT attempts FROM email_confirmation
                          WHERE mail=%s;""", (login,))
        sql_data, = cursor.fetchone()
        if sql_data > 5:
            delete = True

    if delete:
        delete_confirmation_code(cursor, login)
    return sql_data


@connection_checking
def update_email_confirmation_attempts(cursor: CMySQLCursor, login: str):
    cursor.execute("""UPDATE email_confirmation
                      SET attempts=attempts+1
                      WHERE mail=%s;""", (login,))


@connection_checking
def delete_confirmation_code(cursor: CMySQLCursor, login: str):
    cursor.execute("""DELETE FROM email_confirmation
                      WHERE mail=%s;""", (login,))


@connection_checking
def get_nick(cursor: CMySQLCursor, login_id: int) -> str:
    cursor.execute("""SELECT nick FROM users
                      WHERE id=%s""", (login_id,))
    sql_data = cursor.fetchone()
    return sql_data[0]


@dataclass
class UserGroups:
    name: str
    id: int
    last_message_time: datetime
    message: str
    message_type: str
    sender: str

@connection_checking
def get_user_groups(cursor: CMySQLCursor, login_id: int) -> Union[List[UserGroups], bool]:
    cursor.execute("""SELECT g.name, g.id, m.time, m.content, m.message_type, u.nick FROM((( group_user_link_table
                      LEFT JOIN groups_messages AS m ON group_user_link_table.message_id = m.id)
                      INNER JOIN groups_ AS g ON group_user_link_table.group_id = g.id)
                      LEFT JOIN users AS u ON m.sender_id = u.id)
                      WHERE user_id=%s;""", (login_id,))

    sql_data = cursor.fetchall()
    if sql_data is None:
        return False
    group_chats = []
    for i in sql_data:
        group_chats.append(UserGroups(*i))
    return group_chats


@connection_checking
def get_is_admin(cursor: CMySQLCursor, login_id: int, group_id: int) -> bool:
    cursor.execute("""SELECT is_admin FROM group_user_link_table
                      WHERE user_id=%s AND group_id=%s;""", (login_id, group_id))
    sql_data, = cursor.fetchone()
    return sql_data


@dataclass
class GroupMember:
    id: int
    nick: str
    is_admin: bool

@connection_checking
def get_group_members(cursor: CMySQLCursor, group_id: int) -> List[GroupMember]:
    cursor.execute("""SELECT u.id, u.nick, is_admin FROM group_user_link_table
                      INNER JOIN users AS u ON group_user_link_table.user_id = u.id
                      WHERE group_id=%s;""", (group_id,))
    sql_data = cursor.fetchall()
    return [GroupMember(*i) for i in sql_data]


@connection_checking
def save_message(cursor: CMySQLCursor, content: str, sender: int, receiver: int, message_type: str, is_path: bool, is_bot: bool = False):
    cursor.execute("""INSERT INTO messages(content, sender_id, receiver_id, message_type, is_path, is_bot)
                      VALUES (%s, %s, %s, %s, %s, %s);""", (content, sender, receiver, message_type, is_path, is_bot))


@connection_checking
def save_group_message(cursor: CMySQLCursor, content: str, sender: int, group_id: int, message_type: str, is_path: bool, is_bot: bool = False):
    cursor.execute("""INSERT INTO groups_messages(content, sender_id, group_id, message_type, is_path, is_bot)
                      VALUES (%s, %s, %s, %s, %s, %s);""", (content, sender, group_id, message_type, is_path, is_bot))


@connection_checking
def register_user(cursor: CMySQLCursor, login: str, password: str, nick: str):
    password = functions.hash_password(password)
    try:
        cursor.execute("""INSERT INTO users(login, password, nick, warnings, is_banned)
                          VALUES (%s, %s, %s, 0, 0);""", (login, password, nick))
        user_id = get_user_id(cursor, nick)
        data = {"django_password": config.secret_key, "user_dogsonki_app_id": user_id, "nick": nick, "email": login}
        requests.post("http://127.0.0.1:8000/casino/create_account_dm", data=data)
        return True
    except IntegrityError:
        return False


@connection_checking
def set_user_avatar(cursor: CMySQLCursor, login: str, avatar: bytes):
    cursor.execute("""UPDATE users 
                      SET avatar = %s, avatar_time = NOW()
                      WHERE login = %s;""", (avatar, login))


@connection_checking
def create_session(cursor: CMySQLCursor, login_id: int) -> str:
    session_key = str(binascii.hexlify(os.urandom(20)).decode("UTF-8"))
    cursor.execute("""INSERT INTO sessions(login_id, session_key)
                      VALUES (%s, %s);""", (login_id, session_key))
    return session_key


@connection_checking
def create_confirmation_code(cursor: CMySQLCursor, login: str, code: int) -> Union[bool, int]:
    try:
        cursor.execute("""INSERT INTO email_confirmation(mail, code)
                          VALUES (%s, %s)""", (login, code))
        return True
    except IntegrityError:
        cursor.execute("""SELECT code FROM email_confirmation
                          WHERE mail=%s;""", (login,))
        data = cursor.fetchone()
        return data[0]


@connection_checking
def delete_session(cursor: CMySQLCursor, login_id: int):
    cursor.execute("""DELETE FROM sessions
                      WHERE login_id=%s;""", (login_id,))


@connection_checking
def add_to_group(cursor: CMySQLCursor, login_id: int, group_id: int):
    cursor.execute("""INSERT INTO group_user_link_table(user_id, group_id)
                      VALUES (%s, %s);""", (login_id, group_id))


@connection_checking
def delete_from_group(cursor: CMySQLCursor, login_id: int, group_id: int):
    cursor.execute("""DELETE FROM group_user_link_table
                      WHERE user_id=%s AND group_id=%s;""", (login_id, group_id))


@connection_checking
def create_group(cursor: CMySQLCursor, name: str) -> int:
    cursor.execute("""INSERT INTO groups_(name)
                      VALUES (%s);""", (name,))
    cursor.execute("""SELECT id FROM groups_ 
                      WHERE name = %s
                      ORDER BY id DESC LIMIT 1;""", (name,))
    group_id, = cursor.fetchone()
    return group_id


@connection_checking
def make_admin(cursor: CMySQLCursor, login_id: int, group_id: int):
    cursor.execute("""UPDATE group_user_link_table
                      SET is_admin=1
                      WHERE user_id=%s AND group_id=%s;""", (login_id, group_id))
        

@connection_checking
def check_if_login_exist(cursor: CMySQLCursor, login: str) -> Union[Tuple, None]:
    cursor.execute("""SELECT login FROM users
                      WHERE login = %s;""", (login, ))
    sql_data = cursor.fetchone()
    return sql_data


@connection_checking
def check_if_nick_exist(cursor: CMySQLCursor, nick: str) -> Union[Tuple, None]:
    cursor.execute("""SELECT nick FROM users
                      WHERE nick = %s;""", (nick, ))
    sql_data = cursor.fetchone()
    return sql_data


@connection_checking
def get_group_avatar(cursor: CMySQLCursor, group_id: int) -> Union[Tuple, None]:
    cursor.execute("""SELECT avatar, avatar_time FROM groups_
                      WHERE id=%s""", (group_id, ))
    avatar = cursor.fetchone()
    return avatar


@connection_checking
def set_group_avatar(cursor: CMySQLCursor, group_id: int, avatar: bytes):
    cursor.execute("""UPDATE groups_ 
                      SET avatar = %s, avatar_time = NOW()
                      WHERE id = %s;""", (avatar, group_id))


@connection_checking
def create_users_link(cursor: CMySQLCursor, user1_id: int, user2_id: int):
    cursor.execute("""INSERT INTO users_link_table(user1_id, user2_id)
                      VALUES (%s, %s)""", (user1_id, user2_id))


@connection_checking
def change_password(cursor: CMySQLCursor, login: str, new_password: str):
    new_password = functions.hash_password(new_password)
    cursor.execute("""UPDATE users
                      SET password = %s
                      WHERE login = %s;""", (new_password, login))


@connection_checking
def update_last_time_message(cursor: CMySQLCursor, user1_id: int, user2_id: int, message_id: int):
    cursor.execute("""UPDATE users_link_table
                      SET message_id=%s
                      WHERE (user1_id=%s AND user2_id=%s) OR (user2_id=%s AND user1_id=%s);""",
                   (message_id, user1_id, user2_id, user2_id, user1_id))


@connection_checking
def update_last_time_message_group(cursor: CMySQLCursor, group_id: int, message_id: int):
    cursor.execute("""UPDATE group_user_link_table
                      SET message_id=%s
                      WHERE group_id=%s;""", (message_id, group_id))


@connection_checking
def get_user_avatar_time(cursor: CMySQLCursor, user_id: int) -> datetime:
    cursor.execute("""SELECT avatar_time FROM users
                      WHERE id=%s""", (user_id,))
    data = cursor.fetchone()
    return data[0]


@connection_checking
def get_group_avatar_time(cursor: CMySQLCursor, group_id: int) -> datetime:
    cursor.execute("""SELECT avatar_time FROM groups_
                      WHERE id=%s""", (group_id,))
    data = cursor.fetchone()
    return data[0]


@connection_checking
def get_last_message_id(cursor: CMySQLCursor, sender_id: int, receiver_id: int):
    cursor.execute("""SELECT id, seen, sender_id FROM messages
                      WHERE (sender_id = %s AND receiver_id = %s) OR (sender_id = %s AND receiver_id = %s)
                      ORDER BY messages.id DESC LIMIT 1;""", (sender_id, receiver_id, receiver_id, sender_id))
    sql_data = cursor.fetchone()
    return sql_data


@connection_checking
def get_last_group_message_id(cursor: CMySQLCursor, group_id: int) -> int:
    cursor.execute("""SELECT message_id FROM groups_messages
                      WHERE group_id = %s
                      ORDER BY id DESC LIMIT 1;""", (group_id, ))
    sql_data = cursor.fetchone()
    return sql_data[0]


@connection_checking
def get_last_time_online(cursor: CMySQLCursor, user_id: int) -> datetime:
    cursor.execute("""SELECT last_online FROM users
                      WHERE id=%s""", (user_id,))
    sql_data = cursor.fetchone()
    return sql_data[0]


@connection_checking
def set_last_time_online(cursor: CMySQLCursor, user_id: int, date: datetime):
    cursor.execute("""UPDATE users SET last_online=%s
                      WHERE id=%s""", (date, user_id))


@connection_checking
def accept_invite(cursor: CMySQLCursor, user_id: int, accepting_user_id: int):
    cursor.execute("""UPDATE users_link_table SET is_friend=2
                      WHERE (user1_id = %s AND user2_id = %s) OR (user1_id = %s AND user2_id = %s);""",
                   (user_id, accepting_user_id, accepting_user_id, user_id))
    cursor.execute("""DELETE FROM invites
                      WHERE inviting_id = %s AND invited_id = %s;""",
                   (user_id, accepting_user_id))


@connection_checking
def send_invite(cursor: CMySQLCursor, user_id: int, invited_id: int):
    cursor.execute("""INSERT INTO invites (inviting_id, invited_id)
                      VALUES(%s, %s)""", (user_id, invited_id))
    cursor.execute("""UPDATE users_link_table SET is_friend=1
                      WHERE (user1_id = %s AND user2_id = %s) OR (user1_id = %s AND user2_id = %s);""",
                   (user_id, invited_id, invited_id, user_id))


@connection_checking
def get_invitations(cursor: CMySQLCursor, user_id: int) -> List:
    cursor.execute("""SELECT nick, inviting_id, last_online FROM invites
                      INNER JOIN users ON invites.inviting_id = users.id
                      WHERE invited_id=%s;""", (user_id,))
    sql_data = cursor.fetchall()
    return sql_data


@connection_checking
def set_message_as_seen(cursor: CMySQLCursor, message_id: int):
    cursor.execute("""UPDATE messages SET seen=1
                      WHERE id=%s;""", (message_id,))

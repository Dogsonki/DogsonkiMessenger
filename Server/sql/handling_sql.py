import os
import binascii
from datetime import datetime
from dataclasses import dataclass
import requests
from typing import List, Tuple, Union

from mysql.connector.errors import IntegrityError
from mysql.connector.cursor_cext import CMySQLCursor

from ..network.config import config


@dataclass
class ChatMessage:
    content: str
    sender: str
    time: datetime
    sender_id: int


def get_last_30_messages_from_chatroom(cursor: CMySQLCursor, sender_id: int, receiver_id: int,
                                       number_of_sent_last_messages: int) -> List[ChatMessage]:

    cursor.execute("""SELECT content, u1.nick, time, sender_id FROM ((messages
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


def get_last_30_messages_from_group_chatroom(cursor: CMySQLCursor, group_id: int,
                                             number_of_sent_last_messages: int) -> List[ChatMessage]:

    cursor.execute("""SELECT content, u.nick, time, sender_id FROM ((groups_messages
                      INNER JOIN users AS u ON groups_messages.sender_id = u.id)
                      INNER JOIN groups_ AS g ON groups_messages.group_id = g.id)
                      WHERE group_id = %s
                      ORDER BY groups_messages.id DESC LIMIT %s,30;""", (group_id, number_of_sent_last_messages))
    sql_data = cursor.fetchall()
    messages = []
    for i in sql_data:
        messages.append(ChatMessage(*i))
    return messages


def login_user(cursor: CMySQLCursor, login: str, password: str) -> Tuple:
    cursor.execute("""SELECT id, is_banned FROM users
                      WHERE login = %s and password = %s;""", (login, password))
    sql_data = cursor.fetchone()
    if sql_data is None:
        return False, None
    else:
        return sql_data


def check_session(cursor: CMySQLCursor, login_id: int, session_key: str) -> int:
    cursor.execute("""SELECT login_id FROM sessions
                      WHERE login_id=%s AND session_key=%s;""", (login_id, session_key))
    sql_data = cursor.fetchone()
    if sql_data is None:
        return 0
    else:
        return sql_data[0]


def search_by_nick(cursor: CMySQLCursor, nick: str) -> Union[bool, Tuple]:
    cursor.execute("""SELECT id, nick FROM users
                      WHERE nick LIKE %s;""", ("%" + nick + "%",))
    logins = cursor.fetchall()
    if logins is None:
        return False
    return logins


def get_user_chats(cursor: CMySQLCursor, login: str) -> Union[bool, Tuple]:
    cursor.execute("""SELECT u1.id, u1.nick, u2.id, u2.nick, last_message_time FROM ((users_link_table
                      INNER JOIN users AS u1 ON users_link_table.user1_id = u1.id)
                      INNER JOIN users AS u2 ON users_link_table.user2_id = u2.id) 
                      WHERE u1.login=%s OR u2.login=%s;""", (login, login))
    chats = cursor.fetchall()
    if chats is None:
        return False
    return chats


def get_user_avatar(cursor: CMySQLCursor, login_id: str) -> Union[Tuple, None]:
    cursor.execute("""SELECT avatar FROM users
                      WHERE id=%s""", (login_id, ))
    avatar = cursor.fetchone()
    return avatar


def get_user(cursor: CMySQLCursor, login_id: int) -> Tuple[str, str, str, bool]:
    cursor.execute("""SELECT login, password, nick, is_banned FROM users
                      WHERE id=%s;""", (login_id,))
    sql_data = cursor.fetchone()
    return sql_data


def get_user_id(cursor: CMySQLCursor, login: str) -> int:
    cursor.execute("""SELECT id FROM users
                      WHERE nick=%s""", (login,))
    sql_data = cursor.fetchone()
    return sql_data[0]


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


def update_email_confirmation_attempts(cursor: CMySQLCursor, login: str):
    cursor.execute("""UPDATE email_confirmation
                      SET attempts=attempts+1
                      WHERE mail=%s;""", (login,))


def delete_confirmation_code(cursor: CMySQLCursor, login: str):
    cursor.execute("""DELETE FROM email_confirmation
                      WHERE mail=%s;""", (login,))


def get_nick(cursor: CMySQLCursor, login_id: int) -> str:
    cursor.execute("""SELECT nick FROM users
                      WHERE id=%s""", (login_id,))
    sql_data = cursor.fetchone()
    return sql_data[0]


def get_user_groups(cursor: CMySQLCursor, login_id: int) -> Union[Tuple, bool]:
    cursor.execute("""SELECT g.name, g.id, last_message_time FROM group_user_link_table
                      INNER JOIN groups_ AS g ON group_user_link_table.group_id = g.id
                      WHERE user_id=%s;""", (login_id,))
    sql_data = cursor.fetchall()
    if sql_data is None:
        return False
    return sql_data


def get_is_admin(cursor: CMySQLCursor, login_id: int, group_id: int) -> bool:
    cursor.execute("""SELECT is_admin FROM group_user_link_table
                      WHERE user_id=%s AND group_id=%s;""", (login_id, group_id))
    sql_data, = cursor.fetchone()
    return sql_data


def get_group_members(cursor: CMySQLCursor, group_id: int) -> List:
    cursor.execute("""SELECT u.nick FROM group_user_link_table
                      INNER JOIN users AS u ON group_user_link_table.user_id = u.id
                      WHERE group_id=%s;""", (group_id,))
    sql_data = cursor.fetchall()
    return [i[0] for i in sql_data]


def save_message(cursor: CMySQLCursor, content: str, sender: int, receiver: int):
    cursor.execute("""INSERT INTO messages(content, sender_id, receiver_id)
                      VALUES (%s, %s, %s);""", (content, sender, receiver))


def save_group_message(cursor: CMySQLCursor, content: str, sender: int, group_id: int):
    cursor.execute("""INSERT INTO groups_messages(content, sender_id, group_id)
                      VALUES (%s, %s, %s);""", (content, sender, group_id))


def register_user(cursor: CMySQLCursor, login: str, password: str, nick: str):
    try:
        cursor.execute("""INSERT INTO users(login, password, nick, warnings, is_banned)
                          VALUES (%s, %s, %s, 0, 0);""", (login, password, nick))
        user_id = get_user_id(cursor, nick)
        data = {"django_password": config.secret_key, "user_dogsonki_app_id": user_id, "nick": nick, "email": login}
        requests.post("http://127.0.0.1:8000/casino/create_account_dm", data=data)
        return True
    except IntegrityError:
        return False


def set_user_avatar(cursor: CMySQLCursor, login: str, avatar: bytes):
    cursor.execute("""UPDATE users 
                      SET avatar = %s 
                      WHERE login = %s;""", (avatar, login))


def create_session(cursor: CMySQLCursor, login_id: int) -> str:
    session_key = str(binascii.hexlify(os.urandom(20)).decode("UTF-8"))
    cursor.execute("""INSERT INTO sessions(login_id, session_key)
                      VALUES (%s, %s);""", (login_id, session_key))
    return session_key


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


def delete_session(cursor: CMySQLCursor, login_id: int):
    cursor.execute("""DELETE FROM sessions
                      WHERE login_id=%s;""", (login_id,))


def add_to_group(cursor: CMySQLCursor, login_id: int, group_id: int):
    cursor.execute("""INSERT INTO group_user_link_table(user_id, group_id)
                      VALUES (%s, %s);""", (login_id, group_id))


def delete_from_group(cursor: CMySQLCursor, login_id: int, group_id: int):
    cursor.execute("""DELETE FROM group_user_link_table
                      WHERE user_id=%s AND group_id=%s;""", (login_id, group_id))


def create_group(cursor: CMySQLCursor, name: str) -> int:
    cursor.execute("""INSERT INTO groups_(name)
                      VALUES (%s);""", (name,))
    cursor.execute("""SELECT id FROM groups_ 
                      WHERE name = %s
                      ORDER BY id DESC LIMIT 1;""", (name,))
    group_id, = cursor.fetchone()
    return group_id


def make_admin(cursor: CMySQLCursor, login_id: int, group_id: int):
    cursor.execute("""UPDATE group_user_link_table
                      SET is_admin=1
                      WHERE user_id=%s AND group_id=%s;""", (login_id, group_id))
        

def check_if_login_exist(cursor: CMySQLCursor, login: str) -> Union[Tuple, None]:
    cursor.execute("""SELECT login FROM users
                      WHERE login = %s;""", (login, ))
    sql_data = cursor.fetchone()
    return sql_data


def check_if_nick_exist(cursor: CMySQLCursor, nick: str) -> Union[Tuple, None]:
    cursor.execute("""SELECT nick FROM users
                      WHERE nick = %s;""", (nick, ))
    sql_data = cursor.fetchone()
    return sql_data


def get_group_avatar(cursor: CMySQLCursor, group_id: int) -> Union[Tuple, None]:
    cursor.execute("""SELECT avatar FROM groups_
                      WHERE id=%s""", (group_id, ))
    avatar = cursor.fetchone()
    return avatar


def set_group_avatar(cursor: CMySQLCursor, group_id: int, avatar: bytes):
    cursor.execute("""UPDATE groups_ 
                      SET avatar = %s 
                      WHERE id = %s;""", (avatar, group_id))


def create_users_link(cursor: CMySQLCursor, user1_id: int, user2_id: int):
    cursor.execute("""INSERT INTO users_link_table(user1_id, user2_id)
                      VALUES (%s, %s)""", (user1_id, user2_id))


def change_password(cursor: CMySQLCursor, login: str, new_password: str):
    cursor.execute("""UPDATE users
                      SET password = %s
                      WHERE login = %s;""", (new_password, login))


def update_last_time_message(cursor: CMySQLCursor, user1_id: int, user2_id: int):
    cursor.execute("""UPDATE users_link_table
                      SET last_message_time=CURRENT_TIMESTAMP
                      WHERE (user1_id=%s AND user2_id=%s) OR (user2_id=%s AND user1_id=%s);""",
                   (user1_id, user2_id, user2_id, user1_id))


def update_last_time_message_group(cursor: CMySQLCursor, group_id: int):
    cursor.execute("""UPDATE group_user_link_table
                      SET last_message_time=CURRENT_TIMESTAMP
                      WHERE group_id=%s;""", (group_id,))

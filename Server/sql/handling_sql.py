import os
import binascii

from mysql.connector.errors import IntegrityError
from mysql.connector.cursor_cext import CMySQLCursor


class GetInfoFromDatabase:
    @staticmethod
    def get_last_30_messages_from_chatroom(cursor: CMySQLCursor, sender_id: int, receiver_id: int,
                                           number_of_sent_last_messages: int):

        cursor.execute("""SELECT content, u1.nick, u2.nick, time, sender_id FROM ((messages
                          INNER JOIN users AS u1 ON messages.sender_id = u1.id)
                          INNER JOIN users AS u2 ON messages.receiver_id = u2.id)
                          WHERE (sender_id = %s AND receiver_id = %s) OR (sender_id = %s AND receiver_id = %s)
                          ORDER BY messages.id DESC LIMIT %s,30;""", (sender_id, receiver_id, receiver_id, sender_id,
                                                                      number_of_sent_last_messages))
        sql_data = cursor.fetchall()
        return sql_data

    @staticmethod
    def get_last_30_messages_from_group_chatroom(cursor: CMySQLCursor, group_id: int, number_of_sent_last_messages: int):

        cursor.execute("""SELECT content, u.nick, u2.nick, time, sender_id FROM ((groups_messages
                          INNER JOIN users AS u ON messages.sender_id = u.id)
                          INNER JOIN groups_ AS g ON messages.group_id = g.id)
                          WHERE group_id = %s
                          ORDER BY messages.id DESC LIMIT %s,30;""", (group_id, number_of_sent_last_messages))
        sql_data = cursor.fetchall()
        return sql_data

    @staticmethod
    def login_user(cursor: CMySQLCursor, login: str, password: str):
        cursor.execute("""SELECT id, is_banned FROM users
                          WHERE login = %s and password = %s;""", (login, password))
        sql_data = cursor.fetchone()
        if sql_data is None:
            return False, None
        else:
            return sql_data

    @staticmethod
    def check_session(cursor: CMySQLCursor, login_id: int, session_key: str):
        cursor.execute("""SELECT login_id FROM sessions
                          WHERE login_id=%s AND session_key=%s;""", (login_id, session_key))
        sql_data = cursor.fetchone()
        if sql_data is None:
            return 0
        else:
            return sql_data[0]
        
    @staticmethod
    def search_by_nick(cursor: CMySQLCursor, nick: str):
        cursor.execute("""SELECT id, nick FROM users
                          WHERE nick LIKE %s;""", ("%" + nick + "%",))
        logins = cursor.fetchall()
        if logins is None:
            return False
        return logins

    @staticmethod
    def get_user_chats(cursor: CMySQLCursor, login: str):
        cursor.execute("""SELECT u2.id, u2.nick FROM ((messages
                          INNER JOIN users AS u1 ON messages.sender_id = u1.id)
                          INNER JOIN users AS u2 ON messages.receiver_id = u2.id) 
                          WHERE u1.login=%s OR u2.login=%s
                          GROUP BY u2.login
                          ORDER BY time DESC;""", (login, login))
        chats = cursor.fetchall()
        if chats is None:
            return False
        return chats

    @staticmethod
    def get_user_avatar(cursor: CMySQLCursor, login_id: str):
        cursor.execute("""SELECT avatar FROM users
                          WHERE id=%s""", (login_id, ))
        avatar = cursor.fetchone()
        return avatar

    @staticmethod
    def get_user(cursor: CMySQLCursor, login_id: int):
        cursor.execute("""SELECT login, password, nick, is_banned FROM users
                          WHERE id=%s;""", (login_id,))
        sql_data = cursor.fetchone()
        return sql_data

    @staticmethod
    def get_user_id(cursor: CMySQLCursor, login: str):
        cursor.execute("""SELECT id FROM users
                          WHERE nick=%s""", (login,))
        sql_data = cursor.fetchone()
        return sql_data

    @staticmethod
    def check_email_confirmation(cursor: CMySQLCursor, login: str, code: int):
        delete = False
        cursor.execute("""SELECT id FROM email_confirmation
                          WHERE mail=%s AND code=%s;""", (login, code))
        sql_data = cursor.fetchone()
        if sql_data:
            sql_data = True
            delete = True
        else:
            cursor.execute("""UPDATE email_confirmation
                              SET attempts=attempts+1
                              WHERE mail=%s;""", (login,))
            cursor.execute("""SELECT attempts FROM email_confirmation
                              WHERE mail=%s;""", (login,))
            sql_data = cursor.fetchone()
            if sql_data[0] > 5:
                delete = True

        if delete:
            cursor.execute("""DELETE FROM email_confirmation
                              WHERE mail=%s;""", (login,))
        return sql_data

    @staticmethod
    def get_nick(cursor: CMySQLCursor, login_id: str):
        cursor.execute("""SELECT nick FROM users
                          WHERE id=%s""", (login_id,))
        sql_data = cursor.fetchone()
        return sql_data

    @staticmethod
    def get_user_groups(cursor: CMySQLCursor, login: str):
        cursor.execute("""SELECT g.name, g.id FROM ((group_user_link_table
                          INNER JOIN users AS u ON group_user_link_table.user_id)
                          INNER JOIN groups_ AS g ON group_user_link_table.group_id)
                          WHERE u.login=%s;""", (login,))
        sql_data = cursor.fetchall()
        if sql_data is None:
            return False
        return sql_data

    @staticmethod
    def get_is_admin(cursor: CMySQLCursor, login_id: int, group_id: int) -> bool:
        cursor.execute("""SELECT is_admin FROM group_user_link_table
                          WHERE user_id=%s AND group_id=%s;""", (login_id, group_id))
        sql_data, = cursor.fetchone()
        return sql_data

    @staticmethod
    def get_group_members(cursor: CMySQLCursor, group_id: int):
        cursor.execute("""SELECT u.nick FROM group_user_link_table
                          INNER JOIN users AS u ON group_user_link_table.user_id 
                          WHERE group_id=%s;""", (group_id,))
        sql_data = cursor.fetchall()
        return [i[0] for i in sql_data]


class InsertIntoDatabase:
    @staticmethod
    def save_message(cursor: CMySQLCursor, content: str, sender: int, receiver: int):
        cursor.execute("""INSERT INTO messages(content, sender_id, receiver_id)
                          VALUES (%s, %s, %s);""", (content, sender, receiver))

    @staticmethod
    def save_group_message(cursor: CMySQLCursor, content: str, sender: int, group_id: int):
        cursor.execute("""INSERT INTO groups_messages(content, sender_id, group_id)
                          VALUES (%s, %s, %s);""", (content, sender, group_id))

    @staticmethod
    def register_user(cursor: CMySQLCursor, login: str, password: str, nick: str):
        try:
            cursor.execute("""INSERT INTO users(login, password, nick, warnings, is_banned)
                              VALUES (%s, %s, %s, 0, 0);""", (login, password, nick))
            return True
        except IntegrityError:
            return False

    @staticmethod
    def set_user_avatar(cursor: CMySQLCursor, login: str, avatar: bytes):
        cursor.execute("""UPDATE users 
                          SET avatar = %s 
                          WHERE login = %s;""", (avatar, login))

    @staticmethod
    def create_session(cursor: CMySQLCursor, login_id: int) -> str:
        session_key = str(binascii.hexlify(os.urandom(20)).decode("UTF-8"))
        cursor.execute("""INSERT INTO sessions(login_id, session_key)
                          VALUES (%s, %s);""", (login_id, session_key))
        return session_key

    @staticmethod
    def create_confirmation_code(cursor: CMySQLCursor, login: str, code: int) -> bool:
        try:
            cursor.execute("""INSERT INTO email_confirmation(mail, code)
                              VALUES (%s, %s)""", (login, code))
            return True
        except IntegrityError:
            cursor.execute("""SELECT code FROM email_confirmation
                              WHERE mail=%s;""", (login,))
            data = cursor.fetchone()
            return data[0]

    @staticmethod
    def delete_session(cursor: CMySQLCursor, login_id: int):
        cursor.execute("""DELETE FROM sessions
                          WHERE login_id=%s;""", (login_id,))

    @staticmethod
    def add_to_group(cursor: CMySQLCursor, login_id: int, group_id: int):
        cursor.execute("""INSERT INTO group_user_link_table(user_id, group_id)
                          VALUES (%s, %s);""", (login_id, group_id))

    @staticmethod
    def delete_from_group(cursor: CMySQLCursor, login_id: int, group_id: int):
        cursor.execute("""DELETE FROM group_user_link_table
                          WHERE user_id=%s AND group_id=%s;""", (login_id, group_id))

    @staticmethod
    def create_group(cursor: CMySQLCursor, name: str):
        cursor.execute("""INSERT INTO groups_(name)
                          VALUES (%s);""", (name,))
        cursor.execute("""SELECT id FROM groups_ 
                          WHERE name = %s
                          ORDER BY id DESC LIMIT 1;""", (name,))
        group_id, = cursor.fetchone()
        return group_id

    @staticmethod
    def make_admin(cursor: CMySQLCursor, login_id: int, group_id: int):
        cursor.execute("""UPDATE group_user_link_table
                          SET is_admin=1
                          WHERE user_id=%s AND group_id=%s;""", (login_id, group_id))
        

def check_if_login_exist(cursor: CMySQLCursor, login: str):
    cursor.execute("""SELECT login FROM users
                      WHERE login = %s;""", (login, ))
    sql_data = cursor.fetchone()
    return sql_data


def check_if_nick_exist(cursor: CMySQLCursor, nick: str):
    cursor.execute("""SELECT nick FROM users
                      WHERE nick = %s;""", (nick, ))
    sql_data = cursor.fetchone()
    return sql_data

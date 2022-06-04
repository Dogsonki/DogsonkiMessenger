import os
import binascii

from mysql.connector.errors import IntegrityError
from mysql.connector.cursor_cext import CMySQLCursor


class GetInfoFromDatabase:
    @staticmethod
    def get_last_30_messages_from_chatroom(cursor: CMySQLCursor, sender_id: int, receiver_id: int,
                                           number_of_sent_last_messages: int):

        cursor.execute("""SELECT content, u1.login, u2.login, time FROM ((messages
                          INNER JOIN users AS u1 ON messages.sender_id = u1.id)
                          INNER JOIN users AS u2 ON messages.receiver_id = u2.id)
                          WHERE (sender_id = %s AND receiver_id = %s) OR (sender_id = %s AND receiver_id = %s)
                          ORDER BY messages.id LIMIT %s,30;""", (sender_id, receiver_id, receiver_id, sender_id,
                                                                 number_of_sent_last_messages))
        sql_data = cursor.fetchall()
        return sql_data

    @staticmethod
    def login_user(cursor: CMySQLCursor, login: str, password: str):
        cursor.execute("""SELECT id FROM users
                          WHERE login = %s and password = %s;""", (login, password))
        sql_data = cursor.fetchone()
        if sql_data is None:
            return False
        else:
            return sql_data[0]

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
    def search_by_login(cursor: CMySQLCursor, login: str):
        cursor.execute("""SELECT login FROM users
                          WHERE login LIKE %s;""", ("%" + login,))
        logins = cursor.fetchall()
        if logins is None:
            return False
        return [i for i, in logins]

    @staticmethod
    def get_user_chats(cursor: CMySQLCursor, login: str):
        cursor.execute("""SELECT u2.login FROM ((messages
                          INNER JOIN users AS u1 ON messages.sender_id = u1.id)
                          INNER JOIN users AS u2 ON messages.receiver_id = u2.id) 
                          WHERE u1.login=%s OR u2.login=%s
                          GROUP BY u2.login
                          ORDER BY time DESC;""", (login, login))
        chats = cursor.fetchall()
        if chats is None:
            return False
        return [i for i, in chats]

    @staticmethod
    def get_user_avatar(cursor: CMySQLCursor, login: str):
        cursor.execute("""SELECT avatar FROM users
                          WHERE login=%s""", (login, ))
        avatar = cursor.fetchone()
        return avatar

    @staticmethod
    def get_user(cursor: CMySQLCursor, login_id: int):
        cursor.execute("""SELECT login, password FROM users
                          WHERE id=%s;""", (login_id,))
        sql_data = cursor.fetchone()
        return sql_data

    @staticmethod
    def get_user_id(cursor: CMySQLCursor, login: str):
        cursor.execute("""SELECT id FROM users
                          WHERE login=%s""", (login,))
        sql_data = cursor.fetchone()
        return sql_data


class InsertIntoDatabase:
    @staticmethod
    def save_message(cursor: CMySQLCursor, content: str, sender: int, receiver: int):
        cursor.execute("""INSERT INTO messages(content, sender_id, receiver_id)
                          VALUES (%s, %s, %s);""", (content, sender, receiver))

    @staticmethod
    def register_user(cursor: CMySQLCursor, login: str, password: str):
        try:
            cursor.execute("""INSERT INTO users(login, password, warnings, is_banned)
                              VALUES (%s, %s, 0, 0);""", (login, password))
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
    def delete_session(cursor: CMySQLCursor, login_id: int):
        cursor.execute("""DELETE FROM sessions
                          WHERE login_id=%s;""", (login_id,))


def check_if_login_exist(cursor: CMySQLCursor, login: str):
    cursor.execute("""SELECT login FROM users
                      WHERE login = %s;""", (login, ))
    sql_data = cursor.fetchone()
    return sql_data

from mysql.connector.errors import IntegrityError

from .connection import database_cursor


class GetInfoFromDatabase:
    @staticmethod
    def get_last_30_messages_from_chatroom(sender, receiver, number_of_sent_last_messages):
        if check_if_login_exist(receiver) is None:
            return False

        database_cursor.execute("""SELECT content, sender, receiver, time FROM messages
                                   WHERE (sender = %s and receiver = %s) OR (sender = %s AND receiver = %s)
                                   ORDER BY ID LIMIT %s,30;""", (sender, receiver, receiver, sender, number_of_sent_last_messages))
        sql_data = database_cursor.fetchall()
        return sql_data

    @staticmethod
    def login_user(login, password):
        database_cursor.execute("""SELECT login, password FROM users
                                   WHERE login = %s and password = %s;""", (login, password))
        sql_data = database_cursor.fetchone()
        if sql_data is None:
            return False
        else:
            return True
        
    @staticmethod
    def search_by_login(login):
        database_cursor.execute("""SELECT login FROM users
                                   WHERE login LIKE %s;""", ("%" + login,))
        logins = database_cursor.fetchall()
        if logins is None:
            return False
        else:
            return [i for i, in logins]

    @staticmethod
    def get_user_chats(login):
        database_cursor.execute("""SELECT receiver FROM messages  
                                   WHERE sender=%s OR receiver=%s
                                   GROUP BY receiver
                                   ORDER BY time DESC;""", (login, login))
        chats = database_cursor.fetchall()
        return [i for i, in chats]


class InsertIntoDatabase:
    @staticmethod
    def save_message(content, sender, receiver):
        database_cursor.execute("""INSERT INTO messages(content, sender, receiver)
                                   VALUES (%s, %s, %s);""", (content, sender, receiver))

    @staticmethod
    def register_user(login, password):
        try:
            database_cursor.execute("""INSERT INTO users(login, password, warnings, banned)
                                       VALUES (%s, %s, 0, 0);""", (login, password))
            return True
        except IntegrityError:
            return False


def check_if_login_exist(login):
    database_cursor.execute("""SELECT login FROM users
                               WHERE login = %s;""", (login, ))
    sql_data = database_cursor.fetchone()
    return sql_data



from .connection import database_cursor


class GetInfoFromDatabase:
    @staticmethod
    def get_last_30_messages_from_chatroom(sender, receiver):
        if check_if_login_exist(receiver) is None:
            return False

        database_cursor.execute("""SELECT content, sender, receiver FROM messages
                                   WHERE (sender = ? and receiver = ?) OR (sender = ? AND receiver = ?)
                                   ORDER BY ID DESC LIMIT 30;""", (sender, receiver, receiver, sender))
        sql_data = database_cursor.fetchall()
        return sql_data

    @staticmethod
    def login_user(login, password):
        database_cursor.execute("""SELECT login, password FROM users
                                   WHERE login = ? and password = ?;""", (login, password))
        sql_data = database_cursor.fetchone()
        if sql_data is None:
            return False
        else:
            return True


class InsertIntoDatabase:
    @staticmethod
    def save_message(content, sender, receiver):
        database_cursor.execute("""INSERT INTO messages(content, sender, receiver)
                                   VALUES (?, ?, ?);""", (content, sender, receiver))

    @staticmethod
    def register_user(login, password):
        database_cursor.execute("""INSERT INTO users(login, password, warnings, banned)
                                   VALUES (?, ?, 0, 0);""", (login, password))


def check_if_login_exist(login):
    database_cursor.execute("""SELECT login FROM users
                               WHERE login = ?;""", (login, ))
    sql_data = database_cursor.fetchone()
    return sql_data



from .connection import database_cursor


class CreateDatabase:
    def create_all_tables(self):
        self.create_users_table()
        self.create_messages_table()

    @staticmethod
    def create_users_table():
        database_cursor.execute("""CREATE TABLE IF NOT EXISTS users (
                                   id INTEGER PRIMARY KEY AUTO_INCREMENT,
                                   login VARCHAR(50) NOT NULL UNIQUE,
                                   password VARCHAR(50) NOT NULL,
                                   warnings INTEGER NOT NULL,
                                   banned BIT NOT NULL,
                                   avatar BLOB
                                   );""")

    @staticmethod
    def create_messages_table():
        database_cursor.execute("""CREATE TABLE IF NOT EXISTS messages (
                                   id INTEGER PRIMARY KEY AUTO_INCREMENT,
                                   content TEXT NOT NULL,
                                   sender VARCHAR(50) NOT NULL,
                                   receiver VARCHAR(50) NOT NULL,
                                   time TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                                   );""")

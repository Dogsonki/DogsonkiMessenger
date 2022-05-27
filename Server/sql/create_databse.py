from .connection import get_cursor


class CreateDatabase:
    def create_all_tables(self):
        cursor = get_cursor()
        self.create_users_table(cursor)
        self.create_messages_table(cursor)
        self.create_session_table(cursor)
        cursor.close()

    @staticmethod
    def create_users_table(cursor):
        cursor.execute("""CREATE TABLE IF NOT EXISTS users (
                                   id INTEGER PRIMARY KEY AUTO_INCREMENT,
                                   login VARCHAR(50) NOT NULL UNIQUE,
                                   password VARCHAR(50) NOT NULL,
                                   warnings INTEGER NOT NULL,
                                   is_banned BIT NOT NULL,
                                   avatar MEDIUMBLOB
                                   );""")

    @staticmethod
    def create_messages_table(cursor):
        cursor.execute("""CREATE TABLE IF NOT EXISTS messages (
                                   id INTEGER PRIMARY KEY AUTO_INCREMENT,
                                   content TEXT NOT NULL,
                                   sender_id INTEGER,
                                   receiver_id INTEGER,
                                   time TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                                   
                                   FOREIGN KEY (sender_id) REFERENCES users(id) ON DELETE SET NULL,
                                   FOREIGN KEY (receiver_id) REFERENCES users(id) ON DELETE SET NULL
                                   );""")

    @staticmethod
    def create_session_table(cursor):
        cursor.execute("""CREATE TABLE IF NOT EXISTS sessions (
                                   id INTEGER PRIMARY KEY AUTO_INCREMENT,
                                   login_id INTEGER NOT NULL, 
                                   session_key VARCHAR(50),
                                   
                                   FOREIGN KEY (login_id) REFERENCES users(id) ON DELETE CASCADE
                                   );""")

from mysql.connector.cursor_cext import CMySQLCursor

from .connection import get_cursor


class CreateDatabase:
    cursor: CMySQLCursor

    def create_all_tables(self):
        self.cursor = get_cursor()
        self.create_users_table()
        self.create_messages_table()
        self.create_session_table()
        self.create_confirmation_mail_table()
        self.create_group_table()
        self.create_group_user_link_table()
        self.create_group_messages_table()
        self.cursor.close()

    def create_users_table(self):
        self.cursor.execute("""CREATE TABLE IF NOT EXISTS users (
                                   id INTEGER PRIMARY KEY AUTO_INCREMENT,
                                   nick VARCHAR(50) NOT NULL UNIQUE,
                                   login VARCHAR(50) NOT NULL UNIQUE,
                                   password VARCHAR(50) NOT NULL,
                                   warnings INTEGER NOT NULL,
                                   is_banned BIT NOT NULL,
                                   avatar MEDIUMBLOB
                                   );""")

    def create_messages_table(self):
        self.cursor.execute("""CREATE TABLE IF NOT EXISTS messages (
                                   id INTEGER PRIMARY KEY AUTO_INCREMENT,
                                   content TEXT NOT NULL,
                                   sender_id INTEGER,
                                   receiver_id INTEGER,
                                   time TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                                   
                                   FOREIGN KEY (sender_id) REFERENCES users(id) ON DELETE SET NULL,
                                   FOREIGN KEY (receiver_id) REFERENCES users(id) ON DELETE SET NULL
                                   );""")

    def create_session_table(self):
        self.cursor.execute("""CREATE TABLE IF NOT EXISTS sessions (
                                   id INTEGER PRIMARY KEY AUTO_INCREMENT,
                                   login_id INTEGER NOT NULL, 
                                   session_key VARCHAR(50),
                                   
                                   FOREIGN KEY (login_id) REFERENCES users(id) ON DELETE CASCADE
                                   );""")

    def create_confirmation_mail_table(self):
        self.cursor.execute("""CREATE TABLE IF NOT EXISTS email_confirmation (
                                  id INTEGER PRIMARY KEY AUTO_INCREMENT,
                                  creation_time DATETIME DEFAULT CURRENT_TIMESTAMP,
                                  mail VARCHAR(50) NOT NULL UNIQUE,
                                  code INTEGER NOT NULL,
                                  attempts SMALLINT DEFAULT 0
                                  );""")

    def create_group_table(self):
        self.cursor.execute("""CREATE TABLE IF NOT EXISTS groups_ (
                                   id INTEGER PRIMARY KEY AUTO_INCREMENT,
                                   name VARCHAR(50),
                                   avatar MEDIUMBLOB
                                   );""")

    def create_group_user_link_table(self):
        self.cursor.execute("""CREATE TABLE IF NOT EXISTS group_user_link_table (
                                  user_id INTEGER NOT NULL,
                                  group_id INTEGER NOT NULL,
                                  is_admin BIT DEFAULT 0,
                               
                                  FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
                                  FOREIGN KEY (group_id) REFERENCES groups_(id) ON DELETE CASCADE
                                  );""")

    def create_group_messages_table(self):
        self.cursor.execute("""CREATE TABLE IF NOT EXISTS groups_messages (
                                   id INTEGER PRIMARY KEY AUTO_INCREMENT,
                                   content TEXT NOT NULL,
                                   sender_id INTEGER,
                                   group_id INTEGER,
                                   time TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                                   
                                   FOREIGN KEY (sender_id) REFERENCES users(id) ON DELETE SET NULL,
                                   FOREIGN KEY (group_id) REFERENCES groups_(id) ON DELETE SET NULL                   
                                   );""")

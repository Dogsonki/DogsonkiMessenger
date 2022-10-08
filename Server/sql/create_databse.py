from mysql.connector.cursor_cext import CMySQLCursor

from .connection import sql_con


class CreateDatabase:
    cursor: CMySQLCursor

    def create_all_tables(self):
        self.cursor = sql_con.get_cursor()
        self.create_users_table()
        self.create_messages_table()
        self.create_users_link_table()
        self.create_session_table()
        self.create_confirmation_mail_table()
        self.create_group_table()
        self.create_group_messages_table()
        self.create_group_user_link_table()
        self.cursor.close()

    def create_users_table(self):
        self.cursor.execute("""CREATE TABLE IF NOT EXISTS users (
                                   id INTEGER PRIMARY KEY AUTO_INCREMENT,
                                   nick VARCHAR(50) NOT NULL UNIQUE,
                                   login VARCHAR(50) NOT NULL UNIQUE,
                                   password VARCHAR(150) NOT NULL,
                                   warnings INTEGER NOT NULL,
                                   is_banned BIT NOT NULL,
                                   avatar MEDIUMBLOB,
                                   avatar_time TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                                   );""")

    def create_users_link_table(self):
        self.cursor.execute("""CREATE TABLE IF NOT EXISTS users_link_table (
                                  user1_id INTEGER NOT NULL,
                                  user2_id INTEGER NOT NULL,
                                  is_friend BIT DEFAULT 0,
                                  message_id INTEGER DEFAULT NULL,

                                  FOREIGN KEY (user1_id) REFERENCES users(id) ON DELETE CASCADE,
                                  FOREIGN KEY (user2_id) REFERENCES users(id) ON DELETE CASCADE,
                                  FOREIGN KEY (message_id) REFERENCES messages(id) ON DELETE CASCADE
                                  );""")

    def create_messages_table(self):
        self.cursor.execute("""CREATE TABLE IF NOT EXISTS messages (
                                   id INTEGER PRIMARY KEY AUTO_INCREMENT,
                                   content TEXT NOT NULL,
                                   sender_id INTEGER,
                                   receiver_id INTEGER,
                                   time TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                                   message_type VARCHAR(5),
                                   is_path BIT NOT NULL,
                                   
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
                                   avatar MEDIUMBLOB,
                                   avatar_time TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                                   );""")

    def create_group_user_link_table(self):
        self.cursor.execute("""CREATE TABLE IF NOT EXISTS group_user_link_table (
                                  user_id INTEGER NOT NULL,
                                  group_id INTEGER NOT NULL,
                                  is_admin BIT DEFAULT 0,
                                  is_accepted_by_user BIT DEFAULT 0,
                                  message_id INTEGER DEFAULT NULL,
                               
                                  FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
                                  FOREIGN KEY (group_id) REFERENCES groups_(id) ON DELETE CASCADE,
                                  FOREIGN KEY (message_id) REFERENCES groups_messages(id) ON DELETE CASCADE
                                  );""")

    def create_group_messages_table(self):
        self.cursor.execute("""CREATE TABLE IF NOT EXISTS groups_messages (
                                   id INTEGER PRIMARY KEY AUTO_INCREMENT,
                                   content TEXT NOT NULL,
                                   sender_id INTEGER,
                                   group_id INTEGER,
                                   time TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                                   message_type VARCHAR(5),
                                   is_path BIT NOT NULL,
                                   
                                   FOREIGN KEY (sender_id) REFERENCES users(id) ON DELETE SET NULL,
                                   FOREIGN KEY (group_id) REFERENCES groups_(id) ON DELETE SET NULL                   
                                   );""")

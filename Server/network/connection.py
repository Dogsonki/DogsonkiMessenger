from Server.sql import handling_sql

current_connections = {}
INSERT_SQL = handling_sql.InsertIntoDatabase()
SELECT_SQL = handling_sql.GetInfoFromDatabase()


class Connection:
    def __init__(self, connection):
        self.connection = connection
        self.login = ""
        self.password = ""

    def send_message(self, message):
        self.connection.send(bytes(message, "UTF-8"))

    def receive_message(self):
        try:
            return self.connection.recv(1024).decode("UTF-8")
        except (ConnectionAbortedError, ConnectionResetError):
            del current_connections[self.login]
            return False


class LoginUser:
    def __init__(self, connection: Connection):
        self.connection = connection
        self.login_app_code = "0001"
        self.registering_app_code = "0002"

    def login_user(self):
        while True:
            action_info = self.connection.receive_message()
            if action_info == "logging":
                self.connection.login, self.connection.password = self.get_login_data()
                if SELECT_SQL.login_user(self.connection.login, self.connection.password):
                    self.connection.send_message(f"{self.login_app_code}-Logged as {self.connection.login}")
                    break
                else:
                    self.connection.send_message(f"{self.login_app_code}-{'Wrong login/password'}")
            elif action_info == "registering":
                self.connection.login, self.connection.password = self.get_register_data()
                if self.validate_login_data():
                    INSERT_SQL.register_user(self.connection.login, self.connection.password)
                    self.connection.send_message(f"{self.registering_app_code}-Registered {self.connection.login}")
                    break
                else:
                    self.connection.send_message(f"{self.registering_app_code}-Error: wrong password")
            else:
                self.connection.send_message(f"Error - should receive 'logging' or 'registering'")

    def get_login_data(self):
        login_data = []
        for _ in range(2):
            data_from_user = self.connection.receive_message()
            if data_from_user == "Q":
                return "", ""
            login_data.append(data_from_user)
        return login_data

    def get_register_data(self):
        registering_data = []
        for _ in range(2):
            data_from_user = self.connection.receive_message()
            if data_from_user == "Q":
                return "", ""
            registering_data.append(data_from_user)
        return registering_data

    def validate_login_data(self):
        if 2 <= len(self.connection.login) <= 50 and 2 <= len(self.connection.password) <= 50:
            return True
        return False

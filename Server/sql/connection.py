import json
from dataclasses import dataclass

import mysql.connector
from mysql.connector.cursor_cext import CMySQLCursor
from mysql.connector.connection_cext import CMySQLConnection


@dataclass
class DbConfig:
    host: str
    user: str
    password: str
    database: str


with open("db_config.json") as file:
    db_config: DbConfig = json.load(file, object_hook=lambda d: DbConfig(**d))


class SqlCon:
    DATABASE_CONNECTION: CMySQLConnection

    def __init__(self):
        self.create_connection()

    def create_connection(self):
        self.DATABASE_CONNECTION = mysql.connector.connect(
            host=db_config.host,
            user=db_config.user,
            password=db_config.password,
            database=db_config.database,
            autocommit=True
        )

    def get_cursor(self, reset: bool = True) -> CMySQLCursor:
        try:
            return self.DATABASE_CONNECTION.cursor(buffered=True)
        except mysql.connector.errors.OperationalError:
            if reset:
                self.create_connection()
                self.get_cursor(False)


sql_con = SqlCon()

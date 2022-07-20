import json
from dataclasses import dataclass

import mysql.connector
from mysql.connector.cursor_cext import CMySQLCursor


@dataclass
class DbConfig:
    host: str
    user: str
    password: str
    database: str


with open("db_config.json") as file:
    db_config: DbConfig = json.load(file, object_hook=lambda d: DbConfig(**d))

DATABASE_CONNECTION = mysql.connector.connect(
    host=db_config.host,
    user=db_config.user,
    password=db_config.password,
    database=db_config.database,
    autocommit=True
)


def get_cursor() -> CMySQLCursor:
    return DATABASE_CONNECTION.cursor(buffered=True)

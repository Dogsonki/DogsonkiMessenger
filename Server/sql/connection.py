import json

import mysql.connector


with open("db_config.json") as file:
    mysql_connect_data = json.load(file)

DATABASE_CONNECTION = mysql.connector.connect(
    host=mysql_connect_data["host"],
    user=mysql_connect_data["user"],
    password=mysql_connect_data["password"],
    database=mysql_connect_data["database"],
    autocommit=True
)

database_cursor = DATABASE_CONNECTION.cursor()


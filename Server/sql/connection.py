import sqlite3

DATABASE_CONNECTION = sqlite3.connect("data//database.db", check_same_thread=False, isolation_level=None)
database_cursor = DATABASE_CONNECTION.cursor()

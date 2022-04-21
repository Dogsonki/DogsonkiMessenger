import os
os.sys.path.append(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))  # fix "no module named Server"
import json
import socket
import threading

from network import client_menu
from network.connection import Connection, LoginUser


def listen_for_connections():
    SERVER.listen(10)
    print("Waiting for connections...")
    while True:
        connection, address = SERVER.accept()
        threading._start_new_thread(on_new_connection, (connection, ))
        print(f"Accepted new connection: {address}")


def on_new_connection(connection):
    connection = Connection(connection)
    LoginUser(connection).login_user()
    client_menu.ClientMenu(connection).listening()


if __name__ == '__main__':
    print("\nMADE BY DOGSON\n")
    SERVER = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    with open("config.json") as file:
        server_config = json.load(file)
    SERVER.bind((server_config["ip"], server_config["port"]))
    listen_for_connections()

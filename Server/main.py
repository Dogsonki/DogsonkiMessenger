import os
os.sys.path.append(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))  # fix "no module named Server"
import json
import socket
import threading

from network.client_menu import ClientMenu
from network.connection import Client
from sql.create_databse import CreateDatabase


def listen_for_connections():
    SERVER.listen(10)
    print("Waiting for connections...")
    while True:
        connection, address = SERVER.accept()
        threading._start_new_thread(on_new_connection, (connection, address))
        print(f"Accepted new connection: {address}")


def on_new_connection(connection, address):
    client = Client(connection, address)
    client.get_login_action()
    ClientMenu(client).listening()


if __name__ == '__main__':
    print("\nMADE BY DOGSON\n")
    CreateDatabase().create_all_tables()
    SERVER = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    SERVER.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
    with open("config.json") as file:
        server_config = json.load(file)
    SERVER.bind((server_config["ip"], server_config["port"]))
    listen_for_connections()

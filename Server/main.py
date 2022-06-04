import sys
import os
import socket
import threading
import json
from dataclasses import dataclass

sys.path.append(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))  # this fixes "no module named Server"

from network.client_menu import ClientMenu
from network.connection import Client
from sql.create_databse import CreateDatabase


def listen_for_connections(sock: socket.socket):
    sock.listen(10)  # can accept max 10 connections at the same time
    print("Waiting for connections...")
    while True:
        connection, address = sock.accept()
        threading.Thread(target=on_new_connection, args=(connection, address)).start()
        print(f"Accepted new connection: {address}")


def on_new_connection(connection: socket.socket, address):
    client = Client(connection, address)
    client.get_login_action()
    ClientMenu(client).listening()


@dataclass
class Config:
    ip: str
    port: str


def main():
    print("\nMADE BY DOGSON\n")
    CreateDatabase().create_all_tables()

    server = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    server.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)

    with open("config.json") as file:
        config: Config = json.load(file, object_hook=lambda d: Config(**d))

    server.bind((config.ip, config.port))
    listen_for_connections(server)


if __name__ == '__main__':
    main()

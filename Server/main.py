import json
import socket
import threading
import sys

from dataclasses import dataclass
from pathlib import Path

sys.path.append(str(Path(__file__).parent.parent))  # fix "no module named Server"

from network.client_menu import ClientMenu
from network.connection import Client
from sql.create_databse import CreateDatabase


def listen_for_connections(sock: socket.socket):
    sock.listen(10)
    print("Waiting for connections...")
    while True:
        connection, address = sock.accept()
        threading._start_new_thread(on_new_connection, (connection, address))
        print(f"Accepted new connection: {address}")


def on_new_connection(connection, address):
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

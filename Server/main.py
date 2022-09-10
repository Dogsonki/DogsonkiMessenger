import sys
import os
import socket
import threading
from typing import Tuple
import ssl

sys.path.append(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))  # this fixes "no module named Server"

from network.client_menu import ClientMenu
from network.connection import Client
from sql.create_databse import CreateDatabase
from network.config import config


context = ssl.SSLContext(ssl.PROTOCOL_TLS_SERVER)
context.load_cert_chain("/etc/ssl/cert.pem", "/etc/ssl/key.pem")
context.set_ciphers("AES256-GCM-SHA384")


def listen_for_connections(sock: socket.socket):
    print("Waiting for connections...")
    s_sock = context.wrap_socket(sock, server_side=True)
    while True:
        connection, address = s_sock.accept()
        threading.Thread(target=on_new_connection, args=(connection, address)).start()
        print(f"Accepted new connection: {address}")


def on_new_connection(connection: socket.socket, address: Tuple[str, int]):  # address: [ip, port]
    client = Client(connection, address)
    client.get_login_action()
    ClientMenu(client).listening()


def main():
    print("\nMADE BY DOGSON\n")
    CreateDatabase().create_all_tables()

    server = socket.socket(socket.AF_INET, socket.SOCK_STREAM, 0)
    server.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)

    server.bind((config.ip, config.port))
    server.listen(10)  # can accept max 10 connections at the same time
    listen_for_connections(server)


if __name__ == '__main__':
    main()

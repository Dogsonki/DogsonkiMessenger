from .connection import Client, MessageType
from . import casino


COMMANDS: dict = {
    "!daily": casino.daily,
    "!bet": casino.bet,
    "jackpotbuy": casino.jackpotbuy,
    "!zdrapka": casino.zdrapka,
    "!sklep": casino.sklep,
    "!slots": casino.slots
}


def check_command(client: Client, data: dict):
    data["user_dogsonki_app_id"] = client.login_id
    response = COMMANDS[data["command"]](data)
    client.send_message(response, MessageType.BOT_COMMAND)

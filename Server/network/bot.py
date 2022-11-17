from .connection import Client, MessageType
import time
from . import casino


COMMANDS: dict = {
    "!daily": casino.daily,
    "!bet": casino.bet,
    "jackpotbuy": casino.jackpotbuy,
    "!zdrapka": casino.zdrapka,
    "!sklep": casino.sklep,
    "!slots": casino.slots,
    "!mem": casino.mem
}


def check_command(client: Client, data: dict) -> list[dict]:
    data["user_dogsonki_app_id"] = client.login_id
    response, message_type = COMMANDS[data["command"]](data)
    data = [{"user": None, "message": response, "time": time.time(),
             "user_id": None, "is_group": None, "group_id": None,
             "message_type": message_type, "id": None}]
    return data

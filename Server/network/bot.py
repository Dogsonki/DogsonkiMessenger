import time
import base64

from .connection import Client
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


def check_command(client: Client, data: dict, is_group) -> dict:
    data["user_dogsonki_app_id"] = client.login_id
    response, message_type = COMMANDS[data["command"]](data)
    response = str(base64.b64encode(bytes(response, "UTF-8")))[2:-1]
    data = {"user": 0, "message": response, "time": time.time(),
             "user_id": 0, "is_group": is_group, "group_id": 0,
             "message_type": message_type, "id": 0, "is_bot": True}
    return data

import requests
from .config import config


def daily(data: dict) -> str:
    data["django_password"] = config.secret_key
    message = requests.post("http://127.0.0.1:8000/casino/set_daily_dm", data=data)
    return message.json()["message"]


def bet(data: dict) -> str:
    data["django_password"] = config.secret_key
    message = requests.post("http://127.0.0.1:8000/casino/bet_dm", data=data)
    return message.json()["message"]


def jackpotbuy(data: dict) -> str:
    data["django_password"] = config.secret_key
    message = requests.post("http://127.0.0.1:8000/casino/jackpot_buy_dm", data=data)
    return message.json()["message"]


def zdrapka(data: dict) -> str:
    data["django_password"] = config.secret_key
    message = requests.post("http://127.0.0.1:8000/casino/buy_scratch_card_dm", data=data)
    return message.json()["message"]


def sklep(data: dict) -> str:
    data["django_password"] = config.secret_key
    message = requests.post("http://127.0.0.1:8000/casino/shop_dm", data=data)
    return message.json()["message"]


def slots(data: dict) -> str:
    data["django_password"] = config.secret_key
    message = requests.post("http://127.0.0.1:8000/casino/slots_dm", data=data)
    return message.json()["message"]

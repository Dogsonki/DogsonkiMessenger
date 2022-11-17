import requests
import random as rd
import os

from .config import config


MEMES = [os.path.join("media//memes", i) for i in os.listdir("media//memes")]


def daily(data: dict) -> [str, str]:
    data["django_password"] = config.secret_key
    message = requests.post("http://127.0.0.1:8000/casino/set_daily_dm", data=data)
    return message.json()["message"], "text"


def bet(data: dict) -> [str, str]:
    data["django_password"] = config.secret_key
    message = requests.post("http://127.0.0.1:8000/casino/bet_dm", data=data)
    return message.json()["message"], "text"


def jackpotbuy(data: dict) -> [str, str]:
    data["django_password"] = config.secret_key
    message = requests.post("http://127.0.0.1:8000/casino/jackpot_buy_dm", data=data)
    return message.json()["message"], "text"


def zdrapka(data: dict) -> [str, str]:
    data["django_password"] = config.secret_key
    message = requests.post("http://127.0.0.1:8000/casino/buy_scratch_card_dm", data=data)
    return message.json()["message"], "text"


def sklep(data: dict) -> [str, str]:
    data["django_password"] = config.secret_key
    message = requests.post("http://127.0.0.1:8000/casino/shop_dm", data=data)
    return message.json()["message"], "text"


def slots(data: dict) -> [str, str]:
    data["django_password"] = config.secret_key
    message = requests.post("http://127.0.0.1:8000/casino/slots_dm", data=data)
    return message.json()["message"], "text"


def mem(data: dict) -> [str, str]:
    meme_path = rd.choice(MEMES)
    return meme_path, "jpeg"

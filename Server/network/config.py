from dataclasses import dataclass
import json


@dataclass
class Config:
    ip: str
    port: str
    secret_key: str


with open("config.json") as file:
    config: Config = json.load(file, object_hook=lambda d: Config(**d))

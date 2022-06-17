import random as rd
import json
from dataclasses import dataclass
import smtplib
import ssl
import certifi
from email.mime.text import MIMEText
from email.mime.multipart import MIMEMultipart


@dataclass
class SmtpConfig:
    hostname: str
    mail: str
    password: str


with open("email_config.json") as file:
    SMTP_CONFIG: SmtpConfig = json.load(file, object_hook=lambda d: SmtpConfig(**d))


def get_confirmation_code() -> int:
    confirmation_code = rd.randint(10000, 99999)
    return confirmation_code


class SmptConnection:
    smpt_connection: smtplib.SMTP

    def __init__(self):
        self.connect()

    def connect(self):
        self.smpt_connection = smtplib.SMTP(host=SMTP_CONFIG.hostname, port=587)
        context = ssl.create_default_context(cafile=certifi.where())
        self.smpt_connection.starttls(context=context)
        self.smpt_connection.login(SMTP_CONFIG.mail, SMTP_CONFIG.password)

    def send_mail(self, receiver: str, message: MIMEMultipart) -> bool:
        try:
            self.smpt_connection.sendmail(SMTP_CONFIG.mail, receiver, message.as_string())
            return True
        except smtplib.SMTPRecipientsRefused:
            return False
        except smtplib.SMTPServerDisconnected:
            self.connect()
            self.smpt_connection.sendmail(SMTP_CONFIG.mail, receiver, message.as_string())
            return True

    @staticmethod
    def create_message(receiver: str, code: int) -> MIMEMultipart:
        message = MIMEMultipart("alternative")
        message["From"] = SMTP_CONFIG.mail
        message["To"] = receiver
        message["Subject"] = "Kod potwierdzający"
        message.attach(MIMEText(f"""<h1>Twój kod to {code}</h1>
                                    Jeśli nie chciałeś/aś połączyć tego maila z aplikacją dogsonki messenger, zignoruj tego maila""",
                                "html", "utf-8"))
        return message

    @staticmethod
    def create_traceback_message(traceback_message: str):
        message = MIMEMultipart("alternative")
        message["From"] = SMTP_CONFIG.mail
        message["To"] = "dogsonkrul@gmail.com"
        message["Subject"] = "Dogsonki error"
        message.attach(MIMEText(traceback_message, "html", "utf-8"))
        return message

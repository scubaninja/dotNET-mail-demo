import smtplib
from email.mime.multipart import MIMEMultipart
from email.mime.text import MIMEText
from abc import ABC, abstractmethod
from typing import Optional

from core.config import get_config
from models.message import Message

class EmailSender(ABC):
    """Base abstract class for email senders"""
    
    @abstractmethod
    async def send_async(self, message: Message) -> bool:
        """Send an email asynchronously"""
        pass

class SmtpEmailSender(EmailSender):
    """SMTP email sender implementation"""
    
    def __init__(self, config=None):
        self.config = config or get_config()
        self.smtp_host = self.config.get("SMTP_HOST")
        self.smtp_user = self.config.get("SMTP_USER")
        self.smtp_password = self.config.get("SMTP_PASSWORD")
    
    async def send_async(self, message: Message) -> bool:
        """Send an email asynchronously via SMTP"""
        if message is None:
            raise ValueError("Message cannot be null")
            
        return await self._send_mail_async(message)
    
    async def _send_mail_async(self, message: Message) -> bool:
        """Implementation of SMTP sending"""
        try:
            # Create message container
            msg = MIMEMultipart('alternative')
            msg['Subject'] = message.subject
            msg['From'] = message.send_from
            msg['To'] = message.send_to
            
            # Attach parts
            if message.text:
                msg.attach(MIMEText(message.text, 'plain'))
            msg.attach(MIMEText(message.html, 'html'))
            
            # Send the message
            with smtplib.SMTP(self.smtp_host) as server:
                if self.smtp_user and self.smtp_password:
                    server.login(self.smtp_user, self.smtp_password)
                server.sendmail(message.send_from, message.send_to, msg.as_string())
            
            return True
        except Exception as e:
            # In a real app, log the error
            print(f"Error sending email: {e}")
            return False

class TestSmtpEmailSender(SmtpEmailSender):
    """Test implementation that doesn't actually send emails"""
    
    def __init__(self, config=None):
        super().__init__(config)
        self.send_called = False
    
    async def _send_mail_async(self, message: Message) -> bool:
        """Override to prevent actual sending"""
        self.send_called = True
        return True

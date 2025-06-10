from datetime import datetime
from typing import Optional
from sqlalchemy import Column, Integer, String, Text, DateTime
from pydantic import BaseModel

from core.database import Base

class MessageDB(Base):
    """SQLAlchemy model for messages table"""
    __tablename__ = "messages"
    __table_args__ = {"schema": "mail"}
    
    id = Column(Integer, primary_key=True, index=True)
    source = Column(String, default="broadcast", nullable=False)
    slug = Column(String, nullable=True)
    status = Column(String, default="pending", nullable=False)
    send_to = Column(String, nullable=False)
    send_from = Column(String, nullable=False)
    subject = Column(String, nullable=False)
    html = Column(Text, nullable=False)
    send_at = Column(DateTime, nullable=True)
    sent_at = Column(DateTime, nullable=True)
    created_at = Column(DateTime, default=datetime.utcnow, nullable=False)

class Message(BaseModel):
    """Pydantic model for message data transfer"""
    id: Optional[int] = None
    source: str = "broadcast"
    slug: Optional[str] = None
    status: str = "pending"
    send_to: str
    send_from: str
    subject: str
    html: str
    send_at: Optional[datetime] = None
    sent_at: Optional[datetime] = None
    created_at: Optional[datetime] = None
    
    class Config:
        orm_mode = True

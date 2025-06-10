from datetime import datetime
from typing import Optional
from sqlalchemy import Column, Integer, String, Text, DateTime, ForeignKey
from sqlalchemy.orm import relationship
from pydantic import BaseModel

from core.database import Base
from models.message import MarkdownEmail

class InvalidDataException(Exception):
    """Exception for invalid email data"""
    pass

class EmailDB(Base):
    """SQLAlchemy model for emails table"""
    __tablename__ = "emails"
    __table_args__ = {"schema": "mail"}
    
    id = Column(Integer, primary_key=True, index=True)
    sequence_id = Column(Integer, ForeignKey("mail.sequences.id"), nullable=True)
    slug = Column(String, unique=True, nullable=False)
    subject = Column(String, nullable=False)
    preview = Column(String, nullable=True)
    delay_hours = Column(Integer, default=0, nullable=False)
    html = Column(Text, nullable=True)
    created_at = Column(DateTime, default=datetime.utcnow, nullable=False)
    updated_at = Column(DateTime, default=datetime.utcnow, onupdate=datetime.utcnow, nullable=False)
    
    # Relationship
    sequence = relationship("SequenceDB", back_populates="emails")
    broadcasts = relationship("BroadcastDB", back_populates="email")

class Email(BaseModel):
    """Pydantic model for email data transfer"""
    id: Optional[int] = None
    sequence_id: Optional[int] = None
    slug: str
    subject: str
    preview: Optional[str] = None
    delay_hours: int = 0
    html: str
    created_at: Optional[datetime] = None
    updated_at: Optional[datetime] = None
    
    class Config:
        orm_mode = True
        
    @classmethod
    def from_markdown_email(cls, markdown_email: MarkdownEmail) -> "Email":
        """Create an Email from a MarkdownEmail"""
        if not markdown_email.data:
            raise InvalidDataException("Markdown document should contain metadata with Subject and Summary")
        if not markdown_email.html:
            raise InvalidDataException("There should be HTML generated from the markdown content")
            
        return cls(
            slug=markdown_email.data.slug,
            subject=markdown_email.data.subject,
            preview=markdown_email.data.summary,
            html=markdown_email.html,
            delay_hours=0
        )

from datetime import datetime
from typing import Optional, List, Dict, Any
from sqlalchemy import Column, Integer, String, Text, DateTime, ForeignKey, func, select
from sqlalchemy.orm import relationship
from pydantic import BaseModel

from core.database import Base
from models.message import MarkdownEmail
from models.email import Email

class BroadcastDB(Base):
    """SQLAlchemy model for broadcasts table"""
    __tablename__ = "broadcasts"
    __table_args__ = {"schema": "mail"}
    
    id = Column(Integer, primary_key=True, index=True)
    email_id = Column(Integer, ForeignKey("mail.emails.id"), nullable=False)
    slug = Column(String, unique=True, nullable=False)
    status = Column(String, default="pending", nullable=False)
    name = Column(String, nullable=False)
    send_to_tag = Column(String, nullable=True)
    reply_to = Column(String, default="noreply@tailwindtraders.dev", nullable=False)
    created_at = Column(DateTime, default=datetime.utcnow, nullable=False)
    processed_at = Column(DateTime, nullable=True)
    
    # Relationship
    email = relationship("EmailDB", back_populates="broadcasts")

class Broadcast(BaseModel):
    """Pydantic model for broadcast data transfer"""
    id: Optional[int] = None
    email_id: Optional[int] = None
    slug: str
    status: str = "pending"
    name: str
    send_to_tag: Optional[str] = None
    reply_to: str = "noreply@tailwindtraders.dev"
    created_at: Optional[datetime] = None
    processed_at: Optional[datetime] = None
    
    class Config:
        orm_mode = True
    
    @classmethod
    def from_markdown_email(cls, markdown_email: MarkdownEmail) -> "Broadcast":
        """Create a Broadcast from a MarkdownEmail"""
        if not markdown_email:
            raise ValueError("MarkdownEmail cannot be null")
            
        return cls(
            slug=markdown_email.data.slug,
            name=markdown_email.data.subject,
            send_to_tag=markdown_email.data.send_to_tag
        )
    
    @classmethod
    def from_markdown(cls, markdown: str) -> "Broadcast":
        """Create a Broadcast from markdown string"""
        if not markdown:
            raise ValueError("Markdown string cannot be empty")
            
        markdown_email = MarkdownEmail.from_string(markdown)
        return cls.from_markdown_email(markdown_email)
    
    def contact_count(self, db) -> int:
        """Count contacts that would receive this broadcast"""
        from models.contact import ContactDB
        from models.tag import TagDB, tagged
        
        if not db:
            raise ValueError("Database connection cannot be null")
            
        if self.send_to_tag == "*":
            # Count all subscribed contacts
            query = select(func.count()).select_from(ContactDB).where(ContactDB.subscribed == True)
            return db.execute(query).scalar() or 0
        else:
            # Count contacts with specific tag
            query = select(func.count()).select_from(ContactDB).join(
                tagged, tagged.c.contact_id == ContactDB.id
            ).join(
                TagDB, TagDB.id == tagged.c.tag_id
            ).where(
                TagDB.slug == self.send_to_tag,
                ContactDB.subscribed == True
            )
            return db.execute(query).scalar() or 0

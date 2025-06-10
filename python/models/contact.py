from datetime import datetime
from typing import Optional, List
from sqlalchemy import Column, Integer, String, Boolean, DateTime, ForeignKey, Table
from sqlalchemy.orm import relationship
from pydantic import BaseModel, EmailStr
from uuid import uuid4

from core.database import Base

# Association table for contacts and tags
tagged = Table(
    'tagged',
    Base.metadata,
    Column('contact_id', Integer, ForeignKey('contacts.id')),
    Column('tag_id', Integer, ForeignKey('tags.id'))
)

class ContactDB(Base):
    """SQLAlchemy model for contacts table"""
    __tablename__ = "contacts"
    __table_args__ = {"schema": "mail"}
    
    id = Column(Integer, primary_key=True, index=True)
    email = Column(String, unique=True, nullable=False)
    key = Column(String, default=lambda: str(uuid4()), nullable=False)
    subscribed = Column(Boolean, default=True, nullable=False)
    name = Column(String, nullable=True)
    created_at = Column(DateTime, default=datetime.utcnow, nullable=False)
    updated_at = Column(DateTime, default=datetime.utcnow, onupdate=datetime.utcnow, nullable=False)
    
    # Relationships
    tags = relationship("TagDB", secondary=tagged, back_populates="contacts")
    
class Contact(BaseModel):
    """Pydantic model for contact data transfer"""
    id: Optional[int] = None
    email: EmailStr
    key: Optional[str] = None
    subscribed: bool = True
    name: Optional[str] = None
    created_at: Optional[datetime] = None
    updated_at: Optional[datetime] = None
    
    class Config:
        orm_mode = True

class SignUpRequest(BaseModel):
    """Request model for user signup"""
    email: EmailStr
    name: Optional[str] = None

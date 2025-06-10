from datetime import datetime
from typing import Optional, List
from sqlalchemy import Column, Integer, String, Text, DateTime
from sqlalchemy.orm import relationship
from pydantic import BaseModel

from core.database import Base
from models.contact import tagged

class TagDB(Base):
    """SQLAlchemy model for tags table"""
    __tablename__ = "tags"
    __table_args__ = {"schema": "mail"}
    
    id = Column(Integer, primary_key=True, index=True)
    slug = Column(String, unique=True, nullable=False)
    name = Column(String, nullable=True)
    description = Column(Text, nullable=True)
    created_at = Column(DateTime, default=datetime.utcnow, nullable=False)
    updated_at = Column(DateTime, default=datetime.utcnow, onupdate=datetime.utcnow, nullable=False)
    
    # Relationships
    contacts = relationship("ContactDB", secondary=tagged, back_populates="tags")

class Tag(BaseModel):
    """Pydantic model for tag data transfer"""
    id: Optional[int] = None
    slug: str
    name: Optional[str] = None
    description: Optional[str] = None
    created_at: Optional[datetime] = None
    updated_at: Optional[datetime] = None
    
    class Config:
        orm_mode = True

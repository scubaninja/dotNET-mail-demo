from typing import List
from sqlalchemy.orm import Session
from sqlalchemy import select

from models.contact import ContactDB
from models.tag import TagDB

class CommandResult:
    """Result of a command execution"""
    def __init__(self, inserted=0, updated=0, data=None):
        self.inserted = inserted
        self.updated = updated
        self.data = data or {}

class ContactOptOutCommand:
    """Command to opt a contact out of emails"""
    
    def __init__(self, key: str):
        self.key = key
    
    def execute(self, db: Session) -> CommandResult:
        """Execute the command to opt out a contact"""
        if not self.key:
            return CommandResult(updated=0)
            
        # Find the contact by key
        contact = db.execute(
            select(ContactDB).where(ContactDB.key == self.key)
        ).scalar_one_or_none()
        
        if not contact:
            return CommandResult(updated=0)
            
        # Update subscription status
        contact.subscribed = False
        db.commit()
        
        return CommandResult(updated=1)

class LinkClickedCommand:
    """Command to track link clicks"""
    
    def __init__(self, key: str):
        self.key = key
    
    def execute(self, db: Session = None) -> CommandResult:
        """Execute the command to track a link click"""
        # This is a placeholder since the original doesn't implement it
        return CommandResult(updated=1, data={"message": "Link click recorded"})

class BulkTagCommand:
    """Command to tag multiple contacts"""
    
    def __init__(self, tag: str = None, emails: List[str] = None):
        self.tag = tag
        self.emails = emails or []
    
    def execute(self, db: Session) -> CommandResult:
        """Execute the command to tag contacts"""
        if not self.tag or not self.emails:
            return CommandResult()
            
        # Get or create the tag
        tag = db.execute(
            select(TagDB).where(TagDB.slug == self.tag)
        ).scalar_one_or_none()
        
        if not tag:
            tag = TagDB(slug=self.tag, name=self.tag)
            db.add(tag)
            db.flush()
            
        # Get contacts and add the tag
        updated = 0
        for email in self.emails:
            contact = db.execute(
                select(ContactDB).where(ContactDB.email == email)
            ).scalar_one_or_none()
            
            if contact:
                # Check if tag is already applied
                if tag not in contact.tags:
                    contact.tags.append(tag)
                    updated += 1
        
        db.commit()
        return CommandResult(updated=updated)

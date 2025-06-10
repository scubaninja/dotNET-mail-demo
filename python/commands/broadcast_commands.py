from typing import Dict, Any, Optional
from sqlalchemy.orm import Session
from sqlalchemy import select

from models.message import MarkdownEmail
from models.email import Email, EmailDB
from models.broadcast import Broadcast, BroadcastDB
from models.contact import ContactDB

class CommandResult:
    """Result of a command execution"""
    def __init__(self, inserted=0, updated=0, data=None):
        self.inserted = inserted
        self.updated = updated
        self.data = data or {}

class CreateBroadcast:
    """Command to create a broadcast from a markdown email"""
    
    def __init__(self, markdown_email: MarkdownEmail):
        self.markdown_email = markdown_email
    
    def execute(self, db: Session) -> CommandResult:
        """Execute the command to create a broadcast"""
        # Create email record
        email = Email.from_markdown_email(self.markdown_email)
        email_db = EmailDB(
            slug=email.slug,
            subject=email.subject,
            preview=email.preview,
            delay_hours=email.delay_hours,
            html=email.html
        )
        db.add(email_db)
        db.flush()  # Get the ID without committing
        
        # Create broadcast record
        broadcast = Broadcast.from_markdown_email(self.markdown_email)
        broadcast_db = BroadcastDB(
            email_id=email_db.id,
            slug=broadcast.slug,
            name=broadcast.name,
            send_to_tag=broadcast.send_to_tag,
            status=broadcast.status
        )
        db.add(broadcast_db)
        db.flush()
        
        # Queue messages for all contacts
        from models.message_db import MessageDB
        
        # Get contacts that should receive this broadcast
        if broadcast.send_to_tag == "*":
            # All subscribed contacts
            contacts_query = select(ContactDB).where(ContactDB.subscribed == True)
        else:
            # Contacts with specific tag
            from models.tag import TagDB, tagged
            contacts_query = select(ContactDB).join(
                tagged, tagged.c.contact_id == ContactDB.id
            ).join(
                TagDB, TagDB.id == tagged.c.tag_id
            ).where(
                TagDB.slug == broadcast.send_to_tag,
                ContactDB.subscribed == True
            )
        
        contacts = db.execute(contacts_query).scalars().all()
        
        # Create message records
        messages = []
        for contact in contacts:
            message = MessageDB(
                source="broadcast",
                slug=broadcast.slug,
                status="pending",
                send_to=contact.email,
                send_from=broadcast.reply_to,
                subject=email.subject,
                html=email.html
            )
            db.add(message)
            messages.append(message)
        
        db.commit()
        
        # Return result with counts and IDs
        return CommandResult(
            inserted=len(messages),
            data={"broadcast_id": broadcast_db.id, "email_id": email_db.id}
        )

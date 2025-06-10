import pytest
from sqlalchemy import select

from models.message import MarkdownEmail
from models.contact import ContactDB
from models.tag import TagDB
from commands.broadcast_commands import CreateBroadcast
from commands.contact_commands import ContactOptOutCommand, LinkClickedCommand, BulkTagCommand

def test_create_broadcast_command(test_db, sample_markdown):
    """Test creating a broadcast from markdown"""
    # Create test contacts
    contacts = [
        ContactDB(email="user1@example.com", subscribed=True),
        ContactDB(email="user2@example.com", subscribed=True),
        ContactDB(email="user3@example.com", subscribed=False)  # Not subscribed
    ]
    test_db.add_all(contacts)
    test_db.commit()
    
    # Create the broadcast
    markdown_email = MarkdownEmail.from_string(sample_markdown)
    command = CreateBroadcast(markdown_email)
    result = command.execute(test_db)
    
    # Check results
    assert result.inserted == 2  # Only subscribed contacts get messages
    assert "broadcast_id" in result.data
    assert "email_id" in result.data
    
    # Verify database records
    from models.email import EmailDB
    from models.broadcast import BroadcastDB
    from models.message_db import MessageDB
    
    # Check email was created
    email = test_db.execute(select(EmailDB)).scalar_one()
    assert email.subject == "Test Email"
    assert email.slug == "test-email"
    
    # Check broadcast was created
    broadcast = test_db.execute(select(BroadcastDB)).scalar_one()
    assert broadcast.email_id == email.id
    assert broadcast.slug == "test-email"
    
    # Check messages were created
    messages = test_db.execute(select(MessageDB)).scalars().all()
    assert len(messages) == 2
    assert messages[0].send_to in ["user1@example.com", "user2@example.com"]
    assert messages[0].subject == "Test Email"

def test_contact_opt_out_command(test_db):
    """Test opting out a contact"""
    # Create a test contact
    contact = ContactDB(
        email="user@example.com", 
        subscribed=True,
        key="test-key"
    )
    test_db.add(contact)
    test_db.commit()
    
    # Execute the command
    command = ContactOptOutCommand("test-key")
    result = command.execute(test_db)
    
    # Check result
    assert result.updated == 1
    
    # Verify database record
    contact = test_db.execute(
        select(ContactDB).where(ContactDB.key == "test-key")
    ).scalar_one()
    assert contact.subscribed is False

def test_contact_opt_out_invalid_key(test_db):
    """Test opting out a contact with invalid key"""
    command = ContactOptOutCommand("invalid-key")
    result = command.execute(test_db)
    
    assert result.updated == 0

def test_link_clicked_command():
    """Test link clicked command"""
    command = LinkClickedCommand("test-key")
    result = command.execute()
    
    assert result.updated == 1
    assert "message" in result.data

def test_bulk_tag_command(test_db):
    """Test bulk tagging contacts"""
    # Create test contacts
    contacts = [
        ContactDB(email="user1@example.com"),
        ContactDB(email="user2@example.com"),
        ContactDB(email="user3@example.com")
    ]
    test_db.add_all(contacts)
    test_db.commit()
    
    # Execute the command
    command = BulkTagCommand(
        tag="test-tag",
        emails=["user1@example.com", "user2@example.com", "unknown@example.com"]
    )
    result = command.execute(test_db)
    
    # Check result
    assert result.updated == 2  # Only 2 existing contacts should be updated
    
    # Verify database records
    tag = test_db.execute(
        select(TagDB).where(TagDB.slug == "test-tag")
    ).scalar_one()
    assert tag is not None
    
    # Check contacts have the tag
    contact1 = test_db.execute(
        select(ContactDB).where(ContactDB.email == "user1@example.com")
    ).scalar_one()
    assert tag in contact1.tags
    
    contact2 = test_db.execute(
        select(ContactDB).where(ContactDB.email == "user2@example.com")
    ).scalar_one()
    assert tag in contact2.tags
    
    contact3 = test_db.execute(
        select(ContactDB).where(ContactDB.email == "user3@example.com")
    ).scalar_one()
    assert tag not in contact3.tags

def test_bulk_tag_empty_inputs(test_db):
    """Test bulk tagging with empty inputs"""
    command1 = BulkTagCommand(tag="", emails=["user1@example.com"])
    result1 = command1.execute(test_db)
    assert result1.updated == 0
    
    command2 = BulkTagCommand(tag="test-tag", emails=[])
    result2 = command2.execute(test_db)
    assert result2.updated == 0

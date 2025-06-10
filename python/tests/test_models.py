import pytest
from datetime import datetime

from models.message import MarkdownEmail
from models.email import Email, InvalidDataException
from models.broadcast import Broadcast
from models.contact import ContactDB
from models.tag import TagDB

def test_markdown_email_from_string(sample_markdown):
    """Test creating a MarkdownEmail from a string"""
    email = MarkdownEmail.from_string(sample_markdown)
    
    assert email is not None
    assert email.data is not None
    assert email.html is not None
    assert email.data.subject == "Test Email"
    assert email.data.summary == "This is a test email"
    assert "<h1>Hello World</h1>" in email.html

def test_markdown_email_is_valid(sample_markdown):
    """Test MarkdownEmail validation"""
    email = MarkdownEmail.from_string(sample_markdown)
    assert email.is_valid() is True
    
    # Test invalid case
    invalid_md = """---
Title: Just a title without required fields
---

# Content without required fields"""
    email = MarkdownEmail.from_string(invalid_md)
    assert email.is_valid() is False

def test_markdown_email_default_values():
    """Test default values in MarkdownEmail"""
    md = """---
Subject: Test Email
Summary: This is a test email
---

Content"""
    email = MarkdownEmail.from_string(md)
    
    assert email.data is not None
    assert email.data.slug == "test-email"
    assert email.data.send_to_tag == "*"

def test_markdown_email_custom_values():
    """Test custom values in MarkdownEmail"""
    md = """---
Subject: Test Email
Summary: This is a test email
Slug: custom-slug
SendToTag: premium-users
---

Content"""
    email = MarkdownEmail.from_string(md)
    
    assert email.data is not None
    assert email.data.slug == "custom-slug"
    assert email.data.send_to_tag == "premium-users"

def test_email_from_markdown_email(sample_markdown):
    """Test creating an Email from a MarkdownEmail"""
    markdown_email = MarkdownEmail.from_string(sample_markdown)
    email = Email.from_markdown_email(markdown_email)
    
    assert email.slug == "test-email"
    assert email.subject == "Test Email"
    assert email.preview == "This is a test email"
    assert "<h1>Hello World</h1>" in email.html
    assert email.delay_hours == 0

def test_email_null_data():
    """Test Email creation with null data raises exception"""
    markdown_email = MarkdownEmail()
    
    with pytest.raises(InvalidDataException):
        Email.from_markdown_email(markdown_email)

def test_email_null_html():
    """Test Email creation with null HTML raises exception"""
    from pydantic import BaseModel
    
    class MockData(BaseModel):
        subject: str = "Test Subject"
        summary: str = "Test Summary"
        slug: str = "test-slug"
    
    markdown_email = MarkdownEmail()
    markdown_email.data = MockData()
    
    with pytest.raises(InvalidDataException):
        Email.from_markdown_email(markdown_email)

def test_broadcast_from_markdown_email(sample_markdown):
    """Test creating a Broadcast from a MarkdownEmail"""
    markdown_email = MarkdownEmail.from_string(sample_markdown)
    broadcast = Broadcast.from_markdown_email(markdown_email)
    
    assert broadcast.name == "Test Email"
    assert broadcast.slug == "test-email"
    assert broadcast.send_to_tag == "*"
    assert broadcast.status == "pending"
    assert broadcast.id is None
    assert broadcast.email_id is None

def test_broadcast_from_markdown_null_input():
    """Test creating a Broadcast from null input raises exception"""
    with pytest.raises(ValueError):
        Broadcast.from_markdown_email(None)

def test_broadcast_from_markdown_empty_string():
    """Test creating a Broadcast from empty string raises exception"""
    with pytest.raises(ValueError):
        Broadcast.from_markdown("")

def test_broadcast_contact_count(test_db, sample_markdown, sample_markdown_with_tag):
    """Test counting contacts for a broadcast"""
    # Create some test contacts
    contacts = [
        ContactDB(email="user1@example.com", subscribed=True),
        ContactDB(email="user2@example.com", subscribed=True),
        ContactDB(email="user3@example.com", subscribed=True),
        ContactDB(email="user4@example.com", subscribed=False)  # Not subscribed
    ]
    test_db.add_all(contacts)
    test_db.commit()
    
    # Create a tag and assign to some contacts
    tag = TagDB(slug="premium-users", name="Premium Users")
    test_db.add(tag)
    test_db.commit()
    
    # Tag first two contacts
    contacts[0].tags.append(tag)
    contacts[1].tags.append(tag)
    test_db.commit()
    
    # Test with all contacts
    broadcast_all = Broadcast.from_markdown(sample_markdown)
    count_all = broadcast_all.contact_count(test_db)
    assert count_all == 3  # 3 subscribed contacts
    
    # Test with tagged contacts
    broadcast_tagged = Broadcast.from_markdown(sample_markdown_with_tag)
    count_tagged = broadcast_tagged.contact_count(test_db)
    assert count_tagged == 2  # 2 contacts with the premium-users tag

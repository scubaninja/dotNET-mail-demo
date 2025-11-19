"""
Pytest configuration and shared fixtures for Tailwind Mail Service tests.

This module provides common test fixtures, database setup/teardown,
and other shared testing utilities.
"""

import pytest
import os
from datetime import datetime, timezone
from unittest.mock import Mock, MagicMock


@pytest.fixture
def db_connection():
    """
    Fixture providing a mock database connection for testing.
    
    Returns:
        Mock: A mock database connection object
    """
    mock_conn = Mock()
    mock_conn.cursor = Mock()
    mock_conn.commit = Mock()
    mock_conn.rollback = Mock()
    mock_conn.close = Mock()
    return mock_conn


@pytest.fixture
def sample_contact():
    """
    Fixture providing a sample contact for testing.
    
    Returns:
        dict: A dictionary representing a contact
    """
    return {
        'id': 1,
        'name': 'John Doe',
        'email': 'john.doe@example.com',
        'subscribed': True,
        'key': 'test-key-123',
        'created_at': datetime.now(timezone.utc)
    }


@pytest.fixture
def sample_email():
    """
    Fixture providing a sample email for testing.
    
    Returns:
        dict: A dictionary representing an email
    """
    return {
        'id': 1,
        'slug': 'welcome-email',
        'subject': 'Welcome to Tailwind Traders',
        'preview': 'Welcome to our service',
        'delay_hours': 0,
        'html': '<h1>Welcome</h1><p>Thank you for signing up!</p>',
        'created_at': datetime.now(timezone.utc)
    }


@pytest.fixture
def sample_broadcast():
    """
    Fixture providing a sample broadcast for testing.
    
    Returns:
        dict: A dictionary representing a broadcast
    """
    return {
        'id': 1,
        'subject': 'Monthly Newsletter',
        'html': '<h1>Newsletter</h1>',
        'segment': 'all',
        'status': 'draft',
        'created_at': datetime.now(timezone.utc)
    }


@pytest.fixture
def mock_email_sender():
    """
    Fixture providing a mock email sender service.
    
    Returns:
        Mock: A mock email sender
    """
    sender = Mock()
    sender.send = Mock(return_value=True)
    sender.send_bulk = Mock(return_value={'sent': 10, 'failed': 0})
    return sender


@pytest.fixture
def mock_db_service():
    """
    Fixture providing a mock database service.
    
    Returns:
        Mock: A mock database service with common methods
    """
    db = Mock()
    db.connect = Mock()
    db.execute = Mock()
    db.fetchone = Mock()
    db.fetchall = Mock()
    db.insert = Mock(return_value=1)
    db.update = Mock(return_value=1)
    db.delete = Mock(return_value=1)
    return db


@pytest.fixture(autouse=True)
def reset_environment():
    """
    Fixture to reset environment variables between tests.
    Automatically used for all tests.
    """
    original_env = os.environ.copy()
    yield
    os.environ.clear()
    os.environ.update(original_env)


@pytest.fixture
def api_client():
    """
    Fixture providing a test API client.
    
    Returns:
        Mock: A mock API client for testing routes
    """
    client = Mock()
    client.get = Mock()
    client.post = Mock()
    client.put = Mock()
    client.delete = Mock()
    return client


# Accessibility testing fixtures
@pytest.fixture
def accessibility_validator():
    """
    Fixture providing accessibility validation utilities.
    
    Returns:
        Mock: A mock accessibility validator
    """
    validator = Mock()
    validator.check_aria_labels = Mock(return_value=True)
    validator.check_contrast = Mock(return_value=True)
    validator.check_keyboard_navigation = Mock(return_value=True)
    validator.validate_html = Mock(return_value={'errors': [], 'warnings': []})
    return validator

import pytest
import asyncio
from unittest.mock import patch, MagicMock

from models.message import Message
from services.email_sender import SmtpEmailSender, TestSmtpEmailSender
from services.ai import Chat

@pytest.fixture
def sample_message():
    """Sample message for testing"""
    return Message(
        send_to="test@example.com",
        send_from="sender@example.com",
        subject="Test Email",
        html="<p>Test content</p>",
        text="Test content"
    )

@pytest.mark.asyncio
async def test_smtp_email_sender_null_message():
    """Test SMTP sender with null message"""
    sender = SmtpEmailSender()
    
    with pytest.raises(ValueError):
        await sender.send_async(None)

@pytest.mark.asyncio
async def test_test_smtp_email_sender(sample_message):
    """Test the test SMTP sender that doesn't actually send"""
    sender = TestSmtpEmailSender()
    
    # Verify it doesn't throw and marks as sent
    result = await sender.send_async(sample_message)
    
    assert result is True
    assert sender.send_called is True

@patch('smtplib.SMTP')
@pytest.mark.asyncio
async def test_smtp_email_sender_success(mock_smtp, sample_message):
    """Test SMTP sender success path"""
    # Mock the SMTP server context manager
    mock_server = MagicMock()
    mock_smtp.return_value.__enter__.return_value = mock_server
    
    # Create sender and send message
    sender = SmtpEmailSender()
    result = await sender.send_async(sample_message)
    
    # Verify results
    assert result is True
    mock_server.sendmail.assert_called_once()
    args = mock_server.sendmail.call_args[0]
    assert args[0] == "sender@example.com"
    assert args[1] == "test@example.com"

@patch('smtplib.SMTP')
@pytest.mark.asyncio
async def test_smtp_email_sender_with_auth(mock_smtp, sample_message):
    """Test SMTP sender with authentication"""
    # Mock config
    class MockConfig:
        def get(self, key):
            if key == "SMTP_HOST":
                return "smtp.example.com"
            elif key == "SMTP_USER":
                return "user"
            elif key == "SMTP_PASSWORD":
                return "password"
            return ""
    
    # Mock the SMTP server
    mock_server = MagicMock()
    mock_smtp.return_value.__enter__.return_value = mock_server
    
    # Create sender with mock config and send
    sender = SmtpEmailSender(MockConfig())
    result = await sender.send_async(sample_message)
    
    # Verify authentication was used
    assert result is True
    mock_server.login.assert_called_once_with("user", "password")
    mock_server.sendmail.assert_called_once()

@patch('smtplib.SMTP')
@pytest.mark.asyncio
async def test_smtp_email_sender_failure(mock_smtp, sample_message):
    """Test SMTP sender handling errors"""
    # Make the SMTP server raise an exception
    mock_smtp.return_value.__enter__.side_effect = Exception("SMTP error")
    
    # Create sender and send
    sender = SmtpEmailSender()
    result = await sender.send_async(sample_message)
    
    # Verify error handling
    assert result is False

@patch('httpx.AsyncClient.post')
@pytest.mark.asyncio
async def test_ai_chat_service(mock_post):
    """Test AI chat service"""
    # Mock the response
    mock_response = MagicMock()
    mock_response.raise_for_status = MagicMock()
    mock_response.json.return_value = {
        "choices": [
            {"message": {"content": "This is a test response"}}
        ]
    }
    mock_post.return_value = mock_response
    
    # Create chat service and send prompt
    chat = Chat()
    response = await chat.prompt("Test prompt")
    
    # Verify response
    assert response == "This is a test response"
    mock_post.assert_called_once()

@pytest.mark.asyncio
async def test_ai_chat_empty_prompt():
    """Test AI chat with empty prompt"""
    chat = Chat()
    
    with pytest.raises(ValueError):
        await chat.prompt("")

@pytest.mark.asyncio
async def test_ai_chat_no_config():
    """Test AI chat with no configuration"""
    # Mock config
    class MockConfig:
        def get(self, key):
            return ""
    
    chat = Chat(MockConfig())
    response = await chat.prompt("Test prompt")
    
    assert "AI configuration not set up" in response

@patch('httpx.AsyncClient.post')
@pytest.mark.asyncio
async def test_ai_chat_service_error(mock_post):
    """Test AI chat service error handling"""
    # Make the HTTP request raise an exception
    mock_post.side_effect = Exception("API error")
    
    # Create chat service with mock config that has API details
    class MockConfig:
        def get(self, key):
            if key == "AZURE_OPENAI_ENDPOINT":
                return "https://example.com"
            elif key == "AZURE_OPENAI_API_KEY":
                return "fake-key"
            return ""
    
    chat = Chat(MockConfig())
    response = await chat.prompt("Test prompt")
    
    # Verify error handling
    assert "Error communicating with AI service" in response

import pytest
from sqlalchemy import create_engine
from sqlalchemy.orm import sessionmaker
from sqlalchemy.pool import StaticPool
from fastapi.testclient import TestClient

from core.database import Base, get_db
from api.main import app

# Create in-memory SQLite database for testing
SQLALCHEMY_DATABASE_URL = "sqlite:///:memory:"

@pytest.fixture
def test_db():
    """Create a fresh database for each test"""
    engine = create_engine(
        SQLALCHEMY_DATABASE_URL,
        connect_args={"check_same_thread": False},
        poolclass=StaticPool,
    )
    TestingSessionLocal = sessionmaker(autocommit=False, autoflush=False, bind=engine)
    
    # Create tables
    Base.metadata.create_all(bind=engine)
    
    # Use our test database instead of the real one
    def override_get_db():
        try:
            db = TestingSessionLocal()
            yield db
        finally:
            db.close()
    
    # Override the dependency
    app.dependency_overrides[get_db] = override_get_db
    
    yield TestingSessionLocal()
    
    # Clean up
    Base.metadata.drop_all(bind=engine)

@pytest.fixture
def client(test_db):
    """Test client for the API"""
    return TestClient(app)

@pytest.fixture
def sample_markdown():
    """Sample markdown for testing"""
    return """---
Subject: Test Email
Summary: This is a test email
---

# Hello World

This is a test email with markdown content."""

@pytest.fixture
def sample_markdown_with_tag():
    """Sample markdown with a tag for testing"""
    return """---
Subject: Test Email
Summary: This is a test email
SendToTag: premium-users
---

# Hello World

This is a test email with markdown content."""

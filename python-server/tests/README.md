# Tailwind Traders Mail Service - Test Suite

## Overview

This directory contains comprehensive unit, integration, and accessibility tests for the Python implementation of the Tailwind Traders Mail Service.

## Test Structure

```
tests/
├── __init__.py                  # Test suite initialization
├── conftest.py                  # Shared fixtures and configuration
├── models/                      # Model tests
│   ├── test_contact.py         # Contact model tests
│   ├── test_email.py           # Email model tests
│   └── ...
├── commands/                    # Command handler tests
│   ├── test_contact_signup.py  # Signup command tests
│   └── ...
├── services/                    # Service layer tests
│   ├── test_email_sender.py    # Email sending service tests
│   └── ...
├── api/                         # API endpoint tests
│   ├── test_public_routes.py   # Public API tests
│   └── ...
└── integration/                 # Integration and E2E tests
    ├── test_accessibility.py    # Accessibility compliance tests
    └── ...
```

## Running Tests

### Prerequisites

Install test dependencies:

```bash
pip install -r requirements-test.txt
```

### Run All Tests

```bash
pytest
```

### Run Specific Test Categories

```bash
# Unit tests only
pytest -m unit

# Integration tests
pytest -m integration

# Accessibility tests
pytest -m accessibility

# Model tests
pytest -m models

# API tests
pytest -m api

# Fast tests only (exclude slow tests)
pytest -m "not slow"
```

### Run Tests with Coverage

```bash
# Run with coverage report
pytest --cov=app --cov-report=html

# View coverage report
open htmlcov/index.html
```

### Run Tests in Parallel

```bash
# Run tests across multiple cores
pytest -n auto
```

## Test Coverage Goals

- **Minimum Coverage**: 80%
- **Target Coverage**: 90%+
- **Critical Paths**: 100%

### Coverage by Component

| Component | Target Coverage |
|-----------|----------------|
| Models | 95% |
| Commands | 95% |
| Services | 90% |
| API Routes | 90% |
| Utilities | 85% |

## Writing Tests

### Test Naming Convention

- Test files: `test_*.py`
- Test classes: `Test*`
- Test functions: `test_*`

### Example Test

```python
import pytest
from app.models.contact import Contact

class TestContact:
    def test_contact_creation(self):
        """Test creating a contact with valid data"""
        contact = Contact(name="John Doe", email="john@example.com")
        assert contact.name == "John Doe"
        assert contact.email == "john@example.com"
    
    def test_contact_email_validation(self):
        """Test contact creation with invalid email"""
        with pytest.raises(ValueError, match="Invalid email"):
            Contact(name="John Doe", email="invalid-email")
```

### Using Fixtures

Fixtures are defined in `conftest.py` and available to all tests:

```python
def test_contact_save(sample_contact, mock_db_service):
    """Test saving a contact using fixtures"""
    contact = Contact(**sample_contact)
    result = contact.save(mock_db_service)
    assert result > 0
```

### Marking Tests

Use markers to categorize tests:

```python
@pytest.mark.unit
@pytest.mark.models
def test_contact_validation():
    """Test marked as both unit and models"""
    pass

@pytest.mark.slow
def test_full_integration():
    """Test marked as slow"""
    pass
```

## Accessibility Testing

### WCAG 2.1 Level AA Compliance

All UI components must meet WCAG 2.1 Level AA standards. Tests verify:

- ✓ Keyboard navigation
- ✓ Screen reader compatibility  
- ✓ Color contrast (4.5:1 for text, 3:1 for large text)
- ✓ ARIA labels and roles
- ✓ Form accessibility
- ✓ Focus management
- ✓ Error message accessibility

### Running Accessibility Tests

```bash
# Run all accessibility tests
pytest -m accessibility

# Run automated axe-core tests
pytest tests/integration/test_accessibility.py::TestAccessibilityAxeCore
```

### Manual Accessibility Testing

In addition to automated tests, perform manual testing:

1. **Keyboard Navigation**: Navigate entire app using only keyboard
2. **Screen Reader**: Test with NVDA (Windows) or VoiceOver (Mac)
3. **Color Contrast**: Verify with browser DevTools or online tools
4. **Zoom**: Test at 200% zoom level
5. **Mobile**: Test on mobile devices with screen readers

## Security Testing

Tests include security validation:

- Input sanitization
- SQL injection prevention
- XSS prevention
- CSRF protection
- Authentication/authorization

Run security tests:

```bash
pytest -m security

# Run bandit security linter
bandit -r app/

# Check dependencies for vulnerabilities
safety check
```

## Continuous Integration

Tests run automatically on:

- Pull requests
- Pushes to main branch
- Nightly builds

### CI Requirements

All tests must pass with:
- ✓ 80%+ code coverage
- ✓ No accessibility violations
- ✓ No security issues
- ✓ All linters passing

## Test Data

Test data is generated using:

- **Factory Boy**: For creating model instances
- **Faker**: For generating realistic fake data
- **Fixtures**: For reusable test data

## Mocking

Use mocking for external dependencies:

```python
from unittest.mock import Mock, patch

def test_email_sending(mock_email_sender):
    """Test with mocked email sender"""
    mock_email_sender.send.return_value = True
    # ... test code
```

## Database Testing

Integration tests use a test database:

```python
@pytest.fixture
def test_db():
    """Create and cleanup test database"""
    db = create_test_database()
    yield db
    db.cleanup()
```

## Performance Testing

Mark slow tests to exclude from quick runs:

```python
@pytest.mark.slow
def test_bulk_email_sending():
    """Test sending 10,000 emails"""
    pass
```

## Debugging Tests

### Run with verbose output

```bash
pytest -vv
```

### Run specific test

```bash
pytest tests/models/test_contact.py::TestContact::test_contact_creation
```

### Drop into debugger on failure

```bash
pytest --pdb
```

### Show print statements

```bash
pytest -s
```

## Contributing

When adding new features:

1. Write tests first (TDD approach)
2. Ensure tests pass locally
3. Maintain or improve code coverage
4. Add accessibility tests for UI changes
5. Update this README if adding new test categories

## Resources

- [Pytest Documentation](https://docs.pytest.org/)
- [WCAG 2.1 Guidelines](https://www.w3.org/WAI/WCAG21/quickref/)
- [axe-core Rules](https://github.com/dequelabs/axe-core/blob/develop/doc/rule-descriptions.md)
- [Python Testing Best Practices](https://docs.python-guide.org/writing/tests/)

## Questions?

For questions about tests, please:
1. Check this README
2. Review existing tests for examples
3. Ask in team chat
4. Create an issue

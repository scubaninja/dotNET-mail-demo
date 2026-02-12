# Testing Guide for Tailwind Traders Mail Service

## Quick Start

### Install Test Dependencies

```bash
pip install -r requirements-test.txt
```

### Run All Tests

```bash
pytest
# or
make test
```

## Test Philosophy

This project follows Test-Driven Development (TDD) principles:

1. **Write tests first** - Define expected behavior before implementation
2. **Red, Green, Refactor** - Write failing test, make it pass, improve code
3. **Test behavior, not implementation** - Focus on what code does, not how
4. **Keep tests independent** - Each test should run in isolation
5. **Make tests readable** - Tests serve as documentation

## Test Organization

### Directory Structure

```
tests/
├── conftest.py              # Shared fixtures and configuration
├── models/                  # Data model tests
│   ├── test_contact.py
│   └── test_email.py
├── commands/                # Business logic/command tests
│   ├── test_contact_signup.py
│   └── test_broadcast.py
├── services/                # Service layer tests
│   └── test_email_sender.py
├── api/                     # API endpoint tests
│   └── test_public_routes.py
└── integration/             # Integration and E2E tests
    ├── test_accessibility.py
    └── test_database.py
```

### Test Types

#### Unit Tests
Test individual components in isolation using mocks.

```python
@pytest.mark.unit
def test_contact_creation():
    contact = Contact(name="John", email="john@example.com")
    assert contact.name == "John"
```

#### Integration Tests
Test multiple components working together.

```python
@pytest.mark.integration
def test_signup_flow(db_connection):
    # Test full signup process including database
    pass
```

#### Accessibility Tests
Verify WCAG 2.1 Level AA compliance.

```python
@pytest.mark.accessibility
def test_form_has_labels():
    # Verify all form inputs have labels
    pass
```

## Writing Good Tests

### Test Naming

Use descriptive names that explain what is being tested:

```python
# Good
def test_contact_signup_with_duplicate_email_returns_error():
    pass

# Bad
def test_signup():
    pass
```

### Test Structure (Arrange, Act, Assert)

```python
def test_send_email():
    # Arrange - Set up test data
    email = {
        'to': 'user@example.com',
        'subject': 'Test',
        'body': 'Test email'
    }
    sender = EmailSender()
    
    # Act - Execute the code being tested
    result = sender.send(email)
    
    # Assert - Verify the outcome
    assert result is True
```

### Using Fixtures

Define reusable test data in `conftest.py`:

```python
# conftest.py
@pytest.fixture
def sample_contact():
    return {
        'name': 'John Doe',
        'email': 'john@example.com'
    }

# test file
def test_something(sample_contact):
    contact = Contact(**sample_contact)
    # use contact in test
```

### Mocking External Dependencies

```python
from unittest.mock import Mock, patch

def test_email_sending_with_mock():
    mock_sender = Mock()
    mock_sender.send.return_value = True
    
    result = send_email(mock_sender, "test@example.com")
    
    assert result is True
    mock_sender.send.assert_called_once()
```

## Test Coverage

### Running Coverage

```bash
# Generate coverage report
pytest --cov=app --cov-report=html

# View report
open htmlcov/index.html
```

### Coverage Goals

- **Minimum**: 80% overall coverage
- **Target**: 90%+ overall coverage
- **Critical paths**: 100% coverage

### What to Cover

✓ **Happy paths** - Normal, expected behavior
✓ **Edge cases** - Boundary conditions
✓ **Error cases** - Invalid input, exceptions
✓ **Security** - Input validation, sanitization
✓ **Accessibility** - WCAG compliance

## Accessibility Testing

### WCAG 2.1 Level AA Requirements

Our tests verify:

1. **Perceivable**
   - Text alternatives (alt text)
   - Time-based media alternatives
   - Adaptable content
   - Distinguishable (color contrast)

2. **Operable**
   - Keyboard accessible
   - Enough time
   - Seizures and physical reactions
   - Navigable

3. **Understandable**
   - Readable
   - Predictable
   - Input assistance

4. **Robust**
   - Compatible with assistive technologies

### Running Accessibility Tests

```bash
pytest -m accessibility
```

### Manual Accessibility Checklist

- [ ] Test with keyboard only (no mouse)
- [ ] Test with screen reader (NVDA/JAWS/VoiceOver)
- [ ] Test at 200% zoom
- [ ] Check color contrast ratios
- [ ] Verify focus indicators are visible
- [ ] Test with browser accessibility tools

## Debugging Tests

### Run Specific Test

```bash
pytest tests/models/test_contact.py::TestContact::test_contact_creation
```

### Drop into Debugger

```bash
pytest --pdb
```

### Show Print Statements

```bash
pytest -s
```

### Verbose Output

```bash
pytest -vv
```

### Run Last Failed Tests

```bash
pytest --lf
```

## Best Practices

### DO ✓

- ✓ Write tests before code (TDD)
- ✓ Keep tests simple and focused
- ✓ Use descriptive test names
- ✓ Test one thing per test
- ✓ Mock external dependencies
- ✓ Clean up test data
- ✓ Run tests frequently
- ✓ Maintain high coverage

### DON'T ✗

- ✗ Test implementation details
- ✗ Write tests that depend on each other
- ✗ Ignore failing tests
- ✗ Skip edge cases
- ✗ Commit code without tests
- ✗ Let coverage drop below 80%
- ✗ Test external libraries
- ✗ Make tests too complex

## Common Testing Patterns

### Testing Exceptions

```python
def test_invalid_email_raises_error():
    with pytest.raises(ValueError, match="Invalid email"):
        Contact(name="Test", email="invalid")
```

### Parametrized Tests

```python
@pytest.mark.parametrize("email,valid", [
    ("test@example.com", True),
    ("invalid", False),
    ("user@domain.co.uk", True),
])
def test_email_validation(email, valid):
    is_valid = validate_email(email)
    assert is_valid == valid
```

### Testing Async Code

```python
@pytest.mark.asyncio
async def test_async_function():
    result = await async_send_email()
    assert result is True
```

## CI/CD Integration

Tests run automatically on:
- Every pull request
- Pushes to main/develop
- Nightly builds

### CI Requirements

All of these must pass:
- ✓ All tests pass
- ✓ Coverage ≥ 80%
- ✓ No accessibility violations
- ✓ No security issues
- ✓ Linters pass (flake8, pylint)
- ✓ Code formatted (black, isort)

## Performance Testing

### Mark Slow Tests

```python
@pytest.mark.slow
def test_bulk_operation():
    # Test that takes a long time
    pass
```

### Run Without Slow Tests

```bash
pytest -m "not slow"
```

## Security Testing

### Input Validation

```python
def test_sql_injection_prevention():
    malicious_input = "'; DROP TABLE users; --"
    with pytest.raises(ValueError):
        query_database(malicious_input)
```

### XSS Prevention

```python
def test_html_sanitization():
    malicious_html = '<script>alert("xss")</script>'
    safe_html = sanitize_html(malicious_html)
    assert '<script>' not in safe_html
```

## Test Data

### Factories

Use Factory Boy for complex test data:

```python
class ContactFactory(factory.Factory):
    class Meta:
        model = Contact
    
    name = factory.Faker('name')
    email = factory.Faker('email')
```

### Faker

Use Faker for realistic fake data:

```python
from faker import Faker
fake = Faker()

def test_with_fake_data():
    name = fake.name()
    email = fake.email()
    # use in test
```

## Troubleshooting

### Tests Pass Locally but Fail in CI

- Check Python version differences
- Verify all dependencies are in requirements
- Check for hardcoded paths
- Look for timezone issues

### Intermittent Test Failures

- Check for race conditions
- Look for tests that depend on order
- Verify random data isn't causing issues
- Check for external dependencies

### Slow Tests

- Use markers to categorize slow tests
- Mock external API calls
- Use in-memory databases for testing
- Run tests in parallel (`pytest -n auto`)

## Resources

- [Pytest Documentation](https://docs.pytest.org/)
- [WCAG 2.1 Quick Reference](https://www.w3.org/WAI/WCAG21/quickref/)
- [Python Testing Best Practices](https://docs.python-guide.org/writing/tests/)
- [Test-Driven Development](https://en.wikipedia.org/wiki/Test-driven_development)

## Getting Help

1. Check this guide
2. Read test examples in the test suite
3. Consult pytest documentation
4. Ask in team chat
5. Create an issue on GitHub

---

Happy Testing! 🧪

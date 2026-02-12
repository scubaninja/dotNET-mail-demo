# Tailwind Traders Mail Service - Python Implementation

## Overview

This is the Python implementation of the Tailwind Traders Mail Service, converted from the original .NET version. The service provides transactional and bulk email sending capabilities similar to MailChimp.

## Features

- 📧 Transactional email sending via API
- 📬 Bulk email campaigns to segments
- 👥 Contact management with subscription handling
- 📊 Activity tracking and analytics
- 🎨 Markdown-based email templates
- ♿ Accessible UI (WCAG 2.1 Level AA compliant)
- 🔒 Secure with input validation and sanitization

## Project Structure

```
python-server/
├── app/                    # Application code (to be implemented)
│   ├── models/            # Data models
│   ├── commands/          # Command handlers
│   ├── services/          # Business logic services
│   ├── api/              # API routes
│   ├── data/             # Database layer
│   └── utils/            # Utility functions
├── tests/                 # Comprehensive test suite
│   ├── models/           # Model tests
│   ├── commands/         # Command tests
│   ├── services/         # Service tests
│   ├── api/              # API tests
│   └── integration/      # Integration tests
├── pytest.ini            # Pytest configuration
├── requirements-test.txt # Test dependencies
├── Makefile             # Development commands
└── README.md            # This file
```

## Getting Started

### Prerequisites

- Python 3.9+
- PostgreSQL 12+
- pip

### Installation

1. Create a virtual environment:
```bash
python -m venv venv
source venv/bin/activate  # On Windows: venv\Scripts\activate
```

2. Install dependencies:
```bash
make install
# or
pip install -r requirements-test.txt
```

3. Set up environment variables:
```bash
cp .env.example .env
# Edit .env with your configuration
```

4. Run database migrations:
```bash
# To be implemented
```

## Development

### Running Tests

```bash
# Run all tests
make test

# Run with coverage
make test-coverage

# Run specific test categories
make test-unit
make test-integration
make test-accessibility
```

### Code Quality

```bash
# Run linters
make lint

# Format code
make format

# Security checks
make security
```

### Development Server

```bash
# Start development server (to be implemented)
python -m app.main
```

## Testing

The project includes comprehensive test coverage:

- **Unit Tests**: Test individual components in isolation
- **Integration Tests**: Test component interactions
- **Accessibility Tests**: Ensure WCAG 2.1 Level AA compliance
- **Security Tests**: Validate input sanitization and security measures

See [tests/README.md](tests/README.md) for detailed testing documentation.

### Test Coverage Goals

- Minimum: 80%
- Target: 90%+
- Critical paths: 100%

## API Documentation

Once the server is running, API documentation is available at:

- Swagger UI: http://localhost:8000/
- OpenAPI JSON: http://localhost:8000/openapi.json

### Public Endpoints

- `GET /about` - API information
- `POST /signup` - Subscribe to mailing list
- `GET /unsubscribe/{key}` - Unsubscribe from mailing list
- `GET /link/clicked/{key}` - Track link clicks

### Admin Endpoints

- Contact management
- Broadcast creation and sending
- Bulk operations
- Analytics and reporting

## Accessibility

This project is committed to accessibility and aims for WCAG 2.1 Level AA compliance:

- ✓ Keyboard navigation support
- ✓ Screen reader compatibility
- ✓ Proper color contrast (4.5:1 for text)
- ✓ ARIA labels and landmarks
- ✓ Form accessibility
- ✓ Focus management

Run accessibility tests:
```bash
make test-accessibility
```

## Database Schema

The application uses PostgreSQL with the following main tables:

- `contacts` - Contact information and subscription status
- `emails` - Email templates
- `broadcasts` - Email campaigns
- `activities` - Contact activity tracking
- `tags` - Contact tagging
- `messages` - Sent message tracking

## Configuration

Configuration is managed through environment variables:

```bash
# Database
DATABASE_URL=postgresql://user:pass@localhost/maildb

# Email sending
SMTP_HOST=smtp.example.com
SMTP_PORT=587
SMTP_USERNAME=user@example.com
SMTP_PASSWORD=password

# Application
DEBUG=False
SECRET_KEY=your-secret-key
```

## CI/CD

The project includes GitHub Actions workflows for:

- Running tests on multiple Python versions
- Code coverage reporting
- Accessibility validation
- Security scanning
- Linting and code quality checks

## Contributing

1. Fork the repository
2. Create a feature branch
3. Write tests for your changes
4. Ensure all tests pass
5. Run linters and formatters
6. Submit a pull request

### Code Standards

- Follow PEP 8 style guide
- Write docstrings for all public functions
- Maintain test coverage above 80%
- Include accessibility considerations
- Add security validations

## License

MIT License - see LICENSE file for details

## Comparison with .NET Version

This Python implementation maintains feature parity with the .NET version while following Python best practices:

| Feature | .NET | Python |
|---------|------|--------|
| Web Framework | ASP.NET Core | FastAPI |
| ORM | Dapper | SQLAlchemy |
| DI Container | Built-in | FastAPI dependencies |
| Testing | xUnit | pytest |
| API Docs | Swagger | OpenAPI/Swagger |

## Support

For issues, questions, or contributions:
- Create an issue on GitHub
- See [CONTRIBUTING.md](CONTRIBUTING.md)
- Contact the team

## Roadmap

- [ ] Complete Python code conversion
- [x] Comprehensive test suite
- [ ] API documentation
- [ ] Docker containerization
- [ ] Kubernetes deployment
- [ ] Performance benchmarks
- [ ] Load testing
- [ ] Monitoring and logging

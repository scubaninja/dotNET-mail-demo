# Tailwind Traders Mail Service - Documentation

Welcome to the documentation for the Tailwind Traders Mail Service.

## Overview

Tailwind Traders Mail Service is a comprehensive email service system that provides:
- Transactional email sending via API
- Bulk email broadcasting to contact lists
- Contact management with tags and segments
- CLI tool for managing broadcasts via markdown
- Background job processing for email delivery
- PostgreSQL database for data storage

## Getting Started

- [Main README](../README.md) - Project overview and quick start
- [Server README](../server/README.md) - .NET API server documentation
- [CLI README](../cli/README.md) - Node.js CLI documentation
- [Jobs README](../jobs/README.md) - Go jobs service documentation
- [Database README](../db/README.md) - Database schema and setup

## Testing Documentation

### Master Test Plan
- **[TEST_PLAN.md](./TEST_PLAN.md)** - Comprehensive testing strategy for the entire system

### Component Test Plans
1. **[Server API Test Plan](./test-plans/SERVER_TEST_PLAN.md)** - Testing the .NET API
   - API endpoint tests
   - Service layer tests
   - Database integration tests
   - Performance and security tests

2. **[CLI Test Plan](./test-plans/CLI_TEST_PLAN.md)** - Testing the Node.js CLI
   - Command tests
   - Markdown parsing tests
   - API client tests
   - Interactive prompt tests

3. **[Jobs Service Test Plan](./test-plans/JOBS_TEST_PLAN.md)** - Testing the Go jobs service
   - Sender and queuer tests
   - Mage target tests
   - Azure integration tests
   - Performance and container tests

4. **[Database Test Plan](./test-plans/DATABASE_TEST_PLAN.md)** - Testing the PostgreSQL database
   - Schema validation tests
   - Data integrity tests
   - Query performance tests
   - Transaction and security tests

5. **[Integration Test Plan](./test-plans/INTEGRATION_TEST_PLAN.md)** - End-to-end system testing
   - Complete workflow scenarios
   - Cross-component integration
   - System health checks
   - Performance and load tests

## Architecture

### System Components

```
┌─────────────┐      ┌─────────────┐      ┌─────────────┐
│             │      │             │      │             │
│  CLI Tool   │─────▶│  Server API │─────▶│  Database   │
│  (Node.js)  │      │   (.NET)    │      │ (PostgreSQL)│
│             │      │             │      │             │
└─────────────┘      └──────┬──────┘      └─────────────┘
                            │
                            │
                     ┌──────▼──────┐      ┌─────────────┐
                     │             │      │             │
                     │ Jobs Service│─────▶│ SMTP/Azure  │
                     │    (Go)     │      │   Email     │
                     │             │      │  Services   │
                     └─────────────┘      └─────────────┘
```

### Data Flow

1. **Broadcast Creation**: CLI → Server API → Database
2. **Message Queueing**: Server API → Database (messages table)
3. **Email Sending**: Jobs Service → Database → Email Provider
4. **Contact Management**: CLI/API → Database
5. **Subscription Flow**: Public API → Database → Email Confirmation

## User Stories

### Completed Stories
- ✅ **Jill queues a broadcast to 10K contacts** - Queue broadcast with one contact opted out, resulting in 10K messages
- ✅ **Jim wants to signup to Jill's list** - Double opt-in flow with confirmation email
- ✅ **Kim gets too much email** - Unsubscribe via link in email

### Planned Stories
- 🔄 **Jill sends a transactional email** - Individual emails for book purchases without unsubscribe links

## Best Practices

### Testing
- Run tests frequently during development
- Maintain test coverage above 70% for critical components
- Use test email services (Mailpit) for local development
- Reset test database between test runs

### Development
- Follow coding standards in `.github/instructions/copilot-instructions.md`
- Use environment variables for configuration
- Never commit secrets or API keys
- Document API changes in Swagger

### Database
- Use migrations for schema changes
- Always test with seed data
- Use transactions for multi-step operations
- Index frequently queried columns

## Contributing

1. Read the coding standards
2. Review relevant test plans
3. Write tests for new features
4. Ensure all tests pass before submitting PR
5. Update documentation as needed

## Troubleshooting

### Database Connection Issues
```bash
# Check PostgreSQL is running
pg_isready

# Verify connection string
echo $DATABASE_URL

# Test connection
psql $DATABASE_URL -c "SELECT 1;"
```

### API Not Starting
```bash
# Check dependencies
cd server
dotnet restore

# Verify environment variables
dotenv list

# Check logs
dotnet run
```

### CLI Errors
```bash
# Reinstall dependencies
cd cli
npm ci

# Check API connectivity
curl $API_ROOT/health

# Verify mail directory exists
ls -la mail/
```

## Additional Resources

- [PostgreSQL Documentation](https://www.postgresql.org/docs/)
- [.NET 8 Documentation](https://docs.microsoft.com/en-us/dotnet/)
- [Node.js Documentation](https://nodejs.org/docs/)
- [Go Documentation](https://go.dev/doc/)
- [Mage Documentation](https://magefile.org/)

## Support

For questions or issues:
1. Check the relevant component README
2. Review the test plans for examples
3. Search existing GitHub issues
4. Create a new issue with detailed information

---

**Last Updated**: 2024  
**Maintained by**: Tailwind Traders Team

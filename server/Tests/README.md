# Mail Service API Tests

This directory contains tests for the Mail Service API, covering:

- Model validation and functionality
- API endpoint behavior
- Email sending services
- Database interactions (mocked)

## Running Tests

To run all tests:

```bash
dotnet test
```

To run a specific test:

```bash
dotnet test --filter "FullyQualifiedName=Tailwind.Mail.Tests.Models.MarkdownEmailTests.FromString_ValidMarkdown_ReturnsValidObject"
```

## Test Structure

The tests are organized by category:

- **Models/** - Tests for domain models like Email, MarkdownEmail, and Broadcast
- **Api/** - Tests for API endpoints and route handlers
- **Services/** - Tests for services like SmtpEmailSender

## Testing Approach

1. **Unit Tests** - Focus on testing individual components in isolation
2. **Mocking** - Dependencies are mocked using Moq
3. **Arrange-Act-Assert** - Tests follow the AAA pattern for clarity

## Adding New Tests

When adding new tests:

1. Follow the naming convention: `MethodName_Scenario_ExpectedResult`
2. Add new test classes in the appropriate category folder
3. Use mocking for external dependencies
4. Focus on testing one thing per test

## Test Data

Test data is typically defined inline in each test to make tests self-contained.
For complex test data, consider adding helper methods or classes in the test project.

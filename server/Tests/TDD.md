# Test-Driven Development (TDD) Guide

## Overview

This repository follows Test-Driven Development practices to ensure code quality, reliability, and maintainability. All tests are written using xUnit and can be found in the `Tests/` directory.

## Running Tests

### Run all tests
```bash
dotnet test
```

### Run tests with verbose output
```bash
dotnet test --verbosity normal
```

### Run tests and generate coverage report
```bash
dotnet test --collect:"XPlat Code Coverage"
```

## TDD Principles

We follow the Red-Green-Refactor cycle:

1. **Red**: Write a failing test first
2. **Green**: Write the minimum code to make the test pass
3. **Refactor**: Clean up the code while keeping tests green

## Test Organization

Tests are organized by component type:

- **Model Tests**: Test domain models and their business logic
  - `ContactTests.cs` - Contact model validation
  - `EmailTests.cs` - Email creation and validation
  - `MarkdownEmailTests.cs` - Markdown parsing and email generation
  - `ActivityTests.cs` - Activity tracking
  - `TagTests.cs` - Tag management
  - `BroadcastTests.cs` - Broadcast creation and validation
  - `MessageTests.cs` - Message lifecycle and validation

- **Command Tests**: Test command execution and business logic
  - `ContactCommandTests.cs` - Contact signup, opt-in, opt-out commands
  - `BulkTagCommandTests.cs` - Bulk tagging operations
  - `LinkClickedCommandTests.cs` - Link tracking

- **Integration Tests**: Test API endpoints and database interactions (to be added)

## Test Coverage

Current test statistics:
- **Total Tests**: 70
- **Passing**: 70
- **Failing**: 0

### Coverage by Component:
- Models: ✅ Comprehensive (56 tests)
- Commands: ✅ Basic structure tests (14 tests)
- API Endpoints: 📋 Planned
- Services: 📋 Planned

## Writing Good Tests

### Test Naming Convention
Tests follow the pattern: `MethodName_Scenario_ExpectedBehavior`

Example:
```csharp
[Fact]
public void Contact_DefaultConstructor_InitializesKey()
{
    // Test implementation
}
```

### Test Structure (AAA Pattern)
Each test follows the Arrange-Act-Assert pattern:

```csharp
[Fact]
public void Example_Test()
{
    // Arrange - Set up test data and conditions
    var contact = new Contact("John", "john@example.com");

    // Act - Execute the code being tested
    var key = contact.Key;

    // Assert - Verify the results
    Assert.NotNull(key);
    Assert.True(Guid.TryParse(key, out _));
}
```

### Test Categories

1. **Unit Tests**: Test individual components in isolation
   - Should be fast (< 100ms)
   - No external dependencies
   - No database calls

2. **Integration Tests**: Test components working together
   - May involve database
   - May involve external services
   - Slower but more realistic

## Best Practices

✅ **Do:**
- Write tests before implementation (TDD)
- Keep tests focused and simple
- Test edge cases and error conditions
- Use descriptive test names
- Follow the AAA pattern
- Make assertions specific and meaningful

❌ **Don't:**
- Write tests that depend on each other
- Test implementation details
- Ignore failing tests
- Skip writing tests for "simple" code
- Use production data in tests

## Continuous Integration

Tests are automatically run on:
- Every commit
- Every pull request
- Before merging to main

All tests must pass before code can be merged.

## Adding New Tests

When adding new features:

1. Write the test first (Red)
2. Implement the minimum code to pass (Green)
3. Refactor for quality (Refactor)
4. Run the full test suite to ensure no regressions

## Test Configuration

Test configuration is managed via:
- `xunit.runner.json` - xUnit test runner settings
- `Tailwind.Mail.csproj` - Project configuration and test packages

Test packages used:
- `xunit` (v2.4.1) - Testing framework
- `xunit.runner.visualstudio` (v2.4.3) - Visual Studio test adapter
- `Microsoft.NET.Test.Sdk` (v17.1.0) - .NET test SDK
- `coverlet.collector` (v3.1.2) - Code coverage collection

## Future Enhancements

- [ ] Add integration tests for API endpoints
- [ ] Add command execution tests with mock database
- [ ] Add end-to-end tests for complete workflows
- [ ] Implement code coverage reporting
- [ ] Add performance benchmarking tests
- [ ] Add mutation testing

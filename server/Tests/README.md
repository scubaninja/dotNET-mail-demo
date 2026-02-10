# Tests

This directory contains all unit and integration tests for the Tailwind Mail Service.

## Quick Start

```bash
# Run all tests
dotnet test

# Run tests with detailed output
dotnet test --verbosity normal

# List all tests
dotnet test --list-tests
```

## Test Files

### Model Tests
- **ActivityTests.cs** - Tests for Activity model (5 tests)
- **BroadcastTests.cs** - Tests for Broadcast model and factory methods (10 tests)
- **CommandResultTests.cs** - Tests for CommandResult type (5 tests)
- **ContactTests.cs** - Tests for Contact model (4 tests)
- **EmailTests.cs** - Tests for Email model (5 tests)
- **MarkdownEmailTests.cs** - Tests for MarkdownEmail parsing (8 tests)
- **MessageTests.cs** - Tests for Message model and lifecycle (12 tests)
- **TagTests.cs** - Tests for Tag and Tagged models (7 tests)

### Total: 52 tests, all passing ✅

## Test Philosophy

We follow **Test-Driven Development (TDD)** practices. See [TDD.md](TDD.md) for comprehensive guidelines.

## Test Structure

All tests follow the **Arrange-Act-Assert (AAA)** pattern:

```csharp
[Fact]
public void Method_Scenario_ExpectedResult()
{
    // Arrange - Set up test data
    var model = new Model();

    // Act - Execute the code
    var result = model.DoSomething();

    // Assert - Verify expectations
    Assert.Equal(expected, result);
}
```

## xUnit Attributes

- `[Fact]` - Single test case
- `[Theory]` - Parameterized test (not yet used, but available)
- `[InlineData]` - Provides data for theory tests

## Future Tests

Planned test additions:
- Command tests (with mock database)
- API integration tests
- Service tests
- End-to-end workflow tests

## Contributing

When adding new features:
1. Write tests first (TDD Red phase)
2. Implement feature (TDD Green phase)
3. Refactor as needed (TDD Refactor phase)
4. Ensure all tests pass before committing

## CI/CD

Tests run automatically on every commit and pull request. All tests must pass before merging.

applyTo:
  # Apply to all files in src folder and subfolders
  - 'src/**'
  # Apply to specific file types in any folder
  - '**/*.cs'
  - '**/*.json'
  - '**/*.tf'
  # Target specific folders
  - 'deploy/**'
  - 'infrastructure/**/*.tf'
  # Apply to specific files
  - 'Program.cs'
  - 'appsettings.json'
  # Exclude patterns
  - '!**/bin/**'
  - '!**/obj/**'
  - '!**/node_modules/**'

# C# Coding Standards and Best Practices

## ✅ Naming Conventions

| Item              | Convention     | Example                        |
|-------------------|----------------|--------------------------------|
| Class/Method      | PascalCase     | `CustomerService`, `CalculateTotal()` |
| Variable/Parameter| camelCase      | `orderCount`, `filePath`       |
| Interface         | Prefix with "I"| `ILogger`, `IRepository`       |
| Constant          | PascalCase     | `MaxRetryCount`                |
| Enum              | PascalCase     | `OrderStatus.Completed`        |

Avoid abbreviations and acronyms unless widely accepted (e.g., `Html`, `Db`).

## 🧱 Project Structure

- One class per file, file name matches class name
- Group related classes into namespaces and folders
- Structure by **feature**, not layer, where appropriate
- Example structure:
  ```
  /Features
    /UserManagement
    /EmailService
    /Authentication
  /Infrastructure
    /Database
    /Logging
  /Common
    /Models
    /Extensions
  ```

## 🛠️ Code Style

- Use explicit access modifiers (`public`, `private`, etc.) for all types and members.
- Prefer expression-bodied members for simple properties and methods.
- Use object and collection initializers where possible.
- Place using directives outside namespaces and sort them alphabetically.
- Use string interpolation (`$"Hello {name}"`) instead of string concatenation.
- Prefer pattern matching and switch expressions for clarity.
- Limit line length to 120 characters.
- Use auto-properties unless additional logic is required.
- Avoid regions except for large files with clear logical separation.
- Use `var` when the type is obvious from the right side of the assignment; otherwise, use explicit types.


## 📜 Documentation
- Use XML comments for public APIs and methods.
- Use Markdown for README files and documentation.  
- Keep documentation up to date with code changes.
- Use meaningful commit messages that describe the change.
- Use `TODO` comments for incomplete features or bugs, and link to issue tracker.
- Use `FIXME` comments for known issues that need immediate attention.
- Use `NOTE` comments for important information or clarifications.
- Use `HACK` comments for temporary workarounds or hacks.
- Use `REVIEW` comments for code that needs to be reviewed or refactored.
- Use `OPTIMIZE` comments for code that can be optimized or improved.
- Use `DEPRECATED` comments for code that is no longer recommended for use.


## 🧪 Testing
- Use `xUnit` for unit testing.
- Use `Moq` for mocking dependencies.
- Write unit tests for all public methods and properties.
- Use `Arrange-Act-Assert` pattern for structuring tests.
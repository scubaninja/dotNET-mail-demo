---
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
---

# C# Coding Standards and Best Practices
---

## ✅ Naming Conventions

| Item              | Convention     | Example                        |
|-------------------|----------------|--------------------------------|
| Class/Method      | PascalCase     | `CustomerService`, `CalculateTotal()` |
| Variable/Parameter| camelCase      | `orderCount`, `filePath`       |
| Interface         | Prefix with "I"| `ILogger`, `IRepository`       |
| Constant          | PascalCase     | `MaxRetryCount`                |
| Enum              | PascalCase     | `OrderStatus.Completed`        |

Avoid abbreviations and acronyms unless widely accepted (e.g., `Html`, `Db`).

---

## 🧱 Project Structure

- One class per file, file name matches class name.
- Group related classes into namespaces and folders.
- Structure by **feature**, not layer, where appropriate (`/

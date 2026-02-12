# CLI Test Plan - Tailwind Traders Mail Service

## Component Overview

The CLI (Command Line Interface) is a Node.js-based tool for managing email broadcasts and contacts through markdown files. It provides an intuitive interface for creating, validating, and sending broadcasts, as well as managing contact lists.

## Technology Stack

- **Node.js 20 LTS** - Runtime environment
- **Commander.js** - CLI framework
- **Inquirer** - Interactive prompts
- **Gray Matter** - Markdown frontmatter parsing
- **Marked** - Markdown to HTML conversion
- **Axios** - HTTP client for API calls
- **Mocha** - Testing framework
- **Dotenv** - Environment variable management

## Test Scope

### Components to Test

1. **Commands**
   - `mdmail init` - Initialize mail directory structure
   - `mdmail broadcast new` - Create new broadcast
   - `mdmail broadcast generate` - Generate broadcast from AI
   - `mdmail broadcast validate` - Validate broadcast against API
   - `mdmail broadcast send` - Send broadcast via API
   - `mdmail broadcast regenerate` - Regenerate broadcast with AI
   - `mdmail contact` - Contact management commands

2. **Libraries**
   - Directory management (`lib/dirs.js`)
   - Markdown parsing (`lib/parser.js`)
   - API client utilities
   - AI integration utilities

3. **File Operations**
   - Markdown file creation
   - Frontmatter validation
   - HTML generation
   - File archiving (sent broadcasts)

## Test Environment Setup

### Prerequisites
```bash
# Install dependencies
cd cli
npm install

# Setup environment
cat > .env << EOF
API_ROOT="http://localhost:5000/admin"
OPENAI_API_KEY="your-key-here"  # Optional for AI features
EOF

# Create test directory
mkdir -p test
```

### Test Directory Structure
```
cli/
├── test/
│   ├── commands/
│   │   ├── init.test.js
│   │   ├── broadcast.test.js
│   │   └── contact.test.js
│   ├── lib/
│   │   ├── dirs.test.js
│   │   └── parser.test.js
│   └── fixtures/
│       ├── sample-broadcast.md
│       └── invalid-broadcast.md
```

## Unit Tests

### 1. Directory Management Tests (`lib/dirs.js`)

```javascript
describe('Directory Management', () => {
  
  describe('makeDirs()', () => {
    it('should create mail directory structure', () => {
      // Test: Create directories in temp location
      // Verify: mail/, mail/broadcasts/, mail/sequences/, mail/sent/ exist
    });
    
    it('should create README.md in mail directory', () => {
      // Test: Create directories
      // Verify: mail/README.md exists with content
    });
    
    it('should handle existing directories gracefully', () => {
      // Test: Run makeDirs twice
      // Verify: No errors, idempotent behavior
    });
  });
  
  describe('dirsExist()', () => {
    it('should return true when all directories exist', () => {
      // Test: Check after makeDirs
      // Expected: true
    });
    
    it('should return false when directories missing', () => {
      // Test: Check without creating directories
      // Expected: false
    });
  });
});
```

### 2. Markdown Parser Tests (`lib/parser.js`)

```javascript
describe('Markdown Parser', () => {
  
  describe('build()', () => {
    it('should parse valid markdown with frontmatter', () => {
      // Test: Parse sample-broadcast.md
      // Verify: data object contains Subject, Slug, Summary, SendToTag
      // Verify: markdown contains body content
    });
    
    it('should handle markdown without frontmatter', () => {
      // Test: Parse markdown without frontmatter
      // Expected: Error or default values
    });
    
    it('should extract all required frontmatter fields', () => {
      // Test: Parse with all fields
      // Verify: Subject, Slug, Summary, SendToTag present
    });
    
    it('should handle optional Prompt field', () => {
      // Test: Parse with and without Prompt
      // Verify: Prompt is optional
    });
  });
  
  describe('toHTML()', () => {
    it('should convert markdown to HTML', () => {
      // Test: Convert markdown body
      // Verify: HTML tags present
    });
    
    it('should handle code blocks correctly', () => {
      // Test: Markdown with code blocks
      // Verify: Code blocks preserved in HTML
    });
    
    it('should handle links and images', () => {
      // Test: Markdown with links and images
      // Verify: Proper HTML anchor and img tags
    });
  });
});
```

### 3. API Client Tests

```javascript
describe('API Client', () => {
  
  it('should send broadcast to API', async () => {
    // Test: Send valid broadcast
    // Expected: 201 Created response
    // Use mock server or test API
  });
  
  it('should handle API errors gracefully', async () => {
    // Test: Send to non-existent API
    // Expected: Error message displayed
  });
  
  it('should validate broadcast before sending', async () => {
    // Test: Validate broadcast structure
    // Expected: Validation response from API
  });
});
```

## Integration Tests

### 1. Init Command Tests

```javascript
describe('mdmail init', () => {
  
  it('should initialize directory structure', (done) => {
    // Test: Run init command in temp directory
    // Expected: Directories created
    // Expected: Success message displayed
  });
  
  it('should prevent re-initialization', (done) => {
    // Test: Run init twice
    // Expected: Error message about existing directories
  });
  
  it('should create README with instructions', (done) => {
    // Test: Check README content
    // Expected: Helpful instructions present
  });
});
```

### 2. Broadcast New Command Tests

```javascript
describe('mdmail broadcast new', () => {
  
  it('should create broadcast file with valid subject', (done) => {
    // Test: mdmail broadcast new "Test Subject"
    // Expected: File created in mail/broadcasts/
    // Expected: Filename based on slugified subject
  });
  
  it('should generate valid frontmatter', (done) => {
    // Test: Check created file
    // Verify: All required frontmatter fields present
  });
  
  it('should handle subjects with special characters', (done) => {
    // Test: Subject with &, @, #, etc.
    // Expected: Slug properly sanitized
  });
  
  it('should include AI prompt when -p flag used', (done) => {
    // Test: mdmail broadcast new "Subject" -p
    // Expected: Prompt field in frontmatter
    // Expected: AI-generated body (if API key available)
  });
  
  it('should require API initialization', (done) => {
    // Test: Run without mail directory
    // Expected: Error message about running init first
  });
});
```

### 3. Broadcast Validate Command Tests

```javascript
describe('mdmail broadcast validate', () => {
  
  it('should validate correct broadcast format', async () => {
    // Test: Validate valid broadcast
    // Expected: Success message
    // Expected: API validation passed
  });
  
  it('should detect missing required fields', async () => {
    // Test: Validate broadcast without Subject
    // Expected: Error message listing missing fields
  });
  
  it('should validate SendToTag format', async () => {
    // Test: Validate with invalid tag
    // Expected: Error message about tag format
  });
  
  it('should check API connectivity', async () => {
    // Test: Validate with API down
    // Expected: Connection error message
  });
});
```

### 4. Broadcast Send Command Tests

```javascript
describe('mdmail broadcast send', () => {
  
  it('should send valid broadcast to API', async () => {
    // Test: Send valid broadcast
    // Expected: API accepts broadcast
    // Expected: Success message with broadcast ID
  });
  
  it('should prompt for confirmation', async () => {
    // Test: Send command triggers prompt
    // Expected: User must confirm before sending
  });
  
  it('should move sent broadcast to sent folder', async () => {
    // Test: After successful send
    // Expected: File moved to mail/sent/
  });
  
  it('should handle send failures gracefully', async () => {
    // Test: API returns error
    // Expected: Error message displayed
    // Expected: File not moved to sent folder
  });
  
  it('should show preview before sending', async () => {
    // Test: Send command shows preview
    // Expected: Subject, summary, and content preview
  });
});
```

### 5. Broadcast Regenerate Command Tests

```javascript
describe('mdmail broadcast regenerate', () => {
  
  it('should regenerate with AI prompt', async () => {
    // Test: Regenerate broadcast with Prompt in frontmatter
    // Expected: New body generated
    // Expected: Frontmatter preserved
  });
  
  it('should require Prompt in frontmatter', async () => {
    // Test: Regenerate without Prompt
    // Expected: Error message
  });
  
  it('should preserve original if generation fails', async () => {
    // Test: AI API failure
    // Expected: Original file unchanged
    // Expected: Error message
  });
});
```

### 6. Contact Command Tests

```javascript
describe('mdmail contact', () => {
  
  it('should list contacts from API', async () => {
    // Test: List contacts
    // Expected: Formatted contact list
  });
  
  it('should search contacts by email', async () => {
    // Test: Search for specific email
    // Expected: Matching contacts displayed
  });
  
  it('should show contact details', async () => {
    // Test: View single contact
    // Expected: All contact fields displayed
  });
  
  it('should handle empty contact list', async () => {
    // Test: List when no contacts exist
    // Expected: Friendly message
  });
});
```

## End-to-End Tests

### E2E Test 1: Complete Broadcast Workflow

```javascript
describe('E2E: Complete Broadcast Workflow', () => {
  
  it('should complete full broadcast lifecycle', async function() {
    this.timeout(10000); // Increase timeout for E2E
    
    // 1. Initialize CLI
    // 2. Create new broadcast
    // 3. Edit broadcast file
    // 4. Validate broadcast
    // 5. Send broadcast
    // 6. Verify in sent folder
    // 7. Verify API received broadcast
  });
});
```

### E2E Test 2: AI-Powered Broadcast Creation

```javascript
describe('E2E: AI Broadcast Creation', () => {
  
  it('should create and send AI-generated broadcast', async function() {
    this.timeout(15000); // AI calls may be slow
    
    // 1. Create broadcast with prompt
    // 2. Verify AI-generated content
    // 3. Regenerate with different prompt
    // 4. Validate final version
    // 5. Send broadcast
  });
});
```

## Manual Testing Checklist

### Setup and Installation
- [ ] Install CLI globally with `npm install -g`
- [ ] Verify `mdmail` command is available
- [ ] Setup `.env` file with API_ROOT
- [ ] Test with and without OPENAI_API_KEY

### Init Command
- [ ] Run `mdmail init` in empty directory
- [ ] Verify all directories created
- [ ] Check README.md content
- [ ] Try running init again (should error)
- [ ] Navigate to mail directory structure

### Broadcast Creation
- [ ] Create broadcast with simple subject
- [ ] Create broadcast with complex subject (special chars)
- [ ] Create broadcast with `-p` flag (with AI key)
- [ ] Create broadcast with `-p` flag (without AI key)
- [ ] Verify file naming and slug generation
- [ ] Open created file and check frontmatter
- [ ] Edit body content
- [ ] Check markdown syntax highlighting (if applicable)

### Broadcast Validation
- [ ] Validate newly created broadcast
- [ ] Validate broadcast with missing fields
- [ ] Validate broadcast with invalid SendToTag
- [ ] Test validation with API server down
- [ ] Check error messages are clear

### Broadcast Sending
- [ ] Send valid broadcast
- [ ] Review confirmation prompt
- [ ] Cancel send operation
- [ ] Confirm send operation
- [ ] Verify file moved to sent folder
- [ ] Check API received broadcast (via Swagger UI)
- [ ] Try to send already-sent broadcast

### Broadcast Regeneration
- [ ] Regenerate broadcast with Prompt field
- [ ] Compare original and regenerated content
- [ ] Try regenerating without Prompt field
- [ ] Test with AI service unavailable

### Contact Management
- [ ] List all contacts
- [ ] Search for contact by email
- [ ] View contact details
- [ ] Test with no contacts in database
- [ ] Test with 100+ contacts (pagination)

### Error Handling
- [ ] Test all commands without init
- [ ] Test with invalid API_ROOT
- [ ] Test with network disconnected
- [ ] Test with malformed markdown files
- [ ] Verify helpful error messages

### Interactive Prompts
- [ ] Test select prompts (arrow keys)
- [ ] Test confirm prompts (Y/n)
- [ ] Test input prompts (text entry)
- [ ] Test canceling operations (Ctrl+C)

## Performance Tests

### CLI Responsiveness
```javascript
describe('Performance', () => {
  
  it('should list 1000 broadcasts in <2 seconds', async () => {
    // Test: List large number of broadcasts
    // Expected: Quick response time
  });
  
  it('should parse large markdown files quickly', () => {
    // Test: Parse 100KB markdown file
    // Expected: Parse time < 500ms
  });
  
  it('should handle slow API responses gracefully', async () => {
    // Test: Mock slow API (5 second delay)
    // Expected: Loading indicator shown
    // Expected: Timeout after reasonable period
  });
});
```

## Security Tests

### Input Validation
```javascript
describe('Security', () => {
  
  it('should sanitize file names', () => {
    // Test: Subject with path traversal attempt
    // Expected: Sanitized slug, no directory traversal
  });
  
  it('should prevent command injection in subjects', () => {
    // Test: Subject with shell commands
    // Expected: Commands not executed
  });
  
  it('should not expose API credentials', () => {
    // Test: Error messages and logs
    // Expected: No API keys or passwords in output
  });
  
  it('should validate markdown content for XSS', () => {
    // Test: Markdown with script tags
    // Expected: Sanitized or escaped in HTML
  });
});
```

## Test Data

### Sample Broadcast Markdown
```markdown
---
Subject: "Welcome to Tailwind Traders"
Slug: "welcome-to-tailwind-traders"
Summary: "A warm welcome to our new subscribers"
SendToTag: "new-subscriber"
---

# Welcome!

Thank you for subscribing to Tailwind Traders updates.

We're excited to have you on board!

[Visit our website](https://tailwindtraders.dev)
```

### Invalid Broadcast Examples
```markdown
# Missing frontmatter
This is invalid

---
# Missing required fields
InvalidField: "value"
---
Body content

---
Subject: "Missing quotes around value
---
Body content
```

## Test Execution

### Running Tests
```bash
# Run all tests
npm test

# Run specific test file
npm test -- test/commands/broadcast.test.js

# Run with coverage
npm test -- --coverage

# Run in watch mode
npm test -- --watch
```

### Test Scripts in package.json
```json
{
  "scripts": {
    "test": "mocha --recursive",
    "test:watch": "mocha --recursive --watch",
    "test:coverage": "nyc mocha --recursive",
    "test:integration": "mocha test/integration/**/*.test.js"
  }
}
```

## Mock Data and Utilities

### Mock API Server
```javascript
// test/helpers/mockServer.js
const express = require('express');

function createMockServer() {
  const app = express();
  
  app.post('/admin/broadcasts', (req, res) => {
    res.status(201).json({ id: 1, message: 'Created' });
  });
  
  app.get('/admin/contacts', (req, res) => {
    res.json([
      { id: 1, email: 'test@example.com', subscribed: true }
    ]);
  });
  
  return app.listen(5555);
}
```

### Test Helpers
```javascript
// test/helpers/setup.js
const fs = require('fs');
const path = require('path');

function createTestDirectory() {
  const testDir = path.join('/tmp', 'mdmail-test');
  if (!fs.existsSync(testDir)) {
    fs.mkdirSync(testDir, { recursive: true });
  }
  return testDir;
}

function cleanupTestDirectory(testDir) {
  if (fs.existsSync(testDir)) {
    fs.rmSync(testDir, { recursive: true, force: true });
  }
}
```

## Expected Results

### Pass Criteria
- ✅ All unit tests pass
- ✅ All integration tests pass
- ✅ All E2E tests pass
- ✅ Code coverage > 60%
- ✅ No security vulnerabilities
- ✅ Manual testing checklist complete

### Known Issues
- AI tests require API key (may be skipped in CI)
- Interactive prompt tests may be challenging to automate
- Network-dependent tests may fail in offline mode

## Test Metrics

### Coverage Goals
- Commands: 70%
- Libraries: 80%
- Utilities: 75%
- Overall: 60%

### Performance Benchmarks
- Command startup time: < 1 second
- File parsing: < 100ms per file
- API calls: < 3 seconds with timeout
- Bulk operations: < 5 seconds for 100 items

## CI/CD Integration

### GitHub Actions Example
```yaml
- name: Run CLI Tests
  run: |
    cd cli
    npm ci
    npm test
  env:
    API_ROOT: http://localhost:5000/admin
```

## Appendix

### Debugging Tests
```bash
# Run with verbose output
npm test -- --reporter spec

# Debug specific test
node --inspect-brk node_modules/.bin/mocha test/specific.test.js
```

### Test File Template
```javascript
const assert = require('assert');
const { describe, it, beforeEach, afterEach } = require('mocha');

describe('Component Name', () => {
  
  beforeEach(() => {
    // Setup
  });
  
  afterEach(() => {
    // Cleanup
  });
  
  it('should do something', () => {
    // Arrange
    const input = 'test';
    
    // Act
    const result = functionUnderTest(input);
    
    // Assert
    assert.strictEqual(result, 'expected');
  });
});
```

---

**Document Version**: 1.0  
**Last Updated**: 2024  
**Owner**: Test Team

# Quick Test Reference Guide

This is a quick reference guide for running tests across all components of the Tailwind Traders Mail Service.

## Quick Start Testing

### 1. Setup Test Environment

```bash
# Start PostgreSQL
brew services start postgresql@15  # macOS
# or
docker run -d --name postgres -p 5432:5432 -e POSTGRES_PASSWORD=postgres postgres:15

# Create test database
psql postgres -c "CREATE DATABASE tailwind_test;"

# Load schema
cd db
psql tailwind_test < db.sql
psql tailwind_test < seed.sql

# Start Mailpit for email testing
docker run -d --name mailpit -p 8025:8025 -p 1025:1025 axllent/mailpit
```

### 2. Run Tests by Component

```bash
# Server API Tests (.NET)
cd server
make test
# or
dotnet test

# CLI Tests (Node.js)
cd cli
npm test

# Jobs Tests (Go)
cd jobs
go test ./...

# Database Tests
psql tailwind_test < test/database/schema_tests.sql
```

## Test Commands Cheat Sheet

### Server API (.NET)

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test /p:CollectCoverage=true

# Run specific test class
dotnet test --filter "ClassName~ContactTests"

# Run tests in watch mode
dotnet watch test

# Generate coverage report
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

### CLI (Node.js)

```bash
# Run all tests
npm test

# Run in watch mode
npm test -- --watch

# Run with coverage
npm test -- --coverage

# Run specific test file
npm test -- test/commands/broadcast.test.js

# Debug tests
node --inspect-brk node_modules/.bin/mocha test/specific.test.js
```

### Jobs (Go)

```bash
# Run all tests
go test ./...

# Run with verbose output
go test -v ./...

# Run with coverage
go test -cover ./...

# Generate coverage report
go test -coverprofile=coverage.out ./...
go tool cover -html=coverage.out

# Run integration tests only
go test -tags=integration ./...

# Skip integration tests
go test -short ./...

# Race condition detection
go test -race ./...

# Benchmark tests
go test -bench=. ./...
```

### Database (PostgreSQL)

```bash
# Run schema validation tests
psql tailwind_test < test/database/schema_tests.sql

# Test with timing
psql tailwind_test -c "\timing" -c "SELECT COUNT(*) FROM mail.contacts;"

# Reset test database
./test/database/reset-test-db.sh

# Run specific test query
psql tailwind_test -c "SELECT * FROM mail.contacts LIMIT 10;"
```

## Integration Testing

```bash
# Start all services
docker-compose -f docker-compose.test.yml up -d

# Run integration test suite
./test/integration/run-all-tests.sh

# Stop all services
docker-compose -f docker-compose.test.yml down
```

## Quick Test Scenarios

### Test 1: Create and Send Broadcast

```bash
# 1. Initialize CLI
cd cli
node bin/mdmail.js init

# 2. Create broadcast
node bin/mdmail.js broadcast new "Test Broadcast"

# 3. Edit broadcast file
# (Edit mail/broadcasts/test-broadcast.md)

# 4. Validate
node bin/mdmail.js broadcast validate

# 5. Send
node bin/mdmail.js broadcast send

# 6. Verify in Mailpit
open http://localhost:8025
```

### Test 2: Subscribe and Confirm

```bash
# Subscribe
curl -X POST http://localhost:5000/subscribe \
  -H "Content-Type: application/json" \
  -d '{"email": "test@example.com", "name": "Test User"}'

# Get confirmation key
psql tailwind -t -c "SELECT key FROM mail.contacts WHERE email = 'test@example.com';"

# Confirm subscription
curl http://localhost:5000/confirm/{key}

# Verify
psql tailwind -c "SELECT email, subscribed FROM mail.contacts WHERE email = 'test@example.com';"
```

### Test 3: Check System Health

```bash
# API health
curl http://localhost:5000/health

# Database connectivity
psql tailwind -c "SELECT 1;"

# Mailpit health
curl http://localhost:8025/api/v1/info

# Run health check script
./scripts/health-check.sh
```

## Coverage Goals

- **Server API**: 70%+ overall, 80%+ for critical paths
- **CLI**: 60%+ overall, 70%+ for commands
- **Jobs**: 65%+ overall, 75%+ for senders/queuers
- **Integration**: All user stories validated

## Test Data

### Quick Test Contacts

```sql
-- Insert test contacts
INSERT INTO mail.contacts (email, name, subscribed) VALUES
  ('alice@example.com', 'Alice', true),
  ('bob@example.com', 'Bob', true),
  ('charlie@example.com', 'Charlie', false);

-- Create test tag
INSERT INTO mail.tags (slug, name) VALUES ('test', 'Test Tag');

-- Tag a contact
INSERT INTO mail.tagged (contact_id, tag_id)
SELECT c.id, t.id FROM mail.contacts c, mail.tags t
WHERE c.email = 'alice@example.com' AND t.slug = 'test';
```

### Clean Up Test Data

```sql
-- Remove test contacts
DELETE FROM mail.contacts WHERE email LIKE '%example.com';

-- Remove test broadcasts
DELETE FROM mail.broadcasts WHERE id IN (
  SELECT b.id FROM mail.broadcasts b
  INNER JOIN mail.emails e ON e.id = b.email_id
  WHERE e.slug LIKE 'test%'
);
```

## Common Issues

### Database Connection Failed
```bash
# Check if PostgreSQL is running
pg_isready

# Verify connection string
echo $DATABASE_URL

# Reset database
psql postgres -c "DROP DATABASE tailwind_test;"
psql postgres -c "CREATE DATABASE tailwind_test;"
psql tailwind_test < db/db.sql
```

### Tests Timing Out
```bash
# Increase timeout for long-running tests
# .NET: Set timeout in test attribute
# Node.js: this.timeout(10000);
# Go: Use -timeout flag: go test -timeout 30s
```

### Flaky Tests
```bash
# Run multiple times to identify flaky tests
for i in {1..10}; do dotnet test; done

# Use test retries (if framework supports)
# Isolate test data to avoid conflicts
```

## CI/CD Testing

### GitHub Actions

```yaml
# Run tests in CI
- name: Run Tests
  run: |
    cd server && dotnet test
    cd cli && npm test
    cd jobs && go test ./...
```

### Pre-commit Testing

```bash
# Create pre-commit hook
cat > .git/hooks/pre-commit << 'EOF'
#!/bin/bash
echo "Running tests..."
cd server && dotnet test --no-restore || exit 1
cd cli && npm test || exit 1
cd jobs && go test -short ./... || exit 1
EOF

chmod +x .git/hooks/pre-commit
```

## Performance Testing

```bash
# API load test
ab -n 1000 -c 10 http://localhost:5000/admin/contacts

# Database performance test
psql tailwind -c "\timing" -c "EXPLAIN ANALYZE SELECT * FROM mail.contacts WHERE subscribed = true;"

# Jobs throughput test
cd jobs
time mage messages:queue
time mage messages:send
```

## Security Testing

```bash
# SQL injection test
curl -X POST http://localhost:5000/subscribe \
  -H "Content-Type: application/json" \
  -d '{"email": "test@example.com\"; DROP TABLE mail.contacts; --"}'

# Verify no damage
psql tailwind -c "\dt mail.contacts"

# XSS test in broadcast
# (Include script tags in markdown, verify they're escaped)
```

## Useful Links

- [Master Test Plan](./TEST_PLAN.md)
- [Server Test Plan](./test-plans/SERVER_TEST_PLAN.md)
- [CLI Test Plan](./test-plans/CLI_TEST_PLAN.md)
- [Jobs Test Plan](./test-plans/JOBS_TEST_PLAN.md)
- [Database Test Plan](./test-plans/DATABASE_TEST_PLAN.md)
- [Integration Test Plan](./test-plans/INTEGRATION_TEST_PLAN.md)

## Quick Tips

1. **Always reset test database** between runs
2. **Use Mailpit** instead of real SMTP in development
3. **Run tests before committing** code
4. **Check coverage** regularly
5. **Keep test data isolated** from production
6. **Mock external services** (Azure, OpenAI) in unit tests
7. **Use meaningful test names** that describe what's being tested
8. **Clean up after tests** to avoid side effects

---

**For detailed test specifications, see the individual test plan documents.**

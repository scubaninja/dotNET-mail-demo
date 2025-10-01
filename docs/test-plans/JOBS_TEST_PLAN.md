# Jobs Service Test Plan - Tailwind Traders Mail Service

## Component Overview

The Jobs Service is a Go-based microservice that handles background email sending, queue processing, and integrations with external services (Azure Communication Services, SMTP, Service Bus). It uses Mage for task automation and can run as a standalone service or in containers.

## Technology Stack

- **Go 1.21+** - Programming language
- **Mage** - Task automation tool
- **Azure SDK** - Azure service integrations
- **PostgreSQL** - Database connectivity
- **Docker** - Containerization
- **Go testing** - Built-in testing framework
- **Testify** (optional) - Enhanced testing library

## Test Scope

### Components to Test

1. **Senders**
   - `azuresender.go` - Azure Communication Services integration
   - `smtpsender.go` - SMTP email sending
   - `azurecontainerservices.go` - Azure Container Services
   - `testsender.go` - Mock sender for testing

2. **Queuers**
   - `servicebusqueuer.go` - Azure Service Bus integration
   - `testqueuer.go` - Mock queuer for testing

3. **Mage Targets**
   - Test targets (hello, goodbye)
   - Email targets (sendOne, getResult)
   - Docker targets (build, buildDev, run)
   - Deploy targets (storage, containerApps, etc.)
   - ServiceBus targets (send, receive, receiveAll)
   - Messages targets (queue, send)

4. **Core Services**
   - Database connection (`postgres.go`)
   - Azure authentication (`azure.go`)
   - Service Bus operations (`servicebus.go`)
   - Message processing (`messages.go`)

## Test Environment Setup

### Prerequisites
```bash
# Install Go
go version  # Should be 1.21+

# Install Mage
go install github.com/magefile/mage@latest

# Setup environment variables
export DATABASE_URL="postgres://user:password@localhost/tailwind"
export MESSAGES_TYPE="test"  # Use test sender/queuer
export AZURE_SUBSCRIPTION_ID="your-subscription-id"
export AZURE_TENANT_ID="your-tenant-id"
export AZURE_CLIENT_ID="your-client-id"
export AZURE_CLIENT_SECRET="your-client-secret"
```

### Test Database
```bash
# Create test database
psql -c "CREATE DATABASE tailwind_test;"
psql tailwind_test < ../db/db.sql
psql tailwind_test < ../db/seed.sql
```

### Local Testing Setup
```bash
cd jobs
go mod download
mage test:hello  # Verify mage is working
```

## Unit Tests

### 1. Sender Tests

#### Test Sender (`senders/testsender_test.go`)
```go
package senders_test

import (
    "testing"
    "jobs/senders"
)

func TestTestSender_SendOne(t *testing.T) {
    // Test: Send email via test sender
    // Expected: No error, email logged
    sender := senders.NewTestSender()
    err := sender.SendOne("test@example.com", "Subject", "Body")
    
    if err != nil {
        t.Errorf("Expected no error, got %v", err)
    }
}

func TestTestSender_GetResult(t *testing.T) {
    // Test: Get send result
    // Expected: Returns test result
    sender := senders.NewTestSender()
    result, err := sender.GetResult("test-operation-id")
    
    if err != nil {
        t.Errorf("Expected no error, got %v", err)
    }
    
    if result == nil {
        t.Error("Expected result, got nil")
    }
}
```

#### SMTP Sender Tests (`senders/smtpsender_test.go`)
```go
func TestSmtpSender_NewFromEnv(t *testing.T) {
    // Test: Create sender from environment variables
    // Expected: Valid sender with correct config
}

func TestSmtpSender_SendEmail(t *testing.T) {
    // Test: Send email via SMTP
    // Expected: Email sent successfully
    // Use test SMTP server (Mailpit)
}

func TestSmtpSender_HandleSMTPError(t *testing.T) {
    // Test: Handle SMTP connection error
    // Expected: Appropriate error returned
}

func TestSmtpSender_ValidateEmailFormat(t *testing.T) {
    // Test: Validate email addresses
    // Expected: Invalid emails rejected
}
```

#### Azure Sender Tests (`senders/azuresender_test.go`)
```go
func TestAzureSender_NewFromEnv(t *testing.T) {
    // Test: Create sender from environment variables
    // Expected: Valid sender with Azure client
}

func TestAzureSender_SendEmail(t *testing.T) {
    // Test: Send email via Azure Communication Services
    // Expected: Operation ID returned
    // Note: May require mock Azure client
}

func TestAzureSender_GetOperationStatus(t *testing.T) {
    // Test: Check send operation status
    // Expected: Valid status response
}

func TestAzureSender_HandleRateLimiting(t *testing.T) {
    // Test: Handle Azure rate limits
    // Expected: Retry logic works correctly
}
```

### 2. Queuer Tests

#### Test Queuer (`queuers/testqueuer_test.go`)
```go
func TestTestQueuer_Enqueue(t *testing.T) {
    // Test: Add message to queue
    // Expected: Message added successfully
}

func TestTestQueuer_Dequeue(t *testing.T) {
    // Test: Retrieve message from queue
    // Expected: Message retrieved in FIFO order
}

func TestTestQueuer_IsEmpty(t *testing.T) {
    // Test: Check if queue is empty
    // Expected: Correct empty status
}
```

#### Service Bus Queuer Tests (`queuers/servicebusqueuer_test.go`)
```go
func TestServiceBusQueuer_NewFromEnv(t *testing.T) {
    // Test: Create queuer from environment
    // Expected: Valid Service Bus client
}

func TestServiceBusQueuer_SendMessage(t *testing.T) {
    // Test: Send message to Service Bus
    // Expected: Message sent successfully
}

func TestServiceBusQueuer_ReceiveMessages(t *testing.T) {
    // Test: Receive messages from Service Bus
    // Expected: Messages received in batch
}

func TestServiceBusQueuer_CompleteMessage(t *testing.T) {
    // Test: Complete (acknowledge) message
    // Expected: Message removed from queue
}
```

### 3. Database Tests (`postgres_test.go`)

```go
func TestPostgres_Connect(t *testing.T) {
    // Test: Connect to PostgreSQL
    // Expected: Successful connection
}

func TestPostgres_QueryContacts(t *testing.T) {
    // Test: Query contacts from database
    // Expected: Returns contact list
}

func TestPostgres_QueryMessages(t *testing.T) {
    // Test: Query pending messages
    // Expected: Returns messages ready to send
}

func TestPostgres_UpdateMessageStatus(t *testing.T) {
    // Test: Update message send status
    // Expected: Status updated in database
}

func TestPostgres_HandleConnectionPool(t *testing.T) {
    // Test: Connection pooling
    // Expected: Reuses connections efficiently
}
```

### 4. Message Processing Tests (`messages_test.go`)

```go
func TestMessages_Queue(t *testing.T) {
    // Test: Queue messages for sending
    // Expected: Messages inserted into database
}

func TestMessages_Send(t *testing.T) {
    // Test: Send queued messages
    // Expected: Messages sent via configured sender
    // Expected: Status updated in database
}

func TestMessages_ProcessBatch(t *testing.T) {
    // Test: Process batch of messages
    // Expected: All messages processed
    // Expected: Errors handled gracefully
}

func TestMessages_RetryFailures(t *testing.T) {
    // Test: Retry failed message sends
    // Expected: Failed messages retried
    // Expected: Retry count incremented
}
```

## Integration Tests

### 1. End-to-End Message Flow

```go
func TestE2E_QueueAndSendMessage(t *testing.T) {
    // Setup: Configure test sender
    // Action: Queue a message
    // Action: Process send queue
    // Verify: Message sent
    // Verify: Status updated in database
}

func TestE2E_MultipleMessagesBatch(t *testing.T) {
    // Setup: Queue 100 messages
    // Action: Process in batches
    // Verify: All messages sent
    // Verify: Correct order maintained
}
```

### 2. Azure Integration Tests

```go
func TestIntegration_AzureServiceBus(t *testing.T) {
    if testing.Short() {
        t.Skip("Skipping integration test")
    }
    
    // Test: Send and receive via real Service Bus
    // Expected: Message round-trip successful
}

func TestIntegration_AzureCommunicationServices(t *testing.T) {
    if testing.Short() {
        t.Skip("Skipping integration test")
    }
    
    // Test: Send email via real Azure service
    // Expected: Email sent, operation ID received
}
```

### 3. Database Integration Tests

```go
func TestIntegration_DatabaseOperations(t *testing.T) {
    // Setup: Create test database
    // Test: CRUD operations on messages
    // Test: Transaction handling
    // Cleanup: Reset database
}

func TestIntegration_BulkInsert(t *testing.T) {
    // Test: Insert 10,000 messages
    // Expected: All inserted within 5 seconds
    // Expected: No deadlocks or connection issues
}
```

## Mage Target Tests

### 1. Test Namespace

```go
// These are manual tests via command line
// Document expected behavior

func TestMageTarget_TestHello(t *testing.T) {
    // Command: mage test:hello
    // Expected: Prints "hello" with timestamp
    // Expected: Exit code 0
}

func TestMageTarget_TestGoodbye(t *testing.T) {
    // Command: mage test:goodbye
    // Expected: Prints "goodbye" with timestamp
    // Expected: Exit code 0
}
```

### 2. Email Namespace

```bash
# Manual test cases for email targets

# Test: Send test email
mage email:sendOne test@example.com
# Expected: Email sent via configured provider
# Expected: Operation ID printed

# Test: Get operation result
mage email:getResult <operation-id>
# Expected: Status of email send displayed
```

### 3. ServiceBus Namespace

```bash
# Test: Send single message
mage serviceBus:send
# Expected: Message sent to Service Bus

# Test: Send batch of messages
mage serviceBus:sendMessageBatch
# Expected: 10 messages sent with delays

# Test: Receive messages
mage serviceBus:receive
# Expected: Up to 5 messages received and completed

# Test: Receive all messages
mage serviceBus:receiveAll
# Expected: All messages processed until queue empty
```

### 4. Messages Namespace

```bash
# Test: Queue messages
mage messages:queue
# Expected: Messages created in database

# Test: Send queued messages
mage messages:send
# Expected: Messages sent via configured sender
# Expected: Database updated
```

### 5. Docker Namespace

```bash
# Test: Build production container
mage docker:build
# Expected: Container image "jobs" created
# Expected: Size < 50MB (distroless base)

# Test: Build dev container
mage docker:buildDev
# Expected: Container image "jobs" created
# Expected: Includes mage and vim

# Test: Run container
mage docker:run test:hello
# Expected: Container runs mage target
# Expected: Output displayed
```

## Performance Tests

### Load Tests

```go
func TestPerformance_Send1000Messages(t *testing.T) {
    // Test: Send 1000 messages
    // Expected: Complete within 60 seconds
    // Expected: No memory leaks
}

func TestPerformance_ConcurrentSending(t *testing.T) {
    // Test: Send messages concurrently (10 workers)
    // Expected: All messages sent
    // Expected: No race conditions
}

func TestPerformance_DatabaseQueries(t *testing.T) {
    // Test: Query performance with 10K messages
    // Expected: Query time < 100ms
}
```

### Resource Usage Tests

```go
func TestResources_MemoryUsage(t *testing.T) {
    // Test: Monitor memory during 10K message send
    // Expected: Memory usage < 100MB
}

func TestResources_ConnectionPool(t *testing.T) {
    // Test: Database connection pooling
    // Expected: Max connections not exceeded
    // Expected: Connections reused
}

func TestResources_GoroutineLeak(t *testing.T) {
    // Test: Check for goroutine leaks
    // Expected: Goroutines cleaned up after operations
}
```

## Security Tests

### Authentication Tests

```go
func TestSecurity_AzureAuthentication(t *testing.T) {
    // Test: Authenticate with Azure
    // Expected: Token obtained successfully
    // Expected: Token refresh works
}

func TestSecurity_InvalidCredentials(t *testing.T) {
    // Test: Use invalid Azure credentials
    // Expected: Authentication fails gracefully
}
```

### Data Protection Tests

```go
func TestSecurity_NoCredentialsInLogs(t *testing.T) {
    // Test: Check logs for sensitive data
    // Expected: No passwords, keys, or tokens in logs
}

func TestSecurity_SecureConnections(t *testing.T) {
    // Test: Database connections use SSL
    // Expected: TLS/SSL enabled for PostgreSQL
}
```

## Container Tests

### Docker Image Tests

```bash
# Test: Image builds successfully
docker build -t jobs -f Dockerfile .
# Expected: Build succeeds
# Expected: No vulnerabilities (run docker scan)

# Test: Image size
docker images jobs
# Expected: Size < 50MB for production image

# Test: Run container
docker run jobs mage test:hello
# Expected: Container runs and exits successfully

# Test: Container healthcheck
docker inspect jobs
# Expected: Healthcheck configured (if applicable)
```

### Multi-stage Build Tests

```bash
# Test: Development image
docker build -t jobs-dev -f dev.Dockerfile .
# Expected: Includes development tools

# Test: Production image security
docker run jobs ls /bin
# Expected: Minimal binaries (distroless)
```

## Manual Testing Checklist

### Local Development
- [ ] Run `mage` to list all targets
- [ ] Run `mage test:hello` and verify output
- [ ] Run `mage test:goodbye` and verify output
- [ ] Check mage execution time (< 1 second)

### Email Sending
- [ ] Configure SMTP settings for Mailpit
- [ ] Send test email via `mage email:sendOne`
- [ ] Check Mailpit UI for received email
- [ ] Verify email content and formatting
- [ ] Test with Azure Communication Services (if configured)

### Service Bus Operations
- [ ] Configure Azure Service Bus connection
- [ ] Send single message
- [ ] Send batch of messages
- [ ] Receive and process messages
- [ ] Verify message order (FIFO)
- [ ] Test poison message handling

### Message Queue Processing
- [ ] Queue 100 messages via `mage messages:queue`
- [ ] Process messages via `mage messages:send`
- [ ] Check database for status updates
- [ ] Verify all messages sent
- [ ] Check error handling for failed sends

### Database Operations
- [ ] Connect to database
- [ ] Query contacts
- [ ] Query messages
- [ ] Update message status
- [ ] Test transaction rollback
- [ ] Test connection pool exhaustion

### Container Operations
- [ ] Build production container
- [ ] Build development container
- [ ] Run container with test target
- [ ] Run container with email target
- [ ] Test container restart behavior
- [ ] Verify container logs

### Azure Deployment
- [ ] Deploy to Azure Container Apps (if configured)
- [ ] Verify scheduled jobs run correctly
- [ ] Check Azure logs and metrics
- [ ] Test scaling behavior
- [ ] Verify secrets management

## Test Execution

### Running Tests

```bash
# Run all unit tests
go test ./...

# Run with verbose output
go test -v ./...

# Run specific package tests
go test ./senders/...

# Run with coverage
go test -cover ./...

# Generate coverage report
go test -coverprofile=coverage.out ./...
go tool cover -html=coverage.out

# Run integration tests only
go test -tags=integration ./...

# Run without integration tests
go test -short ./...

# Race condition detection
go test -race ./...

# Benchmark tests
go test -bench=. ./...
```

### Mage Target Testing

```bash
# List all available targets
mage -l

# Run specific target
mage test:hello

# Run with verbose output
mage -v test:hello

# Run Docker targets
mage docker:build
mage docker:run test:hello
```

## Test Data

### Sample Message Data

```go
type TestMessage struct {
    ID       int
    SendTo   string
    SendFrom string
    Subject  string
    Body     string
}

var testMessages = []TestMessage{
    {1, "alice@example.com", "noreply@tailwind.dev", "Welcome", "Welcome to Tailwind Traders"},
    {2, "bob@example.com", "noreply@tailwind.dev", "Newsletter", "Monthly newsletter"},
}
```

### Mock Data Generators

```go
func GenerateTestMessages(count int) []TestMessage {
    messages := make([]TestMessage, count)
    for i := 0; i < count; i++ {
        messages[i] = TestMessage{
            ID:       i + 1,
            SendTo:   fmt.Sprintf("user%d@example.com", i),
            SendFrom: "noreply@tailwind.dev",
            Subject:  fmt.Sprintf("Test Message %d", i),
            Body:     "Test email body",
        }
    }
    return messages
}
```

## Expected Results

### Pass Criteria
- ✅ All unit tests pass
- ✅ All integration tests pass (when not in short mode)
- ✅ Code coverage > 60%
- ✅ No race conditions detected
- ✅ Performance benchmarks met
- ✅ Container builds successfully
- ✅ Manual testing checklist complete

### Known Issues
- Azure integration tests require valid credentials
- Service Bus tests may be slow (network latency)
- Container tests require Docker installed
- Some mage targets require Azure resources

## Test Metrics

### Coverage Goals
- Senders: 75%
- Queuers: 75%
- Core services: 70%
- Utilities: 60%
- Overall: 65%

### Performance Benchmarks
- Message send rate: > 100 messages/second
- Database query time: < 100ms (95th percentile)
- Container startup: < 5 seconds
- Memory usage: < 100MB for 10K messages
- Goroutines: No leaks after operations

## CI/CD Integration

### GitHub Actions Example

```yaml
name: Jobs Service Tests

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    
    services:
      postgres:
        image: postgres:15
        env:
          POSTGRES_PASSWORD: postgres
          POSTGRES_DB: tailwind_test
        options: >-
          --health-cmd pg_isready
          --health-interval 10s
          --health-timeout 5s
          --health-retries 5
        ports:
          - 5432:5432
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Set up Go
      uses: actions/setup-go@v4
      with:
        go-version: '1.21'
    
    - name: Install Mage
      run: go install github.com/magefile/mage@latest
    
    - name: Setup Database
      run: |
        psql -h localhost -U postgres -d tailwind_test -f ../db/db.sql
      env:
        PGPASSWORD: postgres
    
    - name: Run Tests
      run: go test -v -race -coverprofile=coverage.out ./...
      env:
        DATABASE_URL: postgres://postgres:postgres@localhost/tailwind_test
        MESSAGES_TYPE: test
    
    - name: Upload Coverage
      uses: codecov/codecov-action@v3
      with:
        files: ./coverage.out
```

## Appendix

### Test Utilities

```go
// test/helpers.go
package test

import (
    "database/sql"
    "testing"
)

func SetupTestDB(t *testing.T) *sql.DB {
    // Setup test database connection
    db, err := sql.Open("postgres", "postgres://localhost/tailwind_test")
    if err != nil {
        t.Fatalf("Failed to connect to test database: %v", err)
    }
    
    t.Cleanup(func() {
        db.Close()
    })
    
    return db
}

func ResetTestData(t *testing.T, db *sql.DB) {
    // Truncate tables and reset to known state
    _, err := db.Exec("TRUNCATE mail.messages RESTART IDENTITY CASCADE")
    if err != nil {
        t.Fatalf("Failed to reset test data: %v", err)
    }
}
```

### Debugging Tips

```bash
# Run with debug output
GODEBUG=gctrace=1 go test -v ./...

# Profile CPU usage
go test -cpuprofile cpu.prof -bench=.
go tool pprof cpu.prof

# Profile memory usage
go test -memprofile mem.prof -bench=.
go tool pprof mem.prof

# Trace execution
go test -trace trace.out ./...
go tool trace trace.out
```

---

**Document Version**: 1.0  
**Last Updated**: 2024  
**Owner**: Test Team

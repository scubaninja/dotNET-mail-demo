# Server API Test Plan - Tailwind Traders Mail Service

## Component Overview

The Server API is a .NET 8.0 Minimal API that provides transactional and bulk email sending capabilities. It manages contacts, broadcasts, sequences, and email templates through a RESTful API.

## Technology Stack

- **.NET 8.0** - Minimal API
- **Dapper** - Database access
- **PostgreSQL** - Data storage
- **xUnit** - Testing framework
- **Microsoft.AspNetCore.Mvc.Testing** - Integration testing
- **Swagger/OpenAPI** - API documentation

## Test Scope

### Components to Test

1. **API Endpoints**
   - Public routes (subscription, unsubscribe, confirmation)
   - Admin broadcast routes
   - Admin contact routes
   - Admin bulk operation routes

2. **Services**
   - SmtpEmailSender
   - BackgroundSend (hosted service)
   - AI service integration
   - Outbox service

3. **Data Layer**
   - Database connection (DB.cs)
   - Contact repository
   - Broadcast repository
   - Message repository
   - Tag repository

4. **Models**
   - Contact
   - Broadcast
   - Email
   - Message
   - MarkdownEmail
   - Tag
   - Activity

## Test Environment Setup

### Prerequisites
```bash
# Database setup
cd server
make db
make seed

# Start Mailpit for email testing
make mailpit

# Environment variables
export DATABASE_URL="postgres://user:password@localhost/tailwind"
export SMTP_HOST="localhost"
export SMTP_PORT="1025"
export DEFAULT_FROM="test@tailwind.dev"
export SEND_WORKER="local"
```

### Test Database
- Use dedicated test database `tailwind_test`
- Reset database before each test run
- Seed with consistent test data

## Unit Tests

### 1. Models Tests

#### Contact Model Tests
```csharp
[Fact]
public void Contact_Should_Generate_Unique_Key()
public void Contact_Should_Default_Subscribed_To_True()
public void Contact_Should_Validate_Email_Format()
public void Contact_Should_Track_Created_And_Updated_Timestamps()
```

#### Broadcast Model Tests
```csharp
[Fact]
public void Broadcast_Should_Have_Valid_Subject()
public void Broadcast_Should_Track_Message_Count()
public void Broadcast_Should_Link_To_Email_Template()
public void Broadcast_Should_Filter_By_Tag()
```

#### Message Model Tests
```csharp
[Fact]
public void Message_Should_Have_Required_Fields()
public void Message_Should_Track_Send_Status()
public void Message_Should_Link_To_Contact_And_Broadcast()
```

### 2. Service Tests

#### EmailSender Service Tests
```csharp
[Fact]
public async Task SendEmail_Should_Send_Email_Successfully()
public async Task SendEmail_Should_Handle_Invalid_Recipient()
public async Task SendEmail_Should_Use_Correct_SMTP_Settings()
public async Task SendEmail_Should_Log_Failures()
```

#### Markdown Parser Tests
```csharp
[Fact]
public void ParseMarkdown_Should_Extract_Frontmatter()
public void ParseMarkdown_Should_Convert_Body_To_HTML()
public void ParseMarkdown_Should_Handle_Invalid_Frontmatter()
public void ParseMarkdown_Should_Preserve_HTML_Entities()
```

### 3. Data Layer Tests

#### Database Connection Tests
```csharp
[Fact]
public async Task DB_Should_Connect_Successfully()
public async Task DB_Should_Handle_Connection_Failures()
public async Task DB_Should_Use_Connection_Pooling()
```

#### Repository Tests
```csharp
[Fact]
public async Task GetContact_Should_Return_Contact_By_Id()
public async Task CreateContact_Should_Insert_New_Contact()
public async Task UpdateContact_Should_Modify_Existing_Contact()
public async Task DeleteContact_Should_Remove_Contact()
public async Task GetContactByEmail_Should_Return_Unique_Contact()
```

## Integration Tests

### 1. Public API Endpoints

#### Subscribe Endpoint
```csharp
[Fact]
public async Task POST_Subscribe_Should_Create_Contact()
{
    // Test: POST /subscribe with email
    // Expected: 201 Created, contact created with subscribed=false
    // Expected: Confirmation email sent
}

[Fact]
public async Task POST_Subscribe_Should_Reject_Duplicate_Email()
{
    // Test: POST /subscribe with existing email
    // Expected: 409 Conflict or appropriate error
}

[Fact]
public async Task POST_Subscribe_Should_Validate_Email_Format()
{
    // Test: POST /subscribe with invalid email
    // Expected: 400 Bad Request
}
```

#### Confirm Subscription Endpoint
```csharp
[Fact]
public async Task GET_Confirm_Should_Activate_Subscription()
{
    // Test: GET /confirm/{key}
    // Expected: 200 OK, subscribed=true
    // Expected: Activity logged
}

[Fact]
public async Task GET_Confirm_Should_Reject_Invalid_Key()
{
    // Test: GET /confirm/invalid-key
    // Expected: 404 Not Found or appropriate error
}
```

#### Unsubscribe Endpoint
```csharp
[Fact]
public async Task GET_Unsubscribe_Should_Deactivate_Subscription()
{
    // Test: GET /unsubscribe/{key}
    // Expected: 200 OK, subscribed=false
    // Expected: Activity logged
}

[Fact]
public async Task GET_Unsubscribe_Should_Handle_Already_Unsubscribed()
{
    // Test: GET /unsubscribe/{key} when already unsubscribed
    // Expected: 200 OK, idempotent behavior
}
```

### 2. Admin API Endpoints

#### Contact Management
```csharp
[Fact]
public async Task GET_Admin_Contacts_Should_Return_List()
{
    // Test: GET /admin/contacts
    // Expected: 200 OK with contact list
}

[Fact]
public async Task GET_Admin_Contacts_Should_Support_Pagination()
{
    // Test: GET /admin/contacts?page=2&limit=10
    // Expected: 200 OK with paginated results
}

[Fact]
public async Task POST_Admin_Contact_Should_Create_Contact()
{
    // Test: POST /admin/contacts with contact data
    // Expected: 201 Created
}

[Fact]
public async Task PUT_Admin_Contact_Should_Update_Contact()
{
    // Test: PUT /admin/contacts/{id}
    // Expected: 200 OK with updated contact
}

[Fact]
public async Task DELETE_Admin_Contact_Should_Remove_Contact()
{
    // Test: DELETE /admin/contacts/{id}
    // Expected: 204 No Content
}
```

#### Broadcast Management
```csharp
[Fact]
public async Task POST_Admin_Broadcast_Should_Create_From_Markdown()
{
    // Test: POST /admin/broadcasts with MarkdownEmail
    // Expected: 201 Created, email template created
}

[Fact]
public async Task POST_Admin_Broadcast_Queue_Should_Create_Messages()
{
    // Test: POST /admin/broadcasts/{id}/queue
    // Expected: 200 OK, messages queued for all subscribed contacts
}

[Fact]
public async Task GET_Admin_Broadcast_Status_Should_Return_Stats()
{
    // Test: GET /admin/broadcasts/{id}/status
    // Expected: 200 OK with message counts (queued, sent, failed)
}
```

#### Bulk Operations
```csharp
[Fact]
public async Task POST_Admin_Bulk_Import_Should_Import_Contacts()
{
    // Test: POST /admin/bulk/import with CSV
    // Expected: 200 OK with import results
}

[Fact]
public async Task POST_Admin_Bulk_Tag_Should_Add_Tags_To_Contacts()
{
    // Test: POST /admin/bulk/tag with contact IDs and tag
    // Expected: 200 OK with affected count
}
```

### 3. Database Integration Tests

#### Contact Operations
```csharp
[Fact]
public async Task CreateContact_Should_Persist_To_Database()
public async Task UpdateContact_Should_Reflect_In_Database()
public async Task DeleteContact_Should_Remove_From_Database()
public async Task GetContactsByTag_Should_Return_Tagged_Contacts()
```

#### Broadcast Operations
```csharp
[Fact]
public async Task QueueBroadcast_Should_Create_Messages_For_Subscribed()
public async Task QueueBroadcast_Should_Exclude_Unsubscribed()
public async Task QueueBroadcast_Should_Filter_By_Tag()
public async Task QueueBroadcast_Should_Update_Message_Count()
```

## End-to-End Tests

### User Story 1: Queue Broadcast to 10K Contacts
```csharp
[Fact]
public async Task E2E_Queue_Broadcast_To_10K_Contacts()
{
    // Setup: Create 10,001 contacts, set one to unsubscribed
    // Action: Create and queue broadcast with tag="*" (all)
    // Verify: Exactly 10,000 messages queued
    // Verify: Message count updated on broadcast
    // Verify: Unsubscribed contact not in queue
    // Verify: Process completes < 30 seconds
}
```

### User Story 2: Double Opt-In Signup
```csharp
[Fact]
public async Task E2E_Double_Opt_In_Flow()
{
    // Action: POST /subscribe with email
    // Verify: Contact created with subscribed=false
    // Verify: Confirmation email sent
    // Action: GET /confirm/{key}
    // Verify: Contact subscribed=true
    // Verify: Activity logged
}
```

### User Story 3: Unsubscribe Flow
```csharp
[Fact]
public async Task E2E_Unsubscribe_Flow()
{
    // Setup: Create subscribed contact
    // Action: GET /unsubscribe/{key}
    // Verify: Contact subscribed=false
    // Verify: Activity logged
    // Action: Queue broadcast
    // Verify: Unsubscribed contact not in queue
}
```

## Performance Tests

### Load Tests
```csharp
[Fact]
public async Task Load_Test_Queue_10K_Messages()
{
    // Test: Queue broadcast to 10,000 contacts
    // Expected: Complete within 30 seconds
    // Expected: No database connection exhaustion
}

[Fact]
public async Task Load_Test_API_Response_Time()
{
    // Test: 100 concurrent GET requests to /admin/contacts
    // Expected: Average response time < 200ms
    // Expected: No 500 errors
}
```

### Database Performance
```csharp
[Fact]
public async Task Performance_Query_Contacts_With_Tag()
{
    // Test: Query contacts with specific tag from 10K contacts
    // Expected: Query time < 100ms
}

[Fact]
public async Task Performance_Create_Messages_Bulk()
{
    // Test: Insert 10,000 messages
    // Expected: Bulk insert time < 5 seconds
}
```

## Security Tests

### Input Validation
```csharp
[Fact]
public async Task Security_Reject_SQL_Injection_In_Email()
{
    // Test: POST /subscribe with SQL injection attempt
    // Expected: 400 Bad Request, no SQL executed
}

[Fact]
public async Task Security_Sanitize_XSS_In_Email_Content()
{
    // Test: POST /admin/broadcasts with XSS attempt in body
    // Expected: HTML escaped in stored content
}
```

### Authentication & Authorization
```csharp
[Fact]
public async Task Security_Admin_Endpoints_Require_Auth()
{
    // Test: GET /admin/contacts without auth
    // Expected: 401 Unauthorized (when auth is implemented)
}
```

## Manual Testing Checklist

### Functional Testing
- [ ] Subscribe with valid email address
- [ ] Subscribe with invalid email address
- [ ] Subscribe with duplicate email address
- [ ] Confirm subscription with valid key
- [ ] Confirm subscription with invalid key
- [ ] Unsubscribe with valid key
- [ ] Unsubscribe when already unsubscribed
- [ ] Create contact via admin API
- [ ] Update contact via admin API
- [ ] Delete contact via admin API
- [ ] Create broadcast from markdown file
- [ ] Queue broadcast to all contacts
- [ ] Queue broadcast to tagged contacts
- [ ] View broadcast status and statistics
- [ ] Import contacts from CSV
- [ ] Apply tags to multiple contacts

### UI/Swagger Testing
- [ ] Open Swagger UI at root URL
- [ ] Test all endpoints via Swagger UI
- [ ] Verify request/response schemas
- [ ] Verify error responses

### Email Testing
- [ ] Verify confirmation email received in Mailpit
- [ ] Verify confirmation link works
- [ ] Verify unsubscribe link in broadcasts
- [ ] Verify email HTML renders correctly
- [ ] Verify email text version exists

## Test Data

### Seed Data
```sql
-- Test contacts
INSERT INTO mail.contacts (email, name, subscribed) VALUES
  ('alice@example.com', 'Alice', true),
  ('bob@example.com', 'Bob', true),
  ('charlie@example.com', 'Charlie', false);

-- Test tags
INSERT INTO mail.tags (slug, name) VALUES
  ('customer', 'Customers'),
  ('newsletter', 'Newsletter Subscribers');

-- Test broadcast
INSERT INTO mail.sequences (slug, name) VALUES
  ('welcome', 'Welcome Series');
```

## Test Execution

### Running Tests
```bash
# Run all tests
cd server
dotnet test

# Run specific test class
dotnet test --filter "ClassName~ContactTests"

# Run with coverage
dotnet test /p:CollectCoverage=true
```

### CI/CD Integration
```yaml
# GitHub Actions example
- name: Run .NET Tests
  run: |
    cd server
    dotnet test --configuration Release --logger "trx;LogFileName=test-results.trx"
```

## Expected Results

### Pass Criteria
- ✅ All unit tests pass
- ✅ All integration tests pass
- ✅ Code coverage > 70% for critical paths
- ✅ No critical or high severity security issues
- ✅ Performance benchmarks met
- ✅ All user stories validated

### Known Issues
- Background send service tests may be flaky (timing dependent)
- External SMTP service tests require network connectivity
- Large dataset tests may timeout on slower machines

## Test Metrics

### Coverage Goals
- Models: 80%
- Services: 75%
- Data Layer: 80%
- API Controllers: 70%
- Overall: 70%

### Performance Benchmarks
- API response time: < 200ms (95th percentile)
- Database queries: < 100ms (95th percentile)
- 10K message queue: < 30 seconds
- Bulk import 1K contacts: < 10 seconds

## Appendix

### Test Utilities
Create test helper classes for:
- Database reset and seeding
- Test HTTP client factory
- Mock email sender
- Test data generators

### Example Test Structure
```csharp
public class ContactApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public ContactApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Test_Example()
    {
        // Arrange
        var contact = new { email = "test@example.com" };
        
        // Act
        var response = await _client.PostAsJsonAsync("/subscribe", contact);
        
        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
}
```

---

**Document Version**: 1.0  
**Last Updated**: 2024  
**Owner**: Test Team

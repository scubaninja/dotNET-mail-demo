# Integration Test Plan - Tailwind Traders Mail Service

## Overview

This document describes the integration testing strategy for the complete Tailwind Traders Mail Service system. Integration tests verify that all components (Server API, CLI, Jobs Service, and Database) work together correctly to deliver end-to-end functionality.

## Purpose

Integration tests validate:
- Cross-component communication
- Data flow through the entire system
- API-to-database interactions
- CLI-to-API interactions
- Jobs-to-database interactions
- End-to-end user workflows
- System behavior under realistic conditions

## Test Environment

### Complete System Setup

```bash
# 1. Start PostgreSQL
brew services start postgresql@15
# or
docker run -d --name postgres -p 5432:5432 \
  -e POSTGRES_PASSWORD=postgres postgres:15

# 2. Setup database
cd db
psql postgres -c "CREATE DATABASE tailwind;"
psql tailwind < db.sql
psql tailwind < seed.sql

# 3. Start Mailpit (email catcher)
docker run -d --name mailpit -p 8025:8025 -p 1025:1025 axllent/mailpit

# 4. Configure environment variables
export DATABASE_URL="postgres://postgres:postgres@localhost/tailwind"
export SMTP_HOST="localhost"
export SMTP_PORT="1025"
export DEFAULT_FROM="noreply@tailwind.dev"
export SEND_WORKER="local"
export API_ROOT="http://localhost:5000/admin"
export MESSAGES_TYPE="test"

# 5. Start Server API
cd server
dotnet watch

# 6. Jobs service (for background processing)
cd jobs
# Run as needed via mage commands
```

### Docker Compose Setup (Alternative)

```yaml
# docker-compose.test.yml
version: '3.8'

services:
  postgres:
    image: postgres:15
    environment:
      POSTGRES_DB: tailwind
      POSTGRES_PASSWORD: postgres
    ports:
      - "5432:5432"
    volumes:
      - ./db/db.sql:/docker-entrypoint-initdb.d/01-schema.sql
      - ./db/seed.sql:/docker-entrypoint-initdb.d/02-seed.sql

  mailpit:
    image: axllent/mailpit
    ports:
      - "8025:8025"
      - "1025:1025"

  server:
    build:
      context: ./server
    environment:
      DATABASE_URL: postgres://postgres:postgres@postgres/tailwind
      SMTP_HOST: mailpit
      SMTP_PORT: 1025
      DEFAULT_FROM: noreply@tailwind.dev
      SEND_WORKER: local
    ports:
      - "5000:5000"
    depends_on:
      - postgres
      - mailpit
```

## Integration Test Scenarios

### Scenario 1: Complete Broadcast Workflow

**Description**: Create, queue, and send a broadcast to multiple contacts

**Components Involved**: CLI → Server API → Database → Jobs Service → SMTP

**Test Steps**:
```bash
# 1. Initialize CLI
cd cli
node bin/mdmail.js init

# 2. Create broadcast via CLI
node bin/mdmail.js broadcast new "Monthly Newsletter"

# 3. Edit the broadcast file
# mail/broadcasts/monthly-newsletter.md
cat > mail/broadcasts/monthly-newsletter.md << 'EOF'
---
Subject: "Monthly Newsletter - January 2024"
Slug: "monthly-newsletter-jan-2024"
Summary: "Your monthly update from Tailwind Traders"
SendToTag: "*"
---

# Hello from Tailwind Traders!

Welcome to our monthly newsletter.

## What's New

- Feature 1
- Feature 2
- Feature 3

Stay tuned for more updates!
EOF

# 4. Validate broadcast
node bin/mdmail.js broadcast validate

# 5. Send broadcast via CLI
node bin/mdmail.js broadcast send

# 6. Verify in database
psql tailwind << EOF
SELECT 
    b.id,
    e.subject,
    COUNT(m.id) as message_count
FROM mail.broadcasts b
INNER JOIN mail.emails e ON e.id = b.email_id
LEFT JOIN mail.messages m ON m.broadcast_id = b.id
WHERE e.slug = 'monthly-newsletter-jan-2024'
GROUP BY b.id, e.subject;
EOF

# 7. Process messages via Jobs service
cd jobs
mage messages:send

# 8. Verify emails in Mailpit
curl http://localhost:8025/api/v1/messages | jq '.messages[] | {to, subject}'
```

**Expected Results**:
- ✅ Broadcast file created with correct frontmatter
- ✅ Validation succeeds with no errors
- ✅ API accepts and stores broadcast
- ✅ Messages queued for all subscribed contacts
- ✅ Jobs service sends all messages
- ✅ Emails appear in Mailpit
- ✅ Database updated with send status

**Verification Queries**:
```sql
-- Check broadcast was created
SELECT * FROM mail.broadcasts 
WHERE email_id IN (SELECT id FROM mail.emails WHERE slug = 'monthly-newsletter-jan-2024');

-- Check messages were queued
SELECT COUNT(*) FROM mail.messages WHERE broadcast_id = 
  (SELECT b.id FROM mail.broadcasts b 
   INNER JOIN mail.emails e ON e.id = b.email_id 
   WHERE e.slug = 'monthly-newsletter-jan-2024');

-- Check message status
SELECT 
    status,
    COUNT(*) as count
FROM mail.messages 
WHERE broadcast_id = 
  (SELECT b.id FROM mail.broadcasts b 
   INNER JOIN mail.emails e ON e.id = b.email_id 
   WHERE e.slug = 'monthly-newsletter-jan-2024')
GROUP BY status;
```

---

### Scenario 2: Contact Subscription Lifecycle

**Description**: Complete contact lifecycle from signup to unsubscribe

**Components Involved**: Server API → Database → Email Service

**Test Steps**:
```bash
# 1. Subscribe via API
curl -X POST http://localhost:5000/subscribe \
  -H "Content-Type: application/json" \
  -d '{"email": "integration-test@example.com", "name": "Integration Test"}'

# 2. Verify contact created
psql tailwind -c "SELECT email, subscribed, key FROM mail.contacts WHERE email = 'integration-test@example.com';"

# 3. Check confirmation email sent
curl http://localhost:8025/api/v1/messages | jq '.messages[] | select(.To[0].Address == "integration-test@example.com")'

# 4. Extract confirmation key from database
KEY=$(psql tailwind -t -c "SELECT key FROM mail.contacts WHERE email = 'integration-test@example.com';")

# 5. Confirm subscription
curl http://localhost:5000/confirm/$KEY

# 6. Verify subscription confirmed
psql tailwind -c "SELECT email, subscribed FROM mail.contacts WHERE email = 'integration-test@example.com';"

# 7. Check activity logged
psql tailwind -c "SELECT description FROM mail.activity WHERE contact_id = (SELECT id FROM mail.contacts WHERE email = 'integration-test@example.com');"

# 8. Unsubscribe
curl http://localhost:5000/unsubscribe/$KEY

# 9. Verify unsubscribed
psql tailwind -c "SELECT email, subscribed FROM mail.contacts WHERE email = 'integration-test@example.com';"

# 10. Check activity logged
psql tailwind -c "SELECT description FROM mail.activity WHERE contact_id = (SELECT id FROM mail.contacts WHERE email = 'integration-test@example.com') ORDER BY created_at;"
```

**Expected Results**:
- ✅ Contact created with `subscribed=false`
- ✅ Unique key generated
- ✅ Confirmation email sent
- ✅ Confirmation updates `subscribed=true`
- ✅ Subscription activity logged
- ✅ Unsubscribe updates `subscribed=false`
- ✅ Unsubscribe activity logged

---

### Scenario 3: Tagged Broadcast Distribution

**Description**: Send broadcast only to contacts with specific tag

**Components Involved**: CLI → Server API → Database

**Test Steps**:
```bash
# 1. Create test contacts with different tags
curl -X POST http://localhost:5000/admin/contacts \
  -H "Content-Type: application/json" \
  -d '{"email": "vip1@example.com", "name": "VIP 1", "subscribed": true}'

curl -X POST http://localhost:5000/admin/contacts \
  -H "Content-Type: application/json" \
  -d '{"email": "vip2@example.com", "name": "VIP 2", "subscribed": true}'

curl -X POST http://localhost:5000/admin/contacts \
  -H "Content-Type: application/json" \
  -d '{"email": "regular@example.com", "name": "Regular", "subscribed": true}'

# 2. Create VIP tag
psql tailwind -c "INSERT INTO mail.tags (slug, name) VALUES ('vip', 'VIP Customers') ON CONFLICT (slug) DO NOTHING;"

# 3. Tag VIP contacts
psql tailwind << EOF
INSERT INTO mail.tagged (contact_id, tag_id)
SELECT c.id, t.id 
FROM mail.contacts c, mail.tags t
WHERE c.email IN ('vip1@example.com', 'vip2@example.com')
AND t.slug = 'vip'
ON CONFLICT DO NOTHING;
EOF

# 4. Create VIP broadcast
cd cli
cat > mail/broadcasts/vip-offer.md << 'EOF'
---
Subject: "Exclusive VIP Offer"
Slug: "vip-offer"
Summary: "Special offer for our VIP customers"
SendToTag: "vip"
---

# Exclusive VIP Offer

Thank you for being a valued VIP customer!
EOF

# 5. Send VIP broadcast
node bin/mdmail.js broadcast send

# 6. Verify only VIP contacts received messages
psql tailwind << EOF
SELECT 
    c.email,
    c.name,
    EXISTS(SELECT 1 FROM mail.tagged t WHERE t.contact_id = c.id AND t.tag_id = (SELECT id FROM mail.tags WHERE slug = 'vip')) as is_vip,
    EXISTS(SELECT 1 FROM mail.messages m WHERE m.contact_id = c.id AND m.broadcast_id = (SELECT b.id FROM mail.broadcasts b INNER JOIN mail.emails e ON e.id = b.email_id WHERE e.slug = 'vip-offer')) as received_message
FROM mail.contacts c
WHERE c.email IN ('vip1@example.com', 'vip2@example.com', 'regular@example.com')
ORDER BY c.email;
EOF
```

**Expected Results**:
- ✅ VIP tag created
- ✅ Only VIP contacts tagged
- ✅ Broadcast created with `SendToTag: "vip"`
- ✅ Messages queued only for VIP contacts (2 messages)
- ✅ Regular contact does not receive message

---

### Scenario 4: Bulk Contact Import

**Description**: Import multiple contacts from CSV via CLI

**Components Involved**: CLI → Server API → Database

**Test Steps**:
```bash
# 1. Create CSV file
cat > /tmp/contacts.csv << 'EOF'
email,name,subscribed
bulk1@example.com,Bulk User 1,true
bulk2@example.com,Bulk User 2,true
bulk3@example.com,Bulk User 3,false
EOF

# 2. Import via API
curl -X POST http://localhost:5000/admin/bulk/import \
  -F "file=@/tmp/contacts.csv"

# 3. Verify imported contacts
psql tailwind -c "SELECT email, name, subscribed FROM mail.contacts WHERE email LIKE 'bulk%' ORDER BY email;"

# 4. Test duplicate import (should handle gracefully)
curl -X POST http://localhost:5000/admin/bulk/import \
  -F "file=@/tmp/contacts.csv"

# 5. Verify no duplicates created
psql tailwind -c "SELECT email, COUNT(*) FROM mail.contacts WHERE email LIKE 'bulk%' GROUP BY email HAVING COUNT(*) > 1;"
```

**Expected Results**:
- ✅ All valid contacts imported
- ✅ Subscription status respected
- ✅ Duplicate imports handled (no duplicates created)
- ✅ Import result summary returned

---

### Scenario 5: 10K Contact Broadcast (Performance)

**Description**: Queue and send broadcast to 10,000 contacts

**Components Involved**: Server API → Database → Jobs Service

**Test Steps**:
```bash
# 1. Create 10,001 test contacts (one unsubscribed)
psql tailwind << EOF
INSERT INTO mail.contacts (email, name, subscribed)
SELECT 
    'perftest' || generate_series || '@example.com',
    'Perf Test ' || generate_series,
    (generate_series <= 10000)  -- First 10,000 subscribed, last one unsubscribed
FROM generate_series(1, 10001);
EOF

# 2. Create performance test broadcast
curl -X POST http://localhost:5000/admin/broadcasts \
  -H "Content-Type: application/json" \
  -d '{
    "subject": "Performance Test Broadcast",
    "slug": "perf-test",
    "summary": "Testing 10K send",
    "sendToTag": "*",
    "body": "# Performance Test\n\nThis is a performance test broadcast."
  }'

# 3. Queue broadcast (measure time)
time curl -X POST http://localhost:5000/admin/broadcasts/{id}/queue

# 4. Verify message count
psql tailwind << EOF
SELECT 
    COUNT(*) as total_messages,
    COUNT(CASE WHEN status = 'queued' THEN 1 END) as queued,
    COUNT(CASE WHEN status = 'sent' THEN 1 END) as sent
FROM mail.messages m
INNER JOIN mail.broadcasts b ON b.id = m.broadcast_id
INNER JOIN mail.emails e ON e.id = b.email_id
WHERE e.slug = 'perf-test';
EOF

# 5. Process messages (measure time)
cd jobs
time mage messages:send

# 6. Verify all sent
psql tailwind << EOF
SELECT 
    status,
    COUNT(*) as count
FROM mail.messages m
INNER JOIN mail.broadcasts b ON b.id = m.broadcast_id
INNER JOIN mail.emails e ON e.id = b.email_id
WHERE e.slug = 'perf-test'
GROUP BY status;
EOF

# 7. Cleanup
psql tailwind -c "DELETE FROM mail.contacts WHERE email LIKE 'perftest%';"
```

**Expected Results**:
- ✅ All 10,001 contacts created
- ✅ Exactly 10,000 messages queued (unsubscribed excluded)
- ✅ Queue operation completes < 30 seconds
- ✅ All messages sent successfully
- ✅ No database errors or timeouts

**Performance Benchmarks**:
- Contact creation: < 10 seconds
- Message queueing: < 30 seconds
- Message sending: < 5 minutes (depends on SMTP)
- Database queries remain fast

---

### Scenario 6: AI-Powered Broadcast Generation

**Description**: Generate broadcast content using AI

**Components Involved**: CLI → OpenAI API → Server API

**Test Steps**:
```bash
# 1. Set OpenAI API key
export OPENAI_API_KEY="your-key-here"

# 2. Create AI-powered broadcast
cd cli
node bin/mdmail.js broadcast new "Summer Sale Announcement" -p

# 3. Check generated content
cat mail/broadcasts/summer-sale-announcement.md

# 4. Regenerate with different prompt
# Edit the Prompt field in frontmatter, then:
node bin/mdmail.js broadcast regenerate

# 5. Validate and send
node bin/mdmail.js broadcast validate
node bin/mdmail.js broadcast send
```

**Expected Results**:
- ✅ Broadcast created with AI-generated content
- ✅ Frontmatter includes Prompt field
- ✅ Content is relevant to subject
- ✅ Regeneration updates body, preserves frontmatter
- ✅ Validation succeeds
- ✅ Send succeeds

---

## Cross-Component Integration Tests

### API-to-Database Integration

```csharp
[Fact]
public async Task API_Create_Contact_Should_Persist_In_Database()
{
    // Test: POST to API creates database record
    var client = _factory.CreateClient();
    var response = await client.PostAsJsonAsync("/subscribe", new {
        email = "api-db-test@example.com",
        name = "API DB Test"
    });
    
    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    
    // Verify in database
    using var connection = new NpgsqlConnection(connectionString);
    var contact = await connection.QuerySingleOrDefaultAsync<Contact>(
        "SELECT * FROM mail.contacts WHERE email = @Email",
        new { Email = "api-db-test@example.com" }
    );
    
    Assert.NotNull(contact);
    Assert.Equal("API DB Test", contact.Name);
}
```

### CLI-to-API Integration

```javascript
describe('CLI to API Integration', () => {
  it('should send broadcast via API', async () => {
    // Create broadcast file
    const broadcast = {
      subject: 'Integration Test',
      slug: 'integration-test',
      summary: 'Test summary',
      sendToTag: '*',
      body: '# Test\n\nIntegration test body'
    };
    
    // Send via CLI utility
    const response = await sendBroadcast(broadcast);
    
    // Verify response
    expect(response.status).to.equal(201);
    expect(response.data).to.have.property('id');
    
    // Verify in API
    const getResponse = await axios.get(`${API_ROOT}/broadcasts/${response.data.id}`);
    expect(getResponse.data.subject).to.equal('Integration Test');
  });
});
```

### Jobs-to-Database Integration

```go
func TestJobs_ProcessMessages_Updates_Database(t *testing.T) {
    // Setup: Create test messages in database
    db := setupTestDB(t)
    defer db.Close()
    
    messageID := insertTestMessage(t, db)
    
    // Action: Process messages via Jobs service
    sender := senders.NewTestSender()
    err := processMessage(db, sender, messageID)
    
    // Verify: Database updated
    var status string
    err = db.QueryRow("SELECT status FROM mail.messages WHERE id = $1", messageID).Scan(&status)
    require.NoError(t, err)
    assert.Equal(t, "sent", status)
}
```

## System Health Checks

### Health Check Endpoints

```bash
# Server API health
curl http://localhost:5000/health

# Database connectivity
psql tailwind -c "SELECT 1;"

# Mailpit health
curl http://localhost:8025/api/v1/info

# Check all services
./scripts/health-check.sh
```

### Health Check Script

```bash
#!/bin/bash
# scripts/health-check.sh

echo "Checking system health..."

# Database
if psql tailwind -c "SELECT 1;" > /dev/null 2>&1; then
    echo "✅ Database: OK"
else
    echo "❌ Database: FAILED"
fi

# Server API
if curl -f http://localhost:5000/health > /dev/null 2>&1; then
    echo "✅ Server API: OK"
else
    echo "❌ Server API: FAILED"
fi

# Mailpit
if curl -f http://localhost:8025/api/v1/info > /dev/null 2>&1; then
    echo "✅ Mailpit: OK"
else
    echo "❌ Mailpit: FAILED"
fi
```

## Data Consistency Tests

### Test: Contact-Message Consistency

```sql
-- Verify no orphaned messages
SELECT COUNT(*) as orphaned_messages
FROM mail.messages m
WHERE NOT EXISTS (
    SELECT 1 FROM mail.contacts c WHERE c.id = m.contact_id
);
-- Expected: 0

-- Verify no messages for unsubscribed contacts in broadcasts
SELECT COUNT(*) as invalid_messages
FROM mail.messages m
INNER JOIN mail.contacts c ON c.id = m.contact_id
WHERE c.subscribed = false
AND m.broadcast_id IS NOT NULL;
-- Expected: 0 (transactional emails ok)
```

### Test: Broadcast-Message Consistency

```sql
-- Verify broadcast message counts are accurate
SELECT 
    b.id,
    b.message_count as reported_count,
    COUNT(m.id) as actual_count,
    (b.message_count = COUNT(m.id)) as counts_match
FROM mail.broadcasts b
LEFT JOIN mail.messages m ON m.broadcast_id = b.id
GROUP BY b.id, b.message_count
HAVING b.message_count != COUNT(m.id);
-- Expected: 0 rows (all counts match)
```

## Error Handling Tests

### Test: API Error Recovery

```bash
# Test: Database connection failure
# Stop PostgreSQL
brew services stop postgresql@15

# Try API call
curl -X GET http://localhost:5000/admin/contacts
# Expected: 503 Service Unavailable with helpful error

# Restart PostgreSQL
brew services start postgresql@15

# Verify recovery
curl -X GET http://localhost:5000/admin/contacts
# Expected: 200 OK
```

### Test: Email Service Failure

```bash
# Test: SMTP failure handling
# Stop Mailpit
docker stop mailpit

# Try to send email
cd jobs
mage email:sendOne test@example.com
# Expected: Error logged, retry attempted

# Restart Mailpit
docker start mailpit

# Verify retry succeeds
mage email:sendOne test@example.com
# Expected: Success
```

## Performance Integration Tests

### Load Test: Concurrent API Requests

```bash
# Use Apache Bench or similar
ab -n 1000 -c 10 http://localhost:5000/admin/contacts
# Expected: All requests succeed
# Expected: Average response time < 200ms
```

### Load Test: Bulk Operations

```bash
# Import 10,000 contacts
time curl -X POST http://localhost:5000/admin/bulk/import \
  -F "file=@large-contacts.csv"
# Expected: Complete within 60 seconds

# Queue broadcast to all contacts
time curl -X POST http://localhost:5000/admin/broadcasts/{id}/queue
# Expected: Complete within 30 seconds
```

## Security Integration Tests

### Test: SQL Injection Prevention

```bash
# Try SQL injection in API
curl -X POST http://localhost:5000/subscribe \
  -H "Content-Type: application/json" \
  -d '{"email": "test@example.com\"; DROP TABLE mail.contacts; --"}'

# Verify table still exists
psql tailwind -c "\dt mail.contacts"
# Expected: Table exists, no data loss
```

### Test: XSS Prevention

```bash
# Try XSS in broadcast content
cd cli
cat > mail/broadcasts/xss-test.md << 'EOF'
---
Subject: "XSS Test"
Slug: "xss-test"
Summary: "Testing XSS prevention"
SendToTag: "*"
---

<script>alert('XSS')</script>

**Bold text**
EOF

node bin/mdmail.js broadcast send

# Check HTML output in database
psql tailwind -c "SELECT html FROM mail.emails WHERE slug = 'xss-test';"
# Expected: Script tags escaped or removed
```

## Manual Integration Testing Checklist

### Complete Workflow Tests
- [ ] Create broadcast in CLI, send via API, process via Jobs
- [ ] Subscribe, confirm, receive broadcast, unsubscribe
- [ ] Import contacts, tag them, send targeted broadcast
- [ ] Create 10K contacts, queue broadcast, verify all sent
- [ ] Use AI to generate broadcast, validate, send

### Error Recovery Tests
- [ ] Database disconnect/reconnect during operation
- [ ] SMTP failure and retry
- [ ] API timeout and retry
- [ ] Partial broadcast send (some succeed, some fail)

### Data Consistency Tests
- [ ] Verify no orphaned records after operations
- [ ] Verify counts match reality
- [ ] Verify timestamps are accurate
- [ ] Verify activity logs are complete

### Cross-Browser/Client Tests
- [ ] Swagger UI works in Chrome, Firefox, Safari
- [ ] CLI works in bash, zsh, PowerShell
- [ ] Email renders correctly in Gmail, Outlook, Apple Mail

## CI/CD Integration Testing

### GitHub Actions Example

```yaml
name: Integration Tests

on: [push, pull_request]

jobs:
  integration-test:
    runs-on: ubuntu-latest
    
    services:
      postgres:
        image: postgres:15
        env:
          POSTGRES_DB: tailwind
          POSTGRES_PASSWORD: postgres
        ports:
          - 5432:5432
      
      mailpit:
        image: axllent/mailpit
        ports:
          - 8025:8025
          - 1025:1025
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup Database
      run: |
        psql -h localhost -U postgres -d tailwind -f db/db.sql
        psql -h localhost -U postgres -d tailwind -f db/seed.sql
      env:
        PGPASSWORD: postgres
    
    - name: Start Server
      run: |
        cd server
        dotnet run &
        sleep 10
      env:
        DATABASE_URL: postgres://postgres:postgres@localhost/tailwind
        SMTP_HOST: localhost
        SMTP_PORT: 1025
    
    - name: Run Integration Tests
      run: |
        cd test/integration
        ./run-all-tests.sh
```

## Test Reporting

### Metrics to Track
- Test execution time
- Number of tests passed/failed
- Component interaction success rate
- API response times
- Database query times
- Email delivery rate
- Error rates by component

### Test Report Template

```markdown
# Integration Test Report

**Date**: 2024-XX-XX
**Test Suite**: Full System Integration
**Environment**: Test

## Summary
- Total Tests: XX
- Passed: XX
- Failed: XX
- Skipped: XX

## Test Results by Component
- Server API: XX/XX passed
- CLI: XX/XX passed
- Jobs: XX/XX passed
- Database: XX/XX passed

## Performance Metrics
- Average API Response Time: XXms
- 10K Broadcast Queue Time: XXs
- Database Query Time (avg): XXms

## Issues Found
1. Issue description
   - Component: X
   - Severity: High/Medium/Low
   - Status: Open/Fixed

## Recommendations
- Recommendation 1
- Recommendation 2
```

## Expected Results

### Pass Criteria
- ✅ All integration scenarios pass
- ✅ All components communicate correctly
- ✅ Data consistency maintained
- ✅ Performance benchmarks met
- ✅ Error handling works correctly
- ✅ Security tests pass

### Performance Benchmarks
- API response time: < 200ms (95th percentile)
- 10K broadcast queue: < 30 seconds
- 10K message send: < 5 minutes
- Database queries: < 100ms
- System recovery: < 30 seconds

---

**Document Version**: 1.0  
**Last Updated**: 2024  
**Owner**: Test Team

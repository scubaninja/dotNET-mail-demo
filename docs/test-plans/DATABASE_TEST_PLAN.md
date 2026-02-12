# Database Test Plan - Tailwind Traders Mail Service

## Component Overview

The Database component is a PostgreSQL database that stores all mail service data including contacts, broadcasts, messages, tags, sequences, and activity logs. It uses a schema-driven design with foreign keys, indexes, and constraints to ensure data integrity.

## Technology Stack

- **PostgreSQL 15+** - Relational database
- **SQL/PLpgSQL** - Schema and functions
- **Dapper** - .NET data access
- **Go database/sql** - Go data access
- **psql** - Command-line client

## Test Scope

### Database Objects to Test

1. **Schema: mail**
   - All tables and relationships
   - Indexes and constraints
   - Default values and triggers

2. **Tables**
   - contacts
   - activity
   - tags
   - tagged (junction table)
   - sequences
   - subscriptions (junction table)
   - emails
   - broadcasts
   - messages

3. **Data Integrity**
   - Primary keys
   - Foreign keys
   - Unique constraints
   - Check constraints
   - Not null constraints

4. **Functions and Procedures**
   - Custom database functions (if any)
   - Stored procedures for complex operations

## Test Environment Setup

### Prerequisites
```bash
# Install PostgreSQL
brew install postgresql@15  # macOS
# or
sudo apt-get install postgresql-15  # Ubuntu

# Start PostgreSQL
brew services start postgresql@15
# or
sudo systemctl start postgresql

# Create test database
psql postgres -c "CREATE DATABASE tailwind_test;"
```

### Database Setup
```bash
cd db

# Create schema
psql tailwind_test < db.sql

# Load seed data
psql tailwind_test < seed.sql
```

### Test Data Reset Script
```bash
#!/bin/bash
# reset-test-db.sh

psql tailwind_test << EOF
DROP SCHEMA IF EXISTS mail CASCADE;
EOF

psql tailwind_test < db.sql
psql tailwind_test < seed.sql
```

## Schema Tests

### 1. Schema Structure Tests

```sql
-- Test: mail schema exists
SELECT schema_name 
FROM information_schema.schemata 
WHERE schema_name = 'mail';
-- Expected: 1 row

-- Test: All tables exist
SELECT table_name 
FROM information_schema.tables 
WHERE table_schema = 'mail'
ORDER BY table_name;
-- Expected: contacts, activity, tags, tagged, sequences, 
--           subscriptions, emails, broadcasts, messages

-- Test: Verify column count for each table
SELECT table_name, COUNT(*) as column_count
FROM information_schema.columns
WHERE table_schema = 'mail'
GROUP BY table_name
ORDER BY table_name;
```

### 2. Contacts Table Tests

```sql
-- Test: contacts table structure
SELECT column_name, data_type, is_nullable, column_default
FROM information_schema.columns
WHERE table_schema = 'mail' AND table_name = 'contacts'
ORDER BY ordinal_position;
-- Expected columns: id, email, key, subscribed, name, created_at, updated_at

-- Test: contacts primary key
SELECT constraint_name, constraint_type
FROM information_schema.table_constraints
WHERE table_schema = 'mail' AND table_name = 'contacts'
AND constraint_type = 'PRIMARY KEY';
-- Expected: Primary key on id

-- Test: contacts unique constraint on email
SELECT constraint_name, constraint_type
FROM information_schema.table_constraints
WHERE table_schema = 'mail' AND table_name = 'contacts'
AND constraint_type = 'UNIQUE';
-- Expected: Unique constraint on email

-- Test: contacts default values
INSERT INTO mail.contacts (email, name) 
VALUES ('test@example.com', 'Test User');

SELECT 
    subscribed = true as subscribed_default,
    key IS NOT NULL as key_generated,
    created_at IS NOT NULL as created_at_set,
    updated_at IS NOT NULL as updated_at_set
FROM mail.contacts 
WHERE email = 'test@example.com';
-- Expected: All true

-- Cleanup
DELETE FROM mail.contacts WHERE email = 'test@example.com';
```

### 3. Activity Table Tests

```sql
-- Test: activity table structure
SELECT column_name, data_type, is_nullable
FROM information_schema.columns
WHERE table_schema = 'mail' AND table_name = 'activity'
ORDER BY ordinal_position;

-- Test: activity foreign key to contacts
SELECT
    tc.constraint_name,
    kcu.column_name,
    ccu.table_name AS foreign_table_name,
    ccu.column_name AS foreign_column_name
FROM information_schema.table_constraints AS tc
JOIN information_schema.key_column_usage AS kcu
    ON tc.constraint_name = kcu.constraint_name
JOIN information_schema.constraint_column_usage AS ccu
    ON ccu.constraint_name = tc.constraint_name
WHERE tc.table_schema = 'mail' 
    AND tc.table_name = 'activity'
    AND tc.constraint_type = 'FOREIGN KEY';
-- Expected: contact_id references contacts(id)
```

### 4. Tags and Tagged Tables Tests

```sql
-- Test: tags table structure
SELECT column_name, data_type, is_nullable
FROM information_schema.columns
WHERE table_schema = 'mail' AND table_name = 'tags'
ORDER BY ordinal_position;

-- Test: tagged junction table
SELECT column_name, data_type
FROM information_schema.columns
WHERE table_schema = 'mail' AND table_name = 'tagged'
ORDER BY ordinal_position;

-- Test: tagged composite primary key
SELECT constraint_name, constraint_type
FROM information_schema.table_constraints
WHERE table_schema = 'mail' AND table_name = 'tagged'
AND constraint_type = 'PRIMARY KEY';
-- Expected: Composite key on (contact_id, tag_id)

-- Test: tagged foreign keys
SELECT
    kcu.column_name,
    ccu.table_name AS foreign_table_name
FROM information_schema.table_constraints AS tc
JOIN information_schema.key_column_usage AS kcu
    ON tc.constraint_name = kcu.constraint_name
JOIN information_schema.constraint_column_usage AS ccu
    ON ccu.constraint_name = tc.constraint_name
WHERE tc.table_schema = 'mail' 
    AND tc.table_name = 'tagged'
    AND tc.constraint_type = 'FOREIGN KEY'
ORDER BY kcu.column_name;
-- Expected: contact_id -> contacts, tag_id -> tags
```

### 5. Messages Table Tests

```sql
-- Test: messages table structure
SELECT column_name, data_type, is_nullable
FROM information_schema.columns
WHERE table_schema = 'mail' AND table_name = 'messages'
ORDER BY ordinal_position;

-- Test: messages foreign keys
SELECT
    kcu.column_name,
    ccu.table_name AS foreign_table_name
FROM information_schema.table_constraints AS tc
JOIN information_schema.key_column_usage AS kcu
    ON tc.constraint_name = kcu.constraint_name
JOIN information_schema.constraint_column_usage AS ccu
    ON ccu.constraint_name = tc.constraint_name
WHERE tc.table_schema = 'mail' 
    AND tc.table_name = 'messages'
    AND tc.constraint_type = 'FOREIGN KEY';
```

## Data Integrity Tests

### 1. Primary Key Tests

```sql
-- Test: Insert duplicate primary key (should fail)
BEGIN;
INSERT INTO mail.contacts (id, email) VALUES (1, 'test1@example.com');
INSERT INTO mail.contacts (id, email) VALUES (1, 'test2@example.com');
ROLLBACK;
-- Expected: Second insert fails with duplicate key error

-- Test: Primary key auto-increment
INSERT INTO mail.contacts (email) VALUES ('auto1@example.com');
INSERT INTO mail.contacts (email) VALUES ('auto2@example.com');
SELECT id FROM mail.contacts WHERE email LIKE 'auto%' ORDER BY id;
-- Expected: Sequential IDs
DELETE FROM mail.contacts WHERE email LIKE 'auto%';
```

### 2. Foreign Key Tests

```sql
-- Test: Insert activity with invalid contact_id (should fail)
BEGIN;
INSERT INTO mail.activity (contact_id, key, description)
VALUES (99999, 'test-key', 'Test activity');
ROLLBACK;
-- Expected: Foreign key violation error

-- Test: Insert activity with valid contact_id
BEGIN;
INSERT INTO mail.contacts (email) VALUES ('fk-test@example.com');
INSERT INTO mail.activity (contact_id, key, description)
SELECT id, 'test-key', 'Test activity'
FROM mail.contacts WHERE email = 'fk-test@example.com';
ROLLBACK;
-- Expected: Success
```

### 3. Unique Constraint Tests

```sql
-- Test: Insert duplicate email (should fail)
BEGIN;
INSERT INTO mail.contacts (email) VALUES ('duplicate@example.com');
INSERT INTO mail.contacts (email) VALUES ('duplicate@example.com');
ROLLBACK;
-- Expected: Second insert fails with unique violation

-- Test: Update to duplicate email (should fail)
BEGIN;
INSERT INTO mail.contacts (email) VALUES ('unique1@example.com');
INSERT INTO mail.contacts (email) VALUES ('unique2@example.com');
UPDATE mail.contacts SET email = 'unique1@example.com'
WHERE email = 'unique2@example.com';
ROLLBACK;
-- Expected: Update fails with unique violation
```

### 4. Not Null Constraint Tests

```sql
-- Test: Insert contact without email (should fail)
BEGIN;
INSERT INTO mail.contacts (name) VALUES ('No Email User');
ROLLBACK;
-- Expected: Not null violation on email

-- Test: Insert contact without key (should succeed with default)
BEGIN;
INSERT INTO mail.contacts (email) VALUES ('default-key@example.com');
SELECT key IS NOT NULL FROM mail.contacts WHERE email = 'default-key@example.com';
ROLLBACK;
-- Expected: key is generated (not null)
```

### 5. Cascade Delete Tests

```sql
-- Test: Delete contact cascades to activity
BEGIN;
INSERT INTO mail.contacts (email) VALUES ('cascade-test@example.com');
INSERT INTO mail.activity (contact_id, key, description)
SELECT id, 'test', 'Test' FROM mail.contacts WHERE email = 'cascade-test@example.com';
DELETE FROM mail.contacts WHERE email = 'cascade-test@example.com';
SELECT COUNT(*) FROM mail.activity WHERE key = 'test';
ROLLBACK;
-- Expected: 0 (activity deleted with contact)

-- Test: Delete tag cascades to tagged
BEGIN;
INSERT INTO mail.tags (slug, name) VALUES ('test-tag', 'Test Tag');
INSERT INTO mail.contacts (email) VALUES ('tag-test@example.com');
INSERT INTO mail.tagged (contact_id, tag_id)
SELECT c.id, t.id FROM mail.contacts c, mail.tags t
WHERE c.email = 'tag-test@example.com' AND t.slug = 'test-tag';
DELETE FROM mail.tags WHERE slug = 'test-tag';
SELECT COUNT(*) FROM mail.tagged WHERE tag_id IN 
    (SELECT id FROM mail.tags WHERE slug = 'test-tag');
ROLLBACK;
-- Expected: 0 (tagged entries deleted)
```

## CRUD Operation Tests

### 1. Contact CRUD Tests

```sql
-- Test: Create contact
INSERT INTO mail.contacts (email, name, subscribed)
VALUES ('crud-test@example.com', 'CRUD Test', true)
RETURNING id, email, subscribed;
-- Expected: New contact returned

-- Test: Read contact
SELECT * FROM mail.contacts 
WHERE email = 'crud-test@example.com';
-- Expected: 1 row with correct data

-- Test: Update contact
UPDATE mail.contacts 
SET name = 'Updated Name', subscribed = false
WHERE email = 'crud-test@example.com'
RETURNING name, subscribed, updated_at;
-- Expected: Updated values, updated_at changed

-- Test: Delete contact
DELETE FROM mail.contacts 
WHERE email = 'crud-test@example.com'
RETURNING id;
-- Expected: Deleted contact id returned

-- Verify deletion
SELECT COUNT(*) FROM mail.contacts 
WHERE email = 'crud-test@example.com';
-- Expected: 0
```

### 2. Tag Management Tests

```sql
-- Test: Create tag
INSERT INTO mail.tags (slug, name, description)
VALUES ('newsletter', 'Newsletter', 'Newsletter subscribers')
ON CONFLICT (slug) DO NOTHING
RETURNING id, slug;

-- Test: Tag contact
WITH contact AS (
    INSERT INTO mail.contacts (email) VALUES ('tagged@example.com')
    ON CONFLICT (email) DO UPDATE SET email = EXCLUDED.email
    RETURNING id
),
tag AS (
    SELECT id FROM mail.tags WHERE slug = 'newsletter'
)
INSERT INTO mail.tagged (contact_id, tag_id)
SELECT contact.id, tag.id FROM contact, tag
ON CONFLICT DO NOTHING;

-- Test: Query contacts by tag
SELECT c.email, c.name
FROM mail.contacts c
INNER JOIN mail.tagged t ON t.contact_id = c.id
INNER JOIN mail.tags tg ON tg.id = t.tag_id
WHERE tg.slug = 'newsletter';

-- Cleanup
DELETE FROM mail.contacts WHERE email = 'tagged@example.com';
DELETE FROM mail.tags WHERE slug = 'newsletter';
```

### 3. Broadcast and Message Tests

```sql
-- Test: Create broadcast with messages
BEGIN;

-- Insert sequence
INSERT INTO mail.sequences (slug, name)
VALUES ('test-broadcast', 'Test Broadcast')
ON CONFLICT (slug) DO UPDATE SET name = EXCLUDED.name
RETURNING id;

-- Insert email template
INSERT INTO mail.emails (sequence_id, slug, subject, html)
SELECT id, 'test-email', 'Test Subject', '<p>Test body</p>'
FROM mail.sequences WHERE slug = 'test-broadcast'
RETURNING id;

-- Insert broadcast
INSERT INTO mail.broadcasts (email_id, send_to_tag, status)
SELECT id, '*', 'draft'
FROM mail.emails WHERE slug = 'test-email'
RETURNING id;

-- Create test contacts
INSERT INTO mail.contacts (email, subscribed) VALUES
    ('broadcast1@example.com', true),
    ('broadcast2@example.com', true),
    ('broadcast3@example.com', false);

-- Queue messages for subscribed contacts only
INSERT INTO mail.messages (broadcast_id, contact_id, send_to, send_from, subject, html, send_at)
SELECT 
    b.id,
    c.id,
    c.email,
    'noreply@tailwind.dev',
    e.subject,
    e.html,
    NOW()
FROM mail.broadcasts b
INNER JOIN mail.emails e ON e.id = b.email_id
CROSS JOIN mail.contacts c
WHERE c.subscribed = true
AND b.email_id IN (SELECT id FROM mail.emails WHERE slug = 'test-email');

-- Verify message count
SELECT COUNT(*) as message_count
FROM mail.messages m
INNER JOIN mail.broadcasts b ON b.id = m.broadcast_id
INNER JOIN mail.emails e ON e.id = b.email_id
WHERE e.slug = 'test-email';
-- Expected: 2 (only subscribed contacts)

ROLLBACK;
```

## Query Performance Tests

### 1. Index Verification

```sql
-- Test: Verify indexes exist
SELECT
    tablename,
    indexname,
    indexdef
FROM pg_indexes
WHERE schemaname = 'mail'
ORDER BY tablename, indexname;
-- Expected: Indexes on primary keys, foreign keys, unique constraints

-- Test: Email lookup uses index
EXPLAIN ANALYZE
SELECT * FROM mail.contacts WHERE email = 'test@example.com';
-- Expected: Index Scan on contacts_email_key or similar
```

### 2. Large Dataset Query Tests

```sql
-- Test: Query performance with 10K contacts
-- First, create test data
INSERT INTO mail.contacts (email, name, subscribed)
SELECT 
    'user' || generate_series || '@example.com',
    'User ' || generate_series,
    (random() > 0.1)  -- 90% subscribed
FROM generate_series(1, 10000);

-- Test: Count subscribed contacts
EXPLAIN ANALYZE
SELECT COUNT(*) FROM mail.contacts WHERE subscribed = true;
-- Expected: < 100ms execution time

-- Test: Find contacts by tag (with proper indexes)
-- Create a tag first
INSERT INTO mail.tags (slug, name) VALUES ('performance-test', 'Performance Test');

-- Tag 1000 contacts
INSERT INTO mail.tagged (contact_id, tag_id)
SELECT c.id, t.id
FROM mail.contacts c
CROSS JOIN mail.tags t
WHERE t.slug = 'performance-test'
AND c.email LIKE 'user%@example.com'
LIMIT 1000;

-- Query tagged contacts
EXPLAIN ANALYZE
SELECT c.email
FROM mail.contacts c
INNER JOIN mail.tagged t ON t.contact_id = c.id
INNER JOIN mail.tags tg ON tg.id = t.tag_id
WHERE tg.slug = 'performance-test';
-- Expected: < 50ms execution time with proper indexes

-- Cleanup
DELETE FROM mail.contacts WHERE email LIKE 'user%@example.com';
DELETE FROM mail.tags WHERE slug = 'performance-test';
```

### 3. Join Performance Tests

```sql
-- Test: Complex join query performance
EXPLAIN ANALYZE
SELECT 
    c.email,
    COUNT(m.id) as message_count,
    COUNT(a.id) as activity_count
FROM mail.contacts c
LEFT JOIN mail.messages m ON m.contact_id = c.id
LEFT JOIN mail.activity a ON a.contact_id = c.id
GROUP BY c.id, c.email
LIMIT 100;
-- Expected: Reasonable execution time with indexes
```

## Data Validation Tests

### 1. Email Format Validation

```sql
-- Note: Add check constraint if not exists
-- ALTER TABLE mail.contacts 
-- ADD CONSTRAINT valid_email 
-- CHECK (email ~* '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$');

-- Test: Insert invalid email format (if constraint exists)
BEGIN;
INSERT INTO mail.contacts (email) VALUES ('invalid-email');
ROLLBACK;
-- Expected: Check constraint violation (if implemented)
```

### 2. Date Validation Tests

```sql
-- Test: created_at is set correctly
INSERT INTO mail.contacts (email) VALUES ('date-test@example.com');
SELECT 
    created_at IS NOT NULL,
    created_at <= NOW(),
    created_at > NOW() - INTERVAL '1 minute'
FROM mail.contacts 
WHERE email = 'date-test@example.com';
-- Expected: All true

-- Test: updated_at changes on update
UPDATE mail.contacts 
SET name = 'Updated'
WHERE email = 'date-test@example.com';

SELECT updated_at > created_at
FROM mail.contacts
WHERE email = 'date-test@example.com';
-- Expected: true (if trigger exists)

-- Cleanup
DELETE FROM mail.contacts WHERE email = 'date-test@example.com';
```

## Transaction Tests

### 1. ACID Properties Tests

```sql
-- Test: Atomicity - All or nothing
BEGIN;
INSERT INTO mail.contacts (email) VALUES ('atomic1@example.com');
INSERT INTO mail.contacts (email) VALUES ('atomic2@example.com');
INSERT INTO mail.contacts (email) VALUES ('atomic2@example.com'); -- Duplicate, should fail
COMMIT;
-- Expected: ROLLBACK, no contacts inserted

-- Verify
SELECT COUNT(*) FROM mail.contacts WHERE email LIKE 'atomic%';
-- Expected: 0

-- Test: Isolation
-- Session 1:
BEGIN;
UPDATE mail.contacts SET name = 'Session 1' WHERE id = 1;
-- Don't commit yet

-- Session 2 (in parallel):
SELECT name FROM mail.contacts WHERE id = 1;
-- Expected: Original name (not 'Session 1')

-- Session 1:
COMMIT;

-- Session 2:
SELECT name FROM mail.contacts WHERE id = 1;
-- Expected: 'Session 1' (now visible)
```

### 2. Deadlock Tests

```sql
-- Test: Detect and handle deadlocks
-- Session 1:
BEGIN;
UPDATE mail.contacts SET name = 'A' WHERE id = 1;

-- Session 2:
BEGIN;
UPDATE mail.contacts SET name = 'B' WHERE id = 2;

-- Session 1:
UPDATE mail.contacts SET name = 'A2' WHERE id = 2; -- Waits

-- Session 2:
UPDATE mail.contacts SET name = 'B2' WHERE id = 1; -- Deadlock!
-- Expected: One transaction fails with deadlock error
```

## Security Tests

### 1. SQL Injection Prevention

```sql
-- Test: Parameterized queries (test in application code)
-- Good: Using parameters
-- SELECT * FROM mail.contacts WHERE email = $1

-- Bad: String concatenation
-- SELECT * FROM mail.contacts WHERE email = ''' + userInput + '''

-- Test query with injection attempt (should return 0 rows)
SELECT * FROM mail.contacts 
WHERE email = ''' OR ''1''=''1';
-- Expected: 0 rows (literal string match, not injection)
```

### 2. Permission Tests

```sql
-- Test: Schema permissions
\dn+ mail
-- Verify appropriate access control

-- Test: Table permissions
\dp mail.contacts
-- Verify appropriate user permissions
```

## Backup and Recovery Tests

### 1. Dump and Restore Tests

```bash
# Test: Dump database
pg_dump -d tailwind_test --schema=mail > backup.sql

# Test: Drop and restore
psql tailwind_test -c "DROP SCHEMA mail CASCADE;"
psql tailwind_test < backup.sql

# Verify data integrity
psql tailwind_test -c "SELECT COUNT(*) FROM mail.contacts;"

# Cleanup
rm backup.sql
```

### 2. Point-in-Time Recovery Test

```bash
# Test: Enable WAL archiving (in postgresql.conf)
# wal_level = replica
# archive_mode = on
# archive_command = 'cp %p /path/to/archive/%f'

# Create base backup
pg_basebackup -D /path/to/backup -Ft -z -P

# Test recovery to specific point in time
```

## Manual Testing Checklist

### Schema Validation
- [ ] All tables created successfully
- [ ] All columns have correct data types
- [ ] Primary keys defined on all tables
- [ ] Foreign keys link tables correctly
- [ ] Unique constraints prevent duplicates
- [ ] Default values work as expected
- [ ] Indexes exist for performance

### Data Operations
- [ ] Insert contacts with all fields
- [ ] Update contact information
- [ ] Delete contacts and verify cascades
- [ ] Create and manage tags
- [ ] Associate tags with contacts
- [ ] Create broadcasts and messages
- [ ] Query contacts by tag
- [ ] Query messages by status

### Performance
- [ ] Query 10K contacts in < 100ms
- [ ] Insert 10K messages in < 5 seconds
- [ ] Complex joins complete quickly
- [ ] No table scans on indexed columns

### Data Integrity
- [ ] Cannot insert duplicate emails
- [ ] Cannot reference non-existent foreign keys
- [ ] Timestamps are set automatically
- [ ] Unique keys are generated
- [ ] Cascading deletes work correctly

## Test Execution

### Running SQL Tests

```bash
# Run all tests in a file
psql tailwind_test < tests/schema_tests.sql

# Run specific test
psql tailwind_test -c "SELECT * FROM mail.contacts LIMIT 10;"

# Run with timing
psql tailwind_test -c "\timing" -c "SELECT COUNT(*) FROM mail.contacts;"

# Run test script
./test/database/run-tests.sh
```

## Expected Results

### Pass Criteria
- ✅ All tables and indexes created
- ✅ All foreign key relationships valid
- ✅ Data integrity constraints enforced
- ✅ Query performance meets benchmarks
- ✅ No security vulnerabilities
- ✅ Backup and restore works correctly

### Performance Benchmarks
- Simple SELECT by primary key: < 1ms
- SELECT by indexed column: < 10ms
- Complex JOIN query: < 100ms
- Insert 10K rows: < 5 seconds
- Count 100K rows: < 100ms

## CI/CD Integration

```yaml
# GitHub Actions example
- name: Test Database
  run: |
    psql -U postgres -c "CREATE DATABASE tailwind_test;"
    psql -U postgres tailwind_test < db/db.sql
    psql -U postgres tailwind_test < db/seed.sql
    psql -U postgres tailwind_test < test/database/schema_tests.sql
```

---

**Document Version**: 1.0  
**Last Updated**: 2024  
**Owner**: Test Team

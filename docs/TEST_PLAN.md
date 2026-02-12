# Tailwind Traders Mail Service - Master Test Plan

## Overview

This document outlines the comprehensive testing strategy for the Tailwind Traders Mail Service, a multi-component email service system consisting of a .NET API server, Node.js CLI, Go jobs service, and PostgreSQL database.

## Purpose

The purpose of this test plan is to:
- Define testing scope and strategy for all components
- Identify test scenarios and acceptance criteria
- Establish quality gates and testing standards
- Provide guidance for manual and automated testing

## Scope

### In Scope
- Server API (.NET) - Transactional and bulk email endpoints
- CLI Tool (Node.js) - Broadcast and contact management
- Jobs Service (Go) - Background job processing and email sending
- Database (PostgreSQL) - Schema, functions, and data integrity
- Integration between all components
- Security and performance testing

### Out of Scope
- Third-party email service provider testing (Azure Communication Services, SMTP providers)
- Infrastructure deployment testing (covered in deployment documentation)
- Load testing beyond basic performance validation

## Test Environment

### Required Components
- PostgreSQL database (local or Docker)
- .NET 8.0 runtime
- Node.js LTS 20
- Go 1.21+
- Mailpit or similar mail catcher for local testing
- Docker (for containerized testing)

### Environment Variables
See individual component test plans for specific environment variable requirements.

## Test Strategy

### 1. Unit Testing
- Test individual functions and methods in isolation
- Mock external dependencies
- Achieve minimum 70% code coverage for critical paths
- **Server**: xUnit tests for services, models, and utilities
- **CLI**: Mocha tests for commands and utilities
- **Jobs**: Go testing framework for senders and queuers

### 2. Integration Testing
- Test component interactions
- Use test database with seed data
- Test API endpoints with HTTP client
- Test database operations with real connections
- Test email sending with test providers

### 3. End-to-End Testing
- Test complete user workflows
- Validate data flow across all components
- Test real-world scenarios from CLI to email delivery

### 4. Manual Testing
- Exploratory testing of new features
- UI/UX validation for CLI
- Edge case validation
- Documentation accuracy verification

### 5. Security Testing
- Input validation and sanitization
- SQL injection prevention
- XSS prevention in email templates
- API authentication and authorization
- Secrets management validation

### 6. Performance Testing
- API response time validation
- Database query performance
- Bulk operation efficiency (10K+ contacts)
- Email sending throughput

## Test Components

### Component Test Plans
1. [Server API Test Plan](./test-plans/SERVER_TEST_PLAN.md)
2. [CLI Test Plan](./test-plans/CLI_TEST_PLAN.md)
3. [Jobs Service Test Plan](./test-plans/JOBS_TEST_PLAN.md)
4. [Database Test Plan](./test-plans/DATABASE_TEST_PLAN.md)
5. [Integration Test Plan](./test-plans/INTEGRATION_TEST_PLAN.md)

## Key User Stories & Acceptance Criteria

### Story 1: Queue Broadcast to 10K Contacts
**As Jill**, I want to queue a broadcast to 10,001 contacts (with one opted out), so that 10K messages are queued for send.

**Acceptance Criteria:**
- ✅ System correctly filters out opted-out contacts
- ✅ Exactly 10,000 messages are queued
- ✅ All messages contain correct recipient information
- ✅ Broadcast tracking is updated with message count
- ✅ Process completes within acceptable time (< 30 seconds)

### Story 2: Double Opt-In Signup
**As Jim**, I want to sign up for Jill's list, so that I receive a confirmation email to verify my subscription.

**Acceptance Criteria:**
- ✅ Signup API endpoint creates contact record
- ✅ Contact is created with `subscribed=false` initially
- ✅ Confirmation email is sent immediately
- ✅ Confirmation link contains unique, secure token
- ✅ Clicking link updates `subscribed=true`
- ✅ Activity is logged for audit trail

### Story 3: Unsubscribe Link
**As Kim**, I want to unsubscribe using an unsubscribe link, so that I stop receiving emails.

**Acceptance Criteria:**
- ✅ Unsubscribe link contains unique contact key
- ✅ Clicking link updates `subscribed=false`
- ✅ Unsubscribe page confirms action
- ✅ Activity is logged
- ✅ Future broadcasts exclude unsubscribed contacts

### Story 4: Transactional Email (Future)
**As Jill**, I want to send transactional emails to book buyers, so they receive individual purchase confirmations.

**Acceptance Criteria:**
- ✅ Transactional emails send individually
- ✅ No unsubscribe link in transactional emails
- ✅ Emails send regardless of subscription status
- ✅ Each email is tracked separately
- ✅ Template variables are properly substituted

## Testing Schedule

### Phase 1: Unit Tests (Week 1-2)
- Implement unit tests for all components
- Achieve baseline code coverage
- Setup CI/CD test automation

### Phase 2: Integration Tests (Week 3-4)
- Implement API integration tests
- Test database operations
- Test CLI-to-API communication
- Test Jobs-to-database integration

### Phase 3: End-to-End Tests (Week 5)
- Implement complete workflow tests
- Test user stories end-to-end
- Performance baseline testing

### Phase 4: Security & Performance (Week 6)
- Security audit and testing
- Performance optimization
- Load testing with realistic data volumes

## Test Execution

### Automated Tests
```bash
# Server tests
cd server
make test

# CLI tests
cd cli
npm test

# Jobs tests
cd jobs
go test ./...
```

### Manual Test Checklist
See individual component test plans for detailed manual testing procedures.

## Defect Management

### Severity Levels
- **Critical**: System crash, data loss, security vulnerability
- **High**: Major feature broken, incorrect data processing
- **Medium**: Feature partially working, workaround available
- **Low**: Cosmetic issue, minor inconvenience

### Defect Workflow
1. Identify and document defect
2. Assign severity level
3. Create GitHub issue with reproduction steps
4. Assign to developer
5. Fix and verify
6. Regression test
7. Close issue

## Entry and Exit Criteria

### Test Entry Criteria
- Code is committed and buildable
- Development environment is set up
- Test data is available
- Test plan is reviewed and approved

### Test Exit Criteria
- All critical and high severity defects are fixed
- 70% code coverage achieved for critical components
- All user stories pass acceptance criteria
- Performance benchmarks are met
- Security vulnerabilities are addressed
- Documentation is complete and accurate

## Risks and Mitigation

| Risk | Impact | Probability | Mitigation |
|------|--------|-------------|------------|
| Database schema changes break tests | High | Medium | Use database migrations, version control schema |
| Third-party email service unavailable | High | Low | Use test email sender, implement retry logic |
| Performance degradation with large datasets | High | Medium | Include performance tests in CI/CD |
| Security vulnerabilities in dependencies | High | Medium | Regular dependency updates, security scanning |
| Integration test environment instability | Medium | Medium | Use Docker for consistent environments |

## Test Metrics

### Key Metrics to Track
- Code coverage percentage
- Test pass/fail rate
- Defect density (defects per 1000 lines of code)
- Mean time to detect (MTTD) defects
- Mean time to resolve (MTTR) defects
- Test execution time
- API response times
- Email sending throughput

## Tools and Frameworks

### Testing Frameworks
- **.NET**: xUnit, Microsoft.AspNetCore.Mvc.Testing
- **Node.js**: Mocha, Chai (potential addition)
- **Go**: testing package, testify (potential addition)

### Test Tools
- **API Testing**: Swagger UI, Postman, curl
- **Database Testing**: psql, pgAdmin
- **Email Testing**: Mailpit, Ethereal Email
- **Code Coverage**: coverlet (.NET), nyc (Node.js), go cover (Go)
- **CI/CD**: GitHub Actions

## References

- [Server README](../server/README.md)
- [CLI README](../cli/README.md)
- [Jobs README](../jobs/README.md)
- [Database Schema](../db/db.sql)
- [Coding Standards](../.github/instructions/copilot-instructions.md)

## Document Control

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2024 | Test Team | Initial test plan creation |

---

**Note**: This is a living document and should be updated as the project evolves, new features are added, or testing strategies change.

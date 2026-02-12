# Welcome to Tailwind Traders Mail Service

We all need email... for better or worse. This service will send transactional emails via API or batch emails to a list, using a tag or predefined segment, like MailChimp does.

## Work In Progress

We're building things out actively... hopefully getting close to showing something soon!

## Architecture

This project consists of multiple components working together:

- **Server** (ASP.NET Core / .NET 8): RESTful API for managing contacts, broadcasts, and email delivery
- **Jobs** (Go): Background workers for processing email queues and batch operations
- **CLI** (Node.js): Command-line tools for contact management and broadcast operations
- **Database** (PostgreSQL): Data persistence for contacts, messages, and activity tracking
- **Infrastructure**: Docker Compose for local development, Azure deployment for production


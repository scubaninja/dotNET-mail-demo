# .NET Mail Demo

A robust email broadcast system built with .NET that allows sending targeted emails to subscribed contacts. This system provides functionality for creating and managing email broadcasts using Markdown formatting.

## Core Features

- **Markdown Email Support**: Create email content using Markdown format
- **Contact Management**: Manage subscribers with a tagging system
- **Targeted Broadcasts**: Send emails to all subscribers or specific tagged segments
- **Admin API**: REST endpoints for broadcast management and validation
- **Transaction Support**: All database operations are wrapped in transactions for data integrity

## Project Structure

- `/server/`
  - `/Models/`: Data models including Broadcast and Email
  - `/Commands/`: Command pattern implementations for business operations
  - `/Api/`: API endpoints and route definitions
    - `/Admin/`: Administrative API endpoints

## Key Components

### Broadcast System
- Creates broadcasts from Markdown documents
- Supports targeted sending using tags
- Tracks broadcast status and metrics
- Handles email queueing and delivery

### Contact Management
- Maintains subscriber lists
- Supports contact tagging
- Tracks subscription status

### API Endpoints

#### Admin Routes
- `/admin/validate`: Validates markdown email content
- `/admin/queue-broadcast`: Queues a new broadcast for delivery
- `/admin/get-chat`: AI-assisted content generation support

## Database Schema

The system uses a PostgreSQL database with the following main tables in the `mail` schema:
- `broadcasts`: Stores broadcast information
- `contacts`: Manages subscriber information
- `tags`: Stores available tags
- `tagged`: Junction table for contact-tag relationships
- `messages`: Queued messages for delivery

## Getting Started

1. Ensure you have .NET 6+ installed
2. Set up a PostgreSQL database
3. Configure the connection string
4. Set the `DEFAULT_FROM` email address in configuration

## Technical Stack

- .NET 6+
- PostgreSQL
- Dapper for data access
- OpenAPI/Swagger for API documentation
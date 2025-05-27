# Mail Service

A Go-based email service for managing contacts, broadcasts, and email campaigns. This service provides a RESTful API for managing email subscriptions, sending broadcasts, and tracking email activities.

## Features

- Contact Management
  - Sign up new contacts
  - Opt-in/opt-out functionality
  - Contact activity tracking
- Broadcast Management
  - Create broadcasts from markdown templates
  - Queue broadcasts for specific contact segments
  - Track broadcast status
- Email Templates
  - Markdown-based email templates
  - HTML conversion
  - Template variables support
- Activity Tracking
  - Track email opens, clicks, and other activities
  - Contact engagement history

## Getting Started

### Prerequisites

- Go 1.20 or later
- PostgreSQL 14 or later
- Docker (optional)

### Installation

1. Clone the repository:
   ```bash
   git clone https://github.com/yourorg/mail-service.git
   cd mail-service
   ```

2. Install dependencies:
   ```bash
   go mod download
   ```

3. Configure the application:
   Create a `config.yaml` file with your configuration:
   ```yaml
   database:
     host: localhost
     port: 5432
     name: maildb
     user: postgres
     password: your_password
   
   smtp:
     host: smtp.example.com
     port: 587
     username: your_username
     password: your_password
   ```

4. Run the migrations:
   ```bash
   make migrate
   ```

5. Start the server:
   ```bash
   go run main.go
   ```

### Docker

To run the service using Docker:

```bash
docker build -t mail-service .
docker run -p 8080:8080 mail-service
```

## API Documentation

### Public Endpoints

- `POST /signup` - Sign up a new contact
- `GET /unsubscribe/{key}` - Unsubscribe a contact
- `GET /confirm/{key}` - Confirm a contact's subscription

### Admin Endpoints

- `POST /admin/broadcasts` - Create a new broadcast
- `POST /admin/broadcasts/{id}/queue` - Queue a broadcast for sending
- `GET /admin/contacts` - List all contacts
- `GET /admin/activities` - List all activities

## Development

### Running Tests

```bash
go test ./...
```

### Contributing

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Acknowledgments

- Original .NET implementation by Tailwind Traders team
- Go community and contributors

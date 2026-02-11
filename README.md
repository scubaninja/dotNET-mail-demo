# Welcome to Tailwind Traders Mail Service

We all need email... for better or worse. This service will send transactional emails via API or batch emails to a list, using a tag or predefined segment, like MailChimp does.

## Quick Start

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download)
- [Docker and Docker Compose](https://docs.docker.com/get-docker/) (recommended)
- OR [PostgreSQL 12+](https://www.postgresql.org/download/)

### Option 1: Using Docker Compose (Recommended)

1. **Clone the repository**
   ```bash
   git clone https://github.com/scubaninja/dotNET-mail-demo.git
   cd dotNET-mail-demo
   ```

2. **Set up environment variables**
   ```bash
   cp .env.example .env
   # Edit .env with your configuration
   ```

3. **Start the services**
   ```bash
   docker-compose up -d
   ```

   This will start:
   - PostgreSQL database (port 5432)
   - Mail server API (port 5000)

4. **Access the API**
   
   Open http://localhost:5000 in your browser to see the Swagger UI.

### Option 2: Manual Setup

1. **Set up PostgreSQL database**
   
   See [POSTGRES_SETUP.md](POSTGRES_SETUP.md) for detailed instructions, or use the automated script:
   
   ```bash
   ./scripts/setup-database.sh setup
   ```

2. **Configure environment variables**
   ```bash
   export DATABASE_URL="postgresql://tailwind:password@localhost:5432/tailwind"
   export ASPNETCORE_ENVIRONMENT="Development"
   export DEFAULT_FROM="test@tailwind.dev"
   ```

3. **Run the mail server**
   ```bash
   cd server
   dotnet restore
   dotnet run
   ```

## Database

This application uses **PostgreSQL** as its database. The database schema includes:

- **Contacts** - Manage email list subscribers
- **Tags** - Organize contacts with tags
- **Sequences** - Create email sequences
- **Broadcasts** - Send batch emails to segments
- **Messages** - Track all sent messages
- **Activity** - Monitor contact engagement

For detailed database setup instructions, see [POSTGRES_SETUP.md](POSTGRES_SETUP.md).

## Architecture

- **Backend**: .NET 8.0 Minimal API
- **Database**: PostgreSQL 16
- **ORM**: Dapper + Dapper.SimpleCRUD
- **Configuration**: Viper.NET (environment-based)
- **Email**: SMTP (configurable)

## Development

### Building the Project

```bash
cd server
dotnet build --configuration Debug
```

### Running Tests

```bash
cd server
dotnet test --configuration Debug
```

### Database Management

Reset database:
```bash
./scripts/setup-database.sh reset
```

Verify database:
```bash
./scripts/setup-database.sh verify
```

## API Documentation

Once the server is running, visit:
- **Swagger UI**: http://localhost:5000
- **OpenAPI Spec**: http://localhost:5000/swagger/v1/swagger.json

## Environment Variables

Required environment variables:

| Variable | Description | Example |
|----------|-------------|---------|
| `DATABASE_URL` | PostgreSQL connection string | `postgresql://user:pass@host:5432/db` |
| `ASPNETCORE_ENVIRONMENT` | Environment name | `Development` or `Production` |
| `DEFAULT_FROM` | Default sender email | `noreply@tailwind.dev` |

Optional variables for email:

| Variable | Description |
|----------|-------------|
| `SMTP_HOST` | SMTP server hostname |
| `SMTP_USER` | SMTP username |
| `SMTP_PASSWORD` | SMTP password |
| `ETHEREAL_USER` | Ethereal email user (for testing) |
| `ETHEREAL_PASSWORD` | Ethereal email password |
| `SEND_WORKER` | Background worker mode (`local` or `queue`) |

See [.env.example](.env.example) for a complete list.

## Project Structure

```
.
├── server/          # .NET API server
│   ├── Api/        # API route handlers
│   ├── Commands/   # Business logic commands
│   ├── Data/       # Database access layer
│   ├── Models/     # Data models
│   └── Services/   # Background services
├── db/             # Database schema and seeds
├── scripts/        # Utility scripts
├── deploy/         # Deployment configurations
└── docs/           # Additional documentation
```

## Work In Progress

We're building things out actively... hopefully getting close to showing something soon!

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License.
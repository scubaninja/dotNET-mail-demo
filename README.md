# Tailwind Traders Mail Service

An email management platform for sending transactional and broadcast emails through a REST API. Organize your contacts with tags and send targeted campaigns—similar to services like MailChimp.

## What This Project Does

This mail service enables you to:

- **Send transactional emails** - Trigger individual emails programmatically
- **Create broadcast campaigns** - Queue bulk emails to all subscribers or specific segments
- **Manage contacts** - Full CRUD operations for your subscriber list
- **Segment with tags** - Organize contacts into groups for targeted messaging
- **Handle subscriptions** - Public endpoints let users opt-in and opt-out
- **Author emails in Markdown** - Write email content in markdown with YAML frontmatter metadata
- **Process emails asynchronously** - Background worker handles the actual sending
- **Explore the API** - Swagger UI documentation available at the root URL

## Architecture Overview

The project is organized into four main modules:

| Module | Technology | Description |
|--------|------------|-------------|
| `server/` | .NET 8 Minimal API | The main REST API handling all email operations |
| `cli/` | Node.js + Commander | Command-line interface for broadcast management |
| `jobs/` | Go + Mage | Background job processor for email queue |
| `db/` | PostgreSQL | Persistent storage using the `mail` schema |

## What You Need

Make sure you have these installed:

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js LTS 20](https://nodejs.org/) (needed for the CLI)
- [Go 1.21 or later](https://golang.org/) (needed for the jobs processor)
- [PostgreSQL 14 or later](https://www.postgresql.org/)
- [Docker](https://www.docker.com/) (optional, helpful for local mail testing)

## Getting Up and Running

### 1. Get the Code

Clone this repository to your local machine:

```sh
cd your-projects-folder
git clone <repository-url>
cd dotNET-mail-demo
```

### 2. Set Up PostgreSQL

Run the schema script against your database:

```sh
cd db
psql mydb < db.sql
psql mydb < seed.sql  # adds some test contacts
```

### 3. Configure Environment Variables

You'll need to set these environment variables (create a `.env` file or export them):

**Required:**
- `DATABASE_URL` - PostgreSQL connection string in format `postgres://user:pass@host:port/dbname`
- `ASPNETCORE_ENVIRONMENT` - Set to `Development` for local work

**SMTP Configuration:**
- `SMTP_HOST` - Your SMTP server hostname
- `SMTP_USER` - SMTP username
- `SMTP_PASSWORD` - SMTP password

**Optional:**
- `DEFAULT_FROM` - Default sender email address
- `ETHEREAL_USER` / `ETHEREAL_PASSWORD` - For testing with Ethereal
- `SEND_WORKER` - Set to `local` to enable the background email sender

### 4. Start the Server

Navigate to the server directory and run:

```sh
cd server
dotnet run
```

Visit `http://localhost:5000` to see the Swagger UI with interactive API documentation.

### 5. Set Up Local Email Capture (Optional)

During development, use Mailpit to catch outgoing emails without actually sending them:

```sh
cd server
make mailpit
```

This starts:
- SMTP capture on port 1025
- Web UI on port 8025 (view captured emails at http://localhost:8025)

## How the Code is Organized

```
dotNET-mail-demo/
├── server/                 # .NET 8 REST API
│   ├── Api/               # Route definitions
│   │   ├── Admin/         # Admin-only routes (broadcasts, contacts, bulk ops)
│   │   └── PublicRoutes.cs # Public subscription routes
│   ├── Commands/          # Business logic commands
│   ├── Data/              # Database access layer
│   ├── Models/            # Entity definitions
│   ├── Services/          # Email sender implementations
│   └── Tests/             # Unit tests using xUnit
├── cli/                    # Node.js CLI tool
│   ├── commands/          # CLI command handlers
│   └── bin/               # Entry point scripts
├── jobs/                   # Go background processor
│   ├── queuers/           # Queue handling
│   ├── senders/           # Email sending implementations
│   └── deploy/            # Deployment configurations
├── db/                     # SQL schema and seeds
│   ├── db.sql             # Schema creation script
│   └── seed.sql           # Test data
├── deploy/                 # Deployment resources
└── docs/                   # Additional documentation
```

## Available API Endpoints

### Public Routes

| Method | Path | What It Does |
|--------|------|--------------|
| `GET` | `/about` | Returns info about the API |
| `GET` | `/unsubscribe/{key}` | Unsubscribes a contact by their unique key |
| `GET` | `/link/clicked/{key}` | Logs a link click for tracking |
| `POST` | `/signup` | Adds a new contact to the list |

### Admin Routes

| Method | Path | What It Does |
|--------|------|--------------|
| `POST` | `/admin/validate` | Validates markdown email content |
| `POST` | `/admin/queue-broadcast` | Creates a broadcast and queues all messages |
| `POST` | `/admin/get-chat` | Generates email content with AI |
| `GET` | `/admin/contacts/search?term={term}` | Searches contacts by name or email |

## Database Tables

All tables live in the `mail` schema:

| Table | Purpose |
|-------|---------|
| `contacts` | Subscriber information and subscription status |
| `tags` | Labels for organizing contacts into groups |
| `tagged` | Junction table linking contacts to their assigned tags |
| `emails` | Email template storage |
| `broadcasts` | Campaign metadata and configuration |
| `messages` | Individual queued emails with delivery status |
| `activity` | Tracks subscriber interactions |
| `sequences` | Drip campaign definitions (planned feature) |

## Development Workflow

### Running Tests

Execute the test suite:

```sh
cd server
dotnet test
```

Or use the Makefile:

```sh
cd server
make test
```

### Building the Project

Compile the server application:

```sh
cd server
dotnet build --configuration Debug
```

> **Note:** Package references only load in Debug mode. Always use `--configuration Debug` for development.

### Working with the CLI

Set up and use the command-line tool:

```sh
cd cli
npm install
source .env  # Sets up aliases
mdmail --help
```

### Using the Jobs Processor

Work with the Go-based job runner:

```sh
cd jobs
mage  # Shows all available targets
mage test:hello  # Test that mage is working
```

## Deploying the Application

### Using Docker Compose

Build and run all services:

```sh
docker-compose up
```

### Azure Container Apps

See [jobs/README.md](./jobs/README.md) for detailed Azure deployment instructions using Mage automation targets.

## Contributing

We welcome contributions! Here's how:

1. Fork the repository
2. Create a feature branch
3. Make your changes with clear commit messages
4. Push your branch
5. Open a Pull Request

## License

This project is licensed under the MIT License.

## Team & Contact

Built by Rob Conery, Aaron Wislang, and the Tailwind Traders Team.

Learn more at https://tailwindtraders.dev
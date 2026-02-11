# Welcome to Tailwind Traders Mail Service

We all need email... for better or worse. This service will send transactional emails via API or batch emails to a list, using a tag or predefined segment, like MailChimp does.

> **Note:** This project is under active development. Contributions and feedback are welcome!

## What's In the Box

- **Server** — A .NET 8 Minimal API for managing contacts, broadcasts, and email sending ([server/](./server/))
- **CLI** — A Node.js command-line tool for creating broadcasts from markdown files ([cli/](./cli/))
- **Jobs** — A Go-based job runner for background email processing and Azure integrations ([jobs/](./jobs/))
- **Database** — PostgreSQL schema and seed data ([db/](./db/))

## Prerequisites

| Tool | Version | Notes |
|------|---------|-------|
| [.NET SDK](https://dotnet.microsoft.com/download) | 8.0+ | For the server API |
| [PostgreSQL](https://www.postgresql.org/download/) | 14+ | Primary database |
| [Docker](https://docs.docker.com/get-docker/) | Latest | For Mailpit and optional containerized workflows |
| [Node.js](https://nodejs.org/) | LTS 20+ | For the CLI tool (optional) |
| [Go](https://go.dev/dl/) | 1.21+ | For the jobs runner (optional) |

## Quick Start

### 1. Clone the repository

```bash
git clone https://github.com/scubaninja/dotNET-mail-demo.git
cd dotNET-mail-demo
```

### 2. Set up the database

Make sure PostgreSQL is running, then create the database and load the schema:

```bash
createdb tailwind
cd db
make db
```

Optionally seed sample data:

```bash
cd ../server
make seed
```

### 3. Configure environment variables

Create a `.env` file (or export variables) for the server. At minimum you need:

```bash
ASPNETCORE_ENVIRONMENT="Development"
DATABASE_URL="postgres://localhost/tailwind"

# SMTP settings — use Ethereal (https://ethereal.email) for free test credentials
SMTP_USER=""
SMTP_PASSWORD=""
SMTP_HOST=""

DEFAULT_FROM="test@tailwind.dev"

# Set to "local" to enable the background email send worker
SEND_WORKER="local"
```

### 4. Start the local email testing server

[Mailpit](https://github.com/axllent/mailpit) captures outgoing emails so you can inspect them in a browser:

```bash
cd server
make mailpit
```

Open [http://localhost:8025](http://localhost:8025) to view captured emails.

### 5. Run the API server

```bash
cd server
dotnet watch
```

The API (with Swagger UI) will be available at [http://localhost:5000](http://localhost:5000).

## API Overview

The API is documented via Swagger/OpenAPI. Once the server is running, visit the root URL to explore endpoints interactively.

### Public Endpoints

| Method | Path | Description |
|--------|------|-------------|
| `GET` | `/about` | API information |
| `POST` | `/signup` | Sign up for the mailing list |
| `GET` | `/unsubscribe/{key}` | Unsubscribe using a unique key |
| `GET` | `/link/clicked/{key}` | Track a link click |

### Admin Endpoints

| Area | Description |
|------|-------------|
| Broadcasts | Create and manage bulk email campaigns |
| Contacts | Manage subscriber contacts and tags |
| Bulk Operations | Batch tag and segment operations |

## Project Structure

```
dotNET-mail-demo/
├── server/               # .NET 8 Minimal API
│   ├── Api/              # Route handlers (public + admin)
│   ├── Models/           # Data models (Contact, Message, Broadcast, etc.)
│   ├── Data/             # Database access layer (Dapper ORM)
│   ├── Services/         # Background send, AI, email sender
│   ├── Commands/         # Command pattern implementations
│   ├── Tests/            # xUnit tests
│   └── Makefile          # Build/run/test shortcuts
├── cli/                  # Node.js CLI for markdown-based broadcasts
├── db/                   # PostgreSQL schema (db.sql) and seed data (seed.sql)
├── jobs/                 # Go-based job runner (Mage)
├── deploy/               # Deployment resources (Azure/Docker/K8s)
├── docs/                 # Additional documentation
└── docker-compose.yml    # Docker Compose configuration
```

## Running Tests

Tests use [xUnit](https://xunit.net/) and live in `server/Tests/`. Run them with:

```bash
cd server
make test
```

Or directly:

```bash
cd server
dotnet test
```

## Using the CLI

The CLI reads markdown files and creates broadcasts. See [cli/README.md](./cli/README.md) for full details.

```bash
cd cli
npm install
alias mdmail="node ./bin/mdmail.js"  # add to your shell profile to persist
```

## Deployment

Deployment resources for Azure, Docker, and Kubernetes are in the [deploy/](./deploy/) directory. The [jobs/](./jobs/) service includes Mage targets for Azure Container Apps, Service Bus, and more — see [jobs/README.md](./jobs/README.md).

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b my-feature`)
3. Make your changes
4. Run the tests (`cd server && make test`)
5. Open a pull request

## License

This project is licensed under the [MIT License](https://opensource.org/license/mit/).
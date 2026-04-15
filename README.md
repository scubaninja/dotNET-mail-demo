# Welcome to Tailwind Traders Mail Service

A self-hosted transactional and bulk email service. Send individual transactional emails via API, or queue batch broadcasts to your entire list or a tagged segment—like MailChimp, but fully under your control.

## Architecture

This project consists of multiple components working together:

| Component | Technology | Purpose |
|-----------|------------|---------|
| [**server/**](./server/README.md) | ASP.NET Core / .NET 8 | RESTful API for contacts, broadcasts, and email delivery |
| [**jobs/**](./jobs/README.md) | Go + Mage | Background workers that process the message queue and send emails |
| [**cli/**](./cli/README.md) | Node.js + Commander | Command-line tool for managing contacts and authoring broadcasts |
| [**db/**](./db/README.md) | PostgreSQL | Schema and seed data |
| [**deploy/**](./deploy/README.md) | Docker Compose / Azure Bicep | Local dev and cloud deployment configs |

## Quick Start (Local Development)

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Go 1.21+](https://go.dev/dl/) and [Mage](https://magefile.org/) (`go install github.com/magefile/mage@latest`)
- [Node.js 20 LTS](https://nodejs.org/)
- [PostgreSQL 15+](https://www.postgresql.org/) (or use the provided Docker setup)

### 1. Set up the database

```bash
# Start PostgreSQL via Docker (optional, if you don't have a local instance)
docker compose up -d db

# Create the schema
psql $DATABASE_URL -f db/db.sql

# Load seed data (optional)
psql $DATABASE_URL -f db/seed.sql
```

### 2. Run the API server

```bash
cd server
export ASPNETCORE_ENVIRONMENT="Development"
export DATABASE_URL="postgres://user:pw@localhost/tailwind"
export DEFAULT_FROM="noreply@example.com"
# SMTP settings — see server/README.md for all options
dotnet watch
```

The API (with Swagger UI) is available at `http://localhost:5000`.

### 3. Run the jobs worker

```bash
cd jobs
export MESSAGES_TYPE="smtp"   # or "azure" / "test"
# See jobs/README.md for all required env vars
mage messages:send
```

### 4. Use the CLI

```bash
cd cli
npm install
# Create a .env file — see cli/README.md
node ./bin/mdmail.js --help
```

## Key Workflows

- **Queue a broadcast** — author a markdown file with the CLI, validate it via the API, then queue it for delivery to all (or tagged) contacts.
- **Contact sign-up** — a public `POST /signup` endpoint handles web-form submissions with double opt-in.
- **Unsubscribe** — a public `GET /unsubscribe/{key}` link is embedded in every broadcast email.
- **Bulk tagging** — import a CSV of email addresses and apply one or more tags via the CLI.

## Documentation

- [Server API](./server/README.md)
- [Jobs Worker](./jobs/README.md)
- [CLI](./cli/README.md)
- [Database Schema](./db/README.md)
- [Deployment](./deploy/README.md)

## License

[MIT](https://opensource.org/license/mit/)
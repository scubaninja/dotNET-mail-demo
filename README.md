# Tailwind Traders Mail Service

A self-hosted email list service for sending transactional emails and batch broadcasts to a contact list — think a lightweight, open-source MailChimp alternative.

Broadcasts are authored as Markdown files with frontmatter, validated and queued via a CLI, and delivered by background workers.

## Architecture

| Component | Technology | Purpose |
|-----------|-----------|---------|
| `server/` | ASP.NET Core (.NET 8) | REST API — contacts, broadcasts, email validation & queuing |
| `jobs/` | Go (Mage) | Background workers — queue messages, deliver via SMTP or Azure |
| `cli/` | Node.js (Commander) | Developer CLI — author, validate, and send broadcasts |
| `db/` | PostgreSQL | Schema and seed data |
| `deploy/` | Azure / Docker | Deployment infrastructure |

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Go 1.21+](https://go.dev/dl/)
- [Node.js 20+](https://nodejs.org/)
- [PostgreSQL 15+](https://www.postgresql.org/)
- [Docker](https://www.docker.com/) (optional, for local dev)

## Getting Started

### 1. Database

```bash
# Create the schema
psql -d your_database -f db/db.sql

# Optionally load seed data
psql -d your_database -f db/seed.sql
```

### 2. Server

Configure the connection string and mail settings in `server/appsettings.Development.json` or via environment variables:

```
DATABASE_URL=postgres://user:password@localhost/tailwind
SMTP_HOST=smtp.example.com
SMTP_USER=you@example.com
SMTP_PASSWORD=yourpassword
DEFAULT_FROM=noreply@tailwindtraders.dev

# Or use Ethereal for local testing (https://ethereal.email)
ETHEREAL_USER=...
ETHEREAL_PASSWORD=...
```

```bash
cd server
dotnet run
# API available at http://localhost:5000
```

### 3. CLI

```bash
cd cli
npm install
node bin/mdmail.js init        # create the /mail directory structure
node bin/mdmail.js broadcast new "My Subject"   # draft a new broadcast
node bin/mdmail.js broadcast validate           # validate against the API
node bin/mdmail.js broadcast send               # queue the broadcast for delivery
```

Set `API_ROOT` to point at the running server (defaults to `http://localhost:5000/admin`).

### 4. Jobs

```bash
cd jobs
# Queue messages (reads pending broadcasts from the DB)
MESSAGES_TYPE=smtp mage messages:send

# Options for MESSAGES_TYPE: test (default) | smtp | azure | servicebus-test
```

### Docker Compose (local dev)

```bash
docker compose up
```

## API Endpoints

### Public

| Method | Path | Description |
|--------|------|-------------|
| `GET` | `/about` | API info |
| `POST` | `/signup` | Subscribe a contact |
| `GET` | `/unsubscribe/{key}` | Unsubscribe a contact |
| `GET` | `/link/clicked/{key}` | Track a link click |

### Admin

| Method | Path | Description |
|--------|------|-------------|
| `POST` | `/admin/validate` | Validate broadcast Markdown |
| `POST` | `/admin/queue-broadcast` | Queue a broadcast for send |
| `POST` | `/admin/get-chat` | Generate email body via AI |
| `GET` | `/admin/contacts/search?term=` | Search contacts by name or email |

## Broadcast Markdown Format

Broadcasts are plain Markdown files with YAML frontmatter:

```markdown
---
Subject: "Your email subject"
Slug: "your-email-subject"
Summary: "Short preview text shown in email clients"
SendToTag: "*"
---

Your email body in **Markdown**.
```

`SendToTag` can be `*` (everyone) or one or more tag slugs to target a specific segment.

## Database Schema

- **contacts** — subscribers with opt-in/out status and a unique key
- **tags / tagged** — segment contacts into groups
- **sequences / subscriptions** — drip-style email sequences
- **emails** — email templates (HTML), linked to sequences or standalone
- **broadcasts** — a scheduled or sent batch send linked to an email template
- **messages** — immutable delivery log (one row per email sent)
- **activity** — contact-level event log
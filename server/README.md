# The Mail Service API

This is the .NET 8 Minimal API backend for the Tailwind Traders mail service. It exposes public endpoints for subscriber sign-up/unsubscribe flows and admin endpoints for broadcast management, contact search, and bulk operations.

## Environment Variables

| Variable | Required | Description |
|----------|----------|-------------|
| `ASPNETCORE_ENVIRONMENT` | Yes | `Development` or `Production` |
| `DATABASE_URL` | Yes | PostgreSQL connection string, e.g. `postgres://user:pw@host/db` |
| `DEFAULT_FROM` | Yes | Default sender address, e.g. `noreply@tailwind.dev` |
| `SMTP_HOST` | Yes* | SMTP server hostname |
| `SMTP_USER` | Yes* | SMTP username |
| `SMTP_PASSWORD` | Yes* | SMTP password |
| `ETHEREAL_USER` | No | [ethereal.email](https://ethereal.email) username (dev only) |
| `ETHEREAL_PASSWORD` | No | ethereal.email password (dev only) |
| `SEND_WORKER` | No | Set to `local` to run the background send worker in-process |

\* Required when sending real email.

## Running

```bash
dotnet watch          # development with hot-reload
dotnet run            # production-style run
dotnet test           # run the test suite
```

Swagger UI is served at `http://localhost:5000` (the root path).

## API Endpoints

### Public Routes

| Method | Path | Description |
|--------|------|-------------|
| `GET` | `/about` | Returns service information |
| `POST` | `/signup` | Subscribe a new contact to the mailing list |
| `GET` | `/unsubscribe/{key}` | Unsubscribe a contact using their unique key |
| `GET` | `/link/clicked/{key}` | Track a link click in a broadcast email |

#### `POST /signup`

Subscribe a new contact.

**Request body:**
```json
{
  "name": "Jane Smith",
  "email": "jane@example.com"
}
```

#### `GET /unsubscribe/{key}`

Marks the contact with the given `key` as unsubscribed. Returns `true` on success.

---

### Admin Routes — Broadcasts (`/admin`)

| Method | Path | Description |
|--------|------|-------------|
| `POST` | `/admin/validate` | Validate broadcast markdown before queuing |
| `POST` | `/admin/queue-broadcast` | Queue a broadcast for delivery |
| `POST` | `/admin/get-chat` | Generate email copy via AI (OpenAI) |

#### `POST /admin/validate`

Parses the markdown frontmatter and returns validation status plus the number of contacts that would receive the broadcast.

**Request body:**
```json
{
  "markdown": "---\nSubject: \"Hello!\"\nSummary: \"A test\"\nSendToTag: \"*\"\n---\n\nBody text here."
}
```

**Response:**
```json
{
  "valid": true,
  "message": "The markdown is valid",
  "contacts": 1042,
  "data": { "subject": "Hello!", "summary": "A test", "slug": "hello", "sendToTag": "*" }
}
```

#### `POST /admin/queue-broadcast`

Validates the markdown, creates an `email` record, creates a `broadcast` record, and inserts one `message` row per eligible subscribed contact (or per-tag subset).

**Request body:** same as `/admin/validate`.

**Response:**
```json
{
  "success": true,
  "message": "The broadcast was queued with ID 7 and 1042 messages were created",
  "result": { "inserted": 1042, "data": { "broadcastId": 7, "emailId": 12, "notified": true } }
}
```

#### `POST /admin/get-chat`

Sends a prompt to OpenAI and returns the generated text for use in broadcast copy.

**Request body:**
```json
{ "prompt": "Write a short email announcing our new product." }
```

---

### Admin Routes — Contacts (`/admin/contacts`)

| Method | Path | Description |
|--------|------|-------------|
| `GET` | `/admin/contacts/search?term=…` | Fuzzy search contacts by name or email |

#### `GET /admin/contacts/search`

Returns contacts whose `email` or `name` matches the search term (case-insensitive regex match).

**Query parameter:** `term` — at least 1 character.

**Response:**
```json
{
  "term": "jane",
  "contacts": [
    { "id": 1, "name": "Jane Smith", "email": "jane@example.com", "subscribed": true, "key": "…", "createdAt": "…" }
  ]
}
```

---

### Admin Routes — Bulk Operations (`/admin/bulk`)

| Method | Path | Description |
|--------|------|-------------|
| `POST` | `/admin/bulk/contacts/tag` | Apply one or more tags to a list of contacts |

#### `POST /admin/bulk/contacts/tag`

Upserts the provided contacts and applies the specified tag(s). Multiple tags can be supplied as a comma-separated string.

**Request body:**
```json
{
  "tag": "newsletter,vip",
  "emails": ["jane@example.com", "bob@example.com"]
}
```

**Response:**
```json
{
  "success": true,
  "message": "2 Tag(s) applied to 2 contacts",
  "created": 0,
  "updated": 0
}
```

---

## Markdown Email Format

Broadcasts are authored as markdown files with YAML frontmatter:

```markdown
---
Subject: "Your subject line"
Summary: "One-line preview text shown in the inbox"
Slug: "optional-custom-slug"      # auto-generated from Subject if omitted
SendToTag: "*"                    # "*" = everyone; or a tag slug, e.g. "newsletter"
---

The **body** of your email goes here, in standard markdown.
```

## Stories

- [x] **Jill queues a broadcast to 10K contacts** — queue broadcast to all subscribed contacts, skipping the one opt-out.
- [x] **Jim signs up** — form submission → double opt-in flow.
- [x] **Kim opts out** — unsubscribe via link embedded in email.
- [ ] **Jill sends a transactional email** — individual send without an unsubscribe link.
# The Mail App Database

This is a PostgreSQL database. Instead of using migrations or an ORM-generated schema, the DDL is maintained by hand in two plain SQL files.

## Files

| File | Purpose |
|------|---------|
| `db.sql` | Creates the `mail` schema and all tables. **Destructive** — drops and recreates the schema. |
| `seed.sql` | Inserts sample data for local development and testing. |

## Applying the Schema

```bash
psql $DATABASE_URL -f db/db.sql
psql $DATABASE_URL -f db/seed.sql   # optional
```

Or with the Makefile:

```bash
make db      # runs db.sql
make seed    # runs seed.sql
```

## Schema Overview

All tables live in the `mail` schema.

### `contacts`

Stores subscribers. Each contact has a unique `key` (UUID) used in unsubscribe links.

| Column | Type | Notes |
|--------|------|-------|
| `id` | serial PK | |
| `email` | text | unique |
| `key` | text | UUID, default `gen_random_uuid()` |
| `subscribed` | boolean | default `true` |
| `name` | text | nullable |
| `created_at` | timestamptz | |
| `updated_at` | timestamptz | |

### `tags`

Segments that can be applied to contacts for targeted broadcasts.

| Column | Type | Notes |
|--------|------|-------|
| `id` | serial PK | |
| `slug` | text | unique |
| `name` | text | nullable |
| `description` | text | nullable |
| `created_at` | timestamptz | |
| `updated_at` | timestamptz | |

### `tagged`

Join table linking contacts to tags (many-to-many).

| Column | Type | Notes |
|--------|------|-------|
| `contact_id` | int FK → contacts | composite PK |
| `tag_id` | int FK → tags | composite PK |

### `emails`

Email templates. Used by both broadcasts and sequences.

| Column | Type | Notes |
|--------|------|-------|
| `id` | serial PK | |
| `sequence_id` | int FK → sequences | nullable (null = standalone / broadcast) |
| `slug` | text | unique |
| `subject` | text | |
| `preview` | text | inbox preview text |
| `delay_hours` | int | hours after sequence start; default 0 |
| `html` | text | rendered HTML body |
| `created_at` | timestamptz | |
| `updated_at` | timestamptz | |

### `broadcasts`

A scheduled or sent campaign linking an `email` template to a set of contacts.

| Column | Type | Notes |
|--------|------|-------|
| `id` | serial PK | |
| `email_id` | int FK → emails | |
| `slug` | text | unique |
| `status` | text | default `pending` |
| `name` | text | |
| `send_to_tag` | text | tag slug filter; `null` = all |
| `reply_to` | text | default `noreply@tailwindtraders.dev` |
| `created_at` | timestamptz | |
| `processed_at` | timestamptz | nullable |

### `messages`

An append-only log of every individual email queued for delivery. Not relational — it stands on its own as a historical record.

| Column | Type | Notes |
|--------|------|-------|
| `id` | serial PK | |
| `source` | text | `broadcast`, `sequence`, or `transaction` |
| `slug` | text | identifies the originating broadcast/sequence |
| `status` | text | default `pending`; updated to `sent` or `failed` |
| `send_to` | text | recipient email address |
| `send_from` | text | sender address |
| `subject` | text | |
| `html` | text | full rendered HTML |
| `send_at` | timestamptz | scheduled send time |
| `sent_at` | timestamptz | actual send time |
| `created_at` | timestamptz | |

### `sequences`

Named drip sequences that group a series of timed `emails`.

| Column | Type | Notes |
|--------|------|-------|
| `id` | serial PK | |
| `slug` | text | unique |
| `name` | text | nullable |
| `description` | text | nullable |
| `created_at` | timestamptz | |
| `updated_at` | timestamptz | |

### `subscriptions`

Tracks which contacts are enrolled in which sequences.

| Column | Type | Notes |
|--------|------|-------|
| `contact_id` | int FK → contacts | composite PK |
| `sequence_id` | int FK → sequences | composite PK |
| `created_at` | timestamptz | |

### `activity`

Event log for per-contact activity (e.g. link clicks, opens).

| Column | Type | Notes |
|--------|------|-------|
| `id` | serial PK | |
| `contact_id` | int FK → contacts | |
| `key` | text | event identifier |
| `description` | text | |
| `created_at` | timestamptz | | 
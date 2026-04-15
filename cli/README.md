# The Tailwind Mail CLI

A Node.js command-line tool for authoring broadcast emails and managing contacts. Broadcasts are written as markdown files with YAML frontmatter, which the CLI validates and sends to the API for queuing.

## Prerequisites

- Node.js 20 LTS (`n` is a handy version manager)
- The [server API](../server/README.md) running at `http://localhost:5000`

## Setup

```bash
cd cli
npm install
```

Create a `.env` file in the `cli/` directory:

```bash
API_ROOT="http://localhost:5000/admin"   # points to the running API server
```

Optionally add shell aliases for convenience:

```bash
alias mdmail="node ./bin/mdmail.js"
alias mt="npm run test"
```

## Commands

Run `node ./bin/mdmail.js --help` for a full list. The CLI uses sub-commands:

```
mdmail <command> [sub-command] [options]
```

### `mdmail init`

Creates the local directory structure needed by MDMail:

```
mail/
  broadcasts/   ← markdown broadcast files go here
  sequences/    ← (future) sequence email files
  sent/         ← processed files are moved here
```

Run this once before using any other commands.

```bash
mdmail init
```

---

### `mdmail broadcast new <subject>`

Interactively creates a new broadcast markdown file in `mail/broadcasts/`.

```bash
mdmail broadcast new "March Newsletter"
```

- Prompts whether to use AI (OpenAI) to generate the email body.
- Creates a file at `mail/broadcasts/march-newsletter.md` with pre-filled frontmatter.

Generated file format:

```markdown
---
Subject: "March Newsletter"
Slug: "march-newsletter"
Summary: "Summarize the email here for the preview"
SendToTag: "*"
---

The body of your email goes here.
```

| Frontmatter field | Required | Description |
|-------------------|----------|-------------|
| `Subject` | Yes | Email subject line |
| `Summary` | Yes | One-line inbox preview text |
| `Slug` | No | URL-safe identifier (auto-generated from Subject) |
| `SendToTag` | No | Tag slug to filter recipients; `"*"` sends to everyone |

---

### `mdmail broadcast send`

Validates and queues a broadcast for delivery via the API.

```bash
mdmail broadcast send
```

If more than one draft broadcast exists in `mail/broadcasts/`, you will be prompted to choose one.

The command:
1. Reads the chosen markdown file.
2. Calls `POST /admin/validate` to confirm the markdown is valid and shows you the contact count.
3. Calls `POST /admin/queue-broadcast` to insert messages into the send queue.

---

### `mdmail contact tag <file>`

Reads a CSV file of email addresses from `cli/csvs/` and applies one or more tags to those contacts via the API.

```bash
mdmail contact tag my-list.csv
```

- The CSV should have one email address per line (a header row is ignored if present).
- You will be prompted for the tag(s) to apply (comma-separated for multiple tags).
- Calls `POST /admin/bulk/contacts/tag`.

---

### `mdmail contact search <term>`

Fuzzy-searches contacts by name or email address.

```bash
mdmail contact search jane
```

Calls `GET /admin/contacts/search?term=<term>` and prints the results.

---

## Running Tests

```bash
npm test
```

Uses [Mocha](https://mochajs.org/) with test files under `test/`.

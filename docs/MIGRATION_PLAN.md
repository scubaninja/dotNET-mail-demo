# Migration Plan: C# (.NET 8) to Go

## Table of Contents

1. [Executive Summary](#1-executive-summary)
2. [Current Architecture](#2-current-architecture)
3. [Target Architecture](#3-target-architecture)
4. [Component Mapping](#4-component-mapping)
5. [Phase 1 – Project Scaffolding & Configuration](#5-phase-1--project-scaffolding--configuration)
6. [Phase 2 – Data Layer & Models](#6-phase-2--data-layer--models)
7. [Phase 3 – Business Logic (Commands)](#7-phase-3--business-logic-commands)
8. [Phase 4 – HTTP API Layer](#8-phase-4--http-api-layer)
9. [Phase 5 – Services (Background Workers & Email)](#9-phase-5--services-background-workers--email)
10. [Phase 6 – AI Integration](#10-phase-6--ai-integration)
11. [Phase 7 – CLI Migration](#11-phase-7--cli-migration)
12. [Phase 8 – Testing Strategy](#12-phase-8--testing-strategy)
13. [Phase 9 – Deployment & Infrastructure](#13-phase-9--deployment--infrastructure)
14. [Dependency Mapping](#14-dependency-mapping)
15. [Risk Assessment & Mitigations](#15-risk-assessment--mitigations)
16. [Migration Checklist](#16-migration-checklist)

---

## 1. Executive Summary

This document outlines the plan to migrate the **Tailwind Traders Mail Service** server component from C# (.NET 8 / ASP.NET Core) to Go. The application is a transactional and bulk email sending service with a REST API, background email processing, PostgreSQL persistence, and Azure OpenAI integration.

### Goals

- Replace the C# `server/` component with a Go implementation
- Maintain the same REST API contract (endpoints, request/response shapes)
- Reuse the existing PostgreSQL schema (`db/db.sql`) without changes
- Integrate with the existing Go `jobs/` component where possible
- Maintain the same Docker Compose and deployment workflows

### Scope

| In Scope | Out of Scope |
|---|---|
| `server/` C# → Go rewrite | `db/` schema (unchanged) |
| API routes, models, commands, services | `cli/` Node.js tool (unchanged) |
| Background email worker | `jobs/` existing Go workers (unchanged) |
| Configuration management | Azure infrastructure (Bicep templates) |
| Unit and integration tests | |

---

## 2. Current Architecture

```
┌──────────────────────────────────────────────────────────┐
│                    C# Server (ASP.NET Core / .NET 8)     │
│                                                          │
│  Program.cs ─► Swagger + DI Container + Route Mapping    │
│                                                          │
│  Api/                                                    │
│  ├── PublicRoutes.cs        GET /about, /unsubscribe,    │
│  │                          /link/clicked, POST /signup  │
│  └── Admin/                                              │
│      ├── BroadcastRoutes.cs POST /admin/validate,        │
│      │                      /admin/queue-broadcast,      │
│      │                      /admin/get-chat              │
│      ├── ContactRoutes.cs   GET /admin/contacts/search   │
│      └── BulkOperationRoutes.cs                          │
│                             POST /admin/bulk/contacts/tag│
│                                                          │
│  Models/          Contact, Broadcast, Email, Message,    │
│                   MarkdownEmail, Tag, Activity            │
│                                                          │
│  Commands/        CreateBroadcast, ContactOptOut,         │
│                   ContactOptin, ContactSignup,            │
│                   BulkTag, LinkClicked                    │
│                                                          │
│  Services/        BackgroundSend, SmtpEmailSender,        │
│                   MailHogSender, InMemoryEmailSender,     │
│                   AI (Azure OpenAI)                       │
│                                                          │
│  Data/            DB (Npgsql + Dapper), Extensions        │
└──────────────┬───────────────────────────────────────────┘
               │
               ▼
        ┌─────────────┐
        │  PostgreSQL  │
        │  (mail.*)    │
        └─────────────┘
```

### Key Technologies

| Layer | C# Technology | Purpose |
|---|---|---|
| Web Framework | ASP.NET Core Minimal APIs | HTTP routing |
| ORM | Dapper + SimpleCRUD | Data access |
| Database Driver | Npgsql | PostgreSQL |
| Markdown | Markdig | Markdown → HTML |
| YAML | YamlDotNet | Frontmatter parsing |
| Config | Viper.NET | Environment + appsettings |
| API Docs | Swashbuckle | Swagger/OpenAPI |
| AI | Azure.AI.OpenAI | GPT-4 integration |
| Testing | xUnit | Unit tests |
| DI | Built-in ASP.NET Core DI | Dependency injection |

---

## 3. Target Architecture

```
┌──────────────────────────────────────────────────────────┐
│                    Go Server                             │
│                                                          │
│  cmd/server/main.go ─► Config + Router + DI + Server     │
│                                                          │
│  internal/api/                                           │
│  ├── public_routes.go       GET /about, /unsubscribe,    │
│  │                          /link/clicked, POST /signup  │
│  └── admin/                                              │
│      ├── broadcast_routes.go                             │
│      ├── contact_routes.go                               │
│      └── bulk_operation_routes.go                        │
│                                                          │
│  internal/models/  contact.go, broadcast.go, email.go,   │
│                    message.go, markdown_email.go,         │
│                    tag.go, activity.go                    │
│                                                          │
│  internal/commands/ create_broadcast.go, contact_optout.go│
│                     contact_optin.go, contact_signup.go,  │
│                     bulk_tag.go, link_clicked.go          │
│                                                          │
│  internal/services/ background_send.go, email_sender.go, │
│                     ai.go                                │
│                                                          │
│  internal/data/     db.go, extensions.go                 │
│                                                          │
│  internal/config/   config.go                            │
└──────────────┬───────────────────────────────────────────┘
               │
               ▼
        ┌─────────────┐
        │  PostgreSQL  │
        │  (mail.*)    │
        └─────────────┘
```

### Target Directory Structure

```
server-go/
├── cmd/
│   └── server/
│       └── main.go                 # Entry point
├── internal/
│   ├── api/
│   │   ├── public_routes.go        # Public endpoints
│   │   ├── public_routes_test.go
│   │   └── admin/
│   │       ├── broadcast_routes.go
│   │       ├── broadcast_routes_test.go
│   │       ├── contact_routes.go
│   │       ├── contact_routes_test.go
│   │       ├── bulk_operation_routes.go
│   │       └── bulk_operation_routes_test.go
│   ├── commands/
│   │   ├── command_result.go
│   │   ├── create_broadcast.go
│   │   ├── create_broadcast_test.go
│   │   ├── contact_optout.go
│   │   ├── contact_optin.go
│   │   ├── contact_signup.go
│   │   ├── bulk_tag.go
│   │   └── link_clicked.go
│   ├── config/
│   │   └── config.go               # Env + file-based config
│   ├── data/
│   │   ├── db.go                   # PostgreSQL connection pool
│   │   └── extensions.go           # snake_case helper
│   ├── models/
│   │   ├── contact.go
│   │   ├── contact_test.go
│   │   ├── broadcast.go
│   │   ├── broadcast_test.go
│   │   ├── email.go
│   │   ├── message.go
│   │   ├── message_test.go
│   │   ├── markdown_email.go
│   │   ├── markdown_email_test.go
│   │   ├── tag.go
│   │   └── activity.go
│   └── services/
│       ├── background_send.go
│       ├── email_sender.go         # Interface + implementations
│       └── ai.go                   # Azure OpenAI client
├── go.mod
├── go.sum
├── Makefile
├── Dockerfile
└── README.md
```

---

## 4. Component Mapping

### C# → Go Type/Package Mapping

| C# Component | Go Equivalent |
|---|---|
| `namespace Tailwind.Mail.Models` | `package models` (`internal/models/`) |
| `namespace Tailwind.Mail.Commands` | `package commands` (`internal/commands/`) |
| `namespace Tailwind.Mail.Api` | `package api` (`internal/api/`) |
| `namespace Tailwind.Mail.Api.Admin` | `package admin` (`internal/api/admin/`) |
| `namespace Tailwind.Mail.Services` | `package services` (`internal/services/`) |
| `namespace Tailwind.Data` | `package data` (`internal/data/`) |
| `namespace Tailwind.AI` | `package services` (merged into services) |
| `Program.cs` (entry point) | `cmd/server/main.go` |

### C# → Go Pattern Mapping

| C# Pattern | Go Equivalent |
|---|---|
| ASP.NET Core Minimal APIs | `net/http` + `chi` (or `gorilla/mux`) router |
| Dependency Injection (DI container) | Constructor injection / function parameters |
| `IDbConnection` interface | `*sql.DB` connection pool |
| Dapper + SimpleCRUD ORM | `database/sql` + `sqlx` |
| `BackgroundService` | goroutine with `context.Context` + `time.Ticker` |
| `IEmailSender` interface | Go interface `EmailSender` |
| xUnit tests | `testing` package + `testify` |
| `appsettings.json` + env vars | `os.Getenv` + `encoding/json` config file |
| Swagger/OpenAPI | `swaggo/swag` or hand-maintained OpenAPI spec |
| YAML frontmatter (YamlDotNet) | `gopkg.in/yaml.v3` |
| Markdown rendering (Markdig) | `github.com/yuin/goldmark` |
| `dynamic` / `ExpandoObject` | `map[string]interface{}` or typed structs |

---

## 5. Phase 1 – Project Scaffolding & Configuration

### Duration: ~1 day

### Tasks

1. **Initialize Go module**
   ```bash
   mkdir -p server-go/cmd/server server-go/internal/{api/admin,commands,config,data,models,services}
   cd server-go && go mod init tailwind-mail
   ```

2. **Configuration** (`internal/config/config.go`)

   Replace Viper.NET with direct environment variable reading:

   ```go
   package config

   import "os"

   type Config struct {
       DatabaseURL       string
       SendWorker        string
       DefaultFrom       string
       SMTPHost          string
       SMTPUser          string
       SMTPPassword      string
       AzureOpenAIEndpoint string
       AzureOpenAIAPIKey   string
   }

   func Load() *Config {
       return &Config{
           DatabaseURL:       getEnv("DATABASE_URL", ""),
           SendWorker:        getEnv("SEND_WORKER", ""),
           DefaultFrom:       getEnv("DEFAULT_FROM", "noreply@tailwind.dev"),
           SMTPHost:          getEnv("SMTP_HOST", ""),
           SMTPUser:          getEnv("SMTP_USER", ""),
           SMTPPassword:      getEnv("SMTP_PASSWORD", ""),
           AzureOpenAIEndpoint: getEnv("AZURE_OPENAI_ENDPOINT", ""),
           AzureOpenAIAPIKey:   getEnv("AZURE_OPENAI_API_KEY", ""),
       }
   }

   func getEnv(key, fallback string) string {
       if val := os.Getenv(key); val != "" {
           return val
       }
       return fallback
   }
   ```

3. **Install core dependencies**
   ```bash
   go get github.com/go-chi/chi/v5          # HTTP router
   go get github.com/jmoiron/sqlx            # SQL extensions
   go get github.com/lib/pq                  # PostgreSQL driver
   go get github.com/yuin/goldmark           # Markdown → HTML
   go get gopkg.in/yaml.v3                   # YAML parsing
   go get github.com/stretchr/testify        # Test assertions
   ```

### Verification

- `go build ./...` succeeds
- Config loads from environment variables

---

## 6. Phase 2 – Data Layer & Models

### Duration: ~2 days

### 6.1 Database Connection (`internal/data/db.go`)

Replace Npgsql + Dapper with `database/sql` + `sqlx` + `lib/pq`:

```go
package data

import (
    "fmt"
    "github.com/jmoiron/sqlx"
    _ "github.com/lib/pq"
)

type DB struct {
    Pool *sqlx.DB
}

func NewDB(databaseURL string) (*DB, error) {
    if databaseURL == "" {
        return nil, fmt.Errorf("no DATABASE_URL found in environment")
    }
    pool, err := sqlx.Connect("postgres", databaseURL)
    if err != nil {
        return nil, fmt.Errorf("failed to connect to database: %w", err)
    }
    return &DB{Pool: pool}, nil
}
```

> **Key difference:** C# opens a new connection per request via `IDb.Connect()`. In Go, `sqlx.DB` is a connection pool — pass it around and let the pool manage connections.

### 6.2 Models

Each C# model maps to a Go struct with `db:` tags for `sqlx`:

#### Contact (`internal/models/contact.go`)

| C# Property | Go Field | DB Column |
|---|---|---|
| `int? ID` | `ID int` | `id` |
| `string Email` | `Email string` | `email` |
| `string Name` | `Name string` | `name` |
| `string Key` | `Key string` | `key` |
| `bool Subscribed` | `Subscribed bool` | `subscribed` |
| `DateTimeOffset CreatedAt` | `CreatedAt time.Time` | `created_at` |
| `DateTimeOffset UpdatedAt` | `UpdatedAt time.Time` | `updated_at` |

```go
package models

import (
    "time"
    "github.com/google/uuid"
)

type Contact struct {
    ID         int       `db:"id" json:"id,omitempty"`
    Email      string    `db:"email" json:"email"`
    Name       string    `db:"name" json:"name"`
    Key        string    `db:"key" json:"key"`
    Subscribed bool      `db:"subscribed" json:"subscribed"`
    CreatedAt  time.Time `db:"created_at" json:"created_at"`
    UpdatedAt  time.Time `db:"updated_at" json:"updated_at"`
}

type SignUpRequest struct {
    Name  string `json:"name"`
    Email string `json:"email"`
}

func NewContact(name, email string) Contact {
    return Contact{
        Name:       name,
        Email:      email,
        Key:        uuid.New().String(),
        Subscribed: true,
    }
}
```

#### Broadcast (`internal/models/broadcast.go`)

```go
type Broadcast struct {
    ID        int       `db:"id" json:"id,omitempty"`
    EmailID   int       `db:"email_id" json:"email_id,omitempty"`
    Status    string    `db:"status" json:"status"`
    Name      string    `db:"name" json:"name"`
    Slug      string    `db:"slug" json:"slug"`
    ReplyTo   string    `db:"reply_to" json:"reply_to"`
    SendToTag string    `db:"send_to_tag" json:"send_to_tag"`
    CreatedAt time.Time `db:"created_at" json:"created_at"`
}
```

#### Message (`internal/models/message.go`)

```go
type Message struct {
    ID        int       `db:"id" json:"id,omitempty"`
    Source    string    `db:"source" json:"source"`
    Slug      string    `db:"slug" json:"slug"`
    Status    string    `db:"status" json:"status"`
    SendTo    string    `db:"send_to" json:"send_to"`
    SendFrom  string    `db:"send_from" json:"send_from"`
    Subject   string    `db:"subject" json:"subject"`
    HTML      string    `db:"html" json:"html"`
    SendAt    time.Time `db:"send_at" json:"send_at"`
    SentAt    time.Time `db:"sent_at" json:"sent_at,omitempty"`
    CreatedAt time.Time `db:"created_at" json:"created_at"`
}

func (m *Message) MarkSent() {
    m.Status = "sent"
    m.SentAt = time.Now().UTC()
}

func (m *Message) ReadyToSend() bool {
    return m.Status == "pending" &&
        m.SendTo != "" &&
        m.SendFrom != "" &&
        m.HTML != "" &&
        m.Subject != ""
}
```

#### Email, Tag, Activity, Tagged

Follow the same struct-with-tags pattern. Full model list:

| C# Model | Go File | Key Notes |
|---|---|---|
| `Contact.cs` | `contact.go` | `uuid.New()` replaces `Guid.NewGuid()` |
| `Broadcast.cs` | `broadcast.go` | `FromMarkdownEmail()` factory method |
| `Email.cs` | `email.go` | Constructor takes `MarkdownEmail` |
| `Message.cs` | `message.go` | `MarkSent()`, `ReadyToSend()` methods |
| `MarkdownEmail.cs` | `markdown_email.go` | Uses goldmark + yaml.v3 |
| `Tag.cs` | `tag.go` | Slug generation from name |
| `Tagged` | `tag.go` | Junction table struct |
| `Activity.cs` | `activity.go` | UUID key default |
| `CommandResult` | `command_result.go` | Generic result struct |

### 6.3 MarkdownEmail Parser (`internal/models/markdown_email.go`)

This is the most complex model migration. The C# version uses YamlDotNet to parse YAML frontmatter and Markdig for Markdown → HTML.

```go
package models

import (
    "bytes"
    "fmt"
    "strings"

    "github.com/yuin/goldmark"
    "gopkg.in/yaml.v3"
)

type MarkdownEmailData struct {
    Subject   string `yaml:"Subject"`
    Summary   string `yaml:"Summary"`
    Slug      string `yaml:"Slug"`
    SendToTag string `yaml:"SendToTag"`
}

type MarkdownEmail struct {
    Markdown string
    HTML     string
    Data     *MarkdownEmailData
}

func MarkdownEmailFromString(markdown string) (*MarkdownEmail, error) {
    email := &MarkdownEmail{Markdown: markdown}
    if err := email.render(); err != nil {
        return nil, err
    }
    return email, nil
}

func (e *MarkdownEmail) IsValid() bool {
    return e.Data != nil && e.Data.Subject != "" && e.Data.Summary != ""
}

func (e *MarkdownEmail) render() error {
    if e.Markdown == "" {
        return fmt.Errorf("markdown is empty; set it first")
    }

    // Parse YAML frontmatter (between --- delimiters)
    parts := strings.SplitN(e.Markdown, "---", 3)
    if len(parts) >= 3 {
        e.Data = &MarkdownEmailData{}
        if err := yaml.Unmarshal([]byte(parts[1]), e.Data); err != nil {
            return fmt.Errorf("failed to parse frontmatter: %w", err)
        }
    }

    // Generate slug from subject if not provided
    if e.Data != nil {
        if e.Data.Slug == "" && e.Data.Subject != "" {
            e.Data.Slug = strings.ToLower(strings.ReplaceAll(e.Data.Subject, " ", "-"))
        }
        if e.Data.SendToTag == "" {
            e.Data.SendToTag = "*"
        }
    }

    // Render Markdown → HTML
    var buf bytes.Buffer
    md := goldmark.New()
    if err := md.Convert([]byte(e.Markdown), &buf); err != nil {
        return fmt.Errorf("failed to render markdown: %w", err)
    }
    e.HTML = buf.String()

    return nil
}
```

### 6.4 String Extensions (`internal/data/extensions.go`)

The `ToSnakeCase` extension method maps directly:

```go
package data

import (
    "strings"
    "unicode"
)

func ToSnakeCase(s string) string {
    if s == "ID" {
        return "id"
    }
    var result strings.Builder
    for i, r := range s {
        if unicode.IsUpper(r) {
            if i > 0 {
                result.WriteByte('_')
            }
            result.WriteRune(unicode.ToLower(r))
        } else {
            result.WriteRune(r)
        }
    }
    return result.String()
}
```

> **Note:** The `ToExpando`, `ToValueList`, `ToColumnList`, `ToSettingList` extensions are not needed in Go. `sqlx` handles struct-to-column mapping via `db:` tags.

---

## 7. Phase 3 – Business Logic (Commands)

### Duration: ~2 days

### 7.1 CommandResult (`internal/commands/command_result.go`)

```go
package commands

type CommandResult struct {
    Data     interface{} `json:"data,omitempty"`
    Inserted int         `json:"inserted"`
    Updated  int         `json:"updated"`
    Deleted  int         `json:"deleted"`
}
```

### 7.2 Command Migration Map

| C# Command | Go Function | Key Differences |
|---|---|---|
| `CreateBroadcast.Execute(IDbConnection)` | `CreateBroadcast(tx *sqlx.Tx, ...) (*CommandResult, error)` | Uses `sqlx.Tx` instead of Dapper; returns `error` instead of exceptions |
| `ContactOptOutCommand.Execute(IDbConnection)` | `ContactOptOut(tx *sqlx.Tx, key string) (*CommandResult, error)` | Same pattern |
| `ContactOptinCommand.Execute(IDbConnection)` | `ContactOptIn(tx *sqlx.Tx, contact *models.Contact) (*CommandResult, error)` | Same pattern |
| `ContactSignupCommand.Execute(IDbConnection)` | `ContactSignup(tx *sqlx.Tx, contact *models.Contact) (*CommandResult, error)` | Same pattern |
| `BulkTagCommand.Execute(IDbConnection)` | `BulkTag(tx *sqlx.Tx, tag string, emails []string) (*CommandResult, error)` | Same pattern |
| `LinkClickedCommand.Execute()` | `LinkClicked(key string) string` | No DB interaction |

### 7.3 Key Translation: CreateBroadcast

The most complex command. C# uses Dapper's `conn.Insert()` and raw SQL:

```go
func CreateBroadcast(db *sqlx.DB, doc *models.MarkdownEmail, defaultFrom string) (*CommandResult, error) {
    if defaultFrom == "" {
        defaultFrom = "noreply@tailwind.dev"
    }

    tx, err := db.Beginx()
    if err != nil {
        return nil, fmt.Errorf("failed to begin transaction: %w", err)
    }
    defer tx.Rollback() // no-op if committed

    // Insert email template
    email := models.NewEmailFromMarkdown(doc)
    var emailID int
    err = tx.QueryRow(
        `INSERT INTO mail.emails (slug, subject, preview, html)
         VALUES ($1, $2, $3, $4) RETURNING id`,
        email.Slug, email.Subject, email.Preview, email.HTML,
    ).Scan(&emailID)
    if err != nil {
        return nil, fmt.Errorf("failed to insert email: %w", err)
    }

    // Insert broadcast
    broadcast := models.BroadcastFromMarkdownEmail(doc)
    broadcast.EmailID = emailID
    broadcast.ReplyTo = defaultFrom
    var broadcastID int
    err = tx.QueryRow(
        `INSERT INTO mail.broadcasts (email_id, slug, name, send_to_tag, reply_to)
         VALUES ($1, $2, $3, $4, $5) RETURNING id`,
        broadcast.EmailID, broadcast.Slug, broadcast.Name,
        broadcast.SendToTag, broadcast.ReplyTo,
    ).Scan(&broadcastID)
    if err != nil {
        return nil, fmt.Errorf("failed to insert broadcast: %w", err)
    }

    // Create messages for contacts
    var sql string
    var messagesCreated int64

    if broadcast.SendToTag != "*" {
        sql = `INSERT INTO mail.messages (source, slug, send_to, send_from, subject, html, send_at)
               SELECT 'broadcast', $1, mail.contacts.email, $2, $3, $4, now()
               FROM mail.contacts
               INNER JOIN mail.tagged ON mail.tagged.contact_id = mail.contacts.id
               INNER JOIN mail.tags ON mail.tags.id = mail.tagged.tag_id
               WHERE subscribed = true AND mail.tags.slug = $5`
        result, err := tx.Exec(sql, email.Slug, defaultFrom, email.Subject, email.HTML, broadcast.SendToTag)
        if err != nil {
            return nil, fmt.Errorf("failed to create messages: %w", err)
        }
        messagesCreated, _ = result.RowsAffected()
    } else {
        sql = `INSERT INTO mail.messages (source, slug, send_to, send_from, subject, html, send_at)
               SELECT 'broadcast', $1, mail.contacts.email, $2, $3, $4, now()
               FROM mail.contacts WHERE subscribed = true`
        result, err := tx.Exec(sql, email.Slug, defaultFrom, email.Subject, email.HTML)
        if err != nil {
            return nil, fmt.Errorf("failed to create messages: %w", err)
        }
        messagesCreated, _ = result.RowsAffected()
    }

    // Notify via PostgreSQL NOTIFY
    _, err = tx.Exec("SELECT pg_notify('broadcasts', $1)", broadcast.Slug)
    if err != nil {
        return nil, fmt.Errorf("failed to notify: %w", err)
    }

    if err := tx.Commit(); err != nil {
        return nil, fmt.Errorf("failed to commit transaction: %w", err)
    }

    return &CommandResult{
        Data: map[string]interface{}{
            "BroadcastId": broadcastID,
            "EmailId":     emailID,
            "Notified":    true,
        },
        Inserted: int(messagesCreated),
    }, nil
}
```

### Key Differences (C# vs Go)

| Aspect | C# | Go |
|---|---|---|
| Error handling | `try/catch` + `throw` | `if err != nil { return err }` |
| Transactions | `conn.BeginTransaction()` | `db.Beginx()` + `defer tx.Rollback()` |
| ORM inserts | `conn.Insert(entity, tx)` | Raw SQL with `RETURNING id` |
| Dynamic results | `new { ... }` anonymous types | `map[string]interface{}` |
| Null handling | `int?` nullable types | Pointer types `*int` or `sql.NullInt64` |

---

## 8. Phase 4 – HTTP API Layer

### Duration: ~2 days

### 8.1 Router Setup (`cmd/server/main.go`)

```go
package main

import (
    "log"
    "net/http"

    "github.com/go-chi/chi/v5"
    "github.com/go-chi/chi/v5/middleware"

    "tailwind-mail/internal/api"
    "tailwind-mail/internal/api/admin"
    "tailwind-mail/internal/config"
    "tailwind-mail/internal/data"
    "tailwind-mail/internal/services"
)

func main() {
    cfg := config.Load()

    db, err := data.NewDB(cfg.DatabaseURL)
    if err != nil {
        log.Fatalf("failed to connect to database: %v", err)
    }
    defer db.Pool.Close()

    r := chi.NewRouter()
    r.Use(middleware.Logger)
    r.Use(middleware.Recoverer)

    // Register routes
    api.RegisterPublicRoutes(r, db)
    admin.RegisterBroadcastRoutes(r, db, cfg)
    admin.RegisterContactRoutes(r, db)
    admin.RegisterBulkOperationRoutes(r, db)

    // Start background worker if configured
    if cfg.SendWorker == "local" {
        sender := services.NewSMTPEmailSender(cfg)
        go services.StartBackgroundSend(db, sender)
    }

    log.Println("Server starting on :5000")
    log.Fatal(http.ListenAndServe(":5000", r))
}
```

### 8.2 Route Mapping

| C# Route | Go Route | Handler |
|---|---|---|
| `app.MapGet("/about", ...)` | `r.Get("/about", ...)` | `api.HandleAbout` |
| `app.MapGet("/unsubscribe/{key}", ...)` | `r.Get("/unsubscribe/{key}", ...)` | `api.HandleUnsubscribe` |
| `app.MapGet("/link/clicked/{key}", ...)` | `r.Get("/link/clicked/{key}", ...)` | `api.HandleLinkClicked` |
| `app.MapPost("/signup", ...)` | `r.Post("/signup", ...)` | `api.HandleSignup` |
| `app.MapPost("/admin/validate", ...)` | `r.Post("/admin/validate", ...)` | `admin.HandleValidate` |
| `app.MapPost("/admin/queue-broadcast", ...)` | `r.Post("/admin/queue-broadcast", ...)` | `admin.HandleQueueBroadcast` |
| `app.MapPost("/admin/get-chat", ...)` | `r.Post("/admin/get-chat", ...)` | `admin.HandleGetChat` |
| `app.MapGet("/admin/contacts/search", ...)` | `r.Get("/admin/contacts/search", ...)` | `admin.HandleContactSearch` |
| `app.MapPost("/admin/bulk/contacts/tag", ...)` | `r.Post("/admin/bulk/contacts/tag", ...)` | `admin.HandleBulkTag` |

### 8.3 Example Handler (`internal/api/public_routes.go`)

```go
package api

import (
    "encoding/json"
    "net/http"

    "github.com/go-chi/chi/v5"
    "tailwind-mail/internal/commands"
    "tailwind-mail/internal/data"
    "tailwind-mail/internal/models"
)

func RegisterPublicRoutes(r chi.Router, db *data.DB) {
    r.Get("/about", handleAbout)
    r.Get("/unsubscribe/{key}", handleUnsubscribe(db))
    r.Get("/link/clicked/{key}", handleLinkClicked)
    r.Post("/signup", handleSignup(db))
}

func handleAbout(w http.ResponseWriter, r *http.Request) {
    w.Header().Set("Content-Type", "text/plain")
    w.Write([]byte("Tailwind Traders Mail Services API"))
}

func handleUnsubscribe(db *data.DB) http.HandlerFunc {
    return func(w http.ResponseWriter, r *http.Request) {
        key := chi.URLParam(r, "key")
        result, err := commands.ContactOptOut(db.Pool, key)
        if err != nil {
            http.Error(w, err.Error(), http.StatusInternalServerError)
            return
        }
        json.NewEncoder(w).Encode(result.Updated > 0)
    }
}

func handleSignup(db *data.DB) http.HandlerFunc {
    return func(w http.ResponseWriter, r *http.Request) {
        var req models.SignUpRequest
        if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
            http.Error(w, "invalid request body", http.StatusBadRequest)
            return
        }
        result, err := db.Pool.Exec(
            "INSERT INTO mail.contacts (email, name) VALUES ($1, $2)",
            req.Email, req.Name,
        )
        if err != nil {
            http.Error(w, err.Error(), http.StatusInternalServerError)
            return
        }
        rowsAffected, _ := result.RowsAffected()
        json.NewEncoder(w).Encode(rowsAffected)
    }
}
```

### 8.4 JSON Response Patterns

| C# Pattern | Go Equivalent |
|---|---|
| Implicit JSON serialization (Minimal API) | `json.NewEncoder(w).Encode(response)` |
| `[FromBody]` attribute | `json.NewDecoder(r.Body).Decode(&req)` |
| `[FromQuery]` attribute | `r.URL.Query().Get("param")` |
| `[FromServices]` DI | Closure over dependencies (handler factory) |
| `.WithOpenApi()` | Swagger comments or separate OpenAPI YAML |

---

## 9. Phase 5 – Services (Background Workers & Email)

### Duration: ~2 days

### 9.1 EmailSender Interface (`internal/services/email_sender.go`)

```go
package services

import "tailwind-mail/internal/models"

// EmailSender mirrors the C# IEmailSender interface
type EmailSender interface {
    Send(msg *models.Message) error
    SendBulk(msgs []*models.Message) (int, error)
}
```

#### Implementations

| C# Class | Go Type | Notes |
|---|---|---|
| `InMemoryEmailSender` | `InMemoryEmailSender` | For testing |
| `MailHogSender` | `MailHogSender` | Development (localhost:1025) |
| `SmtpEmailSender` | `SMTPEmailSender` | Production SMTP |

### 9.2 Background Worker (`internal/services/background_send.go`)

Replace `BackgroundService` with a goroutine:

```go
package services

import (
    "context"
    "log"
    "time"

    "tailwind-mail/internal/data"
    "tailwind-mail/internal/models"
)

func StartBackgroundSend(ctx context.Context, db *data.DB, sender EmailSender) {
    ticker := time.NewTicker(1 * time.Minute)
    defer ticker.Stop()

    for {
        select {
        case <-ctx.Done():
            log.Println("Background sender shutting down")
            return
        case <-ticker.C:
            log.Println("Checking for email to send...")
            messages, err := fetchPendingMessages(db)
            if err != nil {
                log.Printf("Error fetching messages: %v", err)
                continue
            }
            if len(messages) > 0 {
                count, err := sender.SendBulk(messages)
                if err != nil {
                    log.Printf("Error sending bulk: %v", err)
                }
                if count > 0 {
                    log.Printf("Sent %d messages", count)
                }
            }
        }
    }
}

func fetchPendingMessages(db *data.DB) ([]*models.Message, error) {
    var messages []*models.Message
    err := db.Pool.Select(&messages,
        "SELECT * FROM mail.messages WHERE status = 'pending' AND send_at <= now()")
    return messages, err
}
```

### 9.3 AI Service (`internal/services/ai.go`)

Replace Azure.AI.OpenAI SDK with REST API calls using `net/http`:

```go
package services

import (
    "bytes"
    "encoding/json"
    "fmt"
    "io"
    "net/http"

    "tailwind-mail/internal/config"
)

type AIChat struct {
    Endpoint string
    APIKey   string
}

func NewAIChat(cfg *config.Config) *AIChat {
    return &AIChat{
        Endpoint: cfg.AzureOpenAIEndpoint,
        APIKey:   cfg.AzureOpenAIAPIKey,
    }
}

func (c *AIChat) Prompt(prompt string) (string, error) {
    url := fmt.Sprintf("%s/openai/deployments/gpt-4/chat/completions?api-version=2024-02-15-preview", c.Endpoint)

    body := map[string]interface{}{
        "messages": []map[string]string{
            {"role": "user", "content": prompt},
        },
    }
    jsonBody, _ := json.Marshal(body)

    req, err := http.NewRequest("POST", url, bytes.NewReader(jsonBody))
    if err != nil {
        return "", err
    }
    req.Header.Set("Content-Type", "application/json")
    req.Header.Set("api-key", c.APIKey)

    resp, err := http.DefaultClient.Do(req)
    if err != nil {
        return "", err
    }
    defer resp.Body.Close()

    respBody, _ := io.ReadAll(resp.Body)
    var result struct {
        Choices []struct {
            Message struct {
                Content string `json:"content"`
            } `json:"message"`
        } `json:"choices"`
    }
    if err := json.Unmarshal(respBody, &result); err != nil {
        return "", err
    }
    if len(result.Choices) == 0 {
        return "", fmt.Errorf("no response from AI")
    }
    return result.Choices[0].Message.Content, nil
}
```

---

## 10. Phase 6 – AI Integration

### Duration: ~0.5 day

Already covered in Phase 5 (Section 9.3). The Azure OpenAI integration replaces the C# `Azure.AI.OpenAI` NuGet package with direct REST calls, which is a more idiomatic Go approach.

### Alternative: Use Azure SDK for Go

```bash
go get github.com/Azure/azure-sdk-for-go/sdk/ai/azopenai
```

This provides a typed Go client similar to the C# SDK.

---

## 11. Phase 7 – CLI Migration

### Duration: Not in scope

The CLI (`cli/`) is a Node.js application that communicates with the server via HTTP. Since the Go server maintains the same REST API contract, **no changes are needed to the CLI**.

Verify compatibility by running the CLI against the Go server during integration testing.

---

## 12. Phase 8 – Testing Strategy

### Duration: ~2 days

### 12.1 Test Framework

| C# (xUnit) | Go Equivalent |
|---|---|
| `[Fact]` | `func TestXxx(t *testing.T)` |
| `[Theory]` / `[InlineData]` | Table-driven tests |
| `Assert.Equal()` | `assert.Equal(t, ...)` (testify) |
| `IClassFixture` | `TestMain(m *testing.M)` |
| `WebApplicationFactory` | `httptest.NewServer` |

### 12.2 Unit Tests (no database required)

| Test File | What It Tests |
|---|---|
| `models/message_test.go` | `MarkSent()`, `ReadyToSend()` logic |
| `models/markdown_email_test.go` | YAML parsing, HTML rendering, validation |
| `models/contact_test.go` | `NewContact()` defaults |
| `models/broadcast_test.go` | `FromMarkdownEmail()` factory |
| `data/extensions_test.go` | `ToSnakeCase()` function |
| `commands/link_clicked_test.go` | Simple string return |
| `config/config_test.go` | Environment variable loading |

### 12.3 Integration Tests (require database)

| Test File | What It Tests |
|---|---|
| `api/public_routes_test.go` | HTTP handler responses |
| `api/admin/broadcast_routes_test.go` | Broadcast validation + queue |
| `api/admin/contact_routes_test.go` | Contact search |
| `commands/create_broadcast_test.go` | Full broadcast creation flow |
| `commands/contact_optout_test.go` | Opt-out with DB |

### 12.4 Example Unit Test

```go
// internal/models/message_test.go
package models

import (
    "testing"
    "github.com/stretchr/testify/assert"
)

func TestMessage_ReadyToSend_WhenAllFieldsPresent(t *testing.T) {
    msg := Message{
        Status:   "pending",
        SendTo:   "user@example.com",
        SendFrom: "noreply@tailwind.dev",
        Subject:  "Test",
        HTML:     "<p>Hello</p>",
    }
    assert.True(t, msg.ReadyToSend())
}

func TestMessage_ReadyToSend_WhenStatusNotPending(t *testing.T) {
    msg := Message{
        Status:   "sent",
        SendTo:   "user@example.com",
        SendFrom: "noreply@tailwind.dev",
        Subject:  "Test",
        HTML:     "<p>Hello</p>",
    }
    assert.False(t, msg.ReadyToSend())
}

func TestMessage_ReadyToSend_WhenMissingFields(t *testing.T) {
    tests := []struct {
        name string
        msg  Message
    }{
        {"missing SendTo", Message{Status: "pending", SendFrom: "a", Subject: "b", HTML: "c"}},
        {"missing SendFrom", Message{Status: "pending", SendTo: "a", Subject: "b", HTML: "c"}},
        {"missing Subject", Message{Status: "pending", SendTo: "a", SendFrom: "b", HTML: "c"}},
        {"missing HTML", Message{Status: "pending", SendTo: "a", SendFrom: "b", Subject: "c"}},
    }
    for _, tt := range tests {
        t.Run(tt.name, func(t *testing.T) {
            assert.False(t, tt.msg.ReadyToSend())
        })
    }
}

func TestMessage_MarkSent(t *testing.T) {
    msg := Message{Status: "pending"}
    msg.MarkSent()
    assert.Equal(t, "sent", msg.Status)
    assert.False(t, msg.SentAt.IsZero())
}
```

### 12.5 Example API Test

```go
// internal/api/public_routes_test.go
package api

import (
    "net/http"
    "net/http/httptest"
    "testing"

    "github.com/stretchr/testify/assert"
)

func TestHandleAbout(t *testing.T) {
    req := httptest.NewRequest("GET", "/about", nil)
    w := httptest.NewRecorder()

    handleAbout(w, req)

    assert.Equal(t, http.StatusOK, w.Code)
    assert.Equal(t, "Tailwind Traders Mail Services API", w.Body.String())
}
```

### 12.6 Running Tests

```bash
# Unit tests only (no DB)
go test ./internal/models/... ./internal/data/... ./internal/config/...

# All tests (requires PostgreSQL)
go test ./...

# With verbose output
go test -v ./...

# With coverage
go test -cover ./...
```

---

## 13. Phase 9 – Deployment & Infrastructure

### Duration: ~1 day

### 13.1 Dockerfile (`server-go/Dockerfile`)

```dockerfile
# Build stage
FROM golang:1.21-alpine AS builder
WORKDIR /app
COPY go.mod go.sum ./
RUN go mod download
COPY . .
RUN CGO_ENABLED=0 GOOS=linux go build -o server ./cmd/server

# Runtime stage
FROM gcr.io/distroless/static-debian12
COPY --from=builder /app/server /server
EXPOSE 5000
CMD ["/server"]
```

### 13.2 Docker Compose Update

```yaml
services:
  server:
    build:
      context: ./server-go
      dockerfile: Dockerfile
    ports:
      - "5000:5000"
    environment:
      - DATABASE_URL=postgresql://user:pass@db:5432/tailwind
      - SEND_WORKER=local
    depends_on:
      - db

  db:
    image: postgres:16
    environment:
      POSTGRES_DB: tailwind
      POSTGRES_USER: user
      POSTGRES_PASSWORD: pass
    volumes:
      - ./db/db.sql:/docker-entrypoint-initdb.d/01-schema.sql
      - ./db/seed.sql:/docker-entrypoint-initdb.d/02-seed.sql
    ports:
      - "5432:5432"
```

### 13.3 Makefile (`server-go/Makefile`)

```makefile
.PHONY: run test build db seed

run:
	go run ./cmd/server

test:
	go test -v ./...

build:
	go build -o bin/server ./cmd/server

db:
	psql tailwind < ../db/db.sql --quiet

seed: db
	psql tailwind < ../db/seed.sql --quiet
```

---

## 14. Dependency Mapping

### NuGet → Go Modules

| NuGet Package | Go Module | Purpose |
|---|---|---|
| `Npgsql` 8.0.1 | `github.com/lib/pq` | PostgreSQL driver |
| `Dapper` 2.1.28 | `github.com/jmoiron/sqlx` | SQL query helper |
| `Dapper.SimpleCRUD` 2.3.0 | (not needed — use raw SQL) | CRUD operations |
| `Markdig` 0.33.0 | `github.com/yuin/goldmark` | Markdown → HTML |
| `YamlDotNet` 15.1.0 | `gopkg.in/yaml.v3` | YAML parsing |
| `Swashbuckle.AspNetCore` 6.5.0 | `github.com/swaggo/swag` | OpenAPI/Swagger |
| `Azure.AI.OpenAI` 1.0.0-beta.13 | `github.com/Azure/azure-sdk-for-go/sdk/ai/azopenai` or `net/http` | AI integration |
| `Viper.NET` 1.0.2 | `os.Getenv()` (stdlib) | Configuration |
| `xunit` 2.4.1 | `testing` (stdlib) + `github.com/stretchr/testify` | Testing |
| `Microsoft.AspNetCore.Mvc.Testing` | `net/http/httptest` (stdlib) | HTTP test helpers |

---

## 15. Risk Assessment & Mitigations

| Risk | Impact | Likelihood | Mitigation |
|---|---|---|---|
| API contract drift | High | Medium | Write API contract tests comparing C# and Go responses |
| `ExpandoObject`/`dynamic` type migration | Medium | High | Use typed structs in Go; avoid `interface{}` where possible |
| Transaction behavior differences | High | Low | Test transaction rollback scenarios explicitly |
| YAML frontmatter parsing differences | Medium | Medium | Port C# test cases to Go; test edge cases |
| Background worker timing differences | Low | Low | Use same 1-minute interval; test with time mocking |
| Swagger/OpenAPI generation | Low | Medium | Consider maintaining a hand-written `openapi.yaml` |
| Dapper column mapping (`snake_case`) | Medium | Low | Verify `db:` struct tags match all columns |
| Concurrent SMTP sending | Medium | Medium | Use goroutine pool instead of `Parallel.ForEach` |

---

## 16. Migration Checklist

### Phase 1: Scaffolding (~1 day)
- [ ] Initialize Go module and directory structure
- [ ] Implement configuration loading (`internal/config/`)
- [ ] Install core Go dependencies
- [ ] Verify `go build` succeeds

### Phase 2: Data Layer & Models (~2 days)
- [ ] Implement database connection pool (`internal/data/db.go`)
- [ ] Implement `ToSnakeCase` helper (`internal/data/extensions.go`)
- [ ] Migrate `Contact` model
- [ ] Migrate `Broadcast` model
- [ ] Migrate `Email` model
- [ ] Migrate `Message` model (with `ReadyToSend`, `MarkSent`)
- [ ] Migrate `Tag` and `Tagged` models
- [ ] Migrate `Activity` model
- [ ] Migrate `MarkdownEmail` parser (YAML + Markdown)
- [ ] Write unit tests for all models

### Phase 3: Business Logic (~2 days)
- [ ] Implement `CommandResult`
- [ ] Migrate `CreateBroadcast` command
- [ ] Migrate `ContactOptOut` command
- [ ] Migrate `ContactOptIn` command
- [ ] Migrate `ContactSignup` command
- [ ] Migrate `BulkTag` command
- [ ] Migrate `LinkClicked` command
- [ ] Write unit tests for commands

### Phase 4: HTTP API (~2 days)
- [ ] Set up Chi router and middleware
- [ ] Implement public routes (`/about`, `/unsubscribe`, `/link/clicked`, `/signup`)
- [ ] Implement admin broadcast routes (`/admin/validate`, `/admin/queue-broadcast`, `/admin/get-chat`)
- [ ] Implement admin contact routes (`/admin/contacts/search`)
- [ ] Implement admin bulk operations (`/admin/bulk/contacts/tag`)
- [ ] Write HTTP handler tests

### Phase 5: Services (~2 days)
- [ ] Implement `EmailSender` interface
- [ ] Implement `InMemoryEmailSender` (testing)
- [ ] Implement `SMTPEmailSender` (production)
- [ ] Implement background send worker (goroutine)
- [ ] Implement AI chat service
- [ ] Write service tests

### Phase 6: Deployment (~1 day)
- [ ] Create Dockerfile (multi-stage build)
- [ ] Update docker-compose.yml
- [ ] Create Makefile
- [ ] Verify end-to-end with Docker Compose

### Phase 7: Validation (~1 day)
- [ ] Run full test suite
- [ ] API contract comparison (C# vs Go responses)
- [ ] CLI compatibility verification
- [ ] Performance comparison (latency, memory)
- [ ] Security review

### Estimated Total: ~11 days

---

## Appendix A: File-by-File Migration Reference

| C# Source File | Go Target File | Status |
|---|---|---|
| `Program.cs` | `cmd/server/main.go` | ○ Not started |
| `Data/DB.cs` | `internal/data/db.go` | ○ Not started |
| `Data/Extensions.cs` | `internal/data/extensions.go` | ○ Not started |
| `Models/Contact.cs` | `internal/models/contact.go` | ○ Not started |
| `Models/Broadcast.cs` | `internal/models/broadcast.go` | ○ Not started |
| `Models/Email.cs` | `internal/models/email.go` | ○ Not started |
| `Models/Message.cs` | `internal/models/message.go` | ○ Not started |
| `Models/MarkdownEmail.cs` | `internal/models/markdown_email.go` | ○ Not started |
| `Models/Tag.cs` | `internal/models/tag.go` | ○ Not started |
| `Models/Activity.cs` | `internal/models/activity.go` | ○ Not started |
| `Commands/CommandResult.cs` | `internal/commands/command_result.go` | ○ Not started |
| `Commands/CreateBroadcast.cs` | `internal/commands/create_broadcast.go` | ○ Not started |
| `Commands/ContactOptOutCommand.cs` | `internal/commands/contact_optout.go` | ○ Not started |
| `Commands/ContactOptinCommand.cs` | `internal/commands/contact_optin.go` | ○ Not started |
| `Commands/ContactSignupCommand.cs` | `internal/commands/contact_signup.go` | ○ Not started |
| `Commands/BulkTagComand.cs` | `internal/commands/bulk_tag.go` | ○ Not started |
| `Commands/LinkClickedCommand.cs` | `internal/commands/link_clicked.go` | ○ Not started |
| `Services/BackgroundSend.cs` | `internal/services/background_send.go` | ○ Not started |
| `Services/Outbox.cs` | `internal/services/email_sender.go` | ○ Not started |
| `Services/AI.cs` | `internal/services/ai.go` | ○ Not started |
| `Api/PublicRoutes.cs` | `internal/api/public_routes.go` | ○ Not started |
| `Api/Admin/BroadcastRoutes.cs` | `internal/api/admin/broadcast_routes.go` | ○ Not started |
| `Api/Admin/ContactRoutes.cs` | `internal/api/admin/contact_routes.go` | ○ Not started |
| `Api/Admin/BulkOperationRoutes.cs` | `internal/api/admin/bulk_operation_routes.go` | ○ Not started |

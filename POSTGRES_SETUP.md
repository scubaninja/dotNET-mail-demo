# PostgreSQL Database Setup Guide

This guide will help you set up and configure the PostgreSQL database for the Tailwind Traders Mail Service.

## Prerequisites

- Docker and Docker Compose (recommended)
- OR PostgreSQL 12+ installed locally
- .NET 8.0 SDK

## Quick Start with Docker Compose

### 1. Copy Environment Variables

```bash
cp .env.example .env
```

Edit `.env` file and update the values as needed.

### 2. Start PostgreSQL Database

```bash
docker-compose up -d postgres
```

This will:
- Start a PostgreSQL 16 container
- Create the `tailwind` database
- Automatically run the schema (`db/db.sql`)
- Load seed data (`db/seed.sql`)

### 3. Verify Database Connection

```bash
docker-compose exec postgres psql -U tailwind -d tailwind -c "\dt mail.*"
```

You should see a list of tables in the `mail` schema.

### 4. Start the Mail Server

```bash
docker-compose up -d mail-server
```

The API will be available at http://localhost:5000

## Manual PostgreSQL Setup

### 1. Install PostgreSQL

#### Ubuntu/Debian
```bash
sudo apt update
sudo apt install postgresql postgresql-contrib
```

#### macOS (using Homebrew)
```bash
brew install postgresql@16
brew services start postgresql@16
```

#### Windows
Download and install from https://www.postgresql.org/download/windows/

### 2. Create Database and User

```bash
# Connect to PostgreSQL
sudo -u postgres psql

# Create user and database
CREATE USER tailwind WITH PASSWORD 'your_password_here';
CREATE DATABASE tailwind OWNER tailwind;

# Grant permissions
GRANT ALL PRIVILEGES ON DATABASE tailwind TO tailwind;

# Exit
\q
```

### 3. Run Database Schema

```bash
psql -U tailwind -d tailwind -f db/db.sql
```

### 4. Load Seed Data (Optional)

```bash
psql -U tailwind -d tailwind -f db/seed.sql
```

### 5. Configure Environment Variables

Create a `.env` file or set environment variables:

```bash
export DATABASE_URL="postgresql://tailwind:your_password_here@localhost:5432/tailwind"
export ASPNETCORE_ENVIRONMENT="Development"
export DEFAULT_FROM="test@tailwind.dev"
```

## Database Schema

The database uses a `mail` schema with the following tables:

### Core Tables

- **contacts** - Email list contacts with subscription status
- **tags** - Categorization tags for contacts
- **tagged** - Many-to-many relationship between contacts and tags
- **sequences** - Email sequence definitions
- **subscriptions** - Contact subscriptions to sequences
- **emails** - Email templates
- **broadcasts** - Broadcast send jobs
- **messages** - Message queue and send log
- **activity** - Contact activity tracking

### Schema Visualization

```
contacts (1) ----< (M) tagged (M) >---- (1) tags
    |                                      
    | (1)                                  
    |                                      
    v (M)                                  
subscriptions (M) >---- (1) sequences (1) ----< (M) emails
                                                       |
                                                       | (1)
                                                       v (M)
                                                   broadcasts (1) ----< (M) messages
```

## Connection String Format

The application expects a `DATABASE_URL` environment variable in the following format:

```
postgresql://[user]:[password]@[host]:[port]/[database]
```

Example:
```
postgresql://tailwind:mypassword@localhost:5432/tailwind
```

## Database Management Commands

### View Tables
```bash
psql -U tailwind -d tailwind -c "\dt mail.*"
```

### Reset Database
```bash
psql -U tailwind -d tailwind -c "DROP SCHEMA IF EXISTS mail CASCADE;"
psql -U tailwind -d tailwind -f db/db.sql
psql -U tailwind -d tailwind -f db/seed.sql
```

### Backup Database
```bash
pg_dump -U tailwind tailwind > backup_$(date +%Y%m%d_%H%M%S).sql
```

### Restore Database
```bash
psql -U tailwind -d tailwind < backup_file.sql
```

## Troubleshooting

### Connection Refused

**Problem:** Cannot connect to PostgreSQL
**Solution:** 
- Check if PostgreSQL is running: `sudo systemctl status postgresql`
- Verify port 5432 is not blocked
- Check `pg_hba.conf` for authentication settings

### Authentication Failed

**Problem:** Password authentication fails
**Solution:**
- Verify credentials in `.env` or environment variables
- Check `pg_hba.conf` authentication method (should be `md5` or `scram-sha-256`)
- Reset password: `ALTER USER tailwind WITH PASSWORD 'new_password';`

### Schema Not Found

**Problem:** Tables not found in `mail` schema
**Solution:**
- Run the schema file: `psql -U tailwind -d tailwind -f db/db.sql`
- Check search_path: `SHOW search_path;`

### Permission Denied

**Problem:** Cannot create tables or access database
**Solution:**
- Grant permissions: `GRANT ALL PRIVILEGES ON DATABASE tailwind TO tailwind;`
- Grant schema permissions: `GRANT ALL ON SCHEMA mail TO tailwind;`

## Development Tips

### Using Viper.NET Configuration

The application uses Viper.NET to load configuration from multiple sources:
1. Environment variables
2. `.env` files
3. `appsettings.json`

The `DATABASE_URL` environment variable takes precedence over all other sources.

### Database Migrations

Currently, the project uses raw SQL files for schema management. To modify the schema:

1. Edit `db/db.sql` with your changes
2. Test locally by resetting the database
3. Apply changes to production database manually

### Running Tests

Tests are excluded from Release builds but included in Debug builds.

```bash
cd server
dotnet test --configuration Debug
```

## Production Deployment

### Security Recommendations

1. **Use Strong Passwords**: Generate secure passwords for production
2. **SSL/TLS Connections**: Enable SSL for database connections
3. **Restrict Access**: Configure firewall rules to limit database access
4. **Regular Backups**: Set up automated backup schedules
5. **Update Regularly**: Keep PostgreSQL updated with security patches

### Connection Pooling

Npgsql provides connection pooling by default. Adjust pool settings in the connection string:

```
postgresql://user:pass@host:5432/db?Pooling=true;MinPoolSize=1;MaxPoolSize=20;
```

### Monitoring

Consider using:
- PostgreSQL built-in stats: `pg_stat_statements`
- pgAdmin for GUI management
- Grafana + Prometheus for metrics

## Additional Resources

- [PostgreSQL Documentation](https://www.postgresql.org/docs/)
- [Npgsql Documentation](https://www.npgsql.org/doc/)
- [Dapper Documentation](https://github.com/DapperLib/Dapper)
- [ASP.NET Core Configuration](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/)

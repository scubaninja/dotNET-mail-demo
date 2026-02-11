#!/bin/bash

# PostgreSQL Database Setup Script for Tailwind Traders Mail Service
# This script automates the database setup process

set -e  # Exit on error

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Default values
DB_USER="${POSTGRES_USER:-tailwind}"
DB_PASSWORD="${POSTGRES_PASSWORD:-tailwind_dev_password}"
DB_NAME="${POSTGRES_DB:-tailwind}"
DB_HOST="${POSTGRES_HOST:-localhost}"
DB_PORT="${POSTGRES_PORT:-5432}"

print_success() {
    echo -e "${GREEN}✓ $1${NC}"
}

print_error() {
    echo -e "${RED}✗ $1${NC}"
}

print_info() {
    echo -e "${YELLOW}ℹ $1${NC}"
}

# Function to check if PostgreSQL is installed
check_postgres() {
    if ! command -v psql &> /dev/null; then
        print_error "PostgreSQL client (psql) is not installed"
        print_info "Please install PostgreSQL or use Docker Compose instead"
        exit 1
    fi
    print_success "PostgreSQL client found"
}

# Function to check if database exists
check_database() {
    if PGPASSWORD=$DB_PASSWORD psql -h $DB_HOST -p $DB_PORT -U $DB_USER -lqt | cut -d \| -f 1 | grep -qw $DB_NAME; then
        return 0
    else
        return 1
    fi
}

# Function to create database
create_database() {
    print_info "Creating database '$DB_NAME'..."
    
    # Try to create using postgres user first
    if sudo -u postgres psql -c "CREATE DATABASE $DB_NAME OWNER $DB_USER;" 2>/dev/null; then
        print_success "Database created successfully"
    else
        # Fallback to creating with provided credentials
        PGPASSWORD=$DB_PASSWORD createdb -h $DB_HOST -p $DB_PORT -U $DB_USER $DB_NAME 2>/dev/null || {
            print_error "Failed to create database"
            print_info "The database might already exist or you may need sudo access"
        }
    fi
}

# Function to run schema
run_schema() {
    print_info "Running database schema..."
    
    if [ ! -f "db/db.sql" ]; then
        print_error "Schema file 'db/db.sql' not found"
        exit 1
    fi
    
    if PGPASSWORD=$DB_PASSWORD psql -h $DB_HOST -p $DB_PORT -U $DB_USER -d $DB_NAME -f db/db.sql -q; then
        print_success "Schema created successfully"
    else
        print_error "Failed to run schema"
        exit 1
    fi
}

# Function to run seed data
run_seed() {
    print_info "Loading seed data..."
    
    if [ ! -f "db/seed.sql" ]; then
        print_error "Seed file 'db/seed.sql' not found"
        exit 1
    fi
    
    if PGPASSWORD=$DB_PASSWORD psql -h $DB_HOST -p $DB_PORT -U $DB_USER -d $DB_NAME -f db/seed.sql -q; then
        print_success "Seed data loaded successfully"
    else
        print_error "Failed to load seed data"
        exit 1
    fi
}

# Function to verify setup
verify_setup() {
    print_info "Verifying database setup..."
    
    TABLE_COUNT=$(PGPASSWORD=$DB_PASSWORD psql -h $DB_HOST -p $DB_PORT -U $DB_USER -d $DB_NAME -t -c "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'mail';" 2>/dev/null || echo "0")
    
    if [ "$TABLE_COUNT" -gt 0 ]; then
        print_success "Found $TABLE_COUNT tables in 'mail' schema"
        
        # List tables
        print_info "Tables in database:"
        PGPASSWORD=$DB_PASSWORD psql -h $DB_HOST -p $DB_PORT -U $DB_USER -d $DB_NAME -c "\dt mail.*"
    else
        print_error "No tables found in 'mail' schema"
        exit 1
    fi
}

# Function to test connection
test_connection() {
    print_info "Testing database connection..."
    
    if PGPASSWORD=$DB_PASSWORD psql -h $DB_HOST -p $DB_PORT -U $DB_USER -d postgres -c "SELECT version();" > /dev/null 2>&1; then
        print_success "Successfully connected to PostgreSQL"
        return 0
    else
        print_error "Cannot connect to PostgreSQL"
        print_info "Connection details:"
        echo "  Host: $DB_HOST"
        echo "  Port: $DB_PORT"
        echo "  User: $DB_USER"
        echo "  Database: postgres"
        return 1
    fi
}

# Function to generate .env file
generate_env() {
    if [ -f ".env" ]; then
        print_info ".env file already exists, skipping generation"
        return
    fi
    
    if [ -f ".env.example" ]; then
        cp .env.example .env
        print_success "Created .env file from .env.example"
        print_info "Please edit .env file with your configuration"
    else
        print_error ".env.example not found"
    fi
}

# Main setup function
main() {
    echo "======================================================"
    echo "  Tailwind Traders Mail Service - Database Setup"
    echo "======================================================"
    echo ""
    
    # Load .env if exists
    if [ -f ".env" ]; then
        print_info "Loading environment variables from .env"
        set -a
        source .env
        set +a
        
        # Reload variables after loading .env
        DB_USER="${POSTGRES_USER:-tailwind}"
        DB_PASSWORD="${POSTGRES_PASSWORD:-tailwind_dev_password}"
        DB_NAME="${POSTGRES_DB:-tailwind}"
        DB_HOST="${POSTGRES_HOST:-localhost}"
        DB_PORT="${POSTGRES_PORT:-5432}"
    fi
    
    # Parse command line arguments
    case "${1:-setup}" in
        setup)
            check_postgres
            test_connection || exit 1
            
            if check_database; then
                print_info "Database '$DB_NAME' already exists"
                read -p "Do you want to reset the database? (y/N) " -n 1 -r
                echo
                if [[ $REPLY =~ ^[Yy]$ ]]; then
                    print_info "Dropping schema..."
                    PGPASSWORD=$DB_PASSWORD psql -h $DB_HOST -p $DB_PORT -U $DB_USER -d $DB_NAME -c "DROP SCHEMA IF EXISTS mail CASCADE;" -q
                    run_schema
                    run_seed
                fi
            else
                create_database
                run_schema
                run_seed
            fi
            
            verify_setup
            print_success "Database setup complete!"
            echo ""
            print_info "Connection string:"
            echo "  postgresql://$DB_USER:****@$DB_HOST:$DB_PORT/$DB_NAME"
            ;;
            
        reset)
            print_info "Resetting database..."
            PGPASSWORD=$DB_PASSWORD psql -h $DB_HOST -p $DB_PORT -U $DB_USER -d $DB_NAME -c "DROP SCHEMA IF EXISTS mail CASCADE;" -q
            run_schema
            run_seed
            verify_setup
            print_success "Database reset complete!"
            ;;
            
        verify)
            verify_setup
            ;;
            
        test)
            test_connection
            ;;
            
        env)
            generate_env
            ;;
            
        *)
            echo "Usage: $0 {setup|reset|verify|test|env}"
            echo ""
            echo "Commands:"
            echo "  setup   - Set up database (create, run schema, load seed data)"
            echo "  reset   - Reset database (drop schema and recreate)"
            echo "  verify  - Verify database setup"
            echo "  test    - Test database connection"
            echo "  env     - Generate .env file from .env.example"
            exit 1
            ;;
    esac
}

# Run main function
main "$@"

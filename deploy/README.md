# Mail Service Deployment

Deployment configuration for local development (Docker Compose) and cloud production (Azure via Bicep).

## Local Development

A `docker-compose.yml` at the repository root starts a local PostgreSQL instance:

```bash
docker compose up -d db
```

Then run each service directly:

```bash
# API server
cd server && dotnet watch

# Jobs worker
cd jobs && mage messages:send

# CLI
cd cli && node ./bin/mdmail.js
```

## Azure Deployment

All Azure infrastructure is defined as [Bicep](https://learn.microsoft.com/en-us/azure/azure-resource-manager/bicep/overview) templates. The [`mage deploy:*`](../jobs/README.md) targets in the jobs service are the primary way to apply them.

### Prerequisites

- [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli) (`az login`)
- [Mage](https://magefile.org/) installed in the `jobs/` directory context

### Typical Deployment Flow

```bash
cd jobs

# 1. Create resource groups
mage deploy:group 231000-storage
mage deploy:group 231000-compute

# 2. Deploy storage services (Service Bus, Key Vault, Blob, Container Registry)
mage deploy:storage 231000-storage

# 3. Assign RBAC roles to your user
mage deploy:rbac 231000-storage

# 4. Deploy the jobs Container App
export AZURE_SERVICEBUS_CONNECTION_STRING='...'
mage deploy:containerApps 231000-compute

# Tear down when done
mage deploy:empty 231000-compute
mage deploy:empty 231000-storage
```

### Supported Azure Services

| Service | Purpose |
|---------|---------|
| Azure Container Apps | Hosts the `jobs` worker container |
| Azure Service Bus | Message queue for broadcast email delivery |
| Azure Blob Storage | General-purpose object storage |
| Azure Key Vault | Secrets management |
| Azure Container Registry | Stores the `jobs` container image |
| Azure Database for PostgreSQL | Production database (optional; higher cost) |

### Container Images

The `jobs` worker is packaged as a container and published to GitHub Container Registry via GitHub Actions (see `jobs/.github/workflows/build-and-publish.yaml`). Two Dockerfiles are provided:

| File | Description |
|------|-------------|
| `jobs/Dockerfile` | Multi-stage, distroless production image |
| `jobs/dev.Dockerfile` | Development image with Go toolchain, Mage, and Vim |

Build locally:

```bash
cd jobs
mage docker:build       # production image
mage docker:buildDev    # development image
mage docker:run         # run locally with the mage target
```
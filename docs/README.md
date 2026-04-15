# Documentation

Component-level documentation for the Tailwind Traders Mail Service.

## Components

| Guide | Description |
|-------|-------------|
| [Server API](../server/README.md) | REST API endpoints, environment variables, and email format |
| [Jobs Worker](../jobs/README.md) | Go background worker, Mage targets, senders and queuers |
| [CLI](../cli/README.md) | Node.js CLI for authoring broadcasts and managing contacts |
| [Database](../db/README.md) | PostgreSQL schema reference |
| [Deployment](../deploy/README.md) | Docker Compose local dev and Azure Bicep cloud deployment |

## Additional Jobs Documentation

The `jobs/docs/` directory contains supplementary reference material for the Go worker:

- [Requirements](../jobs/docs/requirements.md)
- [Deploy](../jobs/docs/deploy.md)
- [Containers](../jobs/docs/containers.md)
- [Queuers](../jobs/docs/queuers.md)
- [Senders](../jobs/docs/senders.md)
- [Environment variables](../jobs/docs/envvars.md)
- [vNext](../jobs/docs/vnext.md)
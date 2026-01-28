# Welcome to Tailwind Traders Mail Service

We all need email... for better or worse. This service will send transactional emails via API or batch emails to a list, using a tag or predefined segment, like MailChimp does.

## Work In Progress

We're building things out actively... hopefully getting close to showing something soon!

## Deployment

This project includes automated containerized deployment to Azure and AWS that runs in parallel.

### Quick Links

- 🚀 [Quick Start Guide](docs/QUICKSTART.md) - Get deploying in minutes
- 📖 [Complete Deployment Guide](docs/DEPLOYMENT.md) - Detailed setup and configuration
- 🔒 [Security Summary](docs/SECURITY-SUMMARY.md) - Security measures and compliance

### Features

- **Parallel Deployment**: Simultaneously deploys to Azure Container Apps and AWS ECS
- **Containerized**: Fully containerized .NET server and Go jobs applications
- **Secure**: OIDC authentication, non-root containers, minimal permissions
- **Tested**: Comprehensive validation and security scanning
- **Documented**: Step-by-step guides and troubleshooting
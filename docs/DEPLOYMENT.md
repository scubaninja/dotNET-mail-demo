# Containerized Deployment Guide

This guide explains how to use the automated containerized deployment workflow that deploys to both Azure and AWS in parallel.

## Overview

The deployment workflow (`deploy-containers.yaml`) automates the following:

1. **Build Phase**: Builds and pushes Docker images to GitHub Container Registry (GHCR)
   - Server application (.NET 8.0)
   - Jobs application (Go)

2. **Parallel Deployment Phase**: Simultaneously deploys to both cloud providers
   - Azure Container Apps
   - AWS Elastic Container Service (ECS)

## Architecture

```
┌─────────────────────────────────────┐
│   Build & Push Container Images    │
│         (GitHub GHCR)               │
└──────────────┬──────────────────────┘
               │
       ┌───────┴────────┐
       │                │
       ▼                ▼
┌──────────────┐  ┌──────────────┐
│   Deploy to  │  │   Deploy to  │
│    Azure     │  │     AWS      │
│ (Parallel)   │  │  (Parallel)  │
└──────────────┘  └──────────────┘
```

## Prerequisites

### Azure Setup

1. **Azure Container Apps Environment**: Create a container apps environment
   ```bash
   az containerapp env create \
     --name mail-env \
     --resource-group your-resource-group \
     --location eastus
   ```

2. **Azure Service Principal**: Set up OIDC federation for GitHub Actions
   - Follow: https://learn.microsoft.com/azure/developer/github/connect-from-azure

### AWS Setup

1. **AWS ECS Cluster**: Create an ECS cluster
   ```bash
   aws ecs create-cluster --cluster-name mail-cluster
   ```

2. **AWS ECR Repository**: Create private ECR repositories for your images
   ```bash
   aws ecr create-repository --repository-name mail-server
   aws ecr create-repository --repository-name mail-jobs
   ```

3. **AWS ECS Task Definitions and Services**: Create task definitions and services
   
   **Note**: Before the workflow can deploy, you must create ECS task definitions that reference your ECR images with the `:latest` tag. The workflow uses `update-service` with `--force-new-deployment` which requires existing services and task definitions.
   
   Example task definition for mail-server:
   ```json
   {
     "family": "mail-server",
     "containerDefinitions": [{
       "name": "mail-server",
       "image": "<AWS_ACCOUNT_ID>.dkr.ecr.<REGION>.amazonaws.com/mail-server:latest",
       "portMappings": [{"containerPort": 8080}],
       "environment": [
         {"name": "DATABASE_URL", "value": "your-connection-string"}
       ]
     }],
     "cpu": "512",
     "memory": "1024"
   }
   ```
   
   Create the service:
   ```bash
   aws ecs create-service \
     --cluster mail-cluster \
     --service-name mail-server \
     --task-definition mail-server \
     --desired-count 1
   ```

4. **AWS IAM Role**: Set up OIDC federation for GitHub Actions
   - Follow: https://docs.github.com/actions/deployment/security-hardening-your-deployments/configuring-openid-connect-in-amazon-web-services

## Required GitHub Secrets

Configure the following secrets in your GitHub repository settings:

### Azure Secrets

| Secret Name | Description | Example |
|------------|-------------|---------|
| `AZURE_CLIENT_ID` | Azure AD App Client ID | `12345678-1234-1234-1234-123456789abc` |
| `AZURE_TENANT_ID` | Azure AD Tenant ID | `87654321-4321-4321-4321-cba987654321` |
| `AZURE_SUBSCRIPTION_ID` | Azure Subscription ID | `abcdef12-3456-7890-abcd-ef1234567890` |
| `AZURE_RESOURCE_GROUP` | Azure Resource Group name | `mail-services-rg` |
| `AZURE_CONTAINER_ENV` | Azure Container Apps Environment name | `mail-env` |
| `AZURE_DATABASE_URL` | PostgreSQL connection string | `Host=xxx.postgres.database.azure.com;Database=tailwind;Username=admin;Password=xxx` |
| `AZURE_SERVICEBUS_CONNECTION_STRING` | Azure Service Bus connection string | `Endpoint=sb://xxx.servicebus.windows.net/;...` |

### AWS Secrets

| Secret Name | Description | Example |
|------------|-------------|---------|
| `AWS_ROLE_ARN` | AWS IAM Role ARN for OIDC | `arn:aws:iam::123456789012:role/GitHubActionsRole` |
| `AWS_REGION` | AWS Region | `us-east-1` |
| `AWS_ECR_REGISTRY` | ECR Registry URL | `123456789012.dkr.ecr.us-east-1.amazonaws.com` |
| `AWS_ECS_CLUSTER` | ECS Cluster name | `mail-cluster` |

## GitHub Environment Setup

Create two environments in your repository settings:
- `azure-production` - for Azure deployments
- `aws-production` - for AWS deployments

This allows you to add environment-specific protection rules and secrets.

## Usage

### Automatic Deployment

The workflow triggers automatically on:
- Push to `main` or `latest` branches
- Changes to `server/**`, `jobs/**`, or workflow files

### Manual Deployment

Trigger manually via GitHub UI:
1. Go to Actions tab
2. Select "Build and Deploy Containers to Azure and AWS"
3. Click "Run workflow"
4. Select branch and click "Run workflow"

## Workflow Jobs

### 1. build-and-push-images
- Builds Docker images for server and jobs
- Pushes to GitHub Container Registry
- Generates image tags with metadata
- **Outputs**: Image URLs for downstream jobs

### 2. deploy-to-azure (runs in parallel)
- Logs into Azure using OIDC
- Deploys/updates Azure Container Apps
- Configures environment variables
- Sets resource limits (0.5 CPU, 1GB memory)

### 3. deploy-to-aws (runs in parallel)
- Configures AWS credentials using OIDC
- Pulls images from GHCR and pushes to ECR
- Updates ECS services with new images
- Forces new deployment

### 4. deployment-summary
- Runs after both parallel deployments complete (even if one fails)
- Provides consolidated status for both deployments
- Exits with error if any deployment failed

## Testing

A comprehensive test workflow is included (`test-deployment-workflow.yaml`) that validates:

1. **Workflow Syntax**: Validates YAML structure and required jobs
2. **Parallel Structure**: Ensures Azure/AWS deploy in parallel
3. **Dockerfile Validation**: Checks both Dockerfiles build successfully
4. **Security Scanning**: Runs Trivy security scans
5. **Required Secrets**: Lists all required secrets

Run tests by pushing changes to workflow files or Dockerfiles.

## Monitoring Deployments

### Azure
```bash
# Check container app status
az containerapp show --name mail-server --resource-group your-resource-group

# View logs
az containerapp logs show --name mail-server --resource-group your-resource-group
```

### AWS
```bash
# Check ECS service status
aws ecs describe-services --cluster mail-cluster --services mail-server

# View CloudWatch logs
aws logs tail /ecs/mail-server --follow
```

## Troubleshooting

### Common Issues

1. **Authentication Failures**
   - Verify OIDC federation is configured correctly
   - Check that GitHub Actions has the correct permissions
   - Ensure secrets are set in the correct environment

2. **Image Pull Failures**
   - Verify GitHub token has packages:read permission
   - Check that images were successfully pushed to GHCR

3. **Deployment Timeouts**
   - Check cloud provider resource quotas
   - Review service logs for application errors
   - Verify network connectivity and security groups

4. **Parallel Deployment Issues**
   - Both deployments are independent; one can succeed while the other fails
   - The deployment-summary job will indicate which deployments succeeded or failed
   - Check individual job logs for specific errors
   - The workflow will exit with an error if any deployment fails

### Debugging

Enable debug logging by setting repository secrets:
- `ACTIONS_STEP_DEBUG` = `true`
- `ACTIONS_RUNNER_DEBUG` = `true`

## Rollback

### Azure
```bash
# Rollback to previous revision
az containerapp revision list --name mail-server --resource-group your-resource-group
az containerapp revision activate --name mail-server --revision <revision-name> --resource-group your-resource-group
```

### AWS
```bash
# Stop current deployment and rollback
aws ecs update-service --cluster mail-cluster --service mail-server --task-definition previous-task-def
```

## Security Considerations

1. **OIDC Authentication**: Uses short-lived tokens instead of long-lived credentials
2. **Minimal Permissions**: Follows principle of least privilege
3. **Secret Management**: Secrets are stored in GitHub and injected at runtime
4. **Image Scanning**: Trivy scans images for vulnerabilities
5. **Multi-stage Builds**: Docker images use multi-stage builds to reduce attack surface

## Cost Optimization

- Uses minimal resource allocations (0.5 CPU, 1GB memory)
- Can be adjusted in the workflow file under container app creation
- Consider using spot instances in AWS for cost savings

## Support

For issues or questions:
1. Check the workflow run logs in GitHub Actions
2. Review cloud provider service logs
3. Verify all prerequisites are met
4. Check that all secrets are correctly configured

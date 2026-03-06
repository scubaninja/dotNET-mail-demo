# Mail Service Deployment

The Azure/Docker/K8s stuff goes in here.

## Overview

This directory contains all infrastructure and deployment configuration for the Tailwind Mail service, following a GitOps model with automated promotion through environments.

```
deploy/
├── iac/                        # Infrastructure as Code (Bicep)
│   ├── main.bicep              # Orchestrator: VNet + ACR + AKS
│   ├── modules/
│   │   ├── network.bicep       # VNet and subnets
│   │   ├── acr.bicep           # Azure Container Registry + AcrPull role
│   │   └── aks.bicep           # AKS cluster with workload identity
│   └── parameters/
│       ├── staging.bicepparam  # Staging environment parameters
│       └── production.bicepparam
└── k8s/                        # Kubernetes manifests (Kustomize)
    ├── base/                   # Shared base manifests
    └── overlays/
        ├── staging/            # Staging overrides (1 replica, smaller limits)
        └── production/         # Production overrides (2 replicas, larger limits)
```

## Prerequisites

- Azure CLI with Bicep extension
- `kubectl`
- `kustomize`
- GitHub repository secrets configured (see below)

## Required GitHub Secrets

| Secret | Description |
|--------|-------------|
| `AZURE_CLIENT_ID` | Federated credential client ID |
| `AZURE_TENANT_ID` | Azure AD tenant ID |
| `AZURE_SUBSCRIPTION_ID` | Azure subscription ID |
| `AZURE_RESOURCE_GROUP` | Resource group for all resources |
| `AZURE_ACR_NAME` | Azure Container Registry name |
| `AZURE_AKS_CLUSTER_NAME_STAGING` | Staging AKS cluster name |
| `AZURE_AKS_CLUSTER_NAME_PRODUCTION` | Production AKS cluster name |
| `DATABASE_URL` | PostgreSQL connection string |

## GitHub Actions Workflows

| Workflow | Trigger | Description |
|----------|---------|-------------|
| `provision-infrastructure.yaml` | Manual | Provisions AKS + ACR + VNet via Bicep |
| `build-image-mail-server.yaml` | Push to `main`/`latest` (server/**) | Builds .NET image, pushes to ACR, triggers staging deploy |
| `deploy-aks-staging.yaml` | Called by build workflow or manual | Deploys to staging AKS |
| `deploy-aks-production.yaml` | Manual with approval | Deploys to production AKS (requires Environment reviewer approval) |

## GitOps Promotion Flow

```
Code push → build-image-mail-server
               ↓
         Builds container image
               ↓
         Pushes to ACR (sha-<hash> tag)
               ↓
         Auto-deploys to staging
               ↓
         (manual trigger + Environment approval)
               ↓
         deploy-aks-production (with sha-<hash> tag)
```

## Provisioning Infrastructure

```bash
# Validate the template first
az deployment group validate \
  --resource-group <your-rg> \
  --template-file deploy/iac/main.bicep \
  --parameters deploy/iac/parameters/staging.bicepparam

# Deploy staging infrastructure
az deployment group create \
  --resource-group <your-rg> \
  --template-file deploy/iac/main.bicep \
  --parameters deploy/iac/parameters/staging.bicepparam
```

Or use the **Provision Infrastructure** GitHub Actions workflow.

## Deploying Manually

```bash
# Get cluster credentials
az aks get-credentials --resource-group <rg> --name <cluster-name>

# Deploy to staging
kustomize build deploy/k8s/overlays/staging | kubectl apply -f -

# Deploy to production
kustomize build deploy/k8s/overlays/production | kubectl apply -f -
```

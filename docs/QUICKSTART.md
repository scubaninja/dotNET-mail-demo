# Quick Start Guide: Containerized Deployment

This quick reference guide helps you get started with the automated deployment workflow.

## ⚡ Quick Deploy

### Trigger Automatic Deployment
Push changes to `main` or `latest` branch:
```bash
git add .
git commit -m "Your changes"
git push origin main
```

### Trigger Manual Deployment
1. Go to GitHub Actions tab
2. Select "Build and Deploy Containers to Azure and AWS"
3. Click "Run workflow"
4. Select branch and click "Run workflow"

## 🔑 Required Setup (One-Time)

### 1. Configure GitHub Environments
```bash
# In GitHub UI: Settings → Environments → New environment
- Create: azure-production
- Create: aws-production
```

### 2. Add GitHub Secrets
Navigate to: Settings → Secrets and variables → Actions → New repository secret

**Azure (7 secrets):**
```
AZURE_CLIENT_ID
AZURE_TENANT_ID
AZURE_SUBSCRIPTION_ID
AZURE_RESOURCE_GROUP
AZURE_CONTAINER_ENV
AZURE_DATABASE_URL
AZURE_SERVICEBUS_CONNECTION_STRING
```

**AWS (4 secrets):**
```
AWS_ROLE_ARN
AWS_REGION
AWS_ECR_REGISTRY
AWS_ECS_CLUSTER
```

### 3. Set Up Cloud Resources

**Azure:**
```bash
# Create container environment
az containerapp env create \
  --name mail-env \
  --resource-group your-rg \
  --location eastus
```

**AWS:**
```bash
# Create ECS cluster
aws ecs create-cluster --cluster-name mail-cluster

# Create ECR repositories
aws ecr create-repository --repository-name mail-server
aws ecr create-repository --repository-name mail-jobs

# Create task definitions and services (see full docs)
```

## 📊 Monitor Deployments

### Check Status
```bash
# Azure
az containerapp show --name mail-server --resource-group your-rg

# AWS
aws ecs describe-services --cluster mail-cluster --services mail-server
```

### View Logs
```bash
# Azure
az containerapp logs show --name mail-server --resource-group your-rg

# AWS
aws logs tail /ecs/mail-server --follow
```

## 🔄 What Gets Deployed

The workflow deploys two applications to both clouds:

1. **Server** (.NET 8.0 API)
   - Port: 8080
   - Image: `ghcr.io/scubaninja/dotnet-mail-demo-server`
   
2. **Jobs** (Go batch processor)
   - Image: `ghcr.io/scubaninja/dotnet-mail-demo-jobs`

## 🚀 Deployment Flow

```
1. Push code → GitHub
         ↓
2. Build containers → Push to GHCR
         ↓
3. Deploy in parallel:
   ├─→ Azure Container Apps
   └─→ AWS ECS
         ↓
4. Summary (both must succeed)
```

## ⏱️ Estimated Times

- Build phase: ~5-10 minutes
- Azure deployment: ~2-3 minutes
- AWS deployment: ~2-3 minutes
- **Total (parallel)**: ~7-13 minutes

## 🐛 Common Issues

### "Environment not found"
```bash
# Create the missing environment in GitHub UI
Settings → Environments → New environment
```

### "Image pull failed"
```bash
# Ensure GITHUB_TOKEN has packages:read permission
# Check that images built successfully
```

### "Service not found" (AWS)
```bash
# Create ECS service first (see full documentation)
aws ecs create-service --cluster mail-cluster \
  --service-name mail-server --task-definition mail-server
```

### "Container app not found" (Azure)
```bash
# The workflow will create it automatically on first run
# Ensure AZURE_CONTAINER_ENV exists
```

## 📚 Full Documentation

For detailed information, see:
- [Complete Deployment Guide](DEPLOYMENT.md)
- [Security Summary](SECURITY-SUMMARY.md)

## 🆘 Need Help?

1. Check the [Troubleshooting section](DEPLOYMENT.md#troubleshooting) in full docs
2. Review workflow logs in GitHub Actions
3. Check cloud provider service logs
4. Open an issue in the repository

## ✅ Pre-flight Checklist

Before first deployment:
- [ ] GitHub environments created (azure-production, aws-production)
- [ ] All secrets configured (11 total)
- [ ] Azure Container Environment created
- [ ] AWS ECS cluster created
- [ ] AWS ECR repositories created
- [ ] AWS ECS services and task definitions created
- [ ] OIDC federation configured for both clouds
- [ ] Reviewed security summary

## 🔐 Security Notes

- Uses OIDC (no long-lived credentials)
- Containers run as non-root user
- Minimal GitHub token permissions
- All actions pinned to versions
- Secrets never logged or exposed

---

**Ready to deploy?** Push to `main` or trigger manually! 🚀

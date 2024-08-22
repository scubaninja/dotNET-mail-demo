# Deploy

## Bicep

We use [Bicep](https://learn.microsoft.com/en-us/azure/azure-resource-manager/bicep/overview?tabs=bicep) to deploy resources to Azure. All deployment scripts are located in the [deploy/](../deploy) directory.

These are deployable as-is, or `deploy:*` [magefile](../magefile.go) targets. For example:

```bash
# deploy compute into a stand-alone resource group
mage deploy:group 231000-compute
export AZURE_SERVICEBUS_CONNECTION_STRING='...'
mage deploy:containerapps 231000-compute

# deploy storage into a stand-alone resource group
mage deploy:group 231000-storage
mage deploy:storage 231000-storage
# deploy the role based access control to assign roles to the currently signed in user
mage deploy:rbac 231000-storage

# empty both resource groups
mage deploy:empty 231000-compute
mage deploy:empty 231000-storage
```

## Compute

We support, or will support, deployment of jobs to the following cloud native compute platforms:

- Azure Virtual Machines
- Azure Container Instances ([view](../deploy/azure-container-apps.bicep))
- Azure Kubernetes Service

You may deploy one or more of these. Our goal is to support all deployment targets equally and interchangeably.

## Storage

We support the deployment of the following storage platforms:

- Azure Blob Storage
- Azure Service Bus
- Azure Key Vault
- Azure Container Registry
- Azure Database for Postgres (Optional)

All of these are deployable via the [deploy/main.bicep](../deploy/main.bicep).

Role Based Access Control (RBAC) for the currently logged in user is deployed via [deploy/rbac.bicep](../deploy/rbac.bicep).

We consider Blob Storage, Service Bus, Key Vault and Container Registry to be "core" storage platforms and deploy them together by default, as they are key to many workflows, and cost effective.

Azure Database for Postgres is highly recommended, though we make its deployment optional as it comes at a higher cost.

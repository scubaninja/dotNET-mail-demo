# Documentation

Documentation for this project is stored in docs directory. Some documentation is linked from this README, but others are unlinked.

## Next steps

- [Requirements](requirements.md)
- [Deploy](deploy.md)
# Containers

This application can be run locally or on a VM with Go and Mage, or built as a container.

Our `jobs` container is built using the Dockerfiles, [Dockerfile](../Dockerfile) and [dev.Dockerfile](../dev.Dockerfile).

The [build-and-publish.yaml](../.github/workflows/build-and-publish.yaml) GitHub Action builds and publishes the `jobs` container, from the `latest` branch, to GitHub Container Registry.
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
# Environment variables

The following are example environment variables for local development, and deployment to Azure.

```bash
# service bus
export AZURE_SERVICEBUS_CONNECTION_STRING='...'
#export AZURE_SERVICEBUS_QUEUE_NAME='tailwind'
# for local testing -- we don't want prod to drain queues
export AZURE_SERVICEBUS_QUEUE_NAME='tailwind-test'
# for default
export AZURE_SERVICEBUS_HOSTNAME='sb231000.servicebus.windows.net'
# azure communication services
export ACS_ENDPOINT='https://acs231000.unitedstates.communication.azure.com/'
export ACS_KEY='...'
export ACS_FROM="DoNotReply@d018bc51-f36f-4493-82fe-0ff5c3377a9e.azurecomm.net"
# smtp via twilio sendgrid
export SMTP_FROM='user@example.com'
export SMTP_SERVER='smtp.sendgrid.net'
export SMTP_PORT='587'
export SMTP_USERNAME='apikey'
export SMTP_PASSWORD='...'
# ethereal.email
export ETHEREAL_NAME='Hello World'
export ETHEREAL_USERNAME='hello.world@ethereal.email'
export ETHEREAL_PASSWORD='...'
export ETHEREAL_SMTP_SERVER='smtp.ethereal.email'
#export SMTP_FROM=$ETHEREAL_USERNAME
#export SMTP_SERVER=$ETHEREAL_SMTP_SERVER
#export SMTP_PORT='587'
#export SMTP_USERNAME=$ETHEREAL_USERNAME
#export SMTP_PASSWORD=$ETHEREAL_PASSWORD
```# Queuers

The `message` job (and `message:*` targets) combine senders and queuers to send emails. We support the following senders:

## Test

This queuer is used for testing. It does not send any emails, but instead logs the email to the console using the `log/slog` package.

We recommend using this queuer if you are developing a new queuer.

## Azure Service Bus

Azure Service Bus is used for produciton queues.

In order to use this queuer, you must set `AZURE_SERVICEBUS_CONNECTION_STRING` and `AZURE_SERVICEBUS_QUEUE_NAME` environment variables.
# Requirements

- An **Azure Subscription** (e.g. [Free](https://aka.ms/azure-free-account) or [Student](https://aka.ms/azure-student-account) account)
- The [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli)
- Bash shell (e.g. macOS, Linux, [Windows Subsystem for Linux (WSL)](https://docs.microsoft.com/en-us/windows/wsl/about), [Multipass](https://multipass.run/), [Azure Cloud Shell](https://docs.microsoft.com/en-us/azure/cloud-shell/quickstart), [GitHub Codespaces](https://github.com/features/codespaces), etc)
- [Go](https://go.dev/dl/) (Optional)
- [Mage](https://magefile.org/) (`go install github.com/magefile/mage@latest`) (Optional)
# Senders

The `message` job (and `message:*` targets) combine senders and queuers to send emails. We support the following senders:

## Test

This queuer is used for testing. It does not send any emails, but instead logs the email to the console using the `log/slog` package.

We recommend using this sender if you are developing a new sender.

## Azure Communication Services

Azure Communication Service is the recommended sender for sending emails in production. We have written a wrapper for the REST API at [azurecontainerservices.go](../senders/azurecontainerservices.go).

## SMTP

This queuer sends emails via SMTP.

We recommend either a production SMTP service, such as Twiliio Sendgrid, or a development SMTP service such as ethereal.email.
# vNext

The following is a list of features that are currently in progress, or planned for the future:

- Managed Identity support to remove the requirement for an `AZURE_SERVICEBUS_CONNECTION_STRING` environment for the Service Bus [queuer](queuers.md).
- Support for [Azure Database for Postgres](https://azure.microsoft.com/en-us/services/postgresql/) as a storage backend (queuer), which will provide a locally deployable alternative to Azure Service Bus.
- Deployment to Azure Kubernetes Service (AKS) with support for auto-scaling using KEDA (Kubernetes Event-driven Autoscaling).
- Deployment to Azure Virtual Machines (VMs) with support for Flatcar Linux.

If you are currently hacking on any of these features, please let us know by opening an issue or pull request.

# vNext

The following is a list of features that are currently in progress, or planned for the future:

- Managed Identity support to remove the requirement for an `AZURE_SERVICEBUS_CONNECTION_STRING` environment for the Service Bus [queuer](queuers.md).
- Support for [Azure Database for Postgres](https://azure.microsoft.com/en-us/services/postgresql/) as a storage backend (queuer), which will provide a locally deployable alternative to Azure Service Bus.
- Deployment to Azure Kubernetes Service (AKS) with support for auto-scaling using KEDA (Kubernetes Event-driven Autoscaling).
- Deployment to Azure Virtual Machines (VMs) with support for Flatcar Linux.

If you are currently hacking on any of these features, please let us know by opening an issue or pull request.

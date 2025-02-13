# Azure Container Apps Deployment with Terraform

This Terraform configuration deploys Azure Container Apps and related resources. It includes the following components:

- **User Assigned Managed Identity**: Used for accessing Azure resources securely.
- **Azure Container Registry (ACR)**: Stores container images.
- **Log Analytics Workspace**: Collects and analyzes logs.
- **Container App Environment**: Hosts the container apps.
- **Container Apps**: Deploys both a web app and a background job app.
- **Container App Job**: Deploys a job that processes messages from an Azure Service Bus queue.

## Variables

- `env_name`: Name of the environment.
- `app_name`: Name of the container app.
- `app_image`: Image for the container app.
- `job_name`: Name of the container job.
- `job_image`: Image for the container job.
- `service_bus_connection`: Connection string for the Azure Service Bus.
- `app_queue_name`: Queue name for the container app.
- `job_queue_name`: Queue name for the container job.
- `location`: Azure region for the resources.

## Usage

1. Initialize Terraform:
    ```sh
    terraform init
    ```

2. Plan the deployment:
    ```sh
    terraform plan
    ```

3. Apply the deployment:
    ```sh
    terraform apply
    ```

## Resources Created

- **Managed Identity**: A user-assigned managed identity.
- **Container Registry**: An Azure Container Registry with admin access enabled.
- **Role Assignment**: Assigns the `AcrPull` role to the managed identity.
- **Log Analytics Workspace**: A workspace for log analytics.
- **Container App Environment**: An environment for hosting container apps.
- **Container App (Web)**: A container app for the web application.
- **Container App (Background Jobs)**: A container app for background jobs.
- **Container App Job**: A job that processes messages from an Azure Service Bus queue.

## Notes

- Ensure that the `service_bus_connection` variable is set with a valid Azure Service Bus connection string.
- The container images should be available in the specified container registry.
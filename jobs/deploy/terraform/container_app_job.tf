resource "azurerm_container_app_job" "job" {
  name                = var.job_name
  location            = var.location
  resource_group_name = azurerm_resource_group.rg.name
  container_app_environment_id = azurerm_container_app_environment.environment.id

  identity {
    type = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.managed_identity.id]
  }

  configuration {
    secrets {
      name  = "connection-string-secret"
      value = var.service_bus_connection
    }
    registries {
      server   = "${azurerm_container_registry.acr.login_server}"
      identity = azurerm_user_assigned_identity.managed_identity.id
    }
    trigger_type = "Event"
    replica_timeout = 300
    replica_retry_limit = 3
    event_trigger_config {
      replica_completion_count = 1
      parallelism = 1
      scale {
        min_executions = 0
        max_executions = 5
        polling_interval = 60
        rule {
          name  = "azure-servicebus-queue-rule"
          type  = "azure-servicebus"
          metadata = {
            queueName    = var.job_queue_name
            messageCount = "5"
          }
          auth {
            trigger_parameter = "connection"
            secret_ref        = "connection-string-secret"
          }
        }
      }
    }
  }

  template {
    container {
      name   = "${var.job_name}-jobs"
      image  = var.job_image
      cpu    = "0.5"
      memory = "1.0Gi"
      command = ["mage", "servicebus:receiveall"]
      env {
        name      = "AZURE_SERVICEBUS_CONNECTION_STRING"
        secret_ref = "connection-string-secret"
      }
      env {
        name  = "AZURE_SERVICEBUS_QUEUE_NAME"
        value = var.job_queue_name
      }
    }
  }
}

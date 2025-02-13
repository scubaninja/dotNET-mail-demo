variable "env_name" {
  default = "my-environment"
}

variable "app_name" {
  default = "my-container-app"
}

variable "app_image" {
  default = "mcr.microsoft.com/azuredocs/containerapps-helloworld:latest"
}

variable "job_name" {
  default = "my-container-job"
}

variable "job_image" {
  default = "ghcr.io/tailwind-traders-dev/jobs:latest"
}

variable "service_bus_connection" {
  default = ""
}

variable "app_queue_name" {
  default = "tailwind"
}

variable "job_queue_name" {
  default = "tailwind-job"
}

variable "location" {
  default = "eastus"
}

resource "azurerm_user_assigned_identity" "managed_identity" {
  name                = "${azurerm_resource_group.rg.name}-identity"
  location            = var.location
  resource_group_name = azurerm_resource_group.rg.name
}

resource "azurerm_container_registry" "acr" {
  name                = "acr${random_string.rand.result}"
  location            = var.location
  resource_group_name = azurerm_resource_group.rg.name
  sku                 = "Standard"
  admin_enabled       = true
}

resource "azurerm_role_assignment" "acr_pull" {
  scope                = azurerm_container_registry.acr.id
  role_definition_name = "AcrPull"
  principal_id         = azurerm_user_assigned_identity.managed_identity.principal_id
}

resource "azurerm_log_analytics_workspace" "log_analytics" {
  name                = "${var.env_name}-logs"
  location            = var.location
  resource_group_name = azurerm_resource_group.rg.name
  sku                 = "PerGB2018"
  retention_in_days   = 30
}

resource "azurerm_container_app_environment" "environment" {
  name                = var.env_name
  location            = var.location
  resource_group_name = azurerm_resource_group.rg.name
  log_analytics {
    workspace_id = azurerm_log_analytics_workspace.log_analytics.id
  }
}

resource "azurerm_container_app" "app" {
  name                = "${var.app_name}-web"
  location            = var.location
  resource_group_name = azurerm_resource_group.rg.name
  container_app_environment_id = azurerm_container_app_environment.environment.id

  identity {
    type = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.managed_identity.id]
  }

  configuration {
    ingress {
      external_enabled = true
      target_port      = 80
    }
    registries {
      server   = "${azurerm_container_registry.acr.login_server}"
      identity = azurerm_user_assigned_identity.managed_identity.id
    }
  }

  template {
    container {
      name   = "${var.app_name}-web"
      image  = var.app_image
      cpu    = "0.5"
      memory = "1.0Gi"
    }
    scale {
      min_replicas = 0
      max_replicas = 1
    }
  }
}

resource "azurerm_container_app" "background_app" {
  name                = "${var.app_name}-jobs"
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
  }

  template {
    container {
      name   = "${var.app_name}-jobs"
      image  = var.job_image
      cpu    = "0.5"
      memory = "1.0Gi"
      command = ["mage", "servicebus:receive"]
      env {
        name      = "AZURE_SERVICEBUS_CONNECTION_STRING"
        secret_ref = "connection-string-secret"
      }
      env {
        name  = "AZURE_SERVICEBUS_QUEUE_NAME"
        value = var.app_queue_name
      }
    }
    scale {
      min_replicas = 0
      max_replicas = 2
      rule {
        name  = "azure-servicebus-queue-rule"
        type  = "azure-servicebus"
        metadata = {
          queueName    = var.app_queue_name
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

resource "random_string" "rand" {
  length  = 6
  special = false
}

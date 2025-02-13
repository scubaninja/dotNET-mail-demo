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

resource "azurerm_container_app_environment" "environment" {
  name                = var.env_name
  location            = var.location
  resource_group_name = azurerm_resource_group.rg.name
  log_analytics {
    workspace_id = azurerm_log_analytics_workspace.log_analytics.id
  }
}

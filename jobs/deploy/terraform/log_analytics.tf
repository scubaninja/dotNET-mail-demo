resource "azurerm_log_analytics_workspace" "log_analytics" {
  name                = "${var.env_name}-logs"
  location            = var.location
  resource_group_name = azurerm_resource_group.rg.name
  sku                 = "PerGB2018"
  retention_in_days   = 30
}

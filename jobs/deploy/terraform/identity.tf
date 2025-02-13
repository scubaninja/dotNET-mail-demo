resource "azurerm_user_assigned_identity" "managed_identity" {
  name                = "${azurerm_resource_group.rg.name}-identity"
  location            = var.location
  resource_group_name = azurerm_resource_group.rg.name
}

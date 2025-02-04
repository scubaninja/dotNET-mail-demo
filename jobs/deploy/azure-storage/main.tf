terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.0"
    }
    random = {
      source  = "hashicorp/random"
      version = "~> 3.0"
    }
  }
}

provider "azurerm" {
  features {}
}

variable "location" {
  type    = string
  default = null # Will use resource group location
}

variable "secret_name" {
  type    = string
  default = "hello"
}

variable "secret_value" {
  type      = string
  sensitive = true
  default   = null # Will be generated
}

variable "deploy_postgres" {
  type    = bool
  default = true
}

data "azurerm_resource_group" "main" {
  name = "your-resource-group-name" # Replace with actual RG name
}

resource "random_string" "unique" {
  length  = 6
  special = false
  upper   = false
}

resource "azurerm_user_assigned_identity" "main" {
  name                = "${data.azurerm_resource_group.main.name}-identity"
  resource_group_name = data.azurerm_resource_group.main.name
  location           = coalesce(var.location, data.azurerm_resource_group.main.location)
}

resource "azurerm_container_registry" "main" {
  name                = "acr${random_string.unique.result}"
  resource_group_name = data.azurerm_resource_group.main.name
  location           = coalesce(var.location, data.azurerm_resource_group.main.location)
  sku                = "Basic"
  admin_enabled      = true
}

resource "azurerm_servicebus_namespace" "main" {
  name                = "sb-${random_string.unique.result}"
  resource_group_name = data.azurerm_resource_group.main.name
  location           = coalesce(var.location, data.azurerm_resource_group.main.location)
  sku                = "Standard"
}
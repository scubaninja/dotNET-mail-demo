# Terraform configuration block
# Specifies required providers and their versions for this configuration
terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"  # Official Azure provider from HashiCorp
      version = "~> 3.0"             # Use version 3.x.x of the provider
    }
  }
}

# Configure the Microsoft Azure Provider
# The features block is required for AzureRM provider 2.0+
provider "azurerm" {
  features {}  # Empty features block - using default provider features
}

# Input Variables
# Location variable defines the Azure region where resources will be created
variable "location" {
  type        = string
  description = "Azure region location"
  default     = null # Will use resource group location
}

# Locals for computed values
locals {
  location = var.location != null ? var.location : data.azurerm_resource_group.current.location
  rand     = substr(random_string.unique.result, 0, 6)
}

# Get current resource group
data "azurerm_resource_group" "current" {
  name = var.resource_group_name # You'll need to define this variable
}

# Random string for unique names
resource "random_string" "unique" {
  length  = 16
  special = false
}

# User Assigned Managed Identity
resource "azurerm_user_assigned_identity" "main" {
  name                = "${data.azurerm_resource_group.current.name}-identity"
  resource_group_name = data.azurerm_resource_group.current.name
  location           = local.location
}

# Container Registry
resource "azurerm_container_registry" "main" {
  name                = "acr${local.rand}"
  resource_group_name = data.azurerm_resource_group.current.name
  location           = local.location
  sku                = "Basic" # Update according to your needs
}
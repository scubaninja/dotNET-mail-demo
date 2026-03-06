// ACR module: provisions Azure Container Registry and grants AcrPull to the AKS kubelet identity

@description('Azure region for all resources')
param location string

@description('Name for the Azure Container Registry (must be globally unique, 5-50 alphanumeric characters)')
param acrName string

@description('SKU for the container registry')
@allowed(['Basic', 'Standard', 'Premium'])
param sku string = 'Standard'

@description('Principal ID of the AKS kubelet managed identity that needs AcrPull access')
param aksKubeletPrincipalId string

@description('Resource tags')
param tags object = {}

// Azure Container Registry
resource acr 'Microsoft.ContainerRegistry/registries@2023-07-01' = {
  name: acrName
  location: location
  tags: tags
  sku: {
    name: sku
  }
  properties: {
    adminUserEnabled: false
    publicNetworkAccess: 'Enabled'
    zoneRedundancy: 'Disabled'
  }
}

// AcrPull role definition (built-in)
var acrPullRoleId = '7f951dda-4ed3-4680-a7ca-43fe172d538d'

// Grant AKS kubelet identity AcrPull on the registry
resource acrPullAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(acr.id, aksKubeletPrincipalId, acrPullRoleId)
  scope: acr
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', acrPullRoleId)
    principalId: aksKubeletPrincipalId
    principalType: 'ServicePrincipal'
  }
}

@description('Login server hostname for the container registry')
output loginServer string = acr.properties.loginServer

@description('Resource ID of the container registry')
output acrId string = acr.id

@description('Name of the container registry')
output acrName string = acr.name

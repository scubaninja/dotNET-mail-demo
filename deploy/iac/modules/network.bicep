// Network module: provisions a VNet with dedicated subnets for AKS and supporting services

@description('Azure region for all resources')
param location string

@description('Name prefix used across all network resources')
param namePrefix string

@description('Address space for the virtual network (CIDR notation)')
param vnetAddressPrefix string = '10.0.0.0/16'

@description('Address prefix for the AKS node subnet')
param aksSubnetPrefix string = '10.0.0.0/22'

@description('Resource tags')
param tags object = {}

// Virtual network
resource vnet 'Microsoft.Network/virtualNetworks@2023-09-01' = {
  name: '${namePrefix}-vnet'
  location: location
  tags: tags
  properties: {
    addressSpace: {
      addressPrefixes: [vnetAddressPrefix]
    }
    subnets: [
      {
        name: 'aks-nodes'
        properties: {
          addressPrefix: aksSubnetPrefix
          // Deny inbound internet traffic to node subnet directly
          privateEndpointNetworkPolicies: 'Enabled'
          privateLinkServiceNetworkPolicies: 'Enabled'
        }
      }
    ]
  }
}

@description('Resource ID of the virtual network')
output vnetId string = vnet.id

@description('Name of the virtual network')
output vnetName string = vnet.name

@description('Resource ID of the AKS node subnet')
output aksSubnetId string = vnet.properties.subnets[0].id

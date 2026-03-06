// Main Bicep template: orchestrates network, ACR, and AKS cluster provisioning
// Deploy with:
//   az deployment group create \
//     --resource-group <rg> \
//     --template-file main.bicep \
//     --parameters @parameters/staging.bicepparam

targetScope = 'resourceGroup'

@description('Environment name (staging or production)')
@allowed(['staging', 'production'])
param environment string

@description('Azure region for all resources')
param location string = resourceGroup().location

@description('Short name prefix for all resources (e.g. "tailwindmail")')
@maxLength(12)
param namePrefix string

@description('Kubernetes version for the AKS cluster')
param kubernetesVersion string = '1.29'

@description('VM size for the AKS system node pool')
param systemNodeVmSize string = 'Standard_D2s_v3'

@description('Minimum node count for the AKS system node pool')
param minNodeCount int = 1

@description('Maximum node count for the AKS system node pool')
param maxNodeCount int = 3

@description('Address space for the virtual network')
param vnetAddressPrefix string = '10.0.0.0/16'

@description('Address prefix for the AKS node subnet')
param aksSubnetPrefix string = '10.0.0.0/22'

@description('Azure Container Registry SKU')
@allowed(['Basic', 'Standard', 'Premium'])
param acrSku string = 'Standard'

// Shared resource tags applied to all resources
var tags = {
  environment: environment
  application: 'tailwind-mail'
  managedBy: 'bicep'
}

// ACR name must be globally unique and alphanumeric only
var acrName = '${replace(namePrefix, '-', '')}${environment}acr'
var clusterName = '${namePrefix}-${environment}-aks'
var dnsPrefix = '${namePrefix}-${environment}'

// Network: VNet + subnets
module network 'modules/network.bicep' = {
  name: 'network'
  params: {
    location: location
    namePrefix: '${namePrefix}-${environment}'
    vnetAddressPrefix: vnetAddressPrefix
    aksSubnetPrefix: aksSubnetPrefix
    tags: tags
  }
}

// AKS cluster (created first so we have the kubelet identity for ACR role assignment)
module aks 'modules/aks.bicep' = {
  name: 'aks'
  params: {
    location: location
    clusterName: clusterName
    kubernetesVersion: kubernetesVersion
    systemNodeVmSize: systemNodeVmSize
    minNodeCount: minNodeCount
    maxNodeCount: maxNodeCount
    dnsPrefix: dnsPrefix
    aksSubnetId: network.outputs.aksSubnetId
    tags: tags
  }
}

// Azure Container Registry (grants AcrPull to AKS kubelet identity)
module acr 'modules/acr.bicep' = {
  name: 'acr'
  params: {
    location: location
    acrName: acrName
    sku: acrSku
    aksKubeletPrincipalId: aks.outputs.kubeletPrincipalId
    tags: tags
  }
}

// Outputs used by CI/CD pipelines
@description('Name of the AKS cluster')
output aksClusterName string = aks.outputs.clusterName

@description('ACR login server (e.g. <name>.azurecr.io)')
output acrLoginServer string = acr.outputs.loginServer

@description('ACR resource name')
output acrName string = acr.outputs.acrName

@description('OIDC issuer URL for workload identity')
output oidcIssuerUrl string = aks.outputs.oidcIssuerUrl

@description('Virtual network name')
output vnetName string = network.outputs.vnetName

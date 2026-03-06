// AKS module: provisions an Azure Kubernetes Service cluster with workload identity and managed identity

@description('Azure region for all resources')
param location string

@description('Name of the AKS cluster')
param clusterName string

@description('Kubernetes version')
param kubernetesVersion string = '1.29'

@description('VM size for the system node pool')
param systemNodeVmSize string = 'Standard_D2s_v3'

@description('Minimum node count for the system node pool')
param minNodeCount int = 1

@description('Maximum node count for the system node pool')
param maxNodeCount int = 3

@description('DNS prefix for the cluster')
param dnsPrefix string

@description('Resource ID of the AKS node subnet')
param aksSubnetId string

@description('Resource tags')
param tags object = {}

// User-assigned managed identity for the AKS control plane
resource aksIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: '${clusterName}-identity'
  location: location
  tags: tags
}

// Network Contributor role on the VNet subnet so AKS can manage NICs and load balancers
var networkContributorRoleId = '4d97b98b-1d4f-4787-a291-c67834d212e7'

resource networkContributorAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(aksSubnetId, aksIdentity.id, networkContributorRoleId)
  scope: resourceGroup()
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', networkContributorRoleId)
    principalId: aksIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

// AKS cluster
resource aks 'Microsoft.ContainerService/managedClusters@2024-01-01' = {
  name: clusterName
  location: location
  tags: tags
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${aksIdentity.id}': {}
    }
  }
  properties: {
    kubernetesVersion: kubernetesVersion
    dnsPrefix: dnsPrefix
    enableRBAC: true

    // Workload identity (OIDC issuer + token projection)
    oidcIssuerProfile: {
      enabled: true
    }
    securityProfile: {
      workloadIdentity: {
        enabled: true
      }
    }

    agentPoolProfiles: [
      {
        name: 'system'
        mode: 'System'
        vmSize: systemNodeVmSize
        count: minNodeCount
        minCount: minNodeCount
        maxCount: maxNodeCount
        enableAutoScaling: true
        osType: 'Linux'
        osSKU: 'AzureLinux'
        vnetSubnetID: aksSubnetId
        maxPods: 110
        nodeTaints: []
      }
    ]

    networkProfile: {
      networkPlugin: 'azure'
      networkPolicy: 'azure'
      loadBalancerSku: 'standard'
    }

    // Use the user-assigned identity for the kubelet (for ACR pull)
    identityProfile: {
      kubeletidentity: {
        resourceId: kubeletIdentity.id
        clientId: kubeletIdentity.properties.clientId
        objectId: kubeletIdentity.properties.principalId
      }
    }

    addonProfiles: {
      httpApplicationRouting: {
        enabled: false
      }
      azurePolicy: {
        enabled: true
      }
    }
  }
  dependsOn: [networkContributorAssignment]
}

// Separate kubelet identity so ACR role assignment can reference it before cluster creation
resource kubeletIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: '${clusterName}-kubelet-identity'
  location: location
  tags: tags
}

@description('Name of the AKS cluster')
output clusterName string = aks.name

@description('Resource ID of the AKS cluster')
output clusterId string = aks.id

@description('OIDC issuer URL for workload identity federation')
output oidcIssuerUrl string = aks.properties.oidcIssuerProfile.issuerURL

@description('Principal ID of the kubelet managed identity (used for ACR pull)')
output kubeletPrincipalId string = kubeletIdentity.properties.principalId

@description('Resource ID of the kubelet managed identity')
output kubeletIdentityId string = kubeletIdentity.id

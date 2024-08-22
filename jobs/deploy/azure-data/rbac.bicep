param principalID string

var rand = substring(uniqueString(resourceGroup().id), 0, 6)

var roleDefinitionId = {
  Owner: '8e3af657-a8ff-443c-a75c-2fe8c4bcb635'
  Contributor: 'b24988ac-6180-42a0-ab88-20f7382dd24c'
  Reader: 'acdd72a7-3385-48ef-bd42-f606fba81ae7'
  KeyVaultAdministrator: '00482a5a-887f-4fb3-b363-3b7fe8e74483'
  AcrPull: '7f951dda-4ed3-4680-a7ca-43fe172d538d'
  ServiceBusDataOwner: '090c5cfd-751d-490a-894a-3ce6f1109419'
  StorageBlobDataContributor: 'ba92f5b4-2d11-453d-a403-e96b0029c9fe'
  KubernetesServiceClusterUserRole: '4abbcc35-e782-43d8-92c5-2d3f1bd2253f'
}

resource serviceBus 'Microsoft.ServiceBus/namespaces@2021-06-01-preview' existing = {
  name: 'servicebus${rand}'
}

var roleAssignmentServiceBusDefinition = 'ServiceBusDataOwner'
resource roleAssignmentServiceBus 'Microsoft.Authorization/roleAssignments@2020-08-01-preview' = {
  name: guid(serviceBus.id, principalID)
  scope: serviceBus
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', roleDefinitionId[roleAssignmentServiceBusDefinition])
    principalId: principalID
  }
}

resource keyVault 'Microsoft.KeyVault/vaults@2022-07-01' existing = {
  name: 'keyvault${rand}'
}

var roleAssignmentKeyVaultDefinition = 'KeyVaultAdministrator'
resource roleAssignmentKeyVault 'Microsoft.Authorization/roleAssignments@2020-08-01-preview' = {
  name: guid(keyVault.id, principalID)
  scope: keyVault
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', roleDefinitionId[roleAssignmentKeyVaultDefinition])
    principalId: principalID
  }
}

resource containerRegistry 'Microsoft.ContainerRegistry/registries@2019-05-01' existing = {
  name: 'acr${rand}'
}

var roleAssignmentContainerRegistryDefinition = 'AcrPull'
resource roleAssignmentContainerRegistry 'Microsoft.Authorization/roleAssignments@2020-08-01-preview' = {
  name: guid(containerRegistry.id, principalID)
  scope: containerRegistry
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', roleDefinitionId[roleAssignmentContainerRegistryDefinition])
    principalId: principalID
  }
}

resource storageAccount 'Microsoft.Storage/storageAccounts@2021-02-01' existing = {
  name: 'storage${rand}'
}

var roleAssignmentStorageDefinition = 'StorageBlobDataContributor'
resource roleAssignmentStorage 'Microsoft.Authorization/roleAssignments@2020-08-01-preview' = {
  name: guid(storageAccount.id, principalID)
  scope: storageAccount
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', roleDefinitionId[roleAssignmentStorageDefinition])
    principalId: principalID
  }
}

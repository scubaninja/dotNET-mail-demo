param location string = resourceGroup().location
param secretName string = 'hello'
@secure()
param secretValue string = newGuid()
param deployPostgres bool = true

var rand = substring(uniqueString(resourceGroup().id), 0, 6)

resource managedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2018-11-30' = {
  name: '${resourceGroup().name}-identity'
  location: location
}

resource containerRegistry 'Microsoft.ContainerRegistry/registries@2019-05-01' = {
  name: 'acr${rand}'
  location: location
  sku: {
    name: 'Basic'
  }
  properties: {
    adminUserEnabled: true
  }
}

resource serviceBus 'Microsoft.ServiceBus/namespaces@2021-06-01-preview' = {
  name: 'servicebus${rand}'
  location: location
  sku: {
    capacity: 1
    name: 'Basic'
    tier: 'Basic'
  }
  properties: {
    disableLocalAuth: false
  }
}

resource storageAccount 'Microsoft.Storage/storageAccounts@2021-02-01' = {
  name: 'storage${rand}'
  location: location
  kind: 'BlockBlobStorage'
  sku: {
    name: 'Premium_LRS'
  }
}

resource keyVault 'Microsoft.KeyVault/vaults@2022-07-01' = {
  name: 'keyvault${rand}'
  location: location
  properties: {
    enableSoftDelete: false
    enableRbacAuthorization: true
    enabledForDeployment: true
    enabledForTemplateDeployment: true
    enabledForDiskEncryption: true
    tenantId: subscription().tenantId
    sku: {
      name: 'standard'
      family: 'A'
    }
    networkAcls: {
      defaultAction: 'Allow'
      bypass: 'AzureServices'
    }
  }
}

resource keyVaultSecret 'Microsoft.KeyVault/vaults/secrets@2021-11-01-preview' = {
  parent: keyVault
  name: secretName
  properties: {
    value: secretValue
  }
}

module postgres 'postgres.bicep' = if (deployPostgres) {
  name: 'postgres'
  params: {
    location: location
  }
}

module postgresAdministrator 'postgres-admin.bicep' = if (deployPostgres) {
  name: 'postgres-admin'
  params: {
    postgresName: postgres.outputs.postgresName
    principalId: managedIdentity.properties.principalId
    principalName: managedIdentity.name
  }
  dependsOn: [
    postgres
  ]
}

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

var roleAssignmentKeyVaultDefinition = 'KeyVaultAdministrator'
resource roleAssignmentKeyVault 'Microsoft.Authorization/roleAssignments@2020-08-01-preview' = {
  name: guid(containerRegistry.id, roleAssignmentKeyVaultDefinition)
  scope: keyVault
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', roleDefinitionId[roleAssignmentKeyVaultDefinition])
    principalId: managedIdentity.properties.principalId
  }
}

var roleAssignmentAcrDefinition = 'AcrPull'
resource roleAssignmentAcr 'Microsoft.Authorization/roleAssignments@2020-08-01-preview' = {
  name: guid(containerRegistry.id, roleAssignmentAcrDefinition)
  scope: containerRegistry
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', roleDefinitionId[roleAssignmentAcrDefinition])
    principalId: managedIdentity.properties.principalId
  }
}

var roleAssignmentStorageAccountDefinition = 'StorageBlobDataContributor'
resource roleAssignmentStorageAccount 'Microsoft.Authorization/roleAssignments@2020-08-01-preview' = {
  name: guid(storageAccount.id, roleAssignmentStorageAccountDefinition)
  scope: storageAccount
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', roleDefinitionId[roleAssignmentStorageAccountDefinition])
    principalId: managedIdentity.properties.principalId
  }
}

var roleAssignmentServiceBusDefinition = 'ServiceBusDataOwner'
resource roleAssignmentServiceBus 'Microsoft.Authorization/roleAssignments@2020-08-01-preview' = {
  name: guid(serviceBus.id, roleAssignmentServiceBusDefinition)
  scope: serviceBus
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', roleDefinitionId[roleAssignmentServiceBusDefinition])
    principalId: managedIdentity.properties.principalId
  }
}

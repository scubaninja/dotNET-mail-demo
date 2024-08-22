param env_name string = 'my-environment'
param app_name string = 'my-container-app'
param app_image string = 'mcr.microsoft.com/azuredocs/containerapps-helloworld:latest'
param job_name string = 'my-container-job'
param job_image string = 'ghcr.io/tailwind-traders-dev/jobs:latest'
param service_bus_connection string = ''
param app_queue_name string = 'tailwind'
param job_queue_name string = 'tailwind-job'

param location string = resourceGroup().location

var rand = substring(uniqueString(resourceGroup().id), 0, 6)

resource managedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2018-11-30' = {
  name: '${resourceGroup().name}-identity'
  location: location
}

resource containerRegistry 'Microsoft.ContainerRegistry/registries@2019-05-01' = {
  name: 'acr${rand}'
  location: location
  sku: {
    name: 'Standard'
  }
  properties: {
    adminUserEnabled: true
  }
}

var roleAssignmentAcrPull = '7f951dda-4ed3-4680-a7ca-43fe172d538d'
resource roleAssignmentAcr 'Microsoft.Authorization/roleAssignments@2020-08-01-preview' = {
  name: guid(containerRegistry.id, roleAssignmentAcrPull)
  scope: containerRegistry
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', roleAssignmentAcrPull)
    principalId: managedIdentity.properties.principalId
  }
}

var logAnalyticsWorkspaceName = '${env_name}-logs'

resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2020-03-01-preview' = {
  name: logAnalyticsWorkspaceName
  location: location
  properties: any({
    retentionInDays: 30
    features: {
      searchVersion: 1
    }
    sku: {
      name: 'PerGB2018'
    }
  })
}

resource environment 'Microsoft.App/managedEnvironments@2022-06-01-preview' = {
  name: env_name
  location: location
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        #disable-next-line use-resource-symbol-reference
        customerId: reference(logAnalyticsWorkspace.id, '2020-03-01-preview').customerId
        #disable-next-line use-resource-symbol-reference
        sharedKey: listKeys(logAnalyticsWorkspace.id, '2020-03-01-preview').primarySharedKey
      }
    }
  }
}

resource app 'Microsoft.App/containerApps@2022-06-01-preview' = {
  name: '${app_name}-web'
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${managedIdentity.id}': {}
    }
  }
  properties: {
    managedEnvironmentId: environment.id
    configuration: {
      ingress: {
        external: true
        targetPort: 80
      }
      registries: [
        {
          server: 'acr${rand}.azurecr.io'
          identity: managedIdentity.id
        }
      ]
    }
    template: {
      containers: [
        {
          name: '${app_name}-web'
          image: app_image
          resources: {
            cpu: json('0.5')
            memory: '1.0Gi'
          }
        }
      ]
      scale: {
        minReplicas: 0
        maxReplicas: 1
      }
    }
  }
}

resource backgroundApp 'Microsoft.App/containerApps@2022-06-01-preview' = {
  name: '${app_name}-jobs'
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${managedIdentity.id}': {}
    }
  }
  properties: {
    managedEnvironmentId: environment.id
    configuration: {
      secrets: [
        {
          name: 'connection-string-secret'
          value: service_bus_connection
        }
      ]
      registries: [
        {
          server: 'acr${rand}.azurecr.io'
          identity: managedIdentity.id
        }
      ]
    }
    template: {
      containers: [
        {
          name: '${app_name}-jobs'
          image: job_image
          command: [
            'mage'
            'servicebus:receive'
          ]
          env: [
            {
              name: 'AZURE_SERVICEBUS_CONNECTION_STRING'
              secretRef: 'connection-string-secret'
            }
            {
              name: 'AZURE_SERVICEBUS_QUEUE_NAME'
              value: app_queue_name
            }
          ]
          resources: {
            cpu: json('0.5')
            memory: '1.0Gi'
          }
        }
      ]
      scale: {
        minReplicas: 0
        maxReplicas: 2
        rules: [
          {
            name: 'azure-servicebus-queue-rule'
            custom: {
              type: 'azure-servicebus'
              metadata: { 
                //namespace: '...'
                queueName: app_queue_name
                messageCount: '5'
              }
              auth: [
                {
                  triggerParameter: 'connection'
                  secretRef: 'connection-string-secret'
                }
              ]
            }
          }
        ]
      }
    }
  }
}

resource job 'Microsoft.App/jobs@2023-05-01' = {
  name: job_name
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${managedIdentity.id}': {}
    }
  }
  properties: {
    environmentId: environment.id
    configuration: {
      secrets: [
        {
          name: 'connection-string-secret'
          value: service_bus_connection
        }
      ]
      registries: [
        {
          server: 'acr${rand}.azurecr.io'
          identity: managedIdentity.id
        }
      ]
      triggerType: 'Event'
      replicaTimeout: 300
      replicaRetryLimit: 3
      eventTriggerConfig: {
        replicaCompletionCount: 1
        parallelism: 1
        scale: {
          minExecutions: 0
          maxExecutions: 5
          pollingInterval: 60
          rules: [
            {
              name: 'azure-servicebus-queue-rule'
              type: 'azure-servicebus'
              metadata: { 
                //namespace: '...'
                queueName: job_queue_name
                messageCount: '5'
              }
              auth: [
                {
                  triggerParameter: 'connection'
                  secretRef: 'connection-string-secret'
                }
              ]
            }
          ]
        }
      }
    }
    template: {
      containers: [
        {
          name: '${job_name}-jobs'
          image: job_image
          env: [
            {
              name: 'AZURE_SERVICEBUS_CONNECTION_STRING'
              secretRef: 'connection-string-secret'
            }
            {
              name: 'AZURE_SERVICEBUS_QUEUE_NAME'
              value: job_queue_name
            }
          ]
          resources: {
            cpu: json('0.5')
            memory: '1.0Gi'
          }
          command: [
            'mage'
            'servicebus:receiveall'
          ]
        }
      ]
    }
  }
}

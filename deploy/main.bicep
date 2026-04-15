@description('Name prefix for all resources')
param appName string = 'tailwind-mail'

@description('Azure region for all resources')
param location string = resourceGroup().location

@description('Container image for the server (e.g. ghcr.io/org/repo:tag)')
param serverImage string = 'mcr.microsoft.com/dotnet/samples:aspnetapp'

@description('PostgreSQL administrator login')
param dbAdminLogin string = 'mailadmin'

@secure()
@description('PostgreSQL administrator password')
param dbAdminPassword string

@description('Default FROM address for outbound email')
param defaultFromEmail string = 'noreply@tailwind.dev'

@description('SMTP host')
param smtpHost string = ''

@description('SMTP username')
param smtpUser string = ''

@secure()
@description('SMTP password')
param smtpPassword string = ''

var rand = substring(uniqueString(resourceGroup().id), 0, 6)
var dbServerName = '${appName}-pg-${rand}'
var dbName = 'tailwind'
var envName = '${appName}-env'
var logWorkspaceName = '${appName}-logs'

// ── Log Analytics ────────────────────────────────────────────────────────────

resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
  name: logWorkspaceName
  location: location
  properties: {
    retentionInDays: 30
    sku: {
      name: 'PerGB2018'
    }
  }
}

// ── Container Apps Environment ───────────────────────────────────────────────

resource containerEnv 'Microsoft.App/managedEnvironments@2023-05-01' = {
  name: envName
  location: location
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalytics.properties.customerId
        sharedKey: logAnalytics.listKeys().primarySharedKey
      }
    }
  }
}

// ── PostgreSQL Flexible Server ───────────────────────────────────────────────

resource postgres 'Microsoft.DBforPostgreSQL/flexibleServers@2022-12-01' = {
  name: dbServerName
  location: location
  sku: {
    name: 'Standard_B1ms'
    tier: 'Burstable'
  }
  properties: {
    administratorLogin: dbAdminLogin
    administratorLoginPassword: dbAdminPassword
    version: '15'
    storage: {
      storageSizeGB: 32
    }
    backup: {
      backupRetentionDays: 7
      geoRedundantBackup: 'Disabled'
    }
    highAvailability: {
      mode: 'Disabled'
    }
  }
}

resource postgresDb 'Microsoft.DBforPostgreSQL/flexibleServers/databases@2022-12-01' = {
  parent: postgres
  name: dbName
  properties: {
    charset: 'UTF8'
    collation: 'en_US.utf8'
  }
}

// Allow Azure services to reach the database
resource postgresFirewall 'Microsoft.DBforPostgreSQL/flexibleServers/firewallRules@2022-12-01' = {
  parent: postgres
  name: 'AllowAzureServices'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

// ── Server Container App ─────────────────────────────────────────────────────

var dbUrl = 'Host=${postgres.properties.fullyQualifiedDomainName};Database=${dbName};Username=${dbAdminLogin};Password=${dbAdminPassword};SslMode=Require;'

resource serverApp 'Microsoft.App/containerApps@2023-05-01' = {
  name: '${appName}-server'
  location: location
  properties: {
    managedEnvironmentId: containerEnv.id
    configuration: {
      ingress: {
        external: true
        targetPort: 8080
      }
      secrets: [
        {
          name: 'db-url'
          value: dbUrl
        }
        {
          name: 'smtp-password'
          value: smtpPassword
        }
      ]
    }
    template: {
      containers: [
        {
          name: '${appName}-server'
          image: serverImage
          env: [
            {
              name: 'ASPNETCORE_ENVIRONMENT'
              value: 'Production'
            }
            {
              name: 'DATABASE_URL'
              secretRef: 'db-url'
            }
            {
              name: 'DEFAULT_FROM'
              value: defaultFromEmail
            }
            {
              name: 'SMTP_HOST'
              value: smtpHost
            }
            {
              name: 'SMTP_USER'
              value: smtpUser
            }
            {
              name: 'SMTP_PASSWORD'
              secretRef: 'smtp-password'
            }
          ]
          resources: {
            cpu: json('0.5')
            memory: '1.0Gi'
          }
        }
      ]
      scale: {
        minReplicas: 1
        maxReplicas: 3
      }
    }
  }
}

// ── Outputs ──────────────────────────────────────────────────────────────────

output serverUrl string = 'https://${serverApp.properties.configuration.ingress.fqdn}'
output postgresHost string = postgres.properties.fullyQualifiedDomainName

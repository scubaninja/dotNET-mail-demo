@description('Specifies the location for all resources')
param location string = resourceGroup().location

@description('Specifies the name of the web app')
param webAppName string

@description('Specifies the SKU for the hosting plan')
param skuName string = 'S1'

@description('Specifies the capacity for the hosting plan')
param skuCapacity int = 1

@description('Specifies the .NET version for the application')
param dotNetVersion string = 'v6.0'

@description('Specifies the source of the application package (e.g., a URL to a zip file)')
param appPackageUrl string

// Resource for the App Service Plan
resource hostingPlan 'Microsoft.Web/serverfarms@2020-12-01' = {
  name: '${webAppName}-plan'
  location: location
  sku: {
    name: skuName
    tier: skuName
    capacity: skuCapacity
  }
}

// Resource for the Azure Web App
resource webApp 'Microsoft.Web/sites@2020-12-01' = {
  name: webAppName
  location: location
  properties: {
    serverFarmId: hostingPlan.id
    httpsOnly: true
    siteConfig: {
      netFrameworkVersion: dotNetVersion
      appSettings: [
        {
          name: 'WEBSITE_RUN_FROM_PACKAGE'
          value: appPackageUrl
        }
      ]
    }
  }
}

// Output the Web App URL
output webAppUrl string = 'https://${webApp.name}.azurewebsites.net'

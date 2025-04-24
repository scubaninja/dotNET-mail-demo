@description('For testing purposes')
param testMode bool = true

// Test parameters
var testParams = {
  webAppName: 'test-webapp'
  location: 'westus'
  appPackageUrl: 'https://test.com/app.zip'
}

// Test deployment
module testDeployment 'deploy.bicep' = if (testMode) {
  name: 'test-deployment'
  params: {
    webAppName: testParams.webAppName
    location: testParams.location
    appPackageUrl: testParams.appPackageUrl
  }
}

// Validation
output testWebAppUrl string = testDeployment.outputs.webAppUrl

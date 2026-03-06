using 'main.bicep'

param environment = 'production'
param namePrefix = 'tailwindmail'
param location = 'eastus'
param kubernetesVersion = '1.29'
param systemNodeVmSize = 'Standard_D4s_v3'
param minNodeCount = 2
param maxNodeCount = 5
param vnetAddressPrefix = '10.2.0.0/16'
param aksSubnetPrefix = '10.2.0.0/22'
param acrSku = 'Standard'

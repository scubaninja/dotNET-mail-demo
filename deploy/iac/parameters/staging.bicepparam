using 'main.bicep'

param environment = 'staging'
param namePrefix = 'tailwindmail'
param location = 'eastus'
param kubernetesVersion = '1.29'
param systemNodeVmSize = 'Standard_D2s_v3'
param minNodeCount = 1
param maxNodeCount = 3
param vnetAddressPrefix = '10.1.0.0/16'
param aksSubnetPrefix = '10.1.0.0/22'
param acrSku = 'Standard'

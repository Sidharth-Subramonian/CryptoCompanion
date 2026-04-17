$ErrorActionPreference = "Stop"

# Configuration Variables
# Replace 'cryptocompanionacr' with a unique name (must be 5-50 alphanumeric characters globally unique)
$AcrName = "cryptocompanionacr$(Get-Random -Maximum 9999)" 
$ResourceGroup = "CryptoCompanion_Cloud_RG"
$Location = "eastus"
$Sku = "Basic" # Basic is cost-effective for development

Write-Host "Creating Azure Container Registry Pipeline..."
Write-Host "-------------------------------------------"

# 1. Login to Azure (uncomment if you aren't already logged in)
# az login

# Register necessary providers (Fixes 'MissingSubscriptionRegistration' error)
Write-Host "Registering Azure Providers (this may take a moment)..."
az provider register --namespace Microsoft.ContainerRegistry
az provider register --namespace Microsoft.ContainerService # Required for AKS later

Write-Host "Waiting for Microsoft.ContainerRegistry registration..."
while ((az provider show -n Microsoft.ContainerRegistry --query registrationState -o tsv) -ne "Registered") {
    Write-Host "Still registering..."
    Start-Sleep -Seconds 5
}
Write-Host "Provider registered!`n"

# 2. Create the Resource Group
Write-Host "1. Creating Resource Group '$ResourceGroup' in '$Location'..."
az group create --name $ResourceGroup --location $Location --output none
Write-Host "   Resource Group created successfully.`n"

# 3. Create the Azure Container Registry (ACR)
Write-Host "2. Creating Azure Container Registry '$AcrName' (this may take a minute)..."
az acr create --resource-group $ResourceGroup --name $AcrName --sku $Sku --admin-enabled true --output none
Write-Host "   ACR '$AcrName' created successfully.`n"

# 4. Get the ACR login server
$LoginServer = az acr show --name $AcrName --query loginServer --output tsv

Write-Host "-------------------------------------------"
Write-Host "SUCCESS!"
Write-Host "Your Azure Container Registry is ready to use."
Write-Host "Login Server: $LoginServer"
Write-Host "ACR Name:     $AcrName"
Write-Host "-------------------------------------------"
Write-Host "Next Steps:"
Write-Host "1. Run 'az acr login --name $AcrName' to authenticate."
Write-Host "2. Build & tag your image using '$LoginServer/cryptocompanionapi:latest'"
Write-Host "3. Update your k8s-deployment.yaml with this new image URL."

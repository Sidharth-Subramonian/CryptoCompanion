$ErrorActionPreference = "Stop"

# Configuration Variables - OLD RELIABLE MODEL IN SWEDEN
$AisName = "cryptocompanion-ai-sw722" # Using the Sweden account that was just created
$ResourceGroup = "rg-cryptocompanion-prod"
$ModelName = "gpt-35-turbo"
$DeploymentName = "advisor-v1"
$ModelVersion = "0125"

Write-Host "Updating Sweden OpenAI Account with gpt-35-turbo..."
Write-Host "-------------------------------------------"

# 1. Register the Cognitive Services provider
Write-Host "1. Ensuring Microsoft.CognitiveServices provider is registered..."
az provider register --namespace Microsoft.CognitiveServices

# 2. Deploy the Model (Most compatible version for students)
Write-Host "3. Deploying Model '$ModelName' (Deployment Name: $DeploymentName)..."
az cognitiveservices account deployment create `
    --name $AisName `
    --resource-group $ResourceGroup `
    --deployment-name $DeploymentName `
    --model-name $ModelName `
    --model-version $ModelVersion `
    --model-format OpenAI `
    --sku-capacity 1 `
    --sku-name "Standard"

Write-Host "   Model deployed successfully.`n"

# 3. Get Keys and Endpoint
$Endpoint = az cognitiveservices account show --name $AisName --resource-group $ResourceGroup --query "properties.endpoint" --output tsv
$Key = az cognitiveservices account keys list --name $AisName --resource-group $ResourceGroup --query "key1" --output tsv

Write-Host "-------------------------------------------"
Write-Host "SUCCESS!"
Write-Host "Endpoint: $Endpoint"
Write-Host "Key:      $Key"
Write-Host "-------------------------------------------"
Write-Host "Note: It may take 1 minute for the deployment to become active."
Write-Host "-------------------------------------------"

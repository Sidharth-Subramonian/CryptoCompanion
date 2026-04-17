$ErrorActionPreference = "Stop"

# Configuration Variables
$ClusterName = "cryptocompanion-aks"
$ResourceGroup = "rg-cryptocompanion-prod"
$Location = "eastus"
$AcrName = "cryptocompanionacr7638" # From your successful push

Write-Host "Creating Azure Kubernetes Service (AKS) Cluster..."
Write-Host "----------------------------------------------"
Write-Host "This process typically takes 5-10 minutes. Please wait..."

# 1. Create the AKS Cluster
# Optimized for 'Azure for Students' (1 node, small VM size)
Write-Host "1. Creating AKS Cluster '$ClusterName' (Standard_B2s, 1 node)..."
az aks create `
    --resource-group $ResourceGroup `
    --name $ClusterName `
    --node-count 1 `
    --generate-ssh-keys `
    --attach-acr $AcrName `
    --node-vm-size Standard_B2s `
    --location $Location

Write-Host "   AKS Cluster created successfully.`n"

# 2. Get Credentials for kubectl
Write-Host "2. Configuring kubectl to connect to the new cluster..."
az aks get-credentials --resource-group $ResourceGroup --name $ClusterName --overwrite-existing
Write-Host "   Credentials updated.`n"

# 3. Verify Connection
Write-Host "3. Verifying cluster connection..."
kubectl get nodes

Write-Host "----------------------------------------------"
Write-Host "SUCCESS!"
Write-Host "Your AKS cluster is ready."
Write-Host "Next Step: Run 'kubectl apply -f k8s-deployment.yaml' to deploy your API."
Write-Host "----------------------------------------------"

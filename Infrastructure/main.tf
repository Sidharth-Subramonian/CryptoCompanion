# Terraform Infrastructure for CryptoCompanion (Review II)

terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.0"
    }
  }
}

provider "azurerm" {
  features {
    cognitive_account {
      purge_soft_delete_on_destroy = true
    }
  }
}

# 1. Resource Group
resource "azurerm_resource_group" "rg" {
  name     = "rg-cryptocompanion-prod"
  location = "East US"
}

# 2. Azure Container Registry (ACR)
resource "azurerm_container_registry" "acr" {
  name                = "cryptocompanionacr7638"
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location
  sku                 = "Basic"
  admin_enabled       = true
}

# 3. Azure Kubernetes Service (AKS)
resource "azurerm_kubernetes_cluster" "aks" {
  name                = "cryptocompanion-aks"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  dns_prefix          = "cryptocompanion"

  default_node_pool {
    name       = "default"
    node_count = 1
    vm_size    = "Standard_B2s"
  }

  identity {
    type = "SystemAssigned"
  }

  # Link ACR to AKS for image pulling
  role_based_access_control_enabled = true
}

# 4. Azure OpenAI Service
resource "azurerm_cognitive_account" "openai" {
  name                = "cryptocompanion-openai"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  kind                = "OpenAI"
  sku_name            = "S0"
  custom_subdomain_name = "cryptocompanion-openai"
}

resource "azurerm_cognitive_deployment" "gpt_model" {
  name                 = "crypto-advisor"
  cognitive_account_id = azurerm_cognitive_account.openai.id
  model {
    format  = "OpenAI"
    name    = "gpt-35-turbo"
    version = "0613"
  }
  scale {
    type = "Standard"
  }
}

# Outputs
output "acr_login_server" {
  value = azurerm_container_registry.acr.login_server
}

output "kubernetes_cluster_name" {
  value = azurerm_kubernetes_cluster.aks.name
}

output "openai_endpoint" {
  value = azurerm_cognitive_account.openai.endpoint
}

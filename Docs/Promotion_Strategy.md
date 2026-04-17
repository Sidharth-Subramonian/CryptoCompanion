# Environment Promotion & Deployment Strategy

This document outlines the DevOps and Infrastructure strategy for the CryptoCompanion platform, satisfying the **Review II** requirements for **Containerization & Orchestration**.

## 1. Environment Promotion (Dev → Prod)

The platform follows a standard containerized promotion flow:

| Stage | Infrastructure | registry / Context | Purpose |
| :--- | :--- | :--- | :--- |
| **Development** | Local Machine / Docker Desktop | `docker.io` (local) | Local testing of API endpoints and DB migrations. |
| **Testing/Staging**| Azure Resource Group | **ACR** (`cryptocompanionacr7638`) | Pre-deployment validation of the container image. |
| **Production** | **Azure Kubernetes Service (AKS)** | **AKS Cluster** | Live hosting with high availability and automated scaling. |

## 2. Infrastructure as Code (IaC)
All cloud resources are managed using **Terraform** (`main.tf`). This ensures that the environment can be recreated identically in any Azure region.

- **Resource Group**: `rg-cryptocompanion-prod`
- **Orchestrator**: Azure Kubernetes Service (AKS)
- **Image Hosting**: Azure Container Registry (ACR)

## 3. CI/CD Orchestration
We utilize **GitHub Actions** for the automation pipeline:
1. **Continuous Integration (CI)**:
   - Triggered on push to `main`.
   - Steps: Checkout → .NET Build → Docker Build → ACR Push.
2. **Continuous Deployment (CD)**:
   - Triggered after successful CI.
   - Steps: Get K8s Credentials → `kubectl apply` (using `k8s-deployment.yaml`).

## 4. Configuration Management
- **Secrets**: API Keys and Connection Strings are managed through `appsettings.json` (for demo purposes) and can be promoted to **Kubernetes Secrets** for production security.
- **Scaling**: Managed by the AKS ReplicaSets (currently set to 2 replicas for the API).

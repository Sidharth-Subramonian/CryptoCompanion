# Technical Cheat Sheet: Project Review II (Viva Prep)

Use this document to quickly look up answers during your technical Q&A session.

## 🚀 DevOps & Infrastructure (Terraform)
*   **What is Terraform?**: Infrastructure as Code (IaC) tool. We use it to provision Azure resources (RG, AKS, ACR) in a repeatable way using HCL (HashiCorp Configuration Language).
*   **Why Terraform?**: Avoids manual "ClickOps" in the portal; provides version control for our infrastructure.
*   **Key Command**: `terraform apply` builds the actual cloud environment matching our `main.tf`.

## 🐳 Containerization (Docker & AKS)
*   **What is ACR?**: Azure Container Registry. A private repository where we store our Docker images.
*   **What is AKS?**: Azure Kubernetes Service. A managed Kubernetes cluster that runs our Docker containers.
*   **What is a Pod?**: The smallest deployable unit in Kubernetes (usually runs 1 container).
*   **Scaling**: We use Horizontal Pod Autoscaling (HPA) or simply increase `replicas` in `k8s-deployment.yaml` to handle more user traffic.
*   **Multi-Stage Build**: Our Dockerfile has a "Build" stage (SDK) and a "Final" stage (ASP.NET Runtime). This keeps the production image small and more secure.

## 🤖 GenAI Integration
*   **The Model**: We use **GPT-3.5 Turbo** (Version 0125) via **Azure OpenAI Service**.
*   **The Use Case**: It provides "Market Intelligence" by analyzing real-time prices and sentiment data to give the user professional investment summaries.
*   **Deployment Region**: We used **Sweden Central** for the OpenAI resource because it offers the highest quota for student and trial subscriptions.
*   **Security**: The API Keys are never hardcoded; they are injected into the AKS Pods using **Environment Variables**.

## 📊 Database Strategy
*   **Dual Data Strategy**:
    *   **SQL Server (Serverless)**: For structured data like Users and Portfolios.
    *   **Cosmos DB (NoSQL)**: For high-velocity, semi-structured data like News articles and Social media signals.
*   **Why Serverless?**: It allows the database to "pause" when not in use, saving 100% of the cost during idle times—perfect for an MVP project.

## 🛠️ Typical Viva Questions:
1.  **"How do you handle secrets?"**: "We use GitHub Actions Secrets and inject them into Kubernetes as environment variables. In a full production app, we would use Azure Key Vault."
2.  **"Why AKS instead of App Service?"**: "To gain experience with container orchestration and to support a more complex, microservice-ready architecture in the future."
3.  **"What happens if your OpenAI quota runs out?"**: "Our code includes an exception handler that detects the 'InsufficientQuota' error and returns a friendly fallback message, ensuring the app doesn't crash."

# Review II: Presentation Slide Outline (10 Slides Max)

Use this outline to create your PowerPoint deck. This structure is designed to hit all 4 evaluation components in under 10 minutes.

---

### Slide 1: Title Slide
*   **Title**: CryptoCompanion: A Modern Cloud-Native AI Platform
*   **Subtitle**: Project Review II – Cloud Computing Integration
*   **Details**: Team Name/ID, Member Names, Date.

### Slide 2: Updated System Architecture
*   **Visual**: Use the Mermaid diagram from `Docs/Azure_Deployment_Strategy.md`.
*   **Key points**: 
    *   Explain the Hybrid model (AKS + App Service).
    *   Mention the Dual-Database strategy (SQL + Cosmos).
    *   Highlight the on-device ML vs Cloud AI split.

### Slide 3: Infrastructure as Code (DevOps)
*   **Visual**: Fragment of `Infrastructure/main.tf` (Resources: AKS, ACR, OpenAI).
*   **Key points**:
    *   Using **Terraform** for reproducible infrastructure.
    *   Decoupling environment creation from application code.
    *   Managing resources in a single `rg-cryptocompanion-prod` group.

### Slide 4: CI/CD Pipeline Flow
*   **Visual**: Screenshot of a GitHub Actions run or the `azure-aks-deploy.yml` logic.
*   **Key points**:
    *   **Automation**: Triggered on push to `main`.
    *   **Pipeline Stages**: Docker Build → ACR Push → AKS Rollout.
    *   **Secret Management**: Using GitHub Actions Secrets for Azure Credentials.

### Slide 5: Containerization (Orchestration)
*   **Visual**: Dockerfile snippet + `kubectl get pods` output.
*   **Key points**:
    *   **Dockerization**: Multi-stage builds for optimized image size.
    *   **Orchestration**: Managed by Azure Kubernetes Service (AKS).
    *   **Registry**: Images stored in private Azure Container Registry (ACR).

### Slide 6: Environment Promotion & Scaling
*   **Visual**: Scaling configuration from `k8s-deployment.yaml`.
*   **Key points**:
    *   **Promotion**: Local Dev → Docker Hub/ACR → AKS Production.
    *   **Scaling**: Horizontal Pod Autoscalers (HPA) to handle market volatility.
    *   **Config**: Environment-specific variables injected at runtime.

### Slide 7: GenAI Integration Use Cases
*   **Visual**: Screenshot of the "Market Intelligence" feature in the app.
*   **Key points**:
    *   **Use Case 1**: Real-time market analysis summary.
    *   **Use Case 2**: Personalized investment suggestions based on portfolio.
    *   **Azure Service**: Azure OpenAI (GPT-3.5 Turbo) via Sweden Central region.

### Slide 8: Security & Reliability
*   **Visual**: Key Vault logo + Managed Identity diagram.
*   **Key points**:
    *   **Security**: Key Vault for API keys (CoinGecko, NewsAPI).
    *   **Reliability**: Serverless SQL DB with auto-pause (Cost-effective HA).
    *   **Monitoring**: Real-time telemetry via Application Insights.

### Slide 9: Project Status & Lessons Learned
*   **Key points**:
    *   **Status**: All cloud infrastructure live and integrated with the MAUI client.
    *   **Challenges**: Managing regional OpenAI quotas and K8s image pull secrets.
    *   **Outcome**: A scalable, production-grade fintech solution.

### Slide 10: Conclusion & Q&A
*   **Final Statement**: "Ready for Review II demonstration."
*   **Contact Info**.

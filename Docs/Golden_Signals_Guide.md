# Guide: The 4 Golden Signals for CryptoCompanion AI

For Review III (Monitoring Slide), you should be prepared to discuss these **4 Golden Signals** in the context of your platform's AI service.

---

### 1. Latency (The "Speed" Signal)
*   **Definition**: The time it takes to service a request.
*   **Our Platform**: We track `OpenAI_Latency_ms`.
*   **Why it matters**: In FinTech, slow data is useless data. If our AI summary takes 10+ seconds, the market data is already stale.

### 2. Traffic (The "Demand" Signal)
*   **Definition**: A measure of how much demand is being placed on your system.
*   **Our Platform**: Tracked as total counts of `OpenAI_MarketIntelligence` requests in Application Insights.
*   **Why it matters**: Helps us decide when to scale our Azure Kubernetes (AKS) pods or choose a higher-tier OpenAI quota.

### 3. Errors (The "Health" Signal)
*   **Definition**: The rate of requests that fail, either explicitly or implicitly.
*   **Our Platform**: We track `TrackException` and HTTP `500` status codes in our telemetry blocks.
*   **Why it matters**: If OpenAI is down (Rate limit exceeded or Azure outage), our app must provide a "Last Known Good" summary instead of crashing.

### 4. Saturation (The "Capacity" Signal)
*   **Definition**: How "full" your service is.
*   **Our Platform**: In our case, this is often the **OpenAI Token Quota** (Tokens per minute).
*   **Why it matters**: If we reach 90% saturation, we must either optimize our prompts (shorter context) or request higher limits from Azure.

---

> [!TIP]
> **Demo Point**: If asked *how* you see these signals, mention the **Azure Monitor Workbook** and the **Application Map** in the Azure Portal.

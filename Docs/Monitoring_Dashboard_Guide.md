# Monitoring & Observability Guide (Review III)

This document outlines the monitoring strategy for the CryptoCompanion platform using **Azure Application Insights** and **Azure Monitor**.

---

## 1. Monitoring Stack
*   **Service**: Azure Application Insights (integrated via SDK in `Program.cs`).
*   **Target**: `cryptocompanion-api` (running on AKS).
*   **Dashboard**: Azure Monitor Dashboards.

## 2. Three Key Metrics (The "Golden Signals")

### Metric 1: Request Success Rate (%)
*   **Definition**: The percentage of HTTP 200/201 responses vs 4xx/5xx errors.
*   **Why it Matters**: Gauges the overall health of the API. A drop here indicates a deployment failure or database connectivity issue.
*   **Target**: >99% in production.

### Metric 2: Dependency Latency (ms) - OpenAI & Cosmos DB
*   **Definition**: The average time taken for a downstream call to the OpenAI API or Cosmos DB to return.
*   **Why it Matters**: Crypto intelligence relies on fast external data. High latency here directly impacts user experience in the mobile app.
*   **Target**: <2000ms for OpenAI; <10ms for Cosmos DB.

### Metric 3: Exception Volume
*   **Definition**: Total number of unhandled exceptions logged in the background workers (`CryptoDataWorker`, etc.).
*   **Why it Matters**: Ensures that our background data engine is consistently fetching new market prices without crashing.

---

## 3. Mocked Alerting Rule

| Alert Name | Condition | Action |
| :--- | :--- | :--- |
| **OpenAI Quota Alert** | Status Code = 429 (Too Many Requests) OR 500 (Internal Error) | Critical e-mail notification to Team Lead. |
| **SQL Server Timeout** | Latency > 30s | Metric-based alert to DevOps team. |

---

## 4. Visualizing Data: Application Insights Map
In the Microsoft Azure Portal, the **Application Map** provides a visual representation of how the CryptoCompanion API communicates with:
1.  **Azure SQL Database** (Structured Portfolios)
2.  **Azure Cosmos DB** (News Aggregator)
3.  **Azure OpenAI** (GenAI Advisor)

---

## 5. Conclusion
With Application Insights, we move from "reactive" to "proactive" engineering—identifying performance bottlenecks before the user ever sees an error.

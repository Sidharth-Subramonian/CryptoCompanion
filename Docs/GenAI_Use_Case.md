# Use Case: GenAI Crypto Intelligence Advisor

This document describes the integration of Generative AI into the CryptoCompanion platform for **Project Review II**.

## 1. Use Case Description
**Feature Name**: Crypto Intelligence Advisor
**Description**: An AI-powered financial analyst that synthesizes real-time market data (Price, Volume, RSI) and news sentiment into human-readable investment summaries.

## 2. Azure AI Service Mapping
To implement this feature, we utilize the following Azure Cloud service:

- **Service Category**: Azure AI Services
- **Specific Service**: **Azure OpenAI Service**
- **Model Used**: `gpt-35-turbo` (Version 0125)
- **deployment Name**: `advisor-v1`
- **Location**: Sweden Central

## 3. Architecture Flow
1. **Trigger**: User requests "Suggestions" from the .NET MAUI mobile app.
2. **Data Aggregation**: The Backend API (`CryptoCompanionApi`) fetches:
   - Top 10 assets from **Azure SQL Database**.
   - Latest 3 news articles from **Azure Cosmos DB**.
3. **AI Inference**: The backend sends this raw data as a system-prompted context to **Azure OpenAI**.
4. **Response**: The AI generates a 3-sentence professional market intelligence summary.
5. **Display**: The mobile app displays the AI summary at the top of the suggestions feed.

## 4. Sample Integration Logic (Backend)
The integration is handled by the `IAiAdvisorService` which builds a prompt dynamically:

```csharp
// Example Prompt Construction
"You are the CryptoCompanion AI Advisor. Here is the current market state for the top assets:
- Bitcoin (BTC): $62,000, 24h Change: 2.5%, RSI: 65.2
...
Based on this data, provide a professional 3-sentence summary..."
```

## 5. Proof of Setup (Azure CLI)
The service was provisioned using the following CLI logic (from our `create_openai.ps1` script):
```powershell
az cognitiveservices account create --name cryptocompanion-ai --kind OpenAI --sku S0
az cognitiveservices account deployment create --deployment-name crypto-advisor --model-name gpt-35-turbo
```

using Azure;
using Azure.AI.OpenAI;
using CryptoCompanionApi.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using System.Text;
using System.Diagnostics;

namespace CryptoCompanionApi.Services;

public class AzureOpenAiAdvisorService : IAiAdvisorService
{
    private readonly OpenAIClient _client;
    private readonly string _deploymentName;
    private readonly TelemetryClient _telemetryClient;

    public AzureOpenAiAdvisorService(IConfiguration configuration, TelemetryClient telemetryClient)
    {
        var endpoint = configuration["OpenAI:Endpoint"] ?? throw new ArgumentNullException("OpenAI:Endpoint is missing");
        var key = configuration["OpenAI:Key"] ?? throw new ArgumentNullException("OpenAI:Key is missing");
        _deploymentName = configuration["OpenAI:DeploymentName"] ?? "advisor-v1";
        _telemetryClient = telemetryClient;

        _client = new OpenAIClient(new Uri(endpoint), new AzureKeyCredential(key));
    }

    public async Task<string> GetMarketIntelligenceAsync(List<CryptoAsset> topAssets, List<NewsArticle> latestNews)
    {
        var stopwatch = Stopwatch.StartNew();
        var prompt = BuildPrompt(topAssets, latestNews);

        var chatCompletionsOptions = new ChatCompletionsOptions()
        {
            DeploymentName = _deploymentName,
            Messages =
            {
                new ChatRequestSystemMessage("You are the CryptoCompanion AI Advisor. Your goal is to provide a concise, professional market intelligence summary based on current price data and news sentiment."),
                new ChatRequestUserMessage(prompt)
            },
            MaxTokens = 500,
            Temperature = 0.7f
        };

        try 
        {
            Response<ChatCompletions> response = await _client.GetChatCompletionsAsync(chatCompletionsOptions);
            stopwatch.Stop();

            // --- TRACK GOLDEN SIGNALS ---
            _telemetryClient.TrackMetric("OpenAI_Latency_ms", stopwatch.ElapsedMilliseconds);
            _telemetryClient.TrackRequest("OpenAI_MarketIntelligence", DateTimeOffset.UtcNow, stopwatch.Elapsed, "200", true);

            return response.Value.Choices[0].Message.Content;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            // Track failure
            _telemetryClient.TrackException(ex);
            _telemetryClient.TrackRequest("OpenAI_MarketIntelligence", DateTimeOffset.UtcNow, stopwatch.Elapsed, "500", false);
            throw;
        }
    }

    private string BuildPrompt(List<CryptoAsset> topAssets, List<NewsArticle> latestNews)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Here is the current market state for the top assets:");
        foreach (var asset in topAssets.Take(5))
        {
            sb.AppendLine($"- {asset.Name} ({asset.Symbol}): ${asset.CurrentPrice:N2}, 24h Change: {asset.PercentChange24h:N2}%, RSI: {asset.RSIScore:N1}");
        }

        sb.AppendLine("\nRecent news headlines and sentiment:");
        foreach (var news in latestNews.Take(3))
        {
            sb.AppendLine($"- {news.Title} (Sentiment: {news.SentimentLabel}, Score: {news.SentimentScore:N2})");
        }

        sb.AppendLine("\nBased on this data, provide a professional 3-sentence summary of the current market intelligence and an overall sentiment outlook.");
        
        return sb.ToString();
    }
}

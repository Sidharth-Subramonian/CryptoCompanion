using CryptoCompanionApi.Data;
using CryptoCompanionApi.Models;
using Microsoft.EntityFrameworkCore;

namespace CryptoCompanionApi.Services;

public interface ISentimentDataService
{
    Task FetchLatestSentimentAsync(CancellationToken cancellationToken);
}

public class SentimentDataWorker : BackgroundService, ISentimentDataService
{
    private readonly ILogger<SentimentDataWorker> _logger;
    private readonly IServiceProvider _serviceProvider;

    // Simple keyword lists for basic sentiment analysis
    private static readonly string[] BullishKeywords = { "bullish", "surge", "rally", "moon", "breakout", "gains", "soar", "rise", "up", "high", "record", "growth", "positive", "buy", "profit", "boost" };
    private static readonly string[] BearishKeywords = { "bearish", "crash", "dump", "plunge", "sell", "drop", "down", "low", "fall", "decline", "loss", "negative", "fear", "risk", "warning", "collapse" };

    public SentimentDataWorker(ILogger<SentimentDataWorker> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Wait for the news worker to have a chance to populate data first
        await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("SentimentDataWorker running at: {time}", DateTimeOffset.Now);
            
            try
            {
                await FetchLatestSentimentAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deriving sentiment data");
            }

            // Poll every 15 minutes
            await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
        }
    }

    public async Task FetchLatestSentimentAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var cosmosDb = scope.ServiceProvider.GetRequiredService<CosmosDbContext>();

        _logger.LogInformation("Deriving sentiment from recent news articles...");

        try
        {
            // Get recent news articles to derive sentiment from
            var recentNews = await cosmosDb.NewsArticles
                .OrderByDescending(a => a.PublishedAt)
                .Take(20)
                .ToListAsync(cancellationToken);

            if (!recentNews.Any())
            {
                _logger.LogWarning("No news articles found to derive sentiment from.");
                return;
            }

            int newEntries = 0;
            foreach (var article in recentNews)
            {
                // Check if we already derived sentiment for this article
                var existing = await cosmosDb.SocialSentiments
                    .Where(s => s.Content == article.Title)
                    .FirstOrDefaultAsync(cancellationToken);

                if (existing != null) continue;

                // Derive sentiment from the article title + summary
                var (sentimentClass, confidence) = AnalyzeSentiment(article.Title + " " + article.Summary);
                
                // Determine which coin this is most related to
                var coinSymbol = article.RelatedCoins?.FirstOrDefault() ?? "BTC";

                cosmosDb.SocialSentiments.Add(new SocialSentiment
                {
                    Platform = "News-Derived",
                    Content = article.Title,
                    Author = article.Source,
                    PostedAt = article.PublishedAt,
                    CoinSymbol = coinSymbol.ToUpper(),
                    SentimentClass = sentimentClass,
                    Confidence = confidence
                });
                newEntries++;
            }

            if (newEntries > 0)
            {
                await cosmosDb.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Derived and saved {count} new sentiment entries from news.", newEntries);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to derive sentiment from news articles.");
        }
    }

    private static (string sentimentClass, double confidence) AnalyzeSentiment(string text)
    {
        var lowerText = text.ToLowerInvariant();
        
        int bullishCount = BullishKeywords.Count(k => lowerText.Contains(k));
        int bearishCount = BearishKeywords.Count(k => lowerText.Contains(k));
        int totalHits = bullishCount + bearishCount;

        if (totalHits == 0)
            return ("Neutral", 0.5);

        if (bullishCount > bearishCount)
        {
            double confidence = Math.Min(0.95, 0.5 + (bullishCount - bearishCount) * 0.1);
            return ("Bullish", confidence);
        }
        else if (bearishCount > bullishCount)
        {
            double confidence = Math.Min(0.95, 0.5 + (bearishCount - bullishCount) * 0.1);
            return ("Bearish", confidence);
        }
        
        return ("Neutral", 0.5);
    }
}

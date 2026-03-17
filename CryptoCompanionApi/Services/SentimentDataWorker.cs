using CryptoCompanionApi.Data;
using CryptoCompanionApi.Models;

namespace CryptoCompanionApi.Services;

public interface ISentimentDataService
{
    Task FetchLatestSentimentAsync(CancellationToken cancellationToken);
}

public class SentimentDataWorker : BackgroundService, ISentimentDataService
{
    private readonly ILogger<SentimentDataWorker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly HttpClient _httpClient;

    public SentimentDataWorker(ILogger<SentimentDataWorker> logger, IServiceProvider serviceProvider, HttpClient httpClient)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _httpClient = httpClient;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("SentimentDataWorker running at: {time}", DateTimeOffset.Now);
            
            try
            {
                await FetchLatestSentimentAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching sentiment data");
            }

            // Poll every 15 minutes
            await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
        }
    }

    public async Task FetchLatestSentimentAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var cosmosDb = scope.ServiceProvider.GetRequiredService<CosmosDbContext>();

        // MOCK IMPLEMENTATION: Fetch from Twitter API / Reddit API
        _logger.LogInformation("Fetching sentiment data from Twitter/Reddit...");
        
        var mockSentiment = new SocialSentiment
        {
            Platform = "Twitter",
            Content = "$BTC is looking incredibly bullish right now! Going to the moon! 🚀",
            Author = "CryptoWhale123",
            PostedAt = DateTime.UtcNow,
            CoinSymbol = "BTC"
        };

        cosmosDb.SocialSentiments.Add(mockSentiment);
        await cosmosDb.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Saved sentiment data to Cosmos DB.");
    }
}

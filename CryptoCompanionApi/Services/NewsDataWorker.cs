using CryptoCompanionApi.Data;
using CryptoCompanionApi.Models;

namespace CryptoCompanionApi.Services;

public interface INewsDataService
{
    Task FetchLatestNewsAsync(CancellationToken cancellationToken);
}

public class NewsDataWorker : BackgroundService, INewsDataService
{
    private readonly ILogger<NewsDataWorker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly HttpClient _httpClient;

    public NewsDataWorker(ILogger<NewsDataWorker> logger, IServiceProvider serviceProvider, HttpClient httpClient)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _httpClient = httpClient;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("NewsDataWorker running at: {time}", DateTimeOffset.Now);
            
            try
            {
                await FetchLatestNewsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching news data");
            }

            // Poll every 30 minutes
            await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
        }
    }

    public async Task FetchLatestNewsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var cosmosDb = scope.ServiceProvider.GetRequiredService<CosmosDbContext>();

        // MOCK IMPLEMENTATION: Fetch from Google News RSS / CryptoPanic API
        _logger.LogInformation("Fetching news data from RSS feeds...");
        
        var mockArticle = new NewsArticle
        {
            Title = "Bitcoin Surges Past Expectations Following SEC Ruling",
            Summary = "The cryptocurrency market saw massive inflows today as Bitcoin broke key resistance levels.",
            Url = "https://example.com/crypto-news",
            Source = "ExampleCryptoNews",
            PublishedAt = DateTime.UtcNow,
            RelatedCoins = new List<string> { "BTC", "ETH" }
        };

        cosmosDb.NewsArticles.Add(mockArticle);
        await cosmosDb.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Saved news article to Cosmos DB.");
    }
}

using CryptoCompanionApi.Data;
using CryptoCompanionApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;

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
    private readonly IConfiguration _configuration;

    public NewsDataWorker(
        ILogger<NewsDataWorker> logger, 
        IServiceProvider serviceProvider, 
        HttpClient httpClient,
        IConfiguration configuration) // Injected configuration
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _httpClient = httpClient;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("NewsDataWorker heartbeat at: {time}", DateTimeOffset.Now);
            
            try
            {
                await FetchLatestNewsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Background Worker encountered an unhandled exception.");
            }

            // Poll every 30 minutes
            await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
        }
    }

    public async Task FetchLatestNewsAsync(CancellationToken cancellationToken)
{
    using var scope = _serviceProvider.CreateScope();
    var cosmosDb = scope.ServiceProvider.GetRequiredService<CosmosDbContext>();

    try 
    {
        var apiKey = _configuration["CryptoCompare:ApiKey"];
        
        var url = $"https://min-api.cryptocompare.com/data/v2/news/?lang=EN&api_key={apiKey}";
        
        var responseString = await _httpClient.GetStringAsync(url, cancellationToken);

        // 1. PEEK at the JSON using JsonDocument (This avoids the "Shape Mismatch" crash)
        using var doc = JsonDocument.Parse(responseString);
        var root = doc.RootElement;

        // 2. Check the Status field first
        if (root.TryGetProperty("Response", out var responseStatus) && responseStatus.GetString() == "Error")
        {
            var msg = root.TryGetProperty("Message", out var message) ? message.GetString() : "Unknown Error";
            _logger.LogError("CryptoCompare API Error: {msg}", msg);
            return; // Exit safely without crashing
        }

        // 3. If we are here, the response is good! Now we can safely deserialize.
        var response = System.Text.Json.JsonSerializer.Deserialize<CryptoCompareResponse>(responseString);

        if (response?.Data == null) return;

        int newArticlesCount = 0;
        foreach (var apiArticle in response.Data.Take(15))
        {
            // This generates: SELECT TOP 1 * FROM c WHERE c.Url = @url -> Much more stable
            var existingArticle = await cosmosDb.NewsArticles
                .Where(a => a.Url == apiArticle.Url)
                .FirstOrDefaultAsync(cancellationToken);


            if (existingArticle == null)
            {
                cosmosDb.NewsArticles.Add(new NewsArticle
                {
                    Title = apiArticle.Title,
                    Summary = apiArticle.Body,
                    Url = apiArticle.Url,
                    Source = apiArticle.Source,
                    PublishedAt = DateTimeOffset.FromUnixTimeSeconds(apiArticle.PublishedOn).DateTime,
                    RelatedCoins = apiArticle.Tags?.Split('|').ToList() ?? new List<string>()
                });
                newArticlesCount++;
            }
        }

        if (newArticlesCount > 0)
        {
            await cosmosDb.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Successfully saved {count} new articles to Cosmos.", newArticlesCount);
        }
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to sync news. Check if the API Key is valid in appsettings.");
    }
}
}

// --- DATA TRANSFER OBJECTS (DTOs) ---

public class CryptoCompareResponse
{
    [JsonPropertyName("Data")]
    public List<CryptoCompareArticle>? Data { get; set; } = new();

    [JsonPropertyName("Message")]
    public string? Message { get; set; }

    [JsonPropertyName("Response")]
    public string? ResponseStatus { get; set; }
}

public class CryptoCompareArticle
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("body")]
    public string Body { get; set; } = string.Empty;

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("source")]
    public string Source { get; set; } = string.Empty;

    [JsonPropertyName("published_on")]
    public long PublishedOn { get; set; }

    [JsonPropertyName("tags")]
    public string Tags { get; set; } = string.Empty;
}
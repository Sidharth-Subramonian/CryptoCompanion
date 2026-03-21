using CryptoCompanionApi.Data;
using CryptoCompanionApi.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CryptoCompanionApi.Services;

public interface ICryptoDataService
{
    Task FetchLatestCryptoDataAsync(CancellationToken cancellationToken);
}

public class CryptoDataWorker : BackgroundService, ICryptoDataService
{
    private readonly ILogger<CryptoDataWorker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly HttpClient _httpClient;

    public CryptoDataWorker(ILogger<CryptoDataWorker> logger, IServiceProvider serviceProvider, HttpClient httpClient)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _httpClient = httpClient;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Wait a bit for the app to fully start
        await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("CryptoDataWorker running at: {time}", DateTimeOffset.Now);
            
            try
            {
                await FetchLatestCryptoDataAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching crypto data");
            }

            // Poll every 5 minutes (CoinGecko free tier: ~30 calls/min)
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }

    public async Task FetchLatestCryptoDataAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        _logger.LogInformation("Fetching crypto data from CoinGecko...");

        try
        {
            var url = "https://api.coingecko.com/api/v3/coins/markets?vs_currency=usd&order=market_cap_desc&per_page=20&page=1&sparkline=false";
            
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "CryptoCompanion/1.0");

            var responseString = await _httpClient.GetStringAsync(url, cancellationToken);
            var coins = JsonSerializer.Deserialize<List<CoinGeckoMarketItem>>(responseString);

            if (coins == null || coins.Count == 0)
            {
                _logger.LogWarning("CoinGecko returned empty response.");
                return;
            }

            int upsertCount = 0;
            foreach (var coin in coins)
            {
                var existing = dbContext.CryptoAssets.FirstOrDefault(c => c.Symbol == coin.Symbol.ToUpper());

                if (existing == null)
                {
                    dbContext.CryptoAssets.Add(new CryptoAsset
                    {
                        Symbol = coin.Symbol.ToUpper(),
                        Name = coin.Name,
                        CurrentPrice = coin.CurrentPrice,
                        MarketCap = coin.MarketCap,
                        Volume24h = coin.TotalVolume,
                        PercentChange24h = coin.PriceChangePercentage24h,
                        LastUpdated = DateTime.UtcNow,
                        RSIScore = 50, // Placeholder – real RSI requires historical candles
                        MovingAverage50d = coin.CurrentPrice * 0.97m, // Approximation
                        MovingAverage200d = coin.CurrentPrice * 0.90m  // Approximation
                    });
                }
                else
                {
                    existing.Name = coin.Name;
                    existing.CurrentPrice = coin.CurrentPrice;
                    existing.MarketCap = coin.MarketCap;
                    existing.Volume24h = coin.TotalVolume;
                    existing.PercentChange24h = coin.PriceChangePercentage24h;
                    existing.LastUpdated = DateTime.UtcNow;
                }
                upsertCount++;
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Saved {count} crypto assets from CoinGecko to SQL.", upsertCount);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error calling CoinGecko API. Rate limited?");
        }
    }
}

// --- CoinGecko DTO ---
public class CoinGeckoMarketItem
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("symbol")]
    public string Symbol { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("current_price")]
    public decimal CurrentPrice { get; set; }

    [JsonPropertyName("market_cap")]
    public decimal MarketCap { get; set; }

    [JsonPropertyName("total_volume")]
    public decimal TotalVolume { get; set; }

    [JsonPropertyName("price_change_percentage_24h")]
    public decimal PriceChangePercentage24h { get; set; }
}

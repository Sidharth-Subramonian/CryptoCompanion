using CryptoCompanionApi.Data;
using CryptoCompanionApi.Models;

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

            // Poll every 5 minutes
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }

    public async Task FetchLatestCryptoDataAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // MOCK IMPLEMENTATION: Fetch from Binance / CoinGecko
        _logger.LogInformation("Fetching crypto data from APIs...");
        
        // TODO: Replace with actual HttpClient calls to Binance API
        var bitcoin = new CryptoAsset 
        { 
            Symbol = "BTC", 
            Name = "Bitcoin", 
            CurrentPrice = 65000 + (decimal)new Random().NextDouble() * 1000, 
            MarketCap = 1200000000000,
            Volume24h = 45000000000,
            PercentChange24h = (decimal)new Random().NextDouble() * 5 - 2,
            LastUpdated = DateTime.UtcNow,
            RSIScore = 55,
            MovingAverage50d = 62000,
            MovingAverage200d = 50000
        };

        var existingAsset = dbContext.CryptoAssets.FirstOrDefault(c => c.Symbol == "BTC");
        if (existingAsset == null)
        {
            dbContext.CryptoAssets.Add(bitcoin);
        }
        else
        {
            existingAsset.CurrentPrice = bitcoin.CurrentPrice;
            existingAsset.PercentChange24h = bitcoin.PercentChange24h;
            existingAsset.LastUpdated = bitcoin.LastUpdated;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Saved crypto data to SQL Database.");
    }
}

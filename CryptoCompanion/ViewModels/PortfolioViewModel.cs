using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CryptoCompanion.Services.ML;
using CryptoCompanion.Services.Api;
using System.Text.Json;
using System.Text;
using System.Security.Cryptography;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
namespace CryptoCompanion.ViewModels;

public partial class PortfolioViewModel : BaseViewModel
{
    private readonly IOnnxInferenceService _mlService;
    private readonly IBackendApiService _apiService;

    [ObservableProperty]
    private string _totalValueDisplay = "Calculating...";

    [ObservableProperty]
    private string _portfolioRiskAnalysis;

    [ObservableProperty]
    private ObservableCollection<PortfolioAsset> _assets = new();

    [ObservableProperty]
    private ISeries[] _portfolioSeries = Array.Empty<ISeries>();
    [ObservableProperty]
    private string _coinDcxApiKey = string.Empty;

    [ObservableProperty]
    private string _coinDcxApiSecret = string.Empty;

    [ObservableProperty]
    private bool _isLinked = false;

    private static readonly HttpClient _httpClient = new HttpClient { BaseAddress = new Uri("https://api.coindcx.com/") };

    public PortfolioViewModel(IOnnxInferenceService mlService, IBackendApiService apiService)
    {
        _mlService = mlService;
        _apiService = apiService;
        Title = "Portfolio Tracker";
        PortfolioRiskAnalysis = "Calculating ML Risk Profile...";
        
        Task.Run(LoadPortfolioFromApiAsync);
    }

    private async Task LoadPortfolioFromApiAsync()
    {
        try
        {
            IsBusy = true;
            
            if (IsLinked && !string.IsNullOrWhiteSpace(CoinDcxApiKey) && !string.IsNullOrWhiteSpace(CoinDcxApiSecret))
            {
                await LoadCoinDcxPortfolioAsync();
            }
            else
            {
                await LoadSimulatedPortfolioAsync();
            }
        }
        catch (Exception ex)
        {
            TotalValueDisplay = $"Error: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadSimulatedPortfolioAsync()
    {
        var cryptoAssets = await _apiService.GetSuggestionsAsync();
        
        if (cryptoAssets == null || !cryptoAssets.Any())
        {
            TotalValueDisplay = "No data — is API running?";
            return;
        }

            // Take top 5 assets and simulate portfolio allocation based on market cap ratios
            var topAssets = cryptoAssets.Take(5).ToList();
            var totalMarketCap = topAssets.Sum(a => a.MarketCap);

            decimal totalValue = 0;
            var portfolioAssets = new List<PortfolioAsset>();

            foreach (var asset in topAssets)
            {
                decimal allocation = totalMarketCap > 0 
                    ? Math.Round(asset.MarketCap / totalMarketCap, 2) 
                    : 0.2m;
                
                // Simulate holding ₹10,00,000 worth, distributed by market cap ratio
                decimal holdingValue = 1000000m * allocation;
                totalValue += holdingValue;

                portfolioAssets.Add(new PortfolioAsset
                {
                    Symbol = asset.Symbol,
                    Value = holdingValue,
                    Allocation = allocation
                });
            }

            MainThread.BeginInvokeOnMainThread(() =>
            {
                Assets.Clear();
                var pieSeries = new List<ISeries>();
                int colorIdx = 0;
                var colors = new[] { SKColor.Parse("#6366F1"), SKColor.Parse("#D946EF"), SKColor.Parse("#22D3EE"), SKColor.Parse("#10B981"), SKColor.Parse("#F59E0B") };
                foreach (var pa in portfolioAssets)
                {
                    Assets.Add(pa);
                    pieSeries.Add(new PieSeries<decimal>
                    {
                        Name = pa.Symbol,
                        Values = new decimal[] { pa.Value },
                        Fill = new SolidColorPaint(colors[colorIdx % colors.Length]),
                        InnerRadius = 60
                    });
                    colorIdx++;
                }
                PortfolioSeries = pieSeries.ToArray();
                TotalValueDisplay = $"Rs. {totalValue:N2}";
            });
    }

    [RelayCommand]
    public async Task LinkCoinDcxAccountAsync()
    {
        if (string.IsNullOrWhiteSpace(CoinDcxApiKey) || string.IsNullOrWhiteSpace(CoinDcxApiSecret))
        {
            TotalValueDisplay = "Enter Key & Secret";
            return;
        }

        IsLinked = true;
        await LoadPortfolioFromApiAsync();
    }

    [RelayCommand]
    public async Task UnlinkAccountAsync()
    {
        IsLinked = false;
        CoinDcxApiKey = string.Empty;
        CoinDcxApiSecret = string.Empty;
        await LoadPortfolioFromApiAsync();
    }

    private async Task LoadCoinDcxPortfolioAsync()
    {
        try
        {
            string apiKey = CoinDcxApiKey?.Trim() ?? string.Empty;
            string apiSecret = CoinDcxApiSecret?.Trim() ?? string.Empty;

            var timeStamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var payload = new { timestamp = timeStamp };
            var jsonPayload = JsonSerializer.Serialize(payload);

            byte[] secretBytes = Encoding.UTF8.GetBytes(apiSecret);
            byte[] payloadBytes = Encoding.UTF8.GetBytes(jsonPayload);

            using var hmac = new HMACSHA256(secretBytes);
            byte[] hashBytes = hmac.ComputeHash(payloadBytes);
            string signature = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

            using var request = new HttpRequestMessage(HttpMethod.Post, "exchange/v1/users/balances");
            request.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            
            // Critical fix: Many crypto APIs reject requests if the header is 'application/json; charset=utf-8'
            request.Content.Headers.ContentType.CharSet = string.Empty; 
            
            request.Headers.Add("X-AUTH-APIKEY", apiKey);
            request.Headers.Add("X-AUTH-SIGNATURE", signature);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                // Fallback to simulation if credentials fail so UI doesn't crash during demo
                var errorText = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"CoinDCX API Error: {errorText}");
                await LoadSimulatedPortfolioAsync();
                TotalValueDisplay = $"API Error: {(int)response.StatusCode}";
                PortfolioRiskAnalysis = $"Reason: {errorText}";
                return;
            }

            var content = await response.Content.ReadAsStringAsync();
            var balances = JsonSerializer.Deserialize<List<CoinDcxBalance>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            decimal totalValue = 0;
            var portfolioAssets = new List<PortfolioAsset>();

            if (balances != null)
            {
                // Fetch real prices from our API
                var cryptoAssets = await _apiService.GetSuggestionsAsync() ?? new List<Models.CryptoAsset>();
                var priceLookup = cryptoAssets.ToDictionary(a => a.Symbol.Split('-')[0].ToUpper(), a => a.CurrentPrice);

                foreach (var balance in balances.Where(b => b.Balance > 0))
                {
                    string currency = balance.Currency.ToUpper();
                    decimal realPrice = 0m;

                    if (currency == "INR")
                    {
                        realPrice = 1m; 
                    }
                    else if (priceLookup.TryGetValue(currency, out decimal livePrice))
                    {
                        realPrice = livePrice;
                    }

                    decimal holdingValue = balance.Balance * realPrice;
                    totalValue += holdingValue;

                    portfolioAssets.Add(new PortfolioAsset
                    {
                        Symbol = balance.Currency,
                        Value = holdingValue,
                        Allocation = 0 // Will recalculate
                    });
                }
            }

            foreach(var asset in portfolioAssets)
            {
                asset.Allocation = totalValue > 0 ? asset.Value / totalValue : 0;
            }

            MainThread.BeginInvokeOnMainThread(() =>
            {
                Assets.Clear();
                var pieSeries = new List<ISeries>();
                int colorIdx = 0;
                var colors = new[] { SKColor.Parse("#6366F1"), SKColor.Parse("#D946EF"), SKColor.Parse("#22D3EE"), SKColor.Parse("#10B981"), SKColor.Parse("#F59E0B") };

                foreach (var pa in portfolioAssets.OrderByDescending(a => a.Value))
                {
                    Assets.Add(pa);
                    pieSeries.Add(new PieSeries<decimal>
                    {
                        Name = pa.Symbol,
                        Values = new decimal[] { pa.Value },
                        Fill = new SolidColorPaint(colors[colorIdx % colors.Length]),
                        InnerRadius = 50
                    });
                    colorIdx++;
                }
                PortfolioSeries = pieSeries.ToArray();
                TotalValueDisplay = $"Rs. {totalValue:N2}";
            });
        }
        catch (Exception ex)
        {
            TotalValueDisplay = $"CoinDCX Error: {ex.Message}";
        }
    }

    [RelayCommand]
    public async Task AnalyzePortfolioRiskAsync()
    {
        if (IsBusy) return;
        
        try
        {
            IsBusy = true;
            await _mlService.InitializeAsync();
            
            // Use real portfolio data for ML inference
            var cryptoAssets = await _apiService.GetSuggestionsAsync();
            
            if (cryptoAssets == null || !cryptoAssets.Any())
            {
                PortfolioRiskAnalysis = "No data available for risk analysis";
                return;
            }

            // Compute portfolio-level features from real data
            var topAssets = cryptoAssets.Take(5).ToList();
            float portfolioVol = (float)topAssets.Average(a => Math.Abs((double)a.PercentChange24h));
            float portfolioReturn = (float)topAssets.Average(a => (double)a.PercentChange24h);
            float avgMcWeight = (float)(topAssets.Average(a => (double)a.MarketCap) / 1_000_000_000);

            int clusterId = _mlService.PredictPortfolioCluster(portfolioVol, portfolioReturn, avgMcWeight);
            
            string riskProfile = clusterId switch
            {
                0 => "Low Risk / Steady Growth (Stable)",
                1 => "High Risk / High Yield (Aggressive)",
                2 => "Moderate Risk / Balanced",
                _ => "Unknown Risk Profile"
            };

            PortfolioRiskAnalysis = $"K-Means Clustering: {riskProfile}";
        }
        catch (Exception ex)
        {
            PortfolioRiskAnalysis = $"Risk Analysis Error: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}

public class PortfolioAsset
{
    public string Symbol { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public decimal Allocation { get; set; }
}

public class CoinDcxBalance
{
    public string Currency { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public decimal LockedBalance { get; set; }
}

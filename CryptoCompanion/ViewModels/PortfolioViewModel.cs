using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CryptoCompanion.Services.ML;
using CryptoCompanion.Services.Api;

namespace CryptoCompanion.ViewModels;

public partial class PortfolioViewModel : BaseViewModel
{
    private readonly IOnnxInferenceService _mlService;
    private readonly IBackendApiService _apiService;

    [ObservableProperty]
    private string _portfolioRiskAnalysis;

    [ObservableProperty]
    private string _totalValueDisplay = "Loading...";

    [ObservableProperty]
    private ObservableCollection<PortfolioAsset> _assets = new();

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
                
                // Simulate holding $10,000 worth, distributed by market cap ratio
                decimal holdingValue = 10000m * allocation;
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
                foreach (var pa in portfolioAssets)
                {
                    Assets.Add(pa);
                }
                TotalValueDisplay = $"${totalValue:N2}";
            });
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

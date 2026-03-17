using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CryptoCompanion.Services.ML;

namespace CryptoCompanion.ViewModels;

public partial class PortfolioViewModel : BaseViewModel
{
    private readonly IOnnxInferenceService _mlService;

    [ObservableProperty]
    private string _portfolioRiskAnalysis;

    [ObservableProperty]
    private ObservableCollection<PortfolioAsset> _assets = new();

    public PortfolioViewModel(IOnnxInferenceService mlService)
    {
        _mlService = mlService;
        Title = "Portfolio Tracker";
        PortfolioRiskAnalysis = "Calculating ML Risk Profile...";
        
        LoadMockPortfolio();
    }

    private void LoadMockPortfolio()
    {
        Assets.Add(new PortfolioAsset { Symbol = "BTC", Value = 12000, Allocation = 60 });
        Assets.Add(new PortfolioAsset { Symbol = "ETH", Value = 6000, Allocation = 30 });
        Assets.Add(new PortfolioAsset { Symbol = "SOL", Value = 2000, Allocation = 10 });
    }

    [RelayCommand]
    public async Task AnalyzePortfolioRiskAsync()
    {
        if (IsBusy) return;
        
        try
        {
            IsBusy = true;
            await _mlService.InitializeAsync();
            
            // Mock holistic portfolio features: Volatility, DailyReturn, MarketCap weight
            float portfolioVol = 3.5f;
            float portfolioReturn = 1.2f;
            float averageMcWeight = 8.0f;
            
            int clusterId = _mlService.PredictPortfolioCluster(portfolioVol, portfolioReturn, averageMcWeight);
            
            string riskProfile = clusterId switch
            {
                0 => "Low Risk / Steady Growth (Stable)",
                1 => "High Risk / High Yield (Aggressive)", // based on our python dummy weights
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

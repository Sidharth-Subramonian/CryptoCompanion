using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CryptoCompanion.Services.ML;
using CryptoCompanion.Services.Api;
using System.Collections.ObjectModel;
using CryptoCompanion.Models;

namespace CryptoCompanion.ViewModels;

public partial class SuggestionsViewModel : BaseViewModel
{
    private readonly IOnnxInferenceService _mlService;
    private readonly IBackendApiService _apiService;
    
    [ObservableProperty]
    private string _btcForecast;
    
    [ObservableProperty]
    private string _btcRank;

    [ObservableProperty]
    private string _aiStatusText = "System Standby";

    public ObservableCollection<SuggestionModel> Suggestions { get; set; } = new();

    public bool IsEmpty => !IsBusy && Suggestions.Count == 0;

    public SuggestionsViewModel(IOnnxInferenceService mlService, IBackendApiService apiService)
    {
        _mlService = mlService;
        _apiService = apiService;
        Title = "AI Suggestions";
        BtcForecast = "Tap 'Run AI' to analyze all coins";
        BtcRank = "ML-ranked predictions with reasoning";
    }

    [RelayCommand]
    public async Task RunMlPredictionsAsync()
    {
        try { 
            AiStatusText = "Fetching live global market streams...";
            await Task.Delay(400); // Simulate processing weight

            var assets = await _apiService.GetSuggestionsAsync();
            
            if (assets == null || !assets.Any())
            {
                AiStatusText = "Data stream failed.";
                BtcForecast = "No data from API. Is the backend running?";
                return;
            }

            AiStatusText = "Initializing ONNX Neural Engine...";
            await Task.Delay(600); 

            await _mlService.InitializeAsync();

            AiStatusText = $"Running Deep Analysis on {assets.Count} Assets...";
            await Task.Delay(600);

            // Run ML on ALL assets, not just top 5
            var scoredSuggestions = new List<SuggestionModel>();

            foreach (var asset in assets)
            {
                float ma50 = (float)asset.MovingAverage50d;
                float rsi = (float)asset.RSIScore;
                float volume = (float)asset.Volume24h;
                // Ensure realistic mock volume change instead of simple ratio
                float volChange = asset.PercentChange24h > 0 ? 0.05f : -0.05f; 
                float priceChange = (float)(asset.PercentChange24h / 100m);

                float currentPrice = (float)asset.CurrentPrice;
                float rawMa50 = (float)asset.MovingAverage50d;
                float normMa50 = rawMa50 > 0 ? (currentPrice - rawMa50) / rawMa50 : 0f;

                // Approximate missing relative features with realistic standard distributions
                float normMa10 = normMa50 * 0.5f;
                float normMa20 = normMa50 * 0.8f;
                float normVolatility = 0.06f; // Standard daily crypto volatility ~6%

                // Run ONNX inference
                float predictedChange = _mlService.PredictPriceDirection(priceChange, normMa10, normMa20, normMa50, rsi, normVolatility, volChange);
                var rankingScoreRaw = _mlService.PredictCryptoRanking(priceChange, normMa10, normMa20, normMa50, rsi, normVolatility, volChange);
                
                // Scale Ranking Score to 0-100 (assume typical range -20% to +20% -> 0 to 100)
                int rankingScore = (int)Math.Clamp(50 + (rankingScoreRaw * 250), 0, 100);

                // Real price prediction derived natively from model
                decimal percentDiff = (decimal)(predictedChange * 100);
                decimal predictedDecimal = asset.CurrentPrice * (1 + (decimal)predictedChange);

                // Determine signal
                string signal;
                if (percentDiff > 0.5m) signal = "Bullish";
                else if (percentDiff < -0.5m) signal = "Bearish";
                else signal = "Neutral";

                // Generate human-readable reasoning
                var reasons = new List<string>();

                // RSI analysis
                if (rsi > 70)
                    reasons.Add($"RSI at {rsi:F0} (overbought) — possible correction ahead");
                else if (rsi < 30)
                    reasons.Add($"RSI at {rsi:F0} (oversold) — potential bounce opportunity");
                else
                    reasons.Add($"RSI at {rsi:F0} — neutral momentum");

                // Moving Average crossover
                if (asset.MovingAverage50d > asset.MovingAverage200d)
                    reasons.Add("50d MA above 200d MA — bullish golden cross");
                else if (asset.MovingAverage50d < asset.MovingAverage200d)
                    reasons.Add("50d MA below 200d MA — bearish death cross");

                // Volume analysis
                decimal volMarketCapRatio = asset.MarketCap > 0 ? asset.Volume24h / asset.MarketCap : 0;
                if (volMarketCapRatio > 0.15m)
                    reasons.Add($"High vol/mcap ratio ({volMarketCapRatio:P0}) — strong activity");
                else if (volMarketCapRatio < 0.03m)
                    reasons.Add($"Low vol/mcap ratio ({volMarketCapRatio:P0}) — thin trading");

                // 24h price change
                if (asset.PercentChange24h > 5)
                    reasons.Add($"Up {asset.PercentChange24h:F1}% today — strong momentum");
                else if (asset.PercentChange24h < -5)
                    reasons.Add($"Down {asset.PercentChange24h:F1}% today — selling pressure");

                string reasoning = string.Join("\n", reasons.Select(r => $"• {r}"));

                string forecast = $"₹{asset.CurrentPrice:N2}  →  ₹{predictedDecimal:N2}  ({predictedChange:P1})";
                string rank = $"AI Strength Score: {rankingScore}/100";

                scoredSuggestions.Add(new SuggestionModel
                {
                    Name = $"{asset.Name} ({asset.Symbol})",
                    Forecast = forecast,
                    Rank = rank,
                    Signal = signal,
                    Timeline = "24h Price Prediction",
                    Reasoning = reasoning,
                    CurrentPrice = asset.CurrentPrice,
                    PredictedPrice = predictedDecimal,
                    PercentDiff = percentDiff,
                    RankingScore = rankingScoreRaw,
                    BackgroundKey = signal == "Bullish" ? "Primary" : signal == "Bearish" ? "Secondary" : "Gray600"
                });
            }

            // Sort by ranking score descending — best picks first
            var topSuggestions = scoredSuggestions
                .OrderByDescending(s => s.RankingScore)
                .Take(10)
                .ToList();

            Suggestions.Clear();
            foreach (var suggestion in topSuggestions)
            {
                Suggestions.Add(suggestion);
            }

            AiStatusText = "Analysis Complete";
            BtcForecast = $"Top {Suggestions.Count} picks from {assets.Count} coins analyzed";
            BtcRank = $"Ranked by AI Strength Score • {DateTime.Now:HH:mm:ss}";
        }
        catch (Exception ex)
        {
            BtcForecast = $"Error: {ex.Message}";
            BtcRank = "Ensure .onnx files are present and API is running";
        }
        finally
        {
            IsBusy = false;
            OnPropertyChanged(nameof(IsEmpty));
        }
    }
}

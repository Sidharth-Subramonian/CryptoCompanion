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
        try
        {
            IsBusy = true;

            // Fetch real crypto asset data from the API
            var assets = await _apiService.GetSuggestionsAsync();
            
            if (assets == null || !assets.Any())
            {
                BtcForecast = "No data from API. Is the backend running?";
                BtcRank = "Start the API with: dotnet run";
                return;
            }

            await _mlService.InitializeAsync();

            // Run ML on ALL assets, not just top 5
            var scoredSuggestions = new List<SuggestionModel>();

            foreach (var asset in assets)
            {
                float ma50 = (float)asset.MovingAverage50d;
                float rsi = (float)asset.RSIScore;
                float volume = (float)asset.Volume24h;
                float volChange = ma50 > 0 ? (float)(asset.Volume24h / asset.MarketCap) : 0.05f;
                float priceChange = (float)(asset.PercentChange24h / 100m);

                float currentPrice = (float)asset.CurrentPrice;
                float rawMa50 = (float)asset.MovingAverage50d;
                float normMa50 = rawMa50 > 0 ? (currentPrice - rawMa50) / rawMa50 : 0f;

                // Approximate missing relative features since backend only provides raw 50d/200d MA
                float normMa10 = 0.01f;
                float normMa20 = 0.02f;
                float normVolatility = 0.05f; // mock scaled volatility

                // Run ONNX inference with new signatures capturing the Regressor percentages
                float predictedChange = _mlService.PredictPriceDirection(priceChange, normMa10, normMa20, normMa50, rsi, normVolatility, volChange);
                var rankingScore = _mlService.PredictCryptoRanking(priceChange, normMa10, normMa20, normMa50, rsi, normVolatility, volChange);
                
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

                string forecast = $"₹{asset.CurrentPrice:N2}  →  ₹{predictedDecimal:N2}  ({predictedChange.ToString("P1")})";
                string rank = $"AI Strength Score: {rankingScore:N0}/100";

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
                    RankingScore = rankingScore,
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

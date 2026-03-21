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
        Title = "Top Suggestions (ML Forecast)";
        BtcForecast = "Tap 'Run AI Predictions' to start";
        BtcRank = "Fetching live data from API...";
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

            Suggestions.Clear();

            // Process top assets with real data from CoinGecko via the API
            foreach (var asset in assets.Take(5))
            {
                float ma50 = (float)asset.MovingAverage50d;
                float rsi = (float)asset.RSIScore;
                float volume = (float)asset.Volume24h;
                float ma200 = (float)asset.MovingAverage200d;
                float volChange = ma50 > 0 ? (float)(asset.Volume24h / asset.MarketCap) : 0.05f;
                float priceChange = (float)(asset.PercentChange24h / 100m);

                // Run ONNX inference with real data
                var predictedPrice = _mlService.PredictPriceForecast(ma50, rsi, volume);
                var rankingScore = _mlService.PredictCryptoRanking(rsi, ma50, ma200, volChange, priceChange);

                string forecast = $"ML Forecast: ${predictedPrice:N0} | Now: ${asset.CurrentPrice:N2} ({asset.PercentChange24h:+0.0;-0.0}%)";
                string rank = $"Ranking Score: {rankingScore:N0}/100";

                Suggestions.Add(new SuggestionModel
                {
                    Name = $"{asset.Name} ({asset.Symbol})",
                    Forecast = forecast,
                    Rank = rank,
                    BackgroundKey = asset.PercentChange24h >= 0 ? "Primary" : "Secondary"
                });
            }

            if (Suggestions.Any())
            {
                BtcForecast = $"Analyzed {Suggestions.Count} assets with live data";
                BtcRank = $"Last updated: {DateTime.Now:HH:mm:ss}";
            }
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

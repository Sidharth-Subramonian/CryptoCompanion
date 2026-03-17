using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CryptoCompanion.Services.ML;
using System.Collections.ObjectModel;

namespace CryptoCompanion.ViewModels;

public partial class SuggestionsViewModel : BaseViewModel
{
    private readonly IOnnxInferenceService _mlService;
    
    [ObservableProperty]
    private string _btcForecast;
    
    [ObservableProperty]
    private string _btcRank;

    public SuggestionsViewModel(IOnnxInferenceService mlService)
    {
        _mlService = mlService;
        Title = "Top Suggestions (ML Forecast)";
        BtcForecast = "Loading ML Models...";
        BtcRank = "Warming up ONNX Runtime...";
    }

    [RelayCommand]
    public async Task RunMlPredictionsAsync()
    {
        try
        {
            IsBusy = true;
            await _mlService.InitializeAsync();

            // Mock Data for Bitcoin (Moving Avg 50d, RSI, 24h Volume)
            float ma50 = 62000f;
            float rsi = 55f;
            float volume = 45000000000f;
            float ma200 = 50000f;
            float volChange = 0.05f;
            float priceChange = 0.02f;

            // Run ONNX Inference locally on the device
            var predictedPrice = _mlService.PredictPriceForecast(ma50, rsi, volume);
            var rankingScore = _mlService.PredictCryptoRanking(rsi, ma50, ma200, volChange, priceChange);

            BtcForecast = $"Linear Regression Forecast: ${predictedPrice:N0} (Next 24h)";
            BtcRank = $"Random Forest Rank: {rankingScore:N0}/100 Score";
        }
        catch (Exception ex)
        {
            BtcForecast = $"Error loading models: {ex.Message}";
            BtcRank = "Ensure .onnx files are marked as MauiAsset";
        }
        finally
        {
            IsBusy = false;
        }
    }
}

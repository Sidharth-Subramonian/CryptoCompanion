using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace CryptoCompanion.Services.ML;

public interface IOnnxInferenceService
{
    Task InitializeAsync();
    float PredictPriceForecast(float movingAverage50d, float rsi, float volume);
    float PredictCryptoRanking(float rsi, float ma50, float ma200, float volumeChangePct, float priceChangePct);
    int PredictPortfolioCluster(float volatility, float avgDailyReturn, float marketCap);
}

public class OnnxInferenceService : IOnnxInferenceService
{
    private InferenceSession? _priceForecastSession;
    private InferenceSession? _cryptoRankingSession;
    private InferenceSession? _portfolioClusterSession;
    private bool _isInitialized;

    public async Task InitializeAsync()
    {
        if (_isInitialized) return;

        _priceForecastSession = await LoadModelAsync("price_forecast.onnx");
        _cryptoRankingSession = await LoadModelAsync("crypto_ranking.onnx");
        _portfolioClusterSession = await LoadModelAsync("portfolio_clustering.onnx");

        _isInitialized = true;
    }

    private async Task<InferenceSession> LoadModelAsync(string modelFileName)
    {
        using var stream = await FileSystem.OpenAppPackageFileAsync(modelFileName);
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        return new InferenceSession(memoryStream.ToArray());
    }

    public float PredictPriceForecast(float movingAverage50d, float rsi, float volume)
    {
        if (_priceForecastSession == null) throw new InvalidOperationException("Models not initialized.");

        var inputTensor = new DenseTensor<float>(
            new[] { movingAverage50d, rsi, volume }, 
            new[] { 1, 3 }); // Batch size 1, 3 features

        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor("float_input", inputTensor)
        };

        using var results = _priceForecastSession.Run(inputs);
        var output = results.First().AsTensor<float>();
        return output.First();
    }

    public float PredictCryptoRanking(float rsi, float ma50, float ma200, float volumeChangePct, float priceChangePct)
    {
         if (_cryptoRankingSession == null) throw new InvalidOperationException("Models not initialized.");

        var inputTensor = new DenseTensor<float>(
            new[] { rsi, ma50, ma200, volumeChangePct, priceChangePct }, 
            new[] { 1, 5 });

        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor("float_input", inputTensor)
        };

        using var results = _cryptoRankingSession.Run(inputs);
        var output = results.First().AsTensor<float>();
        return output.First();
    }
    
    public int PredictPortfolioCluster(float volatility, float avgDailyReturn, float marketCap)
    {
        if (_portfolioClusterSession == null) throw new InvalidOperationException("Models not initialized.");

        var inputTensor = new DenseTensor<float>(
            new[] { volatility, avgDailyReturn, marketCap }, 
            new[] { 1, 3 });

        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor("float_input", inputTensor)
        };

        using var results = _portfolioClusterSession.Run(inputs);
        
        // K-Means output in skl2onnx usually emits two outputs: 'label' and 'scores'
        // Index 0 is the label.
        var output = results.First().AsTensor<long>(); 
        return (int)output.First();
    }
}

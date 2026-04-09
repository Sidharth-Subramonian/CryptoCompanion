using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace CryptoCompanion.Services.ML;

public interface IOnnxInferenceService
{
    Task InitializeAsync();
    float PredictPriceDirection(float ret1, float ma10, float ma20, float ma50, float rsi, float volatility, float volChange);
    float PredictCryptoRanking(float ret1, float ma10, float ma20, float ma50, float rsi, float volatility, float volChange);
    int PredictPortfolioCluster(float volatility, float avgDailyReturn, float marketCap);
    bool PredictAnomaly(float ret1, float volChange);
    bool PredictSentiment(string text);
}

public class OnnxInferenceService : IOnnxInferenceService
{
    private InferenceSession? _priceDirectionSession;
    private InferenceSession? _cryptoRankingSession;
    private InferenceSession? _portfolioClusterSession;
    private InferenceSession? _anomalyDetectorSession;
    private InferenceSession? _sentimentSession;
    private bool _isInitialized;

    public async Task InitializeAsync()
    {
        if (_isInitialized) return;

        _priceDirectionSession = await LoadModelAsync("price_direction.onnx");
        _cryptoRankingSession = await LoadModelAsync("crypto_ranking.onnx");
        _portfolioClusterSession = await LoadModelAsync("portfolio_clustering.onnx");
        _anomalyDetectorSession = await LoadModelAsync("anomaly_detector.onnx");
        _sentimentSession = await LoadModelAsync("sentiment_classifier.onnx");

        _isInitialized = true;
    }

    private async Task<InferenceSession> LoadModelAsync(string modelFileName)
    {
        using var stream = await FileSystem.OpenAppPackageFileAsync(modelFileName);
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        return new InferenceSession(memoryStream.ToArray());
    }

    public float PredictPriceDirection(float ret1, float ma10, float ma20, float ma50, float rsi, float volatility, float volChange)
    {
        if (_priceDirectionSession == null) throw new InvalidOperationException("Models not initialized.");

        var inputTensor = new DenseTensor<float>(
            new[] { ret1, ma10, ma20, ma50, rsi, volatility, volChange }, 
            new[] { 1, 7 });

        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor("float_input", inputTensor)
        };

        using var results = _priceDirectionSession.Run(inputs);
        var output = results.First(r => r.Name == "variable").AsTensor<float>();
        
        return output.First();
    }

    public float PredictCryptoRanking(float ret1, float ma10, float ma20, float ma50, float rsi, float volatility, float volChange)
    {
         if (_cryptoRankingSession == null) throw new InvalidOperationException("Models not initialized.");

        var inputTensor = new DenseTensor<float>(
            new[] { ret1, ma10, ma20, ma50, rsi, volatility, volChange }, 
            new[] { 1, 7 });

        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor("float_input", inputTensor)
        };

        using var results = _cryptoRankingSession.Run(inputs);
        var output = results.First(r => r.Name == "variable").AsTensor<float>();
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
        var output = results.First(r => r.Name == "label").AsTensor<long>(); 
        return (int)output.First();
    }

    public bool PredictAnomaly(float ret1, float volChange)
    {
        if (_anomalyDetectorSession == null) throw new InvalidOperationException("Models not initialized.");

        var inputTensor = new DenseTensor<float>(
            new[] { ret1, volChange }, 
            new[] { 1, 2 });

        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor("float_input", inputTensor)
        };

        using var results = _anomalyDetectorSession.Run(inputs);
        var output = results.First(r => r.Name == "label").AsTensor<long>();
        return output.First() == -1;
    }

    public bool PredictSentiment(string text)
    {
        if (_sentimentSession == null) throw new InvalidOperationException("Models not initialized.");

        var inputTensor = new DenseTensor<string>(
            new[] { text }, 
            new[] { 1, 1 });

        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor("text_input", inputTensor)
        };

        using var results = _sentimentSession.Run(inputs);
        var output = results.First(r => r.Name == "label").AsTensor<long>();
        return output.First() == 1;
    }
}

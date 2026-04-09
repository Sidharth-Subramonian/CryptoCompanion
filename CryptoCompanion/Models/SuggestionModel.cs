namespace CryptoCompanion.Models;

public class SuggestionModel
{
    public string Name { get; set; } = string.Empty;
    public string Forecast { get; set; } = string.Empty;
    public string Rank { get; set; } = string.Empty;
    public string BackgroundKey { get; set; } = "PrimaryBrush";

    // Enhanced prediction details
    public string Signal { get; set; } = "Neutral";       // "Bullish" / "Bearish" / "Neutral"
    public string Timeline { get; set; } = "24h Forecast";
    public string Reasoning { get; set; } = string.Empty;  // Human-readable explanation
    public decimal CurrentPrice { get; set; }
    public decimal PredictedPrice { get; set; }
    public decimal PercentDiff { get; set; }               // Predicted vs current %
    public float RankingScore { get; set; }
}

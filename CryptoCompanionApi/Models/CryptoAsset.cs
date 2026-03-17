namespace CryptoCompanionApi.Models;

public class CryptoAsset
{
    public int Id { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal CurrentPrice { get; set; }
    public decimal MarketCap { get; set; }
    public decimal Volume24h { get; set; }
    public decimal PercentChange24h { get; set; }
    public DateTime LastUpdated { get; set; }
    
    // ML Specific features
    public decimal RSIScore { get; set; }
    public decimal MovingAverage50d { get; set; }
    public decimal MovingAverage200d { get; set; }
}

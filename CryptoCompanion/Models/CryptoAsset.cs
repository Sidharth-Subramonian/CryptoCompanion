using System.Text.Json.Serialization;

namespace CryptoCompanion.Models;

public class CryptoAsset
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("symbol")]
    public string Symbol { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("currentPrice")]
    public decimal CurrentPrice { get; set; }

    [JsonPropertyName("marketCap")]
    public decimal MarketCap { get; set; }

    [JsonPropertyName("volume24h")]
    public decimal Volume24h { get; set; }

    [JsonPropertyName("percentChange24h")]
    public decimal PercentChange24h { get; set; }

    [JsonPropertyName("lastUpdated")]
    public DateTime LastUpdated { get; set; }

    [JsonPropertyName("rsiScore")]
    public decimal RSIScore { get; set; }

    [JsonPropertyName("movingAverage50d")]
    public decimal MovingAverage50d { get; set; }

    [JsonPropertyName("movingAverage200d")]
    public decimal MovingAverage200d { get; set; }
}

using System.Text.Json.Serialization;

namespace CryptoCompanionApi.Models;

public class SocialSentiment
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [JsonPropertyName("platform")]
    public string Platform { get; set; } = string.Empty; // Twitter, Reddit
    
    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;
    
    [JsonPropertyName("author")]
    public string Author { get; set; } = string.Empty;
    
    [JsonPropertyName("postedAt")]
    public DateTime PostedAt { get; set; }
    
    [JsonPropertyName("coinSymbol")]
    public string CoinSymbol { get; set; } = string.Empty;
    
    // ML Classification
    [JsonPropertyName("sentimentClass")]
    public string SentimentClass { get; set; } = "Neutral"; // Bullish, Bearish, Neutral
    
    [JsonPropertyName("confidence")]
    public double Confidence { get; set; }
}

using System.Text.Json.Serialization;

namespace CryptoCompanion.Models;

public class SocialSentiment
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("platform")]
    public string Platform { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    [JsonPropertyName("author")]
    public string Author { get; set; } = string.Empty;

    [JsonPropertyName("postedAt")]
    public DateTime PostedAt { get; set; }

    [JsonPropertyName("coinSymbol")]
    public string CoinSymbol { get; set; } = string.Empty;

    [JsonPropertyName("sentimentClass")]
    public string SentimentClass { get; set; } = "Neutral";

    [JsonPropertyName("confidence")]
    public double Confidence { get; set; }
}

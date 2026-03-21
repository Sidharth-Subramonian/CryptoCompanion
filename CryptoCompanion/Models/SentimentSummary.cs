using System.Text.Json.Serialization;

namespace CryptoCompanion.Models;

public class SentimentSummary
{
    [JsonPropertyName("bullishPercent")]
    public double BullishPercent { get; set; }

    [JsonPropertyName("bearishPercent")]
    public double BearishPercent { get; set; }

    [JsonPropertyName("neutralPercent")]
    public double NeutralPercent { get; set; }

    [JsonPropertyName("totalPosts")]
    public int TotalPosts { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = "Neutral";
}

using System.Text.Json.Serialization;

namespace CryptoCompanionApi.Models;

public class NewsArticle
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;
    
    [JsonPropertyName("summary")]
    public string Summary { get; set; } = string.Empty;
    
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;
    
    [JsonPropertyName("source")]
    public string Source { get; set; } = string.Empty;
    
    [JsonPropertyName("publishedAt")]
    public DateTime PublishedAt { get; set; }
    
    [JsonPropertyName("relatedCoins")]
    public List<string> RelatedCoins { get; set; } = new();
    
    // ML Classification
    [JsonPropertyName("relevanceScore")]
    public double RelevanceScore { get; set; }
    
    [JsonPropertyName("sentimentScore")]
    public double SentimentScore { get; set; } // -1 (Negative) to 1 (Positive)
    
    [JsonPropertyName("sentimentLabel")]
    public string SentimentLabel { get; set; } = "Neutral";
}

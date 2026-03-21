using System.Text.Json.Serialization;

namespace CryptoCompanion.Models;

public class AlertItem
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("time")]
    public string Time { get; set; } = string.Empty;

    [JsonPropertyName("severityPath")]
    public string SeverityPath { get; set; } = string.Empty;

    [JsonPropertyName("algorithm")]
    public string Algorithm { get; set; } = string.Empty;
}

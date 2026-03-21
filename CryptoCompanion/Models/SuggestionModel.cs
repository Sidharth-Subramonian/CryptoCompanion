namespace CryptoCompanion.Models;

public class SuggestionModel
{
    public string Name { get; set; } = string.Empty;
    public string Forecast { get; set; } = string.Empty;
    public string Rank { get; set; } = string.Empty;
    public string BackgroundKey { get; set; } = "PrimaryBrush";
}

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CryptoCompanion.ViewModels;

public partial class AlertsViewModel : BaseViewModel
{
    [ObservableProperty]
    private ObservableCollection<AlertItem> _activeAlerts = new();

    public AlertsViewModel()
    {
        Title = "Price & Volume Alerts";
        LoadMockAlerts();
    }

    private void LoadMockAlerts()
    {
        ActiveAlerts.Add(new AlertItem 
        { 
            Title = "Unusual Volume Detected", 
            Message = "DOGE trading volume spiked 400% in the last 15 minutes.",
            Time = "10 mins ago",
            SeverityPath = "high", // Could map to color in UI
            Algorithm = "Isolation Forest"
        });
        
        ActiveAlerts.Add(new AlertItem 
        { 
            Title = "Price Drop Anomaly", 
            Message = "ETH dropped standard deviation limits (-4%) in a single 5m candle.",
            Time = "1 hour ago",
            SeverityPath = "critical",
            Algorithm = "One-Class SVM"
        });
    }
}

public class AlertItem
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty;
    public string SeverityPath { get; set; } = string.Empty;
    public string Algorithm { get; set; } = string.Empty;
}

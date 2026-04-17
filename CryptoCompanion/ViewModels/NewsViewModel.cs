using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CryptoCompanion.Services.Api;
using CryptoCompanion.Models; // Ensure this points to your MAUI NewsArticle model
using System.Linq;

namespace CryptoCompanion.ViewModels;

public partial class NewsViewModel : BaseViewModel
{
    private readonly IBackendApiService _apiService;
    
    // CHANGE 1: Use the specific Model instead of 'object'
    [ObservableProperty]
    private ObservableCollection<NewsArticle> _newsArticles = new();

    public NewsViewModel(IBackendApiService apiService)
    {
        _apiService = apiService;
        Title = "News Dashboard";
        
        // Auto-load on init
        Task.Run(LoadNewsAsync);
    }
    
    [RelayCommand]
    public async Task LoadNewsAsync()
    {
        if (IsBusy) return;
        
        try
        {
            IsBusy = true;
            
            // CHANGE 2: Ensure your Service returns Task<List<NewsArticle>>
            var news = await _apiService.GetLatestNewsAsync(); 
            
            if (news != null && news.Any())
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    NewsArticles.Clear();
                    foreach (var article in news)
                    {
                        if (string.IsNullOrEmpty(article.SentimentLabel) || article.SentimentLabel == "Neutral")
                        {
                            article.SentimentLabel = AnalyzeSentiment(article.Title + " " + article.Summary);
                        }
                        NewsArticles.Add(article);
                    }
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ViewModel Error: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task OpenUrl(string url)
    {
        if (!string.IsNullOrWhiteSpace(url))
        {
            await Launcher.Default.OpenAsync(new Uri(url));
        }
    }

    private static readonly string[] BullishKeywords = { "bullish", "surge", "rally", "moon", "breakout", "gains", "soar", "rise", "up", "high", "record", "growth", "positive", "buy", "profit", "boost" };
    private static readonly string[] BearishKeywords = { "bearish", "crash", "dump", "plunge", "sell", "drop", "down", "low", "fall", "decline", "loss", "negative", "fear", "risk", "warning", "collapse" };

    private static string AnalyzeSentiment(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return "Neutral";
        
        var lowerText = text.ToLowerInvariant();
        
        int bullishCount = BullishKeywords.Count(k => lowerText.Contains(k));
        int bearishCount = BearishKeywords.Count(k => lowerText.Contains(k));
        int totalHits = bullishCount + bearishCount;

        if (totalHits == 0) return "Neutral";
        if (bullishCount > bearishCount) return "Bullish";
        if (bearishCount > bullishCount) return "Bearish";
        
        return "Neutral";
    }
}
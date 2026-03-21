using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CryptoCompanion.Models;
using CryptoCompanion.Services.Api;

namespace CryptoCompanion.ViewModels;

public partial class SentimentViewModel : BaseViewModel
{
    private readonly IBackendApiService _apiService;

    [ObservableProperty]
    private double _bullishPercent;

    [ObservableProperty]
    private double _bearishPercent;

    [ObservableProperty]
    private double _neutralPercent;

    [ObservableProperty]
    private string _sentimentStatus = "Loading...";

    [ObservableProperty]
    private int _totalPosts;

    [ObservableProperty]
    private ObservableCollection<SocialSentiment> _sentimentPosts = new();

    public SentimentViewModel(IBackendApiService apiService)
    {
        _apiService = apiService;
        Title = "Market Sentiment";
        Task.Run(LoadSentimentAsync);
    }

    [RelayCommand]
    public async Task LoadSentimentAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;

            // Fetch summary
            var summary = await _apiService.GetSentimentSummaryAsync();
            if (summary != null)
            {
                BullishPercent = summary.BullishPercent;
                BearishPercent = summary.BearishPercent;
                NeutralPercent = summary.NeutralPercent;
                TotalPosts = summary.TotalPosts;
                SentimentStatus = summary.TotalPosts > 0
                    ? $"Current Status: {summary.Status} ({summary.BullishPercent * 100:F0}% Bullish)"
                    : "No sentiment data yet. Waiting for analysis...";
            }

            // Fetch individual posts
            var posts = await _apiService.GetSentimentAsync();
            if (posts != null && posts.Any())
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    SentimentPosts.Clear();
                    foreach (var post in posts.Take(20))
                    {
                        SentimentPosts.Add(post);
                    }
                });
            }
        }
        catch (Exception ex)
        {
            SentimentStatus = $"Error: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CryptoCompanion.Services.Api;
using CryptoCompanion.Models; // Ensure this points to your MAUI NewsArticle model

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
}
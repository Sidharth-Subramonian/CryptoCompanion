using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CryptoCompanion.Services.Api;

namespace CryptoCompanion.ViewModels;

public partial class NewsViewModel : BaseViewModel
{
    private readonly IBackendApiService _apiService;
    
    [ObservableProperty]
    private ObservableCollection<object> _newsArticles = new();

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
            var news = await _apiService.GetLatestNewsAsync();
            if (news != null)
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
        finally
        {
            IsBusy = false;
        }
    }
}

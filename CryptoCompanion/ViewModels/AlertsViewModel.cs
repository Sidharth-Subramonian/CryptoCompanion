using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CryptoCompanion.Models;
using CryptoCompanion.Services.Api;

namespace CryptoCompanion.ViewModels;

public partial class AlertsViewModel : BaseViewModel
{
    private readonly IBackendApiService _apiService;
    private readonly PortfolioViewModel _portfolioViewModel;

    [ObservableProperty]
    private ObservableCollection<AlertItem> _activeAlerts = new();

    public AlertsViewModel(IBackendApiService apiService, PortfolioViewModel portfolioViewModel)
    {
        _apiService = apiService;
        _portfolioViewModel = portfolioViewModel;
        Title = "Price & Volume Alerts";
        Task.Run(LoadAlertsAsync);
    }

    [RelayCommand]
    public async Task LoadAlertsAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;

            var alerts = await _apiService.GetAlertsAsync();

            if (alerts != null && alerts.Any())
            {
                var portfolioSymbols = _portfolioViewModel.Assets.Select(a => a.Symbol).ToList();
                
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ActiveAlerts.Clear();
                    foreach (var alert in alerts)
                    {
                        if (portfolioSymbols.Any(s => alert.Message.Contains(s)))
                        {
                            alert.Title = $"⭐ PORTFOLIO ALERT: {alert.Title}";
                            ActiveAlerts.Insert(0, alert);
                        }
                        else
                        {
                            ActiveAlerts.Add(alert);
                        }
                    }
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Alerts Error: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }
}

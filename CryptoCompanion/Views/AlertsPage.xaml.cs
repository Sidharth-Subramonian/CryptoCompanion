using CryptoCompanion.ViewModels;

namespace CryptoCompanion.Views;

public partial class AlertsPage : ContentPage
{
    public AlertsPage(AlertsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}

using CryptoCompanion.ViewModels;

namespace CryptoCompanion.Views;

public partial class NewsPage : ContentPage
{
    public NewsPage(NewsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private async void OnReadArticleTapped(object sender, EventArgs e)
    {
        if (sender is Label label &&
            label.GestureRecognizers.FirstOrDefault() is TapGestureRecognizer tap &&
            tap.CommandParameter is string url &&
            !string.IsNullOrWhiteSpace(url))
        {
            await Launcher.Default.OpenAsync(new Uri(url));
        }
    }
}

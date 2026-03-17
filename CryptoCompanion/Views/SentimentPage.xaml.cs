using CryptoCompanion.ViewModels;

namespace CryptoCompanion.Views;

public partial class SentimentPage : ContentPage
{
    public SentimentPage(SentimentViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}

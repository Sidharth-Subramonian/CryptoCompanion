using CryptoCompanion.ViewModels;

namespace CryptoCompanion.Views;

public partial class NewsPage : ContentPage
{
    public NewsPage(NewsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}

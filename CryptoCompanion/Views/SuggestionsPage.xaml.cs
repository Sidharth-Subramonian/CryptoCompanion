using CryptoCompanion.ViewModels;

namespace CryptoCompanion.Views;

public partial class SuggestionsPage : ContentPage
{
    public SuggestionsPage(SuggestionsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}

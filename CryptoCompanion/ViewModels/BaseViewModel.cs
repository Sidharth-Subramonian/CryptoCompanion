using CommunityToolkit.Mvvm.ComponentModel;

namespace CryptoCompanion.ViewModels;

public partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _title = string.Empty;
    
    public bool IsNotBusy => !IsBusy;
}

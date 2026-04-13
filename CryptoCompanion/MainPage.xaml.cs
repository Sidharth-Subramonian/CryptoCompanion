namespace CryptoCompanion;

public partial class MainPage : ContentPage
{
	int count = 0;

	public MainPage()
	{
		InitializeComponent();
	}

	private void OnCounterClicked(object? sender, EventArgs e)
	{
		count++;

		if (count == 1)
			EnterBtn.Text = $"Clicked {count} time";
		else
			EnterBtn.Text = $"Clicked {count} times";

		SemanticScreenReader.Announce(EnterBtn.Text);
	}
}

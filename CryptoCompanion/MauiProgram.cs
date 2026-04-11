using CryptoCompanion.Services.Api;
using CryptoCompanion.Services.ML;
using CryptoCompanion.ViewModels;
using CryptoCompanion.Views;
using Microsoft.Extensions.Logging;

namespace CryptoCompanion;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

        // Services
        builder.Services.AddSingleton<IOnnxInferenceService, OnnxInferenceService>();
        
        // Use AddHttpClient to configure the typed client
        builder.Services.AddHttpClient<IBackendApiService, BackendApiService>();

        // ViewModels
        builder.Services.AddTransient<SuggestionsViewModel>();
        builder.Services.AddTransient<NewsViewModel>();
        builder.Services.AddTransient<SentimentViewModel>();
        builder.Services.AddSingleton<PortfolioViewModel>();
        builder.Services.AddSingleton<AlertsViewModel>();

        // Views
        builder.Services.AddTransient<SuggestionsPage>();
        builder.Services.AddTransient<NewsPage>();
        builder.Services.AddTransient<SentimentPage>();
        builder.Services.AddTransient<PortfolioPage>();
        builder.Services.AddTransient<AlertsPage>();

		return builder.Build();
	}
}

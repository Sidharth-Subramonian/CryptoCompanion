using System.Net.Http.Json;

namespace CryptoCompanion.Services.Api;

public interface IBackendApiService
{
    Task<List<object>?> GetSuggestionsAsync();
    Task<List<object>?> GetLatestNewsAsync();
}

public class BackendApiService : IBackendApiService
{
    private readonly HttpClient _httpClient;

    // In a real app, this would be your Azure App Service URL, e.g., https://cryptocompanion-api.azurewebsites.net
    // For local development on Android emulator accessing host locahost:
    // private const string BaseUrl = "http://10.0.2.2:5000";
    private const string BaseUrl = "https://localhost:5001"; 

    public BackendApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(BaseUrl);
    }

    public async Task<List<object>?> GetSuggestionsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/suggestions");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<object>>();
            }
        }
        catch (Exception ex)
        {
            // Log exception
            System.Diagnostics.Debug.WriteLine($"API Error: {ex.Message}");
        }
        return new List<object>();
    }

    public async Task<List<object>?> GetLatestNewsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/news");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<object>>();
            }
        }
        catch (Exception ex)
        {
            // Log exception
            System.Diagnostics.Debug.WriteLine($"API Error: {ex.Message}");
        }
        return new List<object>();
    }
}

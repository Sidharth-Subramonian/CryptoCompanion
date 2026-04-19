using System.Net.Http.Json;
using CryptoCompanion.Models;
using System.Diagnostics;

namespace CryptoCompanion.Services.Api;

public interface IBackendApiService
{
    Task<List<CryptoAsset>?> GetSuggestionsAsync();
    Task<List<NewsArticle>?> GetLatestNewsAsync();
    Task<List<SocialSentiment>?> GetSentimentAsync();
    Task<SentimentSummary?> GetSentimentSummaryAsync();
    Task<List<AlertItem>?> GetAlertsAsync();
}

public class BackendApiService : IBackendApiService
{
    private readonly HttpClient _httpClient;
    // Azure App Service endpoint (used previously)
    // private const string BaseUrl = "https://cryptocompanion-api-2101242956.azurewebsites.net"; 
    
    // Kubernetes (AKS) LoadBalancer External IP 
    // TODO: Replace with actual IP from `kubectl get services` after deploying to AKS.
    private const string BaseUrl = "http://40.88.237.159"; 

    public BackendApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(BaseUrl);
    }

    public async Task<List<CryptoAsset>?> GetSuggestionsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/suggestions");
            if (response.IsSuccessStatusCode)
            {
                // API returns a wrapper object: { Assets: [...], MarketIntelligence: "...", ReviewNote: "..." }
                var result = await response.Content.ReadFromJsonAsync<SuggestionsApiResponse>();
                return result?.Assets ?? new List<CryptoAsset>();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"API Error (Suggestions): {ex.Message}");
        }
        return new List<CryptoAsset>();
    }

    // Matches the shape returned by /api/suggestions
    private class SuggestionsApiResponse
    {
        public List<CryptoAsset>? Assets { get; set; }
        public string? MarketIntelligence { get; set; }
        public string? ReviewNote { get; set; }
    }

    public async Task<List<NewsArticle>?> GetLatestNewsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/news");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<NewsArticle>>();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"API Error (News): {ex.Message}");
        }
        return new List<NewsArticle>();
    }

    public async Task<List<SocialSentiment>?> GetSentimentAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/sentiment");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<SocialSentiment>>();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"API Error (Sentiment): {ex.Message}");
        }
        return new List<SocialSentiment>();
    }

    public async Task<SentimentSummary?> GetSentimentSummaryAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/sentiment/summary");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<SentimentSummary>();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"API Error (Sentiment Summary): {ex.Message}");
        }
        return new SentimentSummary();
    }

    public async Task<List<AlertItem>?> GetAlertsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/alerts");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<AlertItem>>();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"API Error (Alerts): {ex.Message}");
        }
        return new List<AlertItem>();
    }
}
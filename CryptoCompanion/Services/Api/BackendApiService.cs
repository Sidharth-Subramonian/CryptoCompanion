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
    private const string BaseUrl = "http://localhost:5237"; 

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
                return await response.Content.ReadFromJsonAsync<List<CryptoAsset>>();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"API Error (Suggestions): {ex.Message}");
        }
        return new List<CryptoAsset>();
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
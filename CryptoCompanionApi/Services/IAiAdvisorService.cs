using CryptoCompanionApi.Models;

namespace CryptoCompanionApi.Services;

public interface IAiAdvisorService
{
    Task<string> GetMarketIntelligenceAsync(List<CryptoAsset> topAssets, List<NewsArticle> latestNews);
}

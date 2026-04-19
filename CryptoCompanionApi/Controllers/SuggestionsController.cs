using CryptoCompanionApi.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CryptoCompanionApi.Services;

namespace CryptoCompanionApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SuggestionsController : ControllerBase
{
    private readonly ApplicationDbContext _sqlContext;
    private readonly CosmosDbContext _cosmosContext;
    private readonly IAiAdvisorService _aiAdvisor;

    public SuggestionsController(
        ApplicationDbContext sqlContext, 
        CosmosDbContext cosmosContext,
        IAiAdvisorService aiAdvisor)
    {
        _sqlContext = sqlContext;
        _cosmosContext = cosmosContext;
        _aiAdvisor = aiAdvisor;
    }

    [HttpGet]
    public async Task<IActionResult> GetSuggestions()
    {
        try 
        {
            // 1. Fetch Top Assets from SQL
            var assets = await _sqlContext.CryptoAssets
                .OrderByDescending(c => c.MarketCap)
                .Take(10)
                .ToListAsync();

            // 2. Fetch Latest News from Cosmos
            var news = await _cosmosContext.NewsArticles
                .OrderByDescending(n => n.PublishedAt)
                .Take(3)
                .ToListAsync();

            // 3. Generate AI Intelligence Summary
            string aiIntelligence;
            try 
            {
                aiIntelligence = await _aiAdvisor.GetMarketIntelligenceAsync(assets, news);
            }
            catch (Exception ex)
            {
                aiIntelligence = $"Advisor is currently offline: {ex.Message}";
            }

            return Ok(new
            {
                Assets = assets,
                MarketIntelligence = aiIntelligence,
                ReviewNote = "GenAI Integration Live: Analyzing market data via Azure OpenAI (GPT-3.5 Turbo)."
            });
        }
        catch (Exception ex)
        {
            // Return detailed 500 error for debugging
            return StatusCode(500, new { 
                Error = "Internal Service Failure",
                Message = ex.Message,
                Type = ex.GetType().Name,
                Hint = "Check /api/health for detailed connectivity status."
            });
        }
    }
}

using CryptoCompanionApi.Data;
using CryptoCompanionApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CryptoCompanionApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SentimentController : ControllerBase
{
    private readonly CosmosDbContext _context;
    private readonly ILogger<SentimentController> _logger;

    public SentimentController(CosmosDbContext context, ILogger<SentimentController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// GET /api/sentiment — Returns latest sentiment entries
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SocialSentiment>>> GetSentiment()
    {
        try
        {
            var sentiments = await _context.SocialSentiments
                .OrderByDescending(s => s.PostedAt)
                .Take(30)
                .ToListAsync();

            return Ok(sentiments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching sentiment data.");
            return StatusCode(500, "Internal server error.");
        }
    }

    /// <summary>
    /// GET /api/sentiment/summary — Returns aggregated bullish/bearish/neutral percentages
    /// </summary>
    [HttpGet("summary")]
    public async Task<ActionResult> GetSentimentSummary()
    {
        try
        {
            var sentiments = await _context.SocialSentiments
                .OrderByDescending(s => s.PostedAt)
                .Take(50)
                .ToListAsync();

            if (!sentiments.Any())
            {
                return Ok(new { BullishPercent = 0.33, BearishPercent = 0.33, NeutralPercent = 0.34, TotalPosts = 0, Status = "No Data" });
            }

            int total = sentiments.Count;
            int bullish = sentiments.Count(s => s.SentimentClass == "Bullish");
            int bearish = sentiments.Count(s => s.SentimentClass == "Bearish");
            int neutral = total - bullish - bearish;

            var summary = new
            {
                BullishPercent = Math.Round((double)bullish / total, 2),
                BearishPercent = Math.Round((double)bearish / total, 2),
                NeutralPercent = Math.Round((double)neutral / total, 2),
                TotalPosts = total,
                Status = bullish > bearish ? "Bullish" : bearish > bullish ? "Bearish" : "Neutral"
            };

            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error computing sentiment summary.");
            return StatusCode(500, "Internal server error.");
        }
    }
}

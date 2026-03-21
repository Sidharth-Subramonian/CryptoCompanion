using CryptoCompanionApi.Data;
using CryptoCompanionApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CryptoCompanionApi.Controllers;

[ApiController]
[Route("api/[controller]")] // This makes the URL: https://localhost:5001/api/news
public class NewsController : ControllerBase
{
    private readonly CosmosDbContext _context;
    private readonly ILogger<NewsController> _logger;

    public NewsController(CosmosDbContext context, ILogger<NewsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<NewsArticle>>> GetNews()
    {
        try
        {
            // Query the 'News' container in Cosmos DB
            // We take the top 20 latest articles, ordered by date
            var news = await _context.NewsArticles
                .OrderByDescending(a => a.PublishedAt)
                .Take(20)
                .ToListAsync();

            if (news == null || !news.Any())
            {
                return NotFound("No news articles found in the database.");
            }

            return Ok(news);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching news.");
            return StatusCode(500, "Internal server error while retrieving news.");
        }
    }
}
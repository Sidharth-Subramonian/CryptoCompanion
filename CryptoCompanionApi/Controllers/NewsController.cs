using CryptoCompanionApi.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CryptoCompanionApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NewsController : ControllerBase
{
    private readonly CosmosDbContext _context;

    public NewsController(CosmosDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetLatestNews()
    {
        // Cosmos DB Entity Framework implementation relies heavily on LINQ translations
        var news = await _context.NewsArticles
            .OrderByDescending(n => n.PublishedAt)
            .Take(20)
            .ToListAsync();

        return Ok(news);
    }
}

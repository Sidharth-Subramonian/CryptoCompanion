using CryptoCompanionApi.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CryptoCompanionApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SuggestionsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public SuggestionsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetSuggestions()
    {
        // In a real scenario, this would apply actual ML models or return the pre-calculated features
        // for the MAUI app to run inference on. Either way, we return the base asset data from SQL.
        
        var assets = await _context.CryptoAssets
            .OrderByDescending(c => c.MarketCap)
            .Take(50)
            .ToListAsync();

        return Ok(assets);
    }
}

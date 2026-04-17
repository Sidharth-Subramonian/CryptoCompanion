using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CryptoCompanionApi.Data;

namespace CryptoCompanionApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SecurityDemoController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SecurityDemoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- VULNERABLE ENDPOINT (For Review III Demo) ---
        // This endpoint is INTENTIONALLY VULNERABLE to SQL Injection.
        // It uses string concatenation instead of parameterized queries.
        [HttpGet("vulnerable-search")]
        public async Task<IActionResult> VulnerableSearch(string coinSymbol)
        {
            // BAD PRACTICE: String concatenation in raw SQL
            // This is easily exploited by passing: BTC' OR '1'='1
            string query = "SELECT * FROM CryptoAssets WHERE Symbol = '" + coinSymbol + "'";
            
            // Execute the raw vulnerable query - Explictly use Relational provider to avoid ambiguity with Cosmos
            var results = await RelationalQueryableExtensions.FromSqlRaw(_context.CryptoAssets, query)
                .ToListAsync();
            
            return Ok(results);
        }

        // --- SECURE ENDPOINT (The Fix) ---
        // This is how the endpoint SHOULD be written to prevent SQL Injection.
        [HttpGet("secure-search")]
        public async Task<IActionResult> SecureSearch(string coinSymbol)
        {
            // GOOD PRACTICE: Using Parameterized queries
            // EF Core handles the escaping for us.
            // Explictly use Relational provider to avoid ambiguity with Cosmos
            var results = await RelationalQueryableExtensions.FromSqlRaw(_context.CryptoAssets, "SELECT * FROM CryptoAssets WHERE Symbol = {0}", coinSymbol)
                .ToListAsync();
            
            return Ok(results);
        }
    }
}

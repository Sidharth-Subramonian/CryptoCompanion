using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CryptoCompanionApi.Data;
using CryptoCompanionApi.Services;

namespace CryptoCompanionApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly ApplicationDbContext _sqlContext;
    private readonly CosmosDbContext _cosmosContext;
    private readonly IConfiguration _config;

    public HealthController(
        ApplicationDbContext sqlContext, 
        CosmosDbContext cosmosContext,
        IConfiguration config)
    {
        _sqlContext = sqlContext;
        _cosmosContext = cosmosContext;
        _config = config;
    }

    [HttpGet]
    public async Task<IActionResult> CheckHealth()
    {
        var report = new Dictionary<string, object>();

        // 1. SQL Check (Verbose Exception)
        try
        {
            await _sqlContext.Database.OpenConnectionAsync();
            report["SQL"] = "Online";
            await _sqlContext.Database.CloseConnectionAsync();
        }
        catch (Exception ex)
        {
            report["SQL"] = new {
                Status = "Failed",
                Error = ex.Message,
                Inner = ex.InnerException?.Message,
                Advice = "If this says 'Login failed' or 'Forbidden', check SQL Firewall Exceptions (Allow Azure Services)."
            };
        }

        // 2. Cosmos Check (Verbose Exception)
        try
        {
            await _cosmosContext.NewsArticles.FirstOrDefaultAsync();
            report["CosmosDB"] = "Online";
        }
        catch (Exception ex)
        {
            report["CosmosDB"] = new {
                Status = "Failed",
                Error = ex.Message,
                Advice = "401 means the Primary Key is wrong OR the endpoint URL has an extra trailing slash."
            };
        }

        // 3. Configuration Metadata
        report["Discovery"] = new {
            SqlKeyPresent = !string.IsNullOrEmpty(_config.GetConnectionString("DefaultConnection")),
            CosmosKeyPresent = !string.IsNullOrEmpty(_config.GetConnectionString("CosmosDbAccountKey")),
            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
        };

        return Ok(report);
    }
}

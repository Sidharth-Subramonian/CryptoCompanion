using CryptoCompanionApi.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CryptoCompanionApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AlertsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AlertsController> _logger;

    public AlertsController(ApplicationDbContext context, ILogger<AlertsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// GET /api/alerts — Dynamically generates alerts from crypto asset anomalies
    /// </summary>
    [HttpGet]
    public async Task<ActionResult> GetAlerts()
    {
        try
        {
            var assets = await _context.CryptoAssets.ToListAsync();

            if (!assets.Any())
            {
                return Ok(Array.Empty<object>());
            }

            var alerts = new List<object>();

            foreach (var asset in assets)
            {
                // Large price drop alert
                if (asset.PercentChange24h < -5)
                {
                    alerts.Add(new
                    {
                        title = $"Price Drop Alert: {asset.Symbol}",
                        message = $"{asset.Name} dropped {asset.PercentChange24h:F1}% in the last 24 hours. Current price: ₹{asset.CurrentPrice:N2}",
                        time = GetRelativeTime(asset.LastUpdated),
                        severityPath = "critical",
                        algorithm = "Threshold Detection"
                    });
                }
                // Large price surge alert
                else if (asset.PercentChange24h > 5)
                {
                    alerts.Add(new
                    {
                        title = $"Price Surge: {asset.Symbol}",
                        message = $"{asset.Name} surged +{asset.PercentChange24h:F1}% in the last 24 hours. Current price: ₹{asset.CurrentPrice:N2}",
                        time = GetRelativeTime(asset.LastUpdated),
                        severityPath = "high",
                        algorithm = "Threshold Detection"
                    });
                }

                // High volume alert (volume > 2x market cap ratio indicates unusual activity)
                if (asset.MarketCap > 0 && (asset.Volume24h / asset.MarketCap) > 0.15m)
                {
                    alerts.Add(new
                    {
                        title = $"Unusual Volume: {asset.Symbol}",
                        message = $"{asset.Name} trading volume (₹{asset.Volume24h:N0}) is unusually high relative to market cap.",
                        time = GetRelativeTime(asset.LastUpdated),
                        severityPath = "high",
                        algorithm = "Volume Anomaly"
                    });
                }

                // Moderate price movement alert
                if (asset.PercentChange24h is < -3 and >= -5)
                {
                    alerts.Add(new
                    {
                        title = $"Moderate Decline: {asset.Symbol}",
                        message = $"{asset.Name} declined {asset.PercentChange24h:F1}% today. Watch for support levels.",
                        time = GetRelativeTime(asset.LastUpdated),
                        severityPath = "medium",
                        algorithm = "Trend Monitor"
                    });
                }
            }

            // Sort by severity
            return Ok(alerts.OrderByDescending(a => 
            {
                var severity = (string)a.GetType().GetProperty("severityPath")!.GetValue(a)!;
                return severity switch { "critical" => 3, "high" => 2, "medium" => 1, _ => 0 };
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating alerts.");
            return StatusCode(500, "Internal server error.");
        }
    }

    private static string GetRelativeTime(DateTime utcTime)
    {
        var diff = DateTime.UtcNow - utcTime;
        if (diff.TotalMinutes < 1) return "Just now";
        if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes} mins ago";
        if (diff.TotalHours < 24) return $"{(int)diff.TotalHours} hours ago";
        return $"{(int)diff.TotalDays} days ago";
    }
}

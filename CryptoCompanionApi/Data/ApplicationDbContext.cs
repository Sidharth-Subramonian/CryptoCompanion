using CryptoCompanionApi.Models;
using Microsoft.EntityFrameworkCore;

namespace CryptoCompanionApi.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<CryptoAsset> CryptoAssets { get; set; }
    public DbSet<Portfolio> Portfolios { get; set; }
    public DbSet<PortfolioItem> PortfolioItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<CryptoAsset>()
            .Property(c => c.CurrentPrice)
            .HasPrecision(18, 8);
            
        modelBuilder.Entity<CryptoAsset>()
            .Property(c => c.MarketCap)
            .HasPrecision(18, 2);
            
        modelBuilder.Entity<CryptoAsset>()
            .Property(c => c.Volume24h)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Portfolio>()
            .Property(p => p.TotalValue)
            .HasPrecision(18, 2);

        modelBuilder.Entity<PortfolioItem>()
            .Property(pi => pi.AmountOwned)
            .HasPrecision(18, 8);

        modelBuilder.Entity<PortfolioItem>()
            .Property(pi => pi.PurchasePrice)
            .HasPrecision(18, 8);
    }
}

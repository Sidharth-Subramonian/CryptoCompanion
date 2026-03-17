using CryptoCompanionApi.Models;
using Microsoft.EntityFrameworkCore;

namespace CryptoCompanionApi.Data;

public class CosmosDbContext : DbContext
{
    public CosmosDbContext(DbContextOptions<CosmosDbContext> options)
        : base(options)
    {
    }

    public DbSet<NewsArticle> NewsArticles { get; set; }
    public DbSet<SocialSentiment> SocialSentiments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<NewsArticle>()
            .ToContainer("NewsArticles")
            .HasPartitionKey(n => n.Source)
            .HasNoDiscriminator();

        modelBuilder.Entity<SocialSentiment>()
            .ToContainer("SocialSentiments")
            .HasPartitionKey(s => s.Platform)
            .HasNoDiscriminator();
    }
}

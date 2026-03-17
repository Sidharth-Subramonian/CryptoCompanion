namespace CryptoCompanionApi.Models;

public class Portfolio
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty; // From Azure AD B2C
    public string Name { get; set; } = string.Empty;
    public decimal TotalValue { get; set; }
    public ICollection<PortfolioItem> Items { get; set; } = new List<PortfolioItem>();
    
    // ML derived risk scores
    public decimal OverallRiskScore { get; set; }
    public string RiskCluster { get; set; } = "Moderate";
}

public class PortfolioItem
{
    public int Id { get; set; }
    public int PortfolioId { get; set; }
    public Portfolio? Portfolio { get; set; }
    public int CryptoAssetId { get; set; }
    public CryptoAsset? CryptoAsset { get; set; }
    public decimal AmountOwned { get; set; }
    public decimal PurchasePrice { get; set; }
}

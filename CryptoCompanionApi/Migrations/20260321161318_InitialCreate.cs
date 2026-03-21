using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CryptoCompanionApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CryptoAssets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Symbol = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CurrentPrice = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    MarketCap = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Volume24h = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PercentChange24h = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RSIScore = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MovingAverage50d = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MovingAverage200d = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CryptoAssets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Portfolios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TotalValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    OverallRiskScore = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RiskCluster = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Portfolios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PortfolioItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PortfolioId = table.Column<int>(type: "int", nullable: false),
                    CryptoAssetId = table.Column<int>(type: "int", nullable: false),
                    AmountOwned = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    PurchasePrice = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PortfolioItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PortfolioItems_CryptoAssets_CryptoAssetId",
                        column: x => x.CryptoAssetId,
                        principalTable: "CryptoAssets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PortfolioItems_Portfolios_PortfolioId",
                        column: x => x.PortfolioId,
                        principalTable: "Portfolios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioItems_CryptoAssetId",
                table: "PortfolioItems",
                column: "CryptoAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioItems_PortfolioId",
                table: "PortfolioItems",
                column: "PortfolioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PortfolioItems");

            migrationBuilder.DropTable(
                name: "CryptoAssets");

            migrationBuilder.DropTable(
                name: "Portfolios");
        }
    }
}

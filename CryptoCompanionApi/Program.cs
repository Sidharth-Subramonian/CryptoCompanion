using CryptoCompanionApi.Data;
using CryptoCompanionApi.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Register SQL Database Context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register Cosmos Database Context
builder.Services.AddDbContext<CosmosDbContext>(options =>
    options.UseCosmos(
        builder.Configuration.GetConnectionString("CosmosDbAccountEndpoint")!,
        builder.Configuration.GetConnectionString("CosmosDbAccountKey")!,
        databaseName: "CryptoCompanionDb"));

// Register HttpClient for external Data Ingestion
builder.Services.AddHttpClient();

// Register Background Data Workers
builder.Services.AddHostedService<CryptoDataWorker>();
builder.Services.AddHostedService<NewsDataWorker>();
builder.Services.AddHostedService<SentimentDataWorker>();

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

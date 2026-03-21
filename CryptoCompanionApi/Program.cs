using CryptoCompanionApi.Data;
using CryptoCompanionApi.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Azure.Cosmos;

var builder = WebApplication.CreateBuilder(args);

// --- 1. SERVICE REGISTRATION ---

// Add OpenAPI / Swagger
builder.Services.AddOpenApi();

// Register SQL Database Context (LocalDB)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register Cosmos Database Context (Configured for Local Emulator)
builder.Services.AddDbContext<CosmosDbContext>(options =>
{
    var endpoint = builder.Configuration.GetConnectionString("CosmosDbAccountEndpoint") 
                   ?? "https://localhost:8081/";
    var key = builder.Configuration.GetConnectionString("CosmosDbAccountKey") 
              ?? "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";

    options.UseCosmos(
        endpoint,
        key,
        databaseName: "CryptoCompanionDb",
        cosmosOptionsAction: cosmosOptions =>
        {
            // BYPASS SSL for Local Emulator: This prevents "SSL Handshake" errors
            cosmosOptions.HttpClientFactory(() => new HttpClient(new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            }));
            
            // Gateway mode is more reliable for local development
            cosmosOptions.ConnectionMode(ConnectionMode.Gateway);
        });
});

// Register HttpClient (Shared instance for your workers)
builder.Services.AddHttpClient();

// Register Background Data Workers (The "Engine Room")
builder.Services.AddHostedService<CryptoDataWorker>();
builder.Services.AddHostedService<NewsDataWorker>();
builder.Services.AddHostedService<SentimentDataWorker>();

// Register Controllers
builder.Services.AddControllers();

var app = builder.Build();

// --- 2. PIPELINE CONFIGURATION ---

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Ensure the Cosmos Database and Containers are created before workers start
using (var scope = app.Services.CreateScope())
{
    try 
    {
        var context = scope.ServiceProvider.GetRequiredService<CosmosDbContext>();
        // Creates the DB and 'News' container based on your OnModelCreating logic
        await context.Database.EnsureCreatedAsync();
        Console.WriteLine("Cosmos DB Emulator: Database and Containers verified/created.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Cosmos Startup Error: {ex.Message}. Is the Emulator running?");
    }
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
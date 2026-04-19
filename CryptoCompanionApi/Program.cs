using CryptoCompanionApi.Data;
using CryptoCompanionApi.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Azure.Cosmos;

var builder = WebApplication.CreateBuilder(args);

// --- 1. SERVICE REGISTRATION ---

// Add Swagger/OpenAPI support (.NET 8 standard)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddApplicationInsightsTelemetry();

// Register SQL Database Context (Azure SQL or LocalDB)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure()
    ));

// Register Cosmos Database Context (Azure Cloud or Local Emulator)
builder.Services.AddDbContext<CosmosDbContext>(options =>
{
    // Prefer a single full connection string (most reliable for production)
    var cosmosConnStr = builder.Configuration.GetConnectionString("CosmosDbConnectionString");
    
    if (!string.IsNullOrEmpty(cosmosConnStr) && cosmosConnStr != "SET_BY_K8S_SECRET")
    {
        Console.WriteLine("Cosmos DB: Using full connection string.");
        options.UseCosmos(
            cosmosConnStr,
            databaseName: "CryptoCompanionDb",
            cosmosOptionsAction: cosmosOptions =>
            {
                // Use Gateway mode — works reliably in all Azure environments
                cosmosOptions.ConnectionMode(ConnectionMode.Gateway);
            });
    }
    else
    {
        // Fallback: separate endpoint + key (for local emulator)
        var endpoint = builder.Configuration.GetConnectionString("CosmosDbAccountEndpoint") 
                       ?? "https://localhost:8081/";
        var key = builder.Configuration.GetConnectionString("CosmosDbAccountKey") 
                  ?? "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";

        Console.WriteLine($"Cosmos DB: Using separate endpoint+key. Endpoint: {endpoint}");
        options.UseCosmos(
            endpoint,
            key,
            databaseName: "CryptoCompanionDb",
            cosmosOptionsAction: cosmosOptions =>
            {
                if (builder.Environment.IsDevelopment())
                {
                    cosmosOptions.HttpClientFactory(() => new HttpClient(new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                    }));
                }
                cosmosOptions.ConnectionMode(ConnectionMode.Gateway);
            });
    }
});

// Register HttpClient (Shared instance for your workers)
builder.Services.AddHttpClient();

// Register Background Data Workers (The "Engine Room")
builder.Services.AddHostedService<CryptoDataWorker>();
builder.Services.AddHostedService<NewsDataWorker>();
builder.Services.AddHostedService<SentimentDataWorker>();

// Register Controllers
builder.Services.AddControllers();

// Register AI Advisor
builder.Services.AddScoped<IAiAdvisorService, AzureOpenAiAdvisorService>();

var app = builder.Build();

// --- 2. PIPELINE CONFIGURATION ---

if (app.Environment.IsDevelopment() || true) // Enable Swagger in production for testing
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Ensure the Cosmos Database and SQL Database are created/migrated at startup
using (var scope = app.Services.CreateScope())
{
    try 
    {
        var cosmosContext = scope.ServiceProvider.GetRequiredService<CosmosDbContext>();
        await cosmosContext.Database.EnsureCreatedAsync();
        Console.WriteLine("Cosmos DB: Database and Containers verified/created.");

        var sqlContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await sqlContext.Database.MigrateAsync();
        Console.WriteLine("SQL DB: Database migrated successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Startup Database Error: {ex.Message}");
    }
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
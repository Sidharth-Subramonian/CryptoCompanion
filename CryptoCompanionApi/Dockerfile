# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY ["CryptoCompanionApi/CryptoCompanionApi.csproj", "CryptoCompanionApi/"]
RUN dotnet restore "CryptoCompanionApi/CryptoCompanionApi.csproj"

# Copy the rest of the code
COPY . .
WORKDIR "/src/CryptoCompanionApi"
RUN dotnet build "CryptoCompanionApi.csproj" -c Release -o /app/build

# Publish Stage
FROM build AS publish
RUN dotnet publish "CryptoCompanionApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CryptoCompanionApi.dll"]

# ==============================================================================
# Multi-Stage Production Dockerfile for AzureTest ASP.NET Core API
# ==============================================================================

FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /source

# Copy solution and project files for caching package restore
COPY src/AzureTest.Contracts/*.csproj src/AzureTest.Contracts/
COPY src/AzureTest.Application/*.csproj src/AzureTest.Application/
COPY src/AzureTest.Domain/*.csproj src/AzureTest.Domain/
COPY src/AzureTest.Infrastructure/*.csproj src/AzureTest.Infrastructure/
COPY src/AzureTest.Persistence/*.csproj src/AzureTest.Persistence/
COPY src/AzureTest.API/*.csproj src/AzureTest.API/

RUN dotnet restore src/AzureTest.API/AzureTest.API.csproj

# Copy all source files and publish release
COPY src/ ./src/
WORKDIR /source/src/AzureTest.API
RUN dotnet publish -c Release -o /app/publish --no-restore

# Final runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:5143
EXPOSE 5143

ENTRYPOINT ["dotnet", "AzureTest.API.dll"]

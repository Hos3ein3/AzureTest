# Stage 1: Build & Publish
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files for layer caching
COPY Directory.Build.props Directory.Packages.props ./
COPY src/AzureTest.Contracts/AzureTest.Contracts.csproj         src/AzureTest.Contracts/
COPY src/AzureTest.Domain/AzureTest.Domain.csproj               src/AzureTest.Domain/
COPY src/AzureTest.Application/AzureTest.Application.csproj     src/AzureTest.Application/
COPY src/AzureTest.Infrastructure/AzureTest.Infrastructure.csproj src/AzureTest.Infrastructure/
COPY src/AzureTest.Persistence/AzureTest.Persistence.csproj     src/AzureTest.Persistence/
COPY src/AzureTest.MVC/AzureTest.MVC.csproj                     src/AzureTest.MVC/
COPY src/AzureTest.API/AzureTest.API.csproj  src/AzureTest.API/
# Restore dependencies
RUN dotnet restore src/AzureTest.MVC/AzureTest.MVC.csproj
RUN dotnet restore src/AzureTest.API/AzureTest.API.csproj
# Copy the rest of the source code
COPY . .

# Publish in Release mode
RUN dotnet publish src/AzureTest.MVC/AzureTest.MVC.csproj \
    --configuration Release --no-restore --output /app/publish/mvc
RUN dotnet publish src/AzureTest.API/AzureTest.API.csproj \
    --configuration Release --no-restore --output /app/publish/api

# Stage 2: Test
FROM build AS test
WORKDIR /src
# Restore and build test project
RUN dotnet restore test/AzureTest.IntegrationTest/AzureTest.IntegrationTest.csproj
RUN dotnet build test/AzureTest.IntegrationTest/AzureTest.IntegrationTest.csproj --no-restore
# Note: running tests here requires Docker-in-Docker setup to work with Testcontainers.
# CMD ["dotnet", "test", "test/AzureTest.IntegrationTest/AzureTest.IntegrationTest.csproj", "--no-build"]

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

RUN groupadd --system appgroup && useradd --system --gid appgroup appuser
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

# MVC image
FROM runtime-base AS mvc
COPY --from=build /app/publish/mvc .
RUN mkdir -p /app/keys && chown -R appuser:appgroup /app
USER appuser
ENTRYPOINT ["dotnet", "AzureTest.MVC.dll"]

# API image
FROM runtime-base AS api
COPY --from=build /app/publish/api .
RUN chown -R appuser:appgroup /app
USER appuser
ENTRYPOINT ["dotnet", "AzureTest.API.dll"]
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

# Restore dependencies
RUN dotnet restore src/AzureTest.MVC/AzureTest.MVC.csproj

# Copy the rest of the source code
COPY . .

# Publish in Release mode
RUN dotnet publish src/AzureTest.MVC/AzureTest.MVC.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Create a non-root user for security best practices (Debian-based image)
RUN groupadd --system appgroup && useradd --system --gid appgroup appuser

COPY --from=build /app/publish .

# Ensure the non-root user owns the files
RUN chown -R appuser:appgroup /app

USER appuser

EXPOSE 8080
EXPOSE 8081

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "AzureTest.MVC.dll"]

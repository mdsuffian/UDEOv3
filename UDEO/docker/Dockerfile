# UDEO Dockerfile
# Multi-stage build for UDEO system

# Build stage for .NET components
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS dotnet-build
WORKDIR /src

# Copy and build .NET projects
COPY ["src/Experts/*.cs", "src/Experts/"]
COPY ["src/Experts/*.csproj", "src/Experts/"]
RUN dotnet build "UDEO-Experts.csproj" -c Release -o /app/build

# Runtime stage
FROM mcr.microsoft.com/powershell:latest AS runtime

# Install system dependencies
RUN apt-get update && apt-get install -y \
    sqlite3 \
    && rm -rf /var/lib/apt/lists/*

# Create app directory
WORKDIR /app

# Copy PowerShell modules
COPY ["src/*.psm1", "./modules/"]
COPY ["config/*.json", "./config/"]
COPY ["scripts/*.ps1", "./scripts/"]

# Copy .NET assemblies (if built)
COPY --from=dotnet-build /app/build ./experts/

# Create necessary directories
RUN mkdir -p /app/logs /app/data /app/test-results

# Set environment variables
ENV UDEO_CONFIG_PATH="/app/config/UDEO-Config.json"
ENV UDEO_SCHEMA_PATH="/app/config/udeo-schema.json"
ENV UDEO_LOG_PATH="/app/logs/udeo.log"
ENV UDEO_AUDIT_LOG_PATH="/app/logs/audit.log"
ENV POWERSHELL_TELEMETRY_OPTOUT=1

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD pwsh -Command "Import-Module ./modules/UDEO-Core.psm1; Get-UDEOStatistics | Out-Null"

# Default command
CMD ["pwsh", "-File", "./scripts/Initialize-UDEO.ps1"]
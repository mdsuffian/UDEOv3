# UDEO v3.1.0 — Multi-stage Dockerfile (C# / .NET 8)
# Builds the UDEO CLI application with optional C++ native extensions.

# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy everything
COPY . .

# Restore
RUN dotnet restore UDEO.sln

# Build
RUN dotnet publish src/UDEO.Cli/UDEO.Cli.csproj \
    -c Release \
    -o /app \
    --no-restore \
    --self-contained false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime
WORKDIR /app

# Create workspace directories
RUN mkdir -p /data/.udeo/store /data/plugins

# Copy published app
COPY --from=build /app .

# Set environment
ENV UDEO_WORKSPACE=/data
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1

# Entry point
ENTRYPOINT ["dotnet", "udeo.dll"]
CMD ["help"]

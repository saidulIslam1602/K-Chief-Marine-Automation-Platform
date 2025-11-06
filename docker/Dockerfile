# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files for dependency restoration
COPY ["src/HMI.Platform.Core/HMI.Platform.Core.csproj", "src/HMI.Platform.Core/"]
COPY ["src/HMI.Platform.API/HMI.Platform.API.csproj", "src/HMI.Platform.API/"]
COPY ["src/HMI.DataAccess/HMI.DataAccess.csproj", "src/HMI.DataAccess/"]
COPY ["src/HMI.VesselControl/HMI.VesselControl.csproj", "src/HMI.VesselControl/"]
COPY ["src/HMI.AlarmSystem/HMI.AlarmSystem.csproj", "src/HMI.AlarmSystem/"]
COPY ["src/HMI.Protocols.OPC/HMI.Protocols.OPC.csproj", "src/HMI.Protocols.OPC/"]
COPY ["src/HMI.Protocols.Modbus/HMI.Protocols.Modbus.csproj", "src/HMI.Protocols.Modbus/"]

# Restore dependencies for the main API project (this will restore all dependencies)
RUN dotnet restore "src/HMI.Platform.API/HMI.Platform.API.csproj"

# Copy all source files (excluding tests and other non-essential files)
COPY ["src/", "src/"]

# Build the API project
WORKDIR "/src/src/HMI.Platform.API"
RUN dotnet build "HMI.Platform.API.csproj" -c Release -o /app/build --no-restore

# Publish stage
FROM build AS publish
RUN dotnet publish "HMI.Platform.API.csproj" -c Release -o /app/publish /p:UseAppHost=false --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

    # Create directories for OPC UA certificates, database, and logs
    RUN mkdir -p "/app/OPC Foundation/CertificateStores" && \
        mkdir -p /app/data && \
        mkdir -p /app/logs

# Create non-root user for security
RUN adduser --disabled-password --gecos "" --uid 1000 appuser && \
    chown -R appuser:appuser /app
USER appuser

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Expose port
EXPOSE 8080

# Copy published application
COPY --from=publish --chown=appuser:appuser /app/publish .

# Add health check
HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "HMI.Platform.API.dll"]


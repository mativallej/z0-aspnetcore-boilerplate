# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files
COPY ["src/Z0.Domain/Z0.Domain.csproj", "Z0.Domain/"]
COPY ["src/Z0.Application/Z0.Application.csproj", "Z0.Application/"]
COPY ["src/Z0.Infrastructure/Z0.Infrastructure.csproj", "Z0.Infrastructure/"]
COPY ["src/Z0.WebApi/Z0.WebApi.csproj", "Z0.WebApi/"]

# Restore dependencies
RUN dotnet restore "Z0.WebApi/Z0.WebApi.csproj"

# Copy source files
COPY src/ .

# Build
RUN dotnet build "Z0.WebApi/Z0.WebApi.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "Z0.WebApi/Z0.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Create non-root user for security
RUN adduser --disabled-password --gecos '' appuser

# Copy published files
COPY --from=publish /app/publish .

# Create logs directory
RUN mkdir -p /app/logs && chown -R appuser:appuser /app/logs

# Switch to non-root user
USER appuser

# Expose port
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl --fail http://localhost:8080/api/v1/health || exit 1

# Entry point
ENTRYPOINT ["dotnet", "Z0.WebApi.dll"]

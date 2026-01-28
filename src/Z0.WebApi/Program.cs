using Serilog;
using Z0.Application;
using Z0.Infrastructure;
using Z0.Infrastructure.Logging;
using Z0.WebApi.Extensions;

// Bootstrap logger for startup
Log.Logger = SerilogConfiguration.CreateBootstrapLogger().CreateLogger();

try
{
    Log.Information("Starting Z0 E-commerce API");

    var builder = WebApplication.CreateBuilder(args);

    // Configure Serilog
    builder.Host.UseSerilogConfiguration();

    // Add services to the container
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();

    // Add API versioning
    builder.Services.AddApiVersioningConfiguration();

    // Add Swagger
    builder.Services.AddSwaggerConfiguration();

    // Add CORS
    builder.Services.AddCorsConfiguration();

    // Add Health Checks
    builder.Services.AddHealthChecksConfiguration(builder.Configuration);

    // Add Application layer services
    builder.Services.AddApplication();

    // Add Infrastructure layer services
    builder.Services.AddInfrastructure(builder.Configuration);

    var app = builder.Build();

    // Configure the HTTP request pipeline
    app.UseExceptionHandling();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwaggerConfiguration();
    }
    else
    {
        app.UseSecurityHeaders();
    }

    app.UseHttpsRedirection();

    app.UseCors("AllowAll");

    app.UseSerilogRequestLogging();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    // Map health check endpoints (in addition to controller-based health checks)
    app.MapHealthChecks("/health");

    Log.Information("Z0 E-commerce API started successfully");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// Make the implicit Program class public for testing
public partial class Program { }

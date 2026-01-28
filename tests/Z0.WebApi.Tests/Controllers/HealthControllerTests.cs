using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Moq;
using Z0.WebApi.Controllers.v1;
using Xunit;

namespace Z0.WebApi.Tests.Controllers;

[Trait("Category", "Unit")]
public class HealthControllerTests
{
    private readonly Mock<HealthCheckService> _healthCheckServiceMock;
    private readonly Mock<ILogger<HealthController>> _loggerMock;
    private readonly HealthController _controller;

    public HealthControllerTests()
    {
        _healthCheckServiceMock = new Mock<HealthCheckService>();
        _loggerMock = new Mock<ILogger<HealthController>>();
        _controller = new HealthController(_healthCheckServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public void GetHealth_ShouldReturnOkWithHealthStatus()
    {
        // Act
        var result = _controller.GetHealth() as OkObjectResult;

        // Assert
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(StatusCodes.Status200OK);

        var response = result.Value;
        response.Should().NotBeNull();
    }

    [Fact]
    public void GetLiveness_ShouldReturnOkWithAliveStatus()
    {
        // Act
        var result = _controller.GetLiveness() as OkObjectResult;

        // Assert
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Fact]
    public async Task GetReadiness_WithHealthyStatus_ShouldReturnOk()
    {
        // Arrange
        var healthReport = new HealthReport(
            new Dictionary<string, HealthReportEntry>
            {
                ["postgresql"] = new HealthReportEntry(
                    HealthStatus.Healthy,
                    "Database is healthy",
                    TimeSpan.FromMilliseconds(100),
                    null,
                    null)
            },
            TimeSpan.FromMilliseconds(100));

        _healthCheckServiceMock
            .Setup(h => h.CheckHealthAsync(It.IsAny<Func<HealthCheckRegistration, bool>?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(healthReport);

        // Act
        var result = await _controller.GetReadiness() as OkObjectResult;

        // Assert
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Fact]
    public async Task GetReadiness_WithUnhealthyStatus_ShouldReturn503()
    {
        // Arrange
        var healthReport = new HealthReport(
            new Dictionary<string, HealthReportEntry>
            {
                ["postgresql"] = new HealthReportEntry(
                    HealthStatus.Unhealthy,
                    "Database connection failed",
                    TimeSpan.FromMilliseconds(5000),
                    new Exception("Connection refused"),
                    null)
            },
            TimeSpan.FromMilliseconds(5000));

        _healthCheckServiceMock
            .Setup(h => h.CheckHealthAsync(It.IsAny<Func<HealthCheckRegistration, bool>?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(healthReport);

        // Act
        var result = await _controller.GetReadiness() as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(StatusCodes.Status503ServiceUnavailable);
    }
}

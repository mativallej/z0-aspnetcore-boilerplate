using System.Net;
using FluentAssertions;
using Xunit;

namespace Z0.WebApi.Tests.IntegrationTests;

[Trait("Category", "Integration")]
public class HealthEndpointTests : IClassFixture<WebApplicationFactoryFixture>
{
    private readonly HttpClient _client;

    public HealthEndpointTests(WebApplicationFactoryFixture factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task HealthEndpoint_ShouldReturnOk()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task LivenessEndpoint_ShouldReturnOk()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/health/live");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ReadinessEndpoint_ShouldReturnOk()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/health/ready");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ItemsEndpoint_WithoutAuth_ShouldReturnUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/items");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}

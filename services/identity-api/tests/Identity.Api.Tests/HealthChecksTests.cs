using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Identity.Api.Tests;

public class HealthChecksTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public HealthChecksTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task HealthEndpointReturnsHealthy()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadAsStringAsync();
        Assert.Contains("healthy", payload, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ReadyEndpointReturnsReady()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/ready");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadAsStringAsync();
        Assert.Contains("ready", payload, StringComparison.OrdinalIgnoreCase);
    }
}

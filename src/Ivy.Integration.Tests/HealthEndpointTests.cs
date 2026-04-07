using System.Net;

namespace Ivy.Integration.Tests;

public class HealthEndpointTests : IClassFixture<IvyTestFixture>
{
    private readonly HttpClient _client;

    public HealthEndpointTests(IvyTestFixture fixture)
    {
        _client = fixture.Client;
    }

    [Fact]
    public async Task HealthCheck_ReturnsOkWithHealthy()
    {
        var response = await _client.GetAsync("/ivy/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Equal("Healthy", content);
    }
}

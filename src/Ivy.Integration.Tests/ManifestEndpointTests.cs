using System.Net;

namespace Ivy.Integration.Tests;

public class ManifestEndpointTests : IClassFixture<IvyTestFixture>
{
    private readonly HttpClient _client;

    public ManifestEndpointTests(IvyTestFixture fixture)
    {
        _client = fixture.Client;
    }

    [Fact]
    public async Task Manifest_WithNoConfig_Returns404()
    {
        var response = await _client.GetAsync("/manifest.json");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}

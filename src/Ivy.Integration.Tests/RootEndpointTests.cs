using System.Net;

namespace Ivy.Integration.Tests;

public class RootEndpointTests : IClassFixture<IvyTestFixture>
{
    private readonly HttpClient _client;

    public RootEndpointTests(IvyTestFixture fixture)
    {
        _client = fixture.Client;
    }

    [Fact]
    public async Task Root_ReturnsOkWithHtml()
    {
        var response = await _client.GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/html", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task SpaFallback_ReturnsOkWithHtml()
    {
        var response = await _client.GetAsync("/some/unknown/spa/path");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/html", response.Content.Headers.ContentType?.MediaType);
    }
}

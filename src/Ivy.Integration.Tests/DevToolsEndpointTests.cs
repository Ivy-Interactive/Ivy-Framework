using System.Net;
using Microsoft.AspNetCore.Builder;

namespace Ivy.Integration.Tests;

public class DevToolsEndpointTests : IAsyncLifetime
{
    private WebApplication? _app;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        var server = new Server(new ServerArgs
        {
            Port = 0,
            Silent = true,
            Host = "127.0.0.1",
            EnableDevTools = true
        });
        _app = server.BuildWebApplication();
        if (_app == null)
            throw new InvalidOperationException("Failed to build WebApplication");
        await _app.StartAsync();
        _client = new HttpClient { BaseAddress = new Uri(_app.Urls.First()) };
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        if (_app != null) await _app.StopAsync();
    }

    [Fact]
    public async Task WidgetSchema_ReturnsJson()
    {
        var response = await _client.GetAsync("/ivy/dev-tools/widget-schema");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task EnvInfo_ReturnsJson()
    {
        var response = await _client.GetAsync("/ivy/dev-tools/env-info");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
    }
}

public class DevToolsDisabledTests : IClassFixture<IvyTestFixture>
{
    private readonly HttpClient _client;

    public DevToolsDisabledTests(IvyTestFixture fixture)
    {
        _client = fixture.Client;
    }

    [Fact]
    public async Task WidgetSchema_WithoutDevTools_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/ivy/dev-tools/widget-schema");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task EnvInfo_WithoutDevTools_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/ivy/dev-tools/env-info");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}

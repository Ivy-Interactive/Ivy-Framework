using System.Net;

namespace Ivy.Integration.Tests;

/// <summary>
/// The fixture binds Host = "127.0.0.1", so the default CORS policy allows loopback origins and
/// nothing else. Assertions are on the CORS response headers only — CORS is browser-enforced, so a
/// disallowed origin still gets the response body.
/// </summary>
public class CorsPolicyTests : IClassFixture<IvyTestFixture>
{
    private const string LoopbackOrigin = "http://localhost:5173";
    private const string ForeignOrigin = "https://evil.example";

    private readonly HttpClient _client;

    public CorsPolicyTests(IvyTestFixture fixture)
    {
        _client = fixture.Client;
    }

    [Fact]
    public async Task Health_WithLoopbackOrigin_EchoesAllowOriginAndCredentials()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/ivy/health");
        request.Headers.Add("Origin", LoopbackOrigin);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(LoopbackOrigin, Assert.Single(response.Headers.GetValues("Access-Control-Allow-Origin")));
        Assert.Equal("true", Assert.Single(response.Headers.GetValues("Access-Control-Allow-Credentials")));
    }

    [Fact]
    public async Task Health_WithForeignOrigin_SendsNoAllowOrigin()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/ivy/health");
        request.Headers.Add("Origin", ForeignOrigin);

        var response = await _client.SendAsync(request);

        Assert.False(response.Headers.Contains("Access-Control-Allow-Origin"));
        Assert.False(response.Headers.Contains("Access-Control-Allow-Credentials"));
    }

    [Fact]
    public async Task Health_WithNullOrigin_SendsNoAllowOrigin()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/ivy/health");
        request.Headers.Add("Origin", "null");

        var response = await _client.SendAsync(request);

        Assert.False(response.Headers.Contains("Access-Control-Allow-Origin"));
    }

    [Fact]
    public async Task LocalFile_WithForeignOrigin_SendsNoAllowOrigin()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/ivy/local-file?path=C:/Windows/win.ini");
        request.Headers.Add("Origin", ForeignOrigin);

        var response = await _client.SendAsync(request);

        // The endpoint itself is disabled here (no DangerouslyAllowLocalFiles), but the point is that
        // a foreign page could not read the response even if it were enabled.
        Assert.False(response.Headers.Contains("Access-Control-Allow-Origin"));
    }

    [Fact]
    public async Task NegotiatePreflight_WithLoopbackOrigin_EchoesAllowOrigin()
    {
        var response = await SendNegotiatePreflight(LoopbackOrigin);

        Assert.Equal(LoopbackOrigin, Assert.Single(response.Headers.GetValues("Access-Control-Allow-Origin")));
        Assert.Equal("true", Assert.Single(response.Headers.GetValues("Access-Control-Allow-Credentials")));
    }

    [Fact]
    public async Task NegotiatePreflight_WithForeignOrigin_SendsNoAllowOrigin()
    {
        var response = await SendNegotiatePreflight(ForeignOrigin);

        Assert.False(response.Headers.Contains("Access-Control-Allow-Origin"));
    }

    private async Task<HttpResponseMessage> SendNegotiatePreflight(string origin)
    {
        var request = new HttpRequestMessage(HttpMethod.Options, "/ivy/messages/negotiate?negotiateVersion=1");
        request.Headers.Add("Origin", origin);
        request.Headers.Add("Access-Control-Request-Method", "POST");
        request.Headers.Add("Access-Control-Request-Headers", "x-requested-with,content-type");

        return await _client.SendAsync(request);
    }
}

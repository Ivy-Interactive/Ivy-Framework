using System.Net;

namespace Ivy.Integration.Tests;

/// <summary>
/// Host header validation is the DNS rebinding mitigation. A loopback bind validates the loopback
/// names by default, and <c>AllowHosts</c> replaces that list: these tests measure both.
/// </summary>
public class HostFilteringTests
{
    [Fact]
    public async Task WithAllowHosts_ForeignHostHeader_ReturnsBadRequest()
    {
        await using var server = await IvyTestServer.CreateAsync(s => s.AllowHosts("localhost", "127.0.0.1"));
        using var client = CreateClient(server);

        var allowed = await client.GetAsync("/ivy/health");
        Assert.Equal(HttpStatusCode.OK, allowed.StatusCode);

        var rejected = await client.SendAsync(WithHost("evil.example"));
        Assert.Equal(HttpStatusCode.BadRequest, rejected.StatusCode);
    }

    [Fact]
    public async Task LoopbackBound_ForeignHostHeader_ReturnsBadRequest()
    {
        await using var server = await IvyTestServer.CreateAsync();
        using var client = CreateClient(server);

        var response = await client.SendAsync(WithHost("evil.example"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("localhost")]
    [InlineData("127.0.0.1")]
    [InlineData("[::1]")]
    public async Task LoopbackBound_LoopbackHostHeaders_AreAccepted(string host)
    {
        await using var server = await IvyTestServer.CreateAsync();
        using var client = CreateClient(server);

        var response = await client.SendAsync(WithHost(host));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    /// <summary>
    /// <c>AllowHosts</c> replaces the loopback default rather than extending it, so naming only a
    /// tunnel hostname stops the loopback names from working.
    /// </summary>
    [Fact]
    public async Task WithAllowHosts_ReplacesTheLoopbackDefault()
    {
        await using var server = await IvyTestServer.CreateAsync(s => s.AllowHosts("app.example.com"));
        using var client = CreateClient(server);

        var response = await client.SendAsync(WithHost("localhost"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private static HttpRequestMessage WithHost(string host)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/ivy/health");

        // A bracketed IPv6 literal does not survive the typed Host setter's validation, and the
        // header has to reach the server verbatim for HostString to match it.
        if (!request.Headers.TryAddWithoutValidation("Host", host))
            request.Headers.Host = host;

        return request;
    }

    /// <summary>
    /// Overriding the Host header also changes the TLS SNI name the handler validates the
    /// certificate against, so the dev certificate has to be accepted without a name match.
    /// </summary>
    private static HttpClient CreateClient(IvyTestServer server)
    {
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };

        return new HttpClient(handler) { BaseAddress = new Uri(server.BaseUrl) };
    }
}

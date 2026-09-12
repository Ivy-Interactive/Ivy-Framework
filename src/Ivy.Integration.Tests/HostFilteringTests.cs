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

        // Set verbatim rather than through the typed Host setter, so the value under test reaches
        // the server exactly as written and it is the server's parsing that is measured here.
        request.Headers.TryAddWithoutValidation("Host", host);

        return request;
    }

    /// <summary>
    /// The harness pins HTTP (<c>ServerArgs.UseTls</c> is false), so this callback never fires today. It
    /// stays because overriding the Host header also changes the TLS SNI name the handler validates the
    /// certificate against: the day a server here opts into TLS, the dev certificate has to be accepted
    /// without a name match, or every case in this class fails on the handshake instead of on its assertion.
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

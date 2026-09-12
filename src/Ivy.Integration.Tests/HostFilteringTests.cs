using System.Net;

namespace Ivy.Integration.Tests;

/// <summary>
/// Host header validation is the DNS rebinding mitigation and is opt-in: these tests measure both
/// the opted-in behaviour and the unchanged default.
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
    public async Task WithoutAllowHosts_ForeignHostHeader_IsAccepted()
    {
        await using var server = await IvyTestServer.CreateAsync();
        using var client = CreateClient(server);

        var response = await client.SendAsync(WithHost("evil.example"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private static HttpRequestMessage WithHost(string host)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/ivy/health");
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

using System.Net;

namespace Ivy.Integration.Tests;

public class AuthEndpointTests : IClassFixture<IvyTestFixture>
{
    private readonly HttpClient _client;

    public AuthEndpointTests(IvyTestFixture fixture)
    {
        _client = fixture.Client;
    }

    [Fact]
    public async Task OAuthLogin_WithoutParams_ReturnsBadRequest()
    {
        var response = await _client.GetAsync("/ivy/auth/oauth-login");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SetAuthCookies_WithMissingBody_ReturnsError()
    {
        var response = await _client.PatchAsync("/ivy/auth/set-auth-cookies", null);

        // Should return an error status (400 or 415) when body is missing
        Assert.True(
            response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.UnsupportedMediaType,
            $"Expected 400 or 415 but got {(int)response.StatusCode}");
    }
}

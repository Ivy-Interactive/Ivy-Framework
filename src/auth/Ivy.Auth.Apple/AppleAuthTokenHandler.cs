using System.Text.Json;

namespace Ivy.Auth.Apple;

/// <summary>Apple OAuth token handler</summary>
public class AppleAuthTokenHandler : IAuthTokenHandler
{
    protected readonly HttpClient HttpClient;

    /// <summary>Initialize Apple auth token handler</summary>
    public AppleAuthTokenHandler(HttpClient httpClient)
    {
        HttpClient = httpClient;
    }

    /// <summary>Refresh Apple OAuth access token</summary>
    public async Task<AuthToken?> RefreshAccessTokenAsync(IAuthTokenHandlerSession authSession, CancellationToken cancellationToken = default)
    {
        var refreshToken = authSession.AuthToken?.RefreshToken;
        if (string.IsNullOrWhiteSpace(refreshToken))
            return null;

        try
        {
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("refresh_token", refreshToken),
                new KeyValuePair<string, string>("grant_type", "refresh_token")
            });

            var response = await HttpClient.PostAsync("https://appleid.apple.com/auth/token", content, cancellationToken);
            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (!root.TryGetProperty("access_token", out var accessTokenProp))
                return null;

            var accessToken = accessTokenProp.GetString();
            if (string.IsNullOrEmpty(accessToken))
                return null;

            var newRefreshToken = root.TryGetProperty("refresh_token", out var refreshProp)
                ? refreshProp.GetString()
                : refreshToken;

            return new AuthToken(accessToken, newRefreshToken);
        }
        catch (Exception ex) when (ex is JsonException or HttpRequestException or TaskCanceledException)
        {
            return null;
        }
    }

    /// <summary>Validate Apple OAuth access token</summary>
    public Task<bool> ValidateAccessTokenAsync(IAuthTokenHandlerSession authSession, CancellationToken cancellationToken = default)
    {
        var token = authSession.AuthToken?.AccessToken;
        return Task.FromResult(!string.IsNullOrWhiteSpace(token));
    }

    /// <summary>Get user info - Apple provides limited user info</summary>
    public Task<UserInfo?> GetUserInfoAsync(IAuthTokenHandlerSession authSession, CancellationToken cancellationToken = default)
    {
        // Apple doesn't provide user info endpoint - user info is provided during initial authentication only
        return Task.FromResult<UserInfo?>(null);
    }

    /// <summary>Get Apple OAuth token lifetime</summary>
    public Task<TokenLifetime?> GetAccessTokenLifetimeAsync(IAuthTokenHandlerSession authSession, CancellationToken cancellationToken = default)
    {
        // Apple tokens typically expire after 24 hours
        return Task.FromResult<TokenLifetime?>(new TokenLifetime(DateTimeOffset.UtcNow.AddHours(24)));
    }
}

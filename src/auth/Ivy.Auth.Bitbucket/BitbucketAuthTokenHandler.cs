using System.Text.Json;

namespace Ivy.Auth.Bitbucket;

/// <summary>Bitbucket OAuth token handler</summary>
[OAuthTokenHandler(OAuthProviders.Bitbucket)]
public class BitbucketAuthTokenHandler : IAuthTokenHandler
{
    protected readonly HttpClient HttpClient;

    /// <summary>Initialize Bitbucket auth token handler</summary>
    public BitbucketAuthTokenHandler(HttpClient httpClient)
    {
        HttpClient = httpClient;
    }

    /// <summary>Refresh Bitbucket OAuth access token</summary>
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

            var response = await HttpClient.PostAsync("https://bitbucket.org/site/oauth2/access_token", content, cancellationToken);
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

    /// <summary>Validate Bitbucket OAuth access token</summary>
    public async Task<bool> ValidateAccessTokenAsync(IAuthTokenHandlerSession authSession, CancellationToken cancellationToken = default)
    {
        var token = authSession.AuthToken?.AccessToken;
        if (string.IsNullOrWhiteSpace(token))
            return false;

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "https://api.bitbucket.org/2.0/user");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await HttpClient.SendAsync(request, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return false;
        }
    }

    /// <summary>Get user info from Bitbucket API</summary>
    public async Task<UserInfo?> GetUserInfoAsync(IAuthTokenHandlerSession authSession, CancellationToken cancellationToken = default)
    {
        var token = authSession.AuthToken?.AccessToken;
        if (string.IsNullOrWhiteSpace(token))
            return null;

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "https://api.bitbucket.org/2.0/user");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await HttpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var uuid = root.GetProperty("uuid").GetString() ?? "";
            var username = root.TryGetProperty("username", out var usernameProp) ? usernameProp.GetString() : null;
            var displayName = root.TryGetProperty("display_name", out var nameProp) ? nameProp.GetString() : null;

            var avatarUrl = root.TryGetProperty("links", out var linksProp)
                && linksProp.TryGetProperty("avatar", out var avatarProp)
                && avatarProp.TryGetProperty("href", out var hrefProp)
                    ? hrefProp.GetString()
                    : null;

            return new UserInfo(uuid, username ?? uuid, displayName, avatarUrl);
        }
        catch (Exception ex) when (ex is JsonException or HttpRequestException or TaskCanceledException)
        {
            return null;
        }
    }

    /// <summary>Get Bitbucket OAuth token lifetime</summary>
    public Task<TokenLifetime?> GetAccessTokenLifetimeAsync(IAuthTokenHandlerSession authSession, CancellationToken cancellationToken = default)
    {
        // Bitbucket tokens typically expire after 1 hour
        return Task.FromResult<TokenLifetime?>(new TokenLifetime(DateTimeOffset.UtcNow.AddHours(1)));
    }
}

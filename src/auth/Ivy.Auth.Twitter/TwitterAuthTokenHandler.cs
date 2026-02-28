using System.Text.Json;

namespace Ivy.Auth.Twitter;

/// <summary>Twitter OAuth token handler</summary>
[OAuthTokenHandler(OAuthProvider.Twitter)]
public class TwitterAuthTokenHandler : IAuthTokenHandler
{
    protected readonly HttpClient HttpClient;

    /// <summary>Initialize Twitter auth token handler</summary>
    public TwitterAuthTokenHandler(HttpClient httpClient)
    {
        HttpClient = httpClient;
    }

    /// <summary>Refresh Twitter OAuth access token</summary>
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

            var response = await HttpClient.PostAsync("https://api.twitter.com/2/oauth2/token", content, cancellationToken);
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

    /// <summary>Validate Twitter OAuth access token</summary>
    public async Task<bool> ValidateAccessTokenAsync(IAuthTokenHandlerSession authSession, CancellationToken cancellationToken = default)
    {
        var token = authSession.AuthToken?.AccessToken;
        if (string.IsNullOrWhiteSpace(token))
            return false;

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "https://api.twitter.com/2/users/me");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await HttpClient.SendAsync(request, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return false;
        }
    }

    /// <summary>Get user info from Twitter API</summary>
    public async Task<UserInfo?> GetUserInfoAsync(IAuthTokenHandlerSession authSession, CancellationToken cancellationToken = default)
    {
        var token = authSession.AuthToken?.AccessToken;
        if (string.IsNullOrWhiteSpace(token))
            return null;

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "https://api.twitter.com/2/users/me?user.fields=profile_image_url");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await HttpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (!root.TryGetProperty("data", out var data))
                return null;

            var id = data.GetProperty("id").GetString() ?? "";
            var username = data.GetProperty("username").GetString();
            var name = data.TryGetProperty("name", out var nameProp) ? nameProp.GetString() : null;
            var profileImage = data.TryGetProperty("profile_image_url", out var imageProp) ? imageProp.GetString() : null;

            return new UserInfo(id, username ?? id, name, profileImage);
        }
        catch (Exception ex) when (ex is JsonException or HttpRequestException or TaskCanceledException)
        {
            return null;
        }
    }

    /// <summary>Get Twitter OAuth token lifetime</summary>
    public Task<TokenLifetime?> GetAccessTokenLifetimeAsync(IAuthTokenHandlerSession authSession, CancellationToken cancellationToken = default)
    {
        // Twitter tokens typically expire after 2 hours
        return Task.FromResult<TokenLifetime?>(new TokenLifetime(DateTimeOffset.UtcNow.AddHours(2)));
    }
}

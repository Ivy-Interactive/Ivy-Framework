using System.Text.Json;

namespace Ivy.Auth.Twitch;

/// <summary>Twitch OAuth token handler</summary>
[OAuthTokenHandler(OAuthProviders.Twitch)]
public class TwitchAuthTokenHandler : IAuthTokenHandler
{
    protected readonly HttpClient HttpClient;

    /// <summary>Initialize Twitch auth token handler</summary>
    public TwitchAuthTokenHandler(HttpClient httpClient)
    {
        HttpClient = httpClient;
    }

    /// <summary>Refresh Twitch OAuth access token</summary>
    public Task<AuthToken?> RefreshAccessTokenAsync(IAuthTokenHandlerSession authSession, CancellationToken cancellationToken = default)
    {
        // Twitch tokens cannot be refreshed through standard OAuth flow when obtained via third-party auth providers
        return Task.FromResult<AuthToken?>(null);
    }

    /// <summary>Validate Twitch OAuth access token</summary>
    public async Task<bool> ValidateAccessTokenAsync(IAuthTokenHandlerSession authSession, CancellationToken cancellationToken = default)
    {
        var token = authSession.AuthToken?.AccessToken;
        if (string.IsNullOrWhiteSpace(token))
            return false;

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "https://api.twitch.tv/helix/users");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await HttpClient.SendAsync(request, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return false;
        }
    }

    /// <summary>Get user info from Twitch API</summary>
    public async Task<UserInfo?> GetUserInfoAsync(IAuthTokenHandlerSession authSession, CancellationToken cancellationToken = default)
    {
        var token = authSession.AuthToken?.AccessToken;
        if (string.IsNullOrWhiteSpace(token))
            return null;

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "https://api.twitch.tv/helix/users");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await HttpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (!root.TryGetProperty("data", out var dataArray) || dataArray.GetArrayLength() == 0)
                return null;

            var user = dataArray[0];
            var id = user.GetProperty("id").GetString() ?? "";
            var login = user.GetProperty("login").GetString();
            var displayName = user.TryGetProperty("display_name", out var nameProp) ? nameProp.GetString() : null;
            var profileImage = user.TryGetProperty("profile_image_url", out var imageProp) ? imageProp.GetString() : null;

            return new UserInfo(id, login ?? id, displayName, profileImage);
        }
        catch (Exception ex) when (ex is JsonException or HttpRequestException or TaskCanceledException)
        {
            return null;
        }
    }

    /// <summary>Get Twitch OAuth token lifetime</summary>
    public Task<TokenLifetime?> GetAccessTokenLifetimeAsync(IAuthTokenHandlerSession authSession, CancellationToken cancellationToken = default)
    {
        // Twitch tokens typically last indefinitely until revoked
        return Task.FromResult<TokenLifetime?>(null);
    }
}

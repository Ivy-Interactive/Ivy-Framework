using System.Text.Json;

namespace Ivy.Auth.Discord;

/// <summary>Discord OAuth token handler</summary>
[OAuthTokenHandler(OAuthProviders.Discord)]
public class DiscordAuthTokenHandler : IAuthTokenHandler
{
    protected readonly HttpClient HttpClient;

    /// <summary>Initialize Discord auth token handler</summary>
    public DiscordAuthTokenHandler(HttpClient httpClient)
    {
        HttpClient = httpClient;
    }

    /// <summary>Refresh Discord OAuth access token</summary>
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

            var response = await HttpClient.PostAsync("https://discord.com/api/oauth2/token", content, cancellationToken);
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

    /// <summary>Validate Discord OAuth access token</summary>
    public async Task<bool> ValidateAccessTokenAsync(IAuthTokenHandlerSession authSession, CancellationToken cancellationToken = default)
    {
        var token = authSession.AuthToken?.AccessToken;
        if (string.IsNullOrWhiteSpace(token))
            return false;

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "https://discord.com/api/users/@me");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await HttpClient.SendAsync(request, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return false;
        }
    }

    /// <summary>Get user info from Discord API</summary>
    public async Task<UserInfo?> GetUserInfoAsync(IAuthTokenHandlerSession authSession, CancellationToken cancellationToken = default)
    {
        var token = authSession.AuthToken?.AccessToken;
        if (string.IsNullOrWhiteSpace(token))
            return null;

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "https://discord.com/api/users/@me");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await HttpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var id = root.GetProperty("id").GetString() ?? "";
            var username = root.GetProperty("username").GetString();
            var discriminator = root.TryGetProperty("discriminator", out var discProp) ? discProp.GetString() : null;
            var avatar = root.TryGetProperty("avatar", out var avatarProp) ? avatarProp.GetString() : null;

            var displayName = discriminator != null && discriminator != "0"
                ? $"{username}#{discriminator}"
                : username;

            var avatarUrl = avatar != null
                ? $"https://cdn.discordapp.com/avatars/{id}/{avatar}.png"
                : null;

            return new UserInfo(id, displayName ?? id, username, avatarUrl);
        }
        catch (Exception ex) when (ex is JsonException or HttpRequestException or TaskCanceledException)
        {
            return null;
        }
    }

    /// <summary>Get Discord OAuth token lifetime</summary>
    public Task<TokenLifetime?> GetAccessTokenLifetimeAsync(IAuthTokenHandlerSession authSession, CancellationToken cancellationToken = default)
    {
        // Discord tokens typically expire after 1 week
        return Task.FromResult<TokenLifetime?>(new TokenLifetime(DateTimeOffset.UtcNow.AddDays(7)));
    }
}

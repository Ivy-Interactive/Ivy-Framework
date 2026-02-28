using System.Text.Json;

namespace Ivy.Auth.Figma;

/// <summary>Figma OAuth token handler</summary>
public class FigmaAuthTokenHandler : IAuthTokenHandler
{
    protected readonly HttpClient HttpClient;

    /// <summary>Initialize Figma auth token handler</summary>
    public FigmaAuthTokenHandler(HttpClient httpClient)
    {
        HttpClient = httpClient;
    }

    /// <summary>Refresh Figma OAuth access token</summary>
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

            var response = await HttpClient.PostAsync("https://www.figma.com/api/oauth/refresh", content, cancellationToken);
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

            return new AuthToken(accessToken, refreshToken);
        }
        catch (Exception ex) when (ex is JsonException or HttpRequestException or TaskCanceledException)
        {
            return null;
        }
    }

    /// <summary>Validate Figma OAuth access token</summary>
    public async Task<bool> ValidateAccessTokenAsync(IAuthTokenHandlerSession authSession, CancellationToken cancellationToken = default)
    {
        var token = authSession.AuthToken?.AccessToken;
        if (string.IsNullOrWhiteSpace(token))
            return false;

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "https://api.figma.com/v1/me");
            request.Headers.Add("X-Figma-Token", token);

            var response = await HttpClient.SendAsync(request, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return false;
        }
    }

    /// <summary>Get user info from Figma API</summary>
    public async Task<UserInfo?> GetUserInfoAsync(IAuthTokenHandlerSession authSession, CancellationToken cancellationToken = default)
    {
        var token = authSession.AuthToken?.AccessToken;
        if (string.IsNullOrWhiteSpace(token))
            return null;

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "https://api.figma.com/v1/me");
            request.Headers.Add("X-Figma-Token", token);

            var response = await HttpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var id = root.GetProperty("id").GetString() ?? "";
            var email = root.TryGetProperty("email", out var emailProp) ? emailProp.GetString() : null;
            var handle = root.TryGetProperty("handle", out var handleProp) ? handleProp.GetString() : null;
            var imgUrl = root.TryGetProperty("img_url", out var imgProp) ? imgProp.GetString() : null;

            return new UserInfo(id, email ?? handle ?? id, handle, imgUrl);
        }
        catch (Exception ex) when (ex is JsonException or HttpRequestException or TaskCanceledException)
        {
            return null;
        }
    }

    /// <summary>Get Figma OAuth token lifetime</summary>
    public Task<TokenLifetime?> GetAccessTokenLifetimeAsync(IAuthTokenHandlerSession authSession, CancellationToken cancellationToken = default)
    {
        // Figma tokens expire after 15 days
        return Task.FromResult<TokenLifetime?>(new TokenLifetime(DateTimeOffset.UtcNow.AddDays(15)));
    }
}

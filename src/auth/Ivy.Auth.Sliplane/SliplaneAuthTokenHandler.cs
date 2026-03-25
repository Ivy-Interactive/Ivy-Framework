using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Ivy.Auth.Sliplane;

/// <summary>Sliplane auth token handler</summary>
[OAuthTokenHandler(OAuthProviders.Sliplane)]
public class SliplaneAuthTokenHandler : IAuthTokenHandler
{
    protected readonly HttpClient HttpClient;
    protected readonly string ClientId;
    protected readonly string ClientSecret;
    protected readonly string AuthorizationUrl;
    protected readonly string TokenUrl;
    protected readonly string Scope;
    private readonly ILogger<SliplaneAuthTokenHandler>? _logger;

    /// <summary>Initialize Sliplane auth token handler</summary>
    public SliplaneAuthTokenHandler(IConfiguration configuration, ILogger<SliplaneAuthTokenHandler>? logger = null)
    {
        ClientId = configuration.GetValue<string>("Sliplane:ClientId")
            ?? throw new InvalidOperationException(
                "Missing required configuration: 'Sliplane:ClientId'. Please set this value in your environment variables or user secrets.");

        ClientSecret = configuration.GetValue<string>("Sliplane:ClientSecret")
            ?? throw new InvalidOperationException(
                "Missing required configuration: 'Sliplane:ClientSecret'. Please set this value in your environment variables or user secrets.");

        AuthorizationUrl = configuration.GetValue<string>("Sliplane:AuthorizationUrl")
            ?? "https://api.sliplane.io/web/oauth/authorize";

        TokenUrl = configuration.GetValue<string>("Sliplane:TokenUrl")
            ?? "https://api.sliplane.io/web/oauth/token";

        Scope = configuration.GetValue<string>("Sliplane:Scope") ?? "full";

        HttpClient = new HttpClient();
        _logger = logger;
    }

    /// <summary>Refresh the access token using the stored refresh token</summary>
    public async Task<AuthToken?> RefreshAccessTokenAsync(IAuthTokenHandlerSession authSession, CancellationToken cancellationToken = default)
    {
        var token = authSession.AuthToken;
        if (token?.RefreshToken == null)
        {
            return null;
        }

        try
        {
            using var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "refresh_token"),
                new KeyValuePair<string, string>("refresh_token", token.RefreshToken),
                new KeyValuePair<string, string>("client_id", ClientId),
                new KeyValuePair<string, string>("client_secret", ClientSecret),
            });

            var response = await HttpClient.PostAsync(TokenUrl, content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger?.LogError("Failed to refresh Sliplane token. Status: {StatusCode}, Response: {Response}",
                    response.StatusCode, errorContent);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var tokenResponse = JsonSerializer.Deserialize<SliplaneTokenResponse>(json);

            if (tokenResponse == null || string.IsNullOrWhiteSpace(tokenResponse.AccessToken))
            {
                return null;
            }

            return new AuthToken(
                tokenResponse.AccessToken,
                tokenResponse.RefreshToken ?? token.RefreshToken
            );
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to refresh Sliplane token");
            return null;
        }
    }

    /// <summary>Validate the access token by calling the Sliplane API</summary>
    public async Task<bool> ValidateAccessTokenAsync(IAuthTokenHandlerSession authSession, CancellationToken cancellationToken = default)
    {
        var token = authSession.AuthToken?.AccessToken;
        if (string.IsNullOrWhiteSpace(token))
            return false;

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "https://ctrl.sliplane.io/v0/projects");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await HttpClient.SendAsync(request, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return false;
        }
    }

    /// <summary>
    /// Get user info from Sliplane using the /v0/me endpoint.
    /// </summary>
    public async Task<UserInfo?> GetUserInfoAsync(IAuthTokenHandlerSession authSession, CancellationToken cancellationToken = default)
    {
        var token = authSession.AuthToken?.AccessToken;
        if (string.IsNullOrWhiteSpace(token))
            return null;

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "https://ctrl.sliplane.io/v0/me");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await HttpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.ValueKind != JsonValueKind.Object ||
                !root.TryGetProperty("user", out var user) ||
                user.ValueKind != JsonValueKind.Object)
            {
                _logger?.LogWarning("Sliplane /v0/me response did not contain a 'user' object. Raw response: {Json}", json);
                return null;
            }

            var id = user.TryGetProperty("id", out var idProp) && idProp.ValueKind == JsonValueKind.String
                ? idProp.GetString()
                : null;

            var email = user.TryGetProperty("email", out var emailProp) && emailProp.ValueKind == JsonValueKind.String
                ? emailProp.GetString()
                : null;

            var name = user.TryGetProperty("name", out var nameProp) && nameProp.ValueKind == JsonValueKind.String
                ? nameProp.GetString()
                : null;

            var avatarUrl = user.TryGetProperty("avatarUrl", out var avatarProp) && avatarProp.ValueKind == JsonValueKind.String
                ? avatarProp.GetString()
                : null;

            if (string.IsNullOrWhiteSpace(id) && string.IsNullOrWhiteSpace(email))
            {
                _logger?.LogWarning("Sliplane /v0/me user object did not contain id or email. Raw response: {Json}", json);
                return null;
            }

            return new UserInfo(
                Id: id ?? email ?? string.Empty,
                Email: email ?? string.Empty,
                FullName: name,
                AvatarUrl: avatarUrl
            );
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            _logger?.LogError(ex, "Failed to get Sliplane user info");
            return null;
        }
    }

    /// <summary>Sliplane tokens do not carry expiration info — returns null</summary>
    public Task<TokenLifetime?> GetAccessTokenLifetimeAsync(IAuthTokenHandlerSession authSession, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<TokenLifetime?>(null);
    }

    protected class SliplaneTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("token_type")]
        public string? TokenType { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; set; }

        [JsonPropertyName("scope")]
        public string? Scope { get; set; }
    }
}

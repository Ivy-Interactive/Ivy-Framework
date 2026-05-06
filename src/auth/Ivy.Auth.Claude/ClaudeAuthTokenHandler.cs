using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Ivy.Auth.Claude;

/// <summary>OAuth access token / refresh token response from Anthropic</summary>
internal sealed class ClaudeTokenResponse
{
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; init; }

    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; init; }

    [JsonPropertyName("expires_in")]
    public int? ExpiresIn { get; init; }

    [JsonPropertyName("error")]
    public string? Error { get; init; }

    [JsonPropertyName("error_description")]
    public string? ErrorDescription { get; init; }
}

/// <summary>Claude (Anthropic) OAuth token handler for brokered sessions and user info</summary>
[OAuthTokenHandler(OAuthProviders.Claude)]
public class ClaudeAuthTokenHandler : IAuthTokenHandler
{
    protected readonly HttpClient HttpClient;
    protected readonly string ClientId;
    protected readonly string? ClientSecret;
    protected readonly string TokenUrl;
    protected readonly string? UserInfoUrl;
    private readonly ILogger<ClaudeAuthTokenHandler>? _logger;

    /// <summary>Initialize Claude auth token handler</summary>
    public ClaudeAuthTokenHandler(IConfiguration configuration, ILogger<ClaudeAuthTokenHandler>? logger = null)
    {
        ClientId = configuration.GetValue<string>("Claude:ClientId")
            ?? throw new InvalidOperationException(
                "Missing required configuration: 'Claude:ClientId'. Set user secrets or environment variables. See Ivy docs for Claude authentication.");

        var secret = configuration.GetValue<string>("Claude:ClientSecret");
        ClientSecret = string.IsNullOrWhiteSpace(secret) ? null : secret;

        TokenUrl = configuration.GetValue<string>("Claude:TokenUrl")
            ?? "https://console.anthropic.com/v1/oauth/token";

        UserInfoUrl = configuration.GetValue<string>("Claude:UserInfoUrl")
            ?? "https://api.anthropic.com/api/oauth/claude_cli/client_data";

        var userAgent = AuthProviderHelpers.GetUserAgent(configuration, "Claude:UserAgent");
        HttpClient = new HttpClient();
        HttpClient.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<AuthToken?> RefreshAccessTokenAsync(
        IAuthTokenHandlerSession authSession,
        CancellationToken cancellationToken = default)
    {
        var refresh = authSession.AuthToken?.RefreshToken;
        if (string.IsNullOrWhiteSpace(refresh))
            return null;

        try
        {
            var requestBody = new RefreshTokenRequest
            {
                GrantType = "refresh_token",
                ClientId = ClientId,
                ClientSecret = ClientSecret,
                RefreshToken = refresh
            };

            using var response = await PostJsonTokenRequestAsync(requestBody, cancellationToken).ConfigureAwait(false);
            var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            var tokenResponse = JsonSerializer.Deserialize<ClaudeTokenResponse>(json);

            if (tokenResponse?.Error != null)
            {
                throw new ClaudeOAuthException(tokenResponse.Error, tokenResponse.ErrorDescription);
            }

            if (string.IsNullOrWhiteSpace(tokenResponse?.AccessToken))
                return null;

            return new AuthToken(
                tokenResponse.AccessToken,
                tokenResponse.RefreshToken ?? refresh
            );
        }
        catch (Exception ex) when (ex is not ClaudeOAuthException)
        {
            _logger?.LogError(ex, "Claude OAuth token refresh failed");
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<bool> ValidateAccessTokenAsync(
        IAuthTokenHandlerSession authSession,
        CancellationToken cancellationToken = default)
    {
        var info = await GetUserInfoAsync(authSession, cancellationToken).ConfigureAwait(false);
        return info != null;
    }

    /// <inheritdoc />
    public async Task<UserInfo?> GetUserInfoAsync(
        IAuthTokenHandlerSession authSession,
        CancellationToken cancellationToken = default)
    {
        var token = authSession.AuthToken?.AccessToken;
        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(UserInfoUrl))
            return null;

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, UserInfoUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                _logger?.LogWarning(
                    "Claude user info request failed with {Status}. Response may change when Anthropic updates OAuth APIs.",
                    response.StatusCode);
                return FallbackUserInfoFromToken(token);
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            return ParseUserInfo(json, token);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            if (!cancellationToken.IsCancellationRequested)
                _logger?.LogError(ex, "Exception during Claude user info request");
            return FallbackUserInfoFromToken(token);
        }
    }

    /// <inheritdoc />
    public Task<TokenLifetime?> GetAccessTokenLifetimeAsync(
        IAuthTokenHandlerSession authSession,
        CancellationToken cancellationToken = default)
        => Task.FromResult<TokenLifetime?>(null);

    internal static UserInfo? ParseUserInfo(string json, string accessTokenFallback)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            string? email = TryGetString(root, "email")
                ?? TryNestedString(root, "account", "email")
                ?? TryNestedString(root, "user", "email")
                ?? TryNestedString(root, "profile", "email");

            string? id = TryGetString(root, "account_uuid")
                ?? TryGetString(root, "user_id")
                ?? TryGetString(root, "id")
                ?? TryNestedString(root, "user", "id");

            string? name = TryGetString(root, "name")
                ?? TryNestedString(root, "profile", "name")
                ?? TryNestedString(root, "user", "name");

            string? avatar = TryGetString(root, "avatar_url")
                ?? TryNestedString(root, "profile", "avatar_url");

            if (string.IsNullOrEmpty(id) && string.IsNullOrEmpty(email))
                return FallbackUserInfoFromToken(accessTokenFallback);

            id ??= email ?? "claude-oauth";
            email ??= id;

            return new UserInfo(id, email, name, avatar);
        }
        catch (JsonException)
        {
            return FallbackUserInfoFromToken(accessTokenFallback);
        }
    }

    private static string? TryGetString(JsonElement root, string name) =>
        root.TryGetProperty(name, out var p) && p.ValueKind == JsonValueKind.String ? p.GetString() : null;

    private static string? TryNestedString(JsonElement root, string obj, string prop)
    {
        if (!root.TryGetProperty(obj, out var inner) || inner.ValueKind != JsonValueKind.Object)
            return null;
        return TryGetString(inner, prop);
    }

    private static UserInfo? FallbackUserInfoFromToken(string token)
    {
        var id = token.Length >= 16 ? token[..16] : token;
        return new UserInfo(id, "claude-oauth@local.invalid", null, null);
    }

    internal async Task<HttpResponseMessage> PostJsonTokenRequestAsync(
        object body,
        CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(body);
        using var request = new HttpRequestMessage(HttpMethod.Post, TokenUrl)
        {
            Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
        };
        request.Headers.TryAddWithoutValidation("Origin", "https://claude.ai");
        request.Headers.TryAddWithoutValidation("Referer", "https://claude.ai/");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        return await HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }

    private sealed class RefreshTokenRequest
    {
        [JsonPropertyName("grant_type")]
        public string GrantType { get; init; } = "";

        [JsonPropertyName("client_id")]
        public string ClientId { get; init; } = "";

        [JsonPropertyName("client_secret")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ClientSecret { get; init; }

        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; init; } = "";
    }
}

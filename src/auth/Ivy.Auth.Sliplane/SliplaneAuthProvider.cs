using Ivy.Core;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Ivy.Auth;
using Ivy.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;


namespace Ivy.Auth.Sliplane;

/// <summary>Sliplane OAuth exception</summary>
public class SliplaneOAuthException(string? error, string? errorDescription)
    : Exception($"Sliplane OAuth error: '{error}' - {errorDescription}")
{
    public string? Error { get; } = error;
    public string? ErrorDescription { get; } = errorDescription;
}

/// <summary>
/// Sliplane OAuth2 authentication provider for Ivy applications.
/// Implements the OAuth2 authorization code flow using the Sliplane API.
/// </summary>
public class SliplaneAuthProvider : IAuthProvider
{
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly string _authorizationUrl;
    private readonly string _tokenUrl;
    private readonly string _scope;
    private readonly HttpClient _httpClient;
    private readonly ILogger<SliplaneAuthProvider>? _logger;

    /// <summary>Initialize Sliplane auth provider from configuration</summary>
    public SliplaneAuthProvider(IConfiguration configuration, ILogger<SliplaneAuthProvider>? logger = null)
    {
        _clientId = configuration.GetValue<string>("Sliplane:ClientId")
            ?? throw new InvalidOperationException(
                "Missing required configuration: 'Sliplane:ClientId'. Please set this value in your environment variables or user secrets.");

        _clientSecret = configuration.GetValue<string>("Sliplane:ClientSecret")
            ?? throw new InvalidOperationException(
                "Missing required configuration: 'Sliplane:ClientSecret'. Please set this value in your environment variables or user secrets.");

        _authorizationUrl = configuration.GetValue<string>("Sliplane:AuthorizationUrl")
            ?? "https://api.sliplane.io/web/oauth/authorize";

        _tokenUrl = configuration.GetValue<string>("Sliplane:TokenUrl")
            ?? "https://api.sliplane.io/web/oauth/token";

        _scope = configuration.GetValue<string>("Sliplane:Scope") ?? "full";

        _httpClient = new HttpClient();
        _logger = logger;
    }

    /// <summary>Not supported — Sliplane only supports OAuth flow</summary>
    public Task<AuthToken?> LoginAsync(IAuthSession authSession, string email, string password, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("Sliplane authentication only supports OAuth flow. Use GetOAuthUriAsync and HandleOAuthCallbackAsync instead.");
    }

    /// <summary>No-op logout — Sliplane tokens are stateless</summary>
    public Task LogoutAsync(IAuthSession authSession, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    /// <summary>Refresh the access token using the stored refresh token</summary>
    public async Task<AuthToken?> RefreshAccessTokenAsync(IAuthSession authSession, CancellationToken cancellationToken = default)
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
                new KeyValuePair<string, string>("client_id", _clientId),
                new KeyValuePair<string, string>("client_secret", _clientSecret),
            });

            var response = await _httpClient.PostAsync(_tokenUrl, content, cancellationToken);

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
    public async Task<bool> ValidateAccessTokenAsync(IAuthSession authSession, CancellationToken cancellationToken = default)
    {
        var token = authSession.AuthToken?.AccessToken;
        if (string.IsNullOrWhiteSpace(token))
            return false;

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "https://ctrl.sliplane.io/v0/projects");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.SendAsync(request, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return false;
        }
    }

    /// <summary>Get user info — Sliplane does not expose a user-info endpoint, so a basic placeholder is returned after token validation</summary>
    public async Task<UserInfo?> GetUserInfoAsync(IAuthSession authSession, CancellationToken cancellationToken = default)
    {
        var token = authSession.AuthToken?.AccessToken;
        if (string.IsNullOrWhiteSpace(token))
            return null;

        try
        {
            // Verify token is valid by calling the projects API
            using var request = new HttpRequestMessage(HttpMethod.Get, "https://ctrl.sliplane.io/v0/projects");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            // Sliplane OAuth does not provide a user info endpoint.
            // Return a placeholder — apps can override this by wrapping the provider.
            return new UserInfo(
                Id: "sliplane-user",
                Email: "user@sliplane.io",
                FullName: "Sliplane User",
                AvatarUrl: null
            );
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            _logger?.LogError(ex, "Failed to get Sliplane user info");
            return null;
        }
    }

    /// <summary>Returns the Sliplane OAuth auth option</summary>
    public AuthOption[] GetAuthOptions() =>
        [new AuthOption(AuthFlow.OAuth, "Sliplane", "sliplane", Icons.Rocket)];

    /// <summary>Build the Sliplane OAuth authorization URI</summary>
    public Task<Uri> GetOAuthUriAsync(IAuthSession authSession, AuthOption option, WebhookEndpoint callback, CancellationToken cancellationToken = default)
    {
        if (option.Id != "sliplane")
        {
            throw new ArgumentException($"Unknown auth option: {option.Id}", nameof(option));
        }

        var callbackUri = callback.GetUri(includeIdInPath: false);

        var authUrl = new UriBuilder(_authorizationUrl)
        {
            Query = string.Join("&", new[]
            {
                $"client_id={Uri.EscapeDataString(_clientId)}",
                $"redirect_uri={Uri.EscapeDataString(callbackUri.ToString())}",
                $"response_type=code",
                $"scope={Uri.EscapeDataString(_scope)}",
                $"state={Uri.EscapeDataString(callback.Id)}",
            })
        };

        return Task.FromResult(authUrl.Uri);
    }

    /// <summary>Handle the OAuth callback and exchange the authorization code for tokens</summary>
    public async Task<AuthToken?> HandleOAuthCallbackAsync(IAuthSession authSession, HttpRequest request, CancellationToken cancellationToken = default)
    {
        var code = request.Query["code"].ToString();
        var error = request.Query["error"].ToString();
        var errorDescription = request.Query["error_description"].ToString();

        if (error.Length > 0 || errorDescription.Length > 0)
        {
            throw new SliplaneOAuthException(error, errorDescription);
        }

        if (code.Length == 0)
        {
            throw new Exception("Received no authorization code from Sliplane.");
        }

        try
        {
            var scheme = request.Headers.TryGetValue("X-Forwarded-Proto", out var forwardedProto)
                ? forwardedProto.ToString()
                : request.Scheme;

            var redirectUri = $"{scheme}://{request.Host}{request.Path}";

            using var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("code", code),
                new KeyValuePair<string, string>("client_id", _clientId),
                new KeyValuePair<string, string>("client_secret", _clientSecret),
                new KeyValuePair<string, string>("redirect_uri", redirectUri),
            });

            var response = await _httpClient.PostAsync(_tokenUrl, content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger?.LogError("Failed to exchange Sliplane code for token. Status: {StatusCode}, Response: {Response}",
                    response.StatusCode, errorContent);
                throw new HttpRequestException(
                    $"Sliplane token exchange failed with status {(int)response.StatusCode}: {errorContent}",
                    null,
                    response.StatusCode
                );
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var tokenResponse = JsonSerializer.Deserialize<SliplaneTokenResponse>(json);

            if (tokenResponse == null || string.IsNullOrWhiteSpace(tokenResponse.AccessToken))
            {
                throw new Exception("Invalid token response from Sliplane.");
            }

            return new AuthToken(
                tokenResponse.AccessToken,
                tokenResponse.RefreshToken
            );
        }
        catch (Exception ex) when (ex is not SliplaneOAuthException and not HttpRequestException)
        {
            _logger?.LogError(ex, "Unexpected error during Sliplane OAuth callback");
            throw new InvalidOperationException($"Unexpected error during Sliplane OAuth token exchange: {ex.Message}", ex);
        }
    }

    /// <summary>Sliplane tokens do not carry expiration info — returns null</summary>
    public Task<TokenLifetime?> GetAccessTokenLifetimeAsync(IAuthSession authSession, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<TokenLifetime?>(null);
    }

    private class SliplaneTokenResponse
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

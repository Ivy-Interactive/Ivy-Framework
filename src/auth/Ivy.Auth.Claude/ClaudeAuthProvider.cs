using System.Text.Json;
using System.Text.Json.Serialization;
using Ivy.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Ivy.Auth.Claude;

/// <summary>Anthropic Claude.ai OAuth 2.0 with PKCE (authorization code flow)</summary>
public class ClaudeAuthProvider : ClaudeAuthTokenHandler, IAuthProvider
{
    private readonly string _redirectUri;
    private readonly string _authorizationUrl;
    private readonly string _scope;
    private string? _codeVerifier;

    /// <summary>Initialize Claude auth provider</summary>
    public ClaudeAuthProvider(IConfiguration configuration, ILogger<ClaudeAuthTokenHandler>? logger = null)
        : base(configuration, logger)
    {
        _redirectUri = configuration.GetValue<string>("Claude:RedirectUri")
            ?? throw new InvalidOperationException(
                "Missing required configuration: 'Claude:RedirectUri'. Must match the callback URL registered for your OAuth client (for example https://localhost:5010/ivy/auth/callback).");

        _authorizationUrl = configuration.GetValue<string>("Claude:AuthorizationUrl")
            ?? "https://claude.ai/oauth/authorize";

        _scope = configuration.GetValue<string>("Claude:Scope")
            ?? "user:profile user:inference";
    }

    /// <inheritdoc />
    public Task<LoginResult> LoginAsync(
        IAuthSession authSession,
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException(
            "Claude authentication only supports OAuth. Use GetOAuthUriAsync and HandleOAuthCallbackAsync instead.");
    }

    /// <inheritdoc />
    public Task<Uri> GetOAuthUriAsync(
        IAuthSession authSession,
        AuthOption option,
        WebhookEndpoint callback,
        CancellationToken cancellationToken = default)
    {
        _codeVerifier = GenerateCodeVerifier();
        var codeChallenge = GenerateCodeChallenge(_codeVerifier);

        var query = string.Join("&", new[]
        {
            $"code=true",
            $"client_id={Uri.EscapeDataString(ClientId)}",
            "response_type=code",
            $"redirect_uri={Uri.EscapeDataString(_redirectUri)}",
            $"scope={Uri.EscapeDataString(_scope)}",
            $"code_challenge={Uri.EscapeDataString(codeChallenge)}",
            "code_challenge_method=S256",
            $"state={Uri.EscapeDataString(callback.Id)}",
        });

        var uri = new UriBuilder(_authorizationUrl) { Query = query }.Uri;
        return Task.FromResult(uri);
    }

    /// <inheritdoc />
    public async Task<AuthToken?> HandleOAuthCallbackAsync(
        IAuthSession authSession,
        HttpRequest request,
        CancellationToken cancellationToken = default)
    {
        var code = request.Query["code"].ToString();
        var error = request.Query["error"].ToString();
        var errorDescription = request.Query["error_description"].ToString();

        if (error.Length > 0 || errorDescription.Length > 0)
            throw new ClaudeOAuthException(error, errorDescription);

        if (string.IsNullOrEmpty(code))
        {
            throw new InvalidOperationException(
                "No authorization code in the OAuth callback. The user may have denied access or the redirect URI may be misconfigured.");
        }

        if (string.IsNullOrEmpty(_codeVerifier))
        {
            throw new InvalidOperationException(
                "PKCE verifier is missing. Start sign-in again from your app (the OAuth flow was not initiated in this process).");
        }

        try
        {
            var state = request.Query["state"].ToString();
            var exchange = new AuthorizationCodeTokenRequest
            {
                GrantType = "authorization_code",
                ClientId = ClientId,
                ClientSecret = ClientSecret,
                Code = code,
                RedirectUri = _redirectUri,
                CodeVerifier = _codeVerifier,
                State = string.IsNullOrEmpty(state) ? null : state
            };

            using var response = await PostJsonTokenRequestAsync(exchange, cancellationToken).ConfigureAwait(false);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"Claude token exchange failed: {(int)response.StatusCode} {response.ReasonPhrase}. {responseContent}",
                    null,
                    response.StatusCode);
            }

            var tokenResponse = JsonSerializer.Deserialize<ClaudeTokenResponse>(responseContent);
            if (tokenResponse?.Error != null)
            {
                throw new ClaudeOAuthException(tokenResponse.Error, tokenResponse.ErrorDescription);
            }

            if (string.IsNullOrWhiteSpace(tokenResponse?.AccessToken))
                return null;

            _codeVerifier = null;

            return new AuthToken(
                tokenResponse.AccessToken,
                tokenResponse.RefreshToken
            );
        }
        catch (HttpRequestException)
        {
            throw;
        }
        catch (ClaudeOAuthException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Claude OAuth token exchange failed: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public Task LogoutAsync(IAuthSession authSession, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    /// <inheritdoc />
    public AuthOption[] GetAuthOptions() =>
        [new AuthOption(AuthFlow.OAuth, "Claude", OAuthProviders.Claude, Icons.ClaudeCode)];

    /// <inheritdoc />
    public Task<BrokeredSessionsResult> GetBrokeredSessionsAsync(
        IAuthSession authSession,
        bool skipCache = false,
        CancellationToken cancellationToken = default)
        => Task.FromResult(BrokeredSessionsResult.Success([]));

    private static string GenerateCodeVerifier()
    {
        var bytes = new byte[32];
        System.Security.Cryptography.RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private static string GenerateCodeChallenge(string codeVerifier)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(codeVerifier);
        var hash = System.Security.Cryptography.SHA256.HashData(bytes);
        return Convert.ToBase64String(hash)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private sealed class AuthorizationCodeTokenRequest
    {
        [JsonPropertyName("grant_type")]
        public string GrantType { get; init; } = "";

        [JsonPropertyName("client_id")]
        public string ClientId { get; init; } = "";

        [JsonPropertyName("client_secret")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ClientSecret { get; init; }

        [JsonPropertyName("code")]
        public string Code { get; init; } = "";

        [JsonPropertyName("redirect_uri")]
        public string RedirectUri { get; init; } = "";

        [JsonPropertyName("code_verifier")]
        public string CodeVerifier { get; init; } = "";

        [JsonPropertyName("state")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? State { get; init; }
    }
}

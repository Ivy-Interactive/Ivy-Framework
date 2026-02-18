using System.Security.Claims;
using System.Text;
using Ivy.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Supabase;
using Supabase.Gotrue;
using GotrueConstants = global::Supabase.Gotrue.Constants;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;

namespace Ivy.Auth.Supabase;

public class SupabaseOAuthException(string? error, string? errorCode, string? errorDescription)
    : Exception($"Supabase error: '{error}', code '{errorCode}' - {errorDescription}")
{
    public string? Error { get; } = error;
    public string? ErrorCode { get; } = errorCode;
    public string? ErrorDescription { get; } = errorDescription;
}

public class SupabaseAuthProvider : IAuthProvider
{
    private readonly global::Supabase.Client _client;
    private readonly HttpClient _httpClient;
    private readonly string _jwksUrl;
    private readonly string _issuer;
    private readonly SymmetricSecurityKey? _legacyJwtKey = null;

    private readonly List<AuthOption> _authOptions = new();

    private string? _pkceCodeVerifier = null;

    private JsonWebKeySet? _cachedJwks = null;
    private DateTime _jwksCacheExpiry = DateTime.MinValue;

    public SupabaseAuthProvider(IConfiguration configuration)
    {
        var url = configuration.GetValue<string>("Supabase:Url") ?? throw new Exception("Supabase:Url is required");
        var apiKey = configuration.GetValue<string>("Supabase:ApiKey") ?? throw new Exception("Supabase:ApiKey is required");
        var legacyJwtSecret = configuration.GetValue<string?>("Supabase:LegacyJwtSecret");
        if (!string.IsNullOrEmpty(legacyJwtSecret))
        {
            var keyBytes = Encoding.UTF8.GetBytes(legacyJwtSecret);
            _legacyJwtKey = new SymmetricSecurityKey(keyBytes);
        }

        var options = new SupabaseOptions
        {
            AutoRefreshToken = false,
            AutoConnectRealtime = false
        };

        _client = new global::Supabase.Client(url, apiKey, options);

        var userAgent = AuthProviderHelpers.GetUserAgent(configuration, "Supabase:UserAgent");
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("User-Agent", userAgent);

        // Setup JWKS URL
        _issuer = new Uri(new Uri(url), "auth/v1").ToString();
        _jwksUrl = $"{_issuer}/.well-known/jwks.json";
    }

    public async Task<AuthToken?> LoginAsync(IAuthSession authSession, string email, string password, CancellationToken cancellationToken)
    {
        var session = await _client.Auth.SignIn(email, password)
            .WaitAsync(cancellationToken);
        var authToken = MakeAuthToken(session);
        return authToken;
    }

    public async Task<Uri> GetOAuthUriAsync(IAuthSession authSession, AuthOption option, WebhookEndpoint callback, CancellationToken cancellationToken)
    {
        var provider = option.Id switch
        {
            "google" => GotrueConstants.Provider.Google,
            "apple" => GotrueConstants.Provider.Apple,
            "discord" => GotrueConstants.Provider.Discord,
            "twitch" => GotrueConstants.Provider.Twitch,
            "figma" => GotrueConstants.Provider.Figma,
            "notion" => GotrueConstants.Provider.Notion,
            "azure" => GotrueConstants.Provider.Azure,
            "workos" => GotrueConstants.Provider.WorkOS,
            "github" => GotrueConstants.Provider.Github,
            "gitlab" => GotrueConstants.Provider.Gitlab,
            "bitbucket" => GotrueConstants.Provider.Bitbucket,
            _ => throw new ArgumentException($"Unknown OAuth provider: {option.Id}"),
        };

        var signInOptions = new SignInOptions
        {
            RedirectTo = callback.GetUri().ToString(),
            FlowType = GotrueConstants.OAuthFlowType.PKCE,
        };

        // Set scopes. These are necessary for Discord, but some providers return errors if they're provided.
        if (provider != GotrueConstants.Provider.Gitlab
            && provider != GotrueConstants.Provider.Figma
            && provider != GotrueConstants.Provider.Twitch
            && provider != GotrueConstants.Provider.WorkOS)
        {
            signInOptions.Scopes = "email openid";
        }

        if (provider == GotrueConstants.Provider.WorkOS)
        {
            if (option.Tag is not string connectionId || string.IsNullOrEmpty(connectionId))
            {
                throw new ArgumentException("WorkOS connection ID not provided.");
            }

            signInOptions.QueryParams = new()
            {
                ["connection"] = connectionId,
            };
        }

        var providerAuthState = await _client.Auth.SignIn(provider, signInOptions)
            .WaitAsync(cancellationToken);
        _pkceCodeVerifier = providerAuthState.PKCEVerifier;

        return providerAuthState.Uri;
    }

    public async Task<AuthToken?> HandleOAuthCallbackAsync(IAuthSession authSession, HttpRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_pkceCodeVerifier))
        {
            throw new InvalidOperationException("PKCE code verifier is not set. OAuth flow was not properly initiated.");
        }

        var code = request.Query["code"].ToString();

        if (string.IsNullOrWhiteSpace(code))
        {
            var error = request.Query["error"].ToString();
            var errorCode = request.Query["error_code"].ToString();
            var errorDescription = request.Query["error_description"].ToString();

            throw new SupabaseOAuthException(error, errorCode, errorDescription);
        }

        try
        {
            var session = await _client.Auth.ExchangeCodeForSession(_pkceCodeVerifier, code)
                .WaitAsync(cancellationToken);
            return MakeAuthToken(session);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to exchange authorization code: {ex.Message}", ex);
        }
    }

    public async Task LogoutAsync(IAuthSession authSession, CancellationToken cancellationToken)
    {
        await _client.Auth.SignOut()
            .WaitAsync(cancellationToken);
    }

    public async Task<AuthToken?> RefreshAccessTokenAsync(IAuthSession authSession, CancellationToken cancellationToken)
    {
        if (authSession.AuthToken is not { } token || token.RefreshToken == null)
        {
            return null;
        }

        try
        {
            var session = await _client.Auth.SetSession(token.AccessToken, token.RefreshToken, forceAccessTokenRefresh: true)
                .WaitAsync(cancellationToken);
            var authToken = MakeAuthToken(session);
            return authToken;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<bool> ValidateAccessTokenAsync(IAuthSession authSession, CancellationToken cancellationToken)
    {
        return await VerifyToken(authSession.AuthToken?.AccessToken, cancellationToken) is not null;
    }

    public async Task<UserInfo?> GetUserInfoAsync(IAuthSession authSession, CancellationToken cancellationToken)
    {
        if (await VerifyToken(authSession.AuthToken?.AccessToken, cancellationToken) is not var (claims, _))
        {
            return null;
        }

        var userId = claims.FindFirst("sub")?.Value;
        var email = claims.FindFirst("email")?.Value;
        string? name = null, avatarUrl = null;

        var metadataJson = claims.FindFirst("user_metadata")?.Value;
        try
        {
            if (!string.IsNullOrEmpty(metadataJson))
            {
                using var doc = JsonDocument.Parse(metadataJson);
                var root = doc.RootElement;

                if (root.TryGetProperty("full_name", out var fullNameProperty))
                {
                    name = fullNameProperty.GetString();
                }
                if (root.TryGetProperty("avatar_url", out var avatarUrlProperty))
                {
                    avatarUrl = avatarUrlProperty.GetString();
                }
            }
        }
        catch (JsonException)
        {
            // Ignore JSON parsing errors
        }

        if (userId == null || email == null)
        {
            return null;
        }

        return new UserInfo(
            userId,
            email,
            name,
            avatarUrl
        );
    }

    public AuthOption[] GetAuthOptions()
    {
        return _authOptions.ToArray();
    }

    public async Task<TokenLifetime?> GetAccessTokenLifetimeAsync(IAuthSession authSession, CancellationToken cancellationToken)
    {
        if (await VerifyToken(authSession.AuthToken?.AccessToken, cancellationToken) is var (_, expiration))
        {
            return new TokenLifetime(expiration);
        }
        else
        {
            return null;
        }
    }

    public SupabaseAuthProvider UseEmailPassword()
    {
        _authOptions.Add(new AuthOption(AuthFlow.EmailPassword));
        return this;
    }

    public SupabaseAuthProvider UseGoogle()
    {
        _authOptions.Add(new AuthOption(AuthFlow.OAuth, "Google", nameof(GotrueConstants.Provider.Google).ToLower(), Icons.Google));
        return this;
    }

    public SupabaseAuthProvider UseApple()
    {
        _authOptions.Add(new AuthOption(AuthFlow.OAuth, "Apple", nameof(GotrueConstants.Provider.Apple).ToLower(), Icons.Apple));
        return this;
    }

    public SupabaseAuthProvider UseDiscord()
    {
        _authOptions.Add(new AuthOption(AuthFlow.OAuth, "Discord", nameof(GotrueConstants.Provider.Discord).ToLower(), Icons.Discord));
        return this;
    }

    public SupabaseAuthProvider UseTwitch()
    {
        _authOptions.Add(new AuthOption(AuthFlow.OAuth, "Twitch", nameof(GotrueConstants.Provider.Twitch).ToLower(), Icons.Twitch));
        return this;
    }

    public SupabaseAuthProvider UseFigma()
    {
        _authOptions.Add(new AuthOption(AuthFlow.OAuth, "Figma", nameof(GotrueConstants.Provider.Figma).ToLower(), Icons.Figma));
        return this;
    }

    public SupabaseAuthProvider UseNotion()
    {
        _authOptions.Add(new AuthOption(AuthFlow.OAuth, "Notion", nameof(GotrueConstants.Provider.Notion).ToLower(), Icons.Notion));
        return this;
    }

    public SupabaseAuthProvider UseAzure()
    {
        _authOptions.Add(new AuthOption(AuthFlow.OAuth, "Azure", nameof(GotrueConstants.Provider.Azure).ToLower(), Icons.Azure));
        return this;
    }

    public SupabaseAuthProvider UseWorkOS(string connectionId)
    {
        _authOptions.Add(new AuthOption(AuthFlow.OAuth, "WorkOS", nameof(GotrueConstants.Provider.WorkOS).ToLower(), Icons.None, connectionId));
        return this;
    }

    public SupabaseAuthProvider UseGithub()
    {
        _authOptions.Add(new AuthOption(AuthFlow.OAuth, "GitHub", nameof(GotrueConstants.Provider.Github).ToLower(), Icons.Github));
        return this;
    }

    public SupabaseAuthProvider UseGitlab()
    {
        _authOptions.Add(new AuthOption(AuthFlow.OAuth, "GitLab", nameof(GotrueConstants.Provider.Gitlab).ToLower(), Icons.Gitlab));
        return this;
    }

    public SupabaseAuthProvider UseBitbucket()
    {
        _authOptions.Add(new AuthOption(AuthFlow.OAuth, "Bitbucket", nameof(GotrueConstants.Provider.Bitbucket).ToLower(), Icons.Bitbucket));
        return this;
    }

    private async Task<(ClaimsPrincipal, DateTimeOffset)?> VerifyToken(string? jwt, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(jwt))
        {
            return null;
        }

        try
        {
            var handler = new JwtSecurityTokenHandler
            {
                InboundClaimTypeMap = new Dictionary<string, string>()
            };

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = "authenticated",
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(2),
            };

            var parsedToken = handler.ReadJwtToken(jwt);
            if (parsedToken.Header.Alg == SecurityAlgorithms.HmacSha256)
            {
                tokenValidationParameters.IssuerSigningKey = _legacyJwtKey;
            }
            else
            {
                // Check cache first
                if (_cachedJwks == null || DateTime.UtcNow >= _jwksCacheExpiry)
                {
                    var jwksJson = await _httpClient.GetStringAsync(_jwksUrl, cancellationToken);
                    _cachedJwks = new JsonWebKeySet(jwksJson);
                    _jwksCacheExpiry = DateTime.UtcNow.AddHours(24);
                }

                if (_cachedJwks.Keys.Count == 0)
                {
                    return null;
                }

                tokenValidationParameters.IssuerSigningKeys = _cachedJwks.Keys;
            }

            var claims = handler.ValidateToken(jwt, tokenValidationParameters, out SecurityToken validatedToken);
            return (claims, validatedToken.ValidTo);
        }
        catch (Exception)
        {
            return null;
        }
    }

    private AuthToken? MakeAuthToken(Session? session) =>
        session?.AccessToken != null
            ? new AuthToken(session.AccessToken, session.RefreshToken)
            : null;

}

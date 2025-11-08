using System.Reflection;
using System.Security.Claims;
using System.Text;
using Ivy.Hooks;
using Ivy.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Supabase;
using Supabase.Gotrue;
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
    private readonly string _jwksUrl;
    private readonly string _issuer;
    private readonly SymmetricSecurityKey? _legacyJwtKey = null;

    private readonly List<AuthOption> _authOptions = new();

    private string? _pkceCodeVerifier = null;

    private JsonWebKeySet? _cachedJwks = null;
    private DateTime _jwksCacheExpiry = DateTime.MinValue;

    public SupabaseAuthProvider()
    {
        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddUserSecrets(Assembly.GetEntryAssembly()!)
            .Build();

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

        // Setup JWKS URL
        _issuer = new Uri(new Uri(url), "auth/v1").ToString();
        _jwksUrl = $"{_issuer}/.well-known/jwks.json";
    }

    public async Task<AuthToken?> LoginAsync(string email, string password, CancellationToken cancellationToken)
    {
        var session = await _client.Auth.SignIn(email, password)
            .WaitAsync(cancellationToken);
        var authToken = MakeAuthToken(session);
        return authToken;
    }

    public async Task<Uri> GetOAuthUriAsync(AuthOption option, WebhookEndpoint callback, CancellationToken cancellationToken)
    {
        var provider = option.Id switch
        {
            "google" => Constants.Provider.Google,
            "apple" => Constants.Provider.Apple,
            "discord" => Constants.Provider.Discord,
            "twitch" => Constants.Provider.Twitch,
            "figma" => Constants.Provider.Figma,
            "notion" => Constants.Provider.Notion,
            "azure" => Constants.Provider.Azure,
            "workos" => Constants.Provider.WorkOS,
            "github" => Constants.Provider.Github,
            "gitlab" => Constants.Provider.Gitlab,
            "bitbucket" => Constants.Provider.Bitbucket,
            _ => throw new ArgumentException($"Unknown OAuth provider: {option.Id}"),
        };

        var signInOptions = new SignInOptions
        {
            RedirectTo = callback.GetUri().ToString(),
            FlowType = Constants.OAuthFlowType.PKCE,
        };

        // Set scopes. These are necessary for Discord, but some providers return errors if they're provided.
        if (provider != Constants.Provider.Gitlab
            && provider != Constants.Provider.Figma
            && provider != Constants.Provider.Twitch
            && provider != Constants.Provider.WorkOS)
        {
            signInOptions.Scopes = "email openid";
        }

        if (provider == Constants.Provider.WorkOS)
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

    public async Task<AuthToken?> HandleOAuthCallbackAsync(HttpRequest request, CancellationToken cancellationToken)
    {
        var code = request.Query["code"];

        var error = request.Query["error"];
        var errorCode = request.Query["error_code"];
        var errorDescription = request.Query["error_description"];
        if (error.Count > 0 || errorCode.Count > 0 || errorDescription.Count > 0)
        {
            throw new SupabaseOAuthException(error, errorCode, errorDescription);
        }
        else if (code.Count == 0)
        {
            throw new Exception("Received no recognized query parameters from Supabase.");
        }

        var session = await _client.Auth.ExchangeCodeForSession(_pkceCodeVerifier!, code.ToString())
            .WaitAsync(cancellationToken);
        var authToken = MakeAuthToken(session);
        return authToken;
    }

    public async Task LogoutAsync(string _, CancellationToken cancellationToken)
    {
        await _client.Auth.SignOut()
            .WaitAsync(cancellationToken);
    }

    public async Task<AuthToken?> RefreshAccessTokenAsync(AuthToken token, CancellationToken cancellationToken)
    {
        if (token.RefreshToken == null)
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

    public async Task<bool> ValidateAccessTokenAsync(string token, CancellationToken cancellationToken)
    {
        return await VerifyToken(token, cancellationToken) is not null;
    }

    public async Task<UserInfo?> GetUserInfoAsync(string token, CancellationToken cancellationToken)
    {
        if (await VerifyToken(token, cancellationToken) is not var (claims, _))
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

    public async Task<DateTimeOffset?> GetTokenExpiration(AuthToken token, CancellationToken cancellationToken)
    {
        if (await VerifyToken(token.AccessToken, cancellationToken) is var (_, expiration))
        {
            return expiration;
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
        _authOptions.Add(new AuthOption(AuthFlow.OAuth, "Google", nameof(Constants.Provider.Google).ToLower(), Icons.Google));
        return this;
    }

    public SupabaseAuthProvider UseApple()
    {
        _authOptions.Add(new AuthOption(AuthFlow.OAuth, "Apple", nameof(Constants.Provider.Apple).ToLower(), Icons.Apple));
        return this;
    }

    public SupabaseAuthProvider UseDiscord()
    {
        _authOptions.Add(new AuthOption(AuthFlow.OAuth, "Discord", nameof(Constants.Provider.Discord).ToLower(), Icons.Discord));
        return this;
    }

    public SupabaseAuthProvider UseTwitch()
    {
        _authOptions.Add(new AuthOption(AuthFlow.OAuth, "Twitch", nameof(Constants.Provider.Twitch).ToLower(), Icons.Twitch));
        return this;
    }

    public SupabaseAuthProvider UseFigma()
    {
        _authOptions.Add(new AuthOption(AuthFlow.OAuth, "Figma", nameof(Constants.Provider.Figma).ToLower(), Icons.Figma));
        return this;
    }

    public SupabaseAuthProvider UseNotion()
    {
        _authOptions.Add(new AuthOption(AuthFlow.OAuth, "Notion", nameof(Constants.Provider.Notion).ToLower(), Icons.Notion));
        return this;
    }

    public SupabaseAuthProvider UseAzure()
    {
        _authOptions.Add(new AuthOption(AuthFlow.OAuth, "Azure", nameof(Constants.Provider.Azure).ToLower(), Icons.Azure));
        return this;
    }

    public SupabaseAuthProvider UseWorkOS(string connectionId)
    {
        _authOptions.Add(new AuthOption(AuthFlow.OAuth, "WorkOS", nameof(Constants.Provider.WorkOS).ToLower(), Icons.None, connectionId));
        return this;
    }

    public SupabaseAuthProvider UseGithub()
    {
        _authOptions.Add(new AuthOption(AuthFlow.OAuth, "GitHub", nameof(Constants.Provider.Github).ToLower(), Icons.Github));
        return this;
    }

    public SupabaseAuthProvider UseGitlab()
    {
        _authOptions.Add(new AuthOption(AuthFlow.OAuth, "GitLab", nameof(Constants.Provider.Gitlab).ToLower(), Icons.Gitlab));
        return this;
    }

    public SupabaseAuthProvider UseBitbucket()
    {
        _authOptions.Add(new AuthOption(AuthFlow.OAuth, "Bitbucket", nameof(Constants.Provider.Bitbucket).ToLower(), Icons.Bitbucket));
        return this;
    }

    private async Task<(ClaimsPrincipal, DateTimeOffset)?> VerifyToken(string jwt, CancellationToken cancellationToken)
    {
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
                    using var httpClient = new HttpClient();
                    var jwksJson = await httpClient.GetStringAsync(_jwksUrl, cancellationToken);
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

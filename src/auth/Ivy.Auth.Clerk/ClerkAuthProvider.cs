using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Ivy.Core;
using Ivy.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Ivy.Auth.Clerk.ApiClient;
using System.Text;
using System.Security.Claims;
using Microsoft.AspNetCore.WebUtilities;
using Ivy.Auth.Clerk.ApiClient.Models;

namespace Ivy.Auth.Clerk;

public class ClerkOAuthException(string? error, string? errorDescription)
    : Exception($"Clerk error: '{error}' - {errorDescription}")
{
    public string? Error { get; } = error;
    public string? ErrorDescription { get; } = errorDescription;
}

public class ClerkAuthProvider : IAuthProvider
{
    private readonly string _secretKey;
    private readonly string _frontendApiDomain;
    private readonly List<AuthOption> _authOptions = [];
    private readonly HttpClient _httpClient;
    private ICollection<SecurityKey>? _signingKeys;
    private DateTime _signingKeysLastFetched = DateTime.MinValue;
    private readonly bool _isProduction;
    private string? _origin = null;

    public static bool OpenOAuthLoginInNewTab => true;

    private static (bool IsProduction, string Key) ParseKey(string name, string type, string key)
    {
        var tokens = key.Split('_', 3);
        if (tokens.Length != 3 || tokens[0] != type || (tokens[1] != "test" && tokens[1] != "live"))
        {
            throw new Exception($"{name} is invalid");
        }
        return (tokens[1] == "live", tokens[2]);
    }

    public ClerkAuthProvider(IConfiguration configuration)
    {
        _secretKey = configuration.GetValue<string>("Clerk:SecretKey") ?? throw new Exception("Clerk:SecretKey is required");
        var publishableKey = configuration.GetValue<string>("Clerk:PublishableKey") ?? throw new Exception("Clerk:PublishableKey is required");

        var (secretIsProduction, _) = ParseKey("Clerk:SecretKey", "sk", _secretKey);
        var (publishableIsProduction, publishableKeyValue) = ParseKey("Clerk:PublishableKey", "pk", publishableKey);
        _isProduction = secretIsProduction;

        if (secretIsProduction != publishableIsProduction)
        {
            throw new Exception("Clerk:SecretKey and Clerk:PublishableKey must both be for the same environment (test or live)");
        }

        try
        {
            var base64Decoded = WebEncoders.Base64UrlDecode(publishableKeyValue);
            var base64DecodedString = Encoding.UTF8.GetString(base64Decoded);

            _frontendApiDomain = base64DecodedString.Split('$', 2)[0];
        }
        catch (Exception ex)
        {
            throw new Exception("Clerk:PublishableKey contains an invalid base64 string", ex);
        }

        _httpClient = new HttpClient();
    }

    private FrontendApiClient MakeFrontendApiClient(IAuthSession authSession)
        => new(_frontendApiDomain, authSession.HttpMessageHandler);

    private static ClerkSession? GetActiveSession(ClerkClient client)
    {
        var activeSessions = client.Sessions.Where(session => session.Status == "active");

        // Prefer the last active session, but don't force it
        return activeSessions.FirstOrDefault(session => session.Id == client.LastActiveSessionId)
            ?? activeSessions.FirstOrDefault();
    }

    private async Task<ClerkSession?> GetActiveSession(FrontendApiClient frontendClient, ClerkCredentials credentials, CancellationToken cancellationToken)
    {
        var clientResponse = await frontendClient.GetCurrentClientAsync(credentials, cancellationToken);
        if (clientResponse.Response is { } client &&
            GetActiveSession(client) is { } session &&
            session?.LastActiveToken.Jwt is { } sessionToken &&
            await ValidateToken(sessionToken, lenientLifetimeValidation: false, cancellationToken) != null)
        {
            return session;
        }
        else
        {
            return null;
        }
    }

    private Task<ClerkCredentials> GetClerkCredentialsAsync(IAuthSession authSession, CancellationToken cancellationToken)
        => GetClerkCredentialsAsync(authSession, includeSessionToken: false, cancellationToken);

    private async Task<ClerkCredentials> GetClerkCredentialsAsync(IAuthSession authSession, bool includeSessionToken, CancellationToken cancellationToken)
    {
        var credentials = new ClerkCredentials();

        var frontendClient = MakeFrontendApiClient(authSession);

        if (_isProduction)
        {
            if (!includeSessionToken || await ValidateToken(authSession.AuthToken?.AccessToken, lenientLifetimeValidation: false, cancellationToken) == null)
            {
                if (await GetActiveSession(frontendClient, credentials, cancellationToken) is { } session)
                {
                    credentials.Session = session;
                    if (includeSessionToken)
                    {
                        authSession.AuthToken = new AuthToken(session.LastActiveToken.Jwt);
                    }
                }
            }
        }
        else
        {
            if (authSession.AuthSessionData is { } devBrowserJwt && devBrowserJwt.StartsWith("dvb_"))
            {
                credentials.DevBrowserToken = devBrowserJwt;
            }
            else
            {
                authSession.AuthSessionData = null;
                var devBrowserTokenResponse = await frontendClient.CreateDevBrowserTokenAsync(cancellationToken);
                devBrowserJwt = devBrowserTokenResponse.Id;
                authSession.AuthSessionData = devBrowserJwt;
                credentials.DevBrowserToken = devBrowserJwt;
            }
        }

        if (includeSessionToken && credentials.SessionToken == null)
        {
            credentials.SessionToken = authSession.AuthToken?.AccessToken;
        }

        return credentials;
    }

    public async Task InitializeAsync(IAuthSession authSession, string requestScheme, string requestHost, CancellationToken cancellationToken = default)
    {
        _origin = $"{requestScheme}://{requestHost}";

        var frontendClient = MakeFrontendApiClient(authSession);
        if (_isProduction)
        {
            await frontendClient.GetEnvironmentAsync(cancellationToken: cancellationToken);
            await GetClerkCredentialsAsync(authSession, includeSessionToken: true, cancellationToken: cancellationToken);
        }
        else
        {
            var credentials = await GetClerkCredentialsAsync(authSession, includeSessionToken: false, cancellationToken: cancellationToken);
            await frontendClient.UpdateEnvironmentAsync(credentials, _origin, cancellationToken);
        }
    }

    private async Task<ICollection<SecurityKey>> GetSigningKeysAsync(CancellationToken cancellationToken)
    {
        // Cache keys for 1 hour
        if (_signingKeys != null && DateTime.UtcNow - _signingKeysLastFetched < TimeSpan.FromHours(1))
        {
            return _signingKeys;
        }

        var jwksUrl = $"https://{_frontendApiDomain}/.well-known/jwks.json";
        var jwksJson = await _httpClient.GetStringAsync(jwksUrl, cancellationToken);
        var jwks = new JsonWebKeySet(jwksJson);

        _signingKeys = jwks.GetSigningKeys();
        _signingKeysLastFetched = DateTime.UtcNow;

        return _signingKeys;
    }

    public async Task<AuthToken?> LoginAsync(IAuthSession authSession, string email, string password, CancellationToken cancellationToken = default)
    {
        try
        {
            var credentials = await GetClerkCredentialsAsync(authSession, cancellationToken: cancellationToken);
            var frontendClient = MakeFrontendApiClient(authSession);

            var signInResponse = await frontendClient.CreatePasswordSignInAsync(credentials, email, password, cancellationToken);

            if (signInResponse.Response?.CreatedSessionId is not { } sessionId)
            {
                return null;
            }

            var newToken = await frontendClient.CreateSessionTokenAsync(sessionId, credentials, cancellationToken);

            if (await ValidateToken(newToken.Jwt, lenientLifetimeValidation: false, cancellationToken) == null)
            {
                throw new Exception("New JWT from Clerk is invalid.");
            }

            return new AuthToken(newToken.Jwt!);
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<Uri> GetOAuthUriAsync(IAuthSession authSession, AuthOption option, WebhookEndpoint callback, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(_origin))
        {
            throw new Exception("ClerkAuthProvider is not initialized. Call InitializeAsync before using.");
        }

        var credentials = await GetClerkCredentialsAsync(authSession, cancellationToken: cancellationToken);

        var strategy = option.Id switch
        {
            "google" => "oauth_google",
            "github" => "oauth_github",
            "twitter" => "oauth_twitter",
            "apple" => "oauth_apple",
            "microsoft" => "oauth_microsoft",
            _ => throw new Exception($"Unsupported OAuth strategy: {option.Id}"),
        };

        var redirectUrl = callback.GetUri(includeIdInPath: true).ToString();
        var frontendClient = MakeFrontendApiClient(authSession);
        var signInResponse = await frontendClient.CreateSignInAsync(credentials, _origin, strategy, redirectUrl, null, cancellationToken);

        var firstFactorVerificationResponse = await frontendClient.PrepareFirstFactorVerificationAsync(credentials, _origin, signInResponse.Response!.Id, strategy, redirectUrl, null, cancellationToken);

        if (firstFactorVerificationResponse.Response?.FirstFactorVerification?.ExternalVerificationRedirectUrl is not { } oauthUri)
        {
            throw new Exception("Failed to get OAuth redirect URL from Clerk.");
        }
        return new Uri(oauthUri);
    }

    public async Task<AuthToken?> HandleOAuthCallbackAsync(IAuthSession authSession, HttpRequest request, CancellationToken cancellationToken = default)
    {
        var sessionId = request.Query["created_session_id"].ToString();
        var credentials = await GetClerkCredentialsAsync(authSession, cancellationToken: cancellationToken);
        var frontendClient = MakeFrontendApiClient(authSession);
        try
        {
            await frontendClient.TouchSessionAsync(sessionId, credentials, cancellationToken);
            var newToken = await frontendClient.CreateSessionTokenAsync(sessionId, credentials, cancellationToken);

            if (await ValidateToken(newToken.Jwt, lenientLifetimeValidation: false, cancellationToken) == null)
            {
                throw new Exception("Failed to get new JWT from Clerk.");
            }
            else
            {
                return new AuthToken(newToken.Jwt!);
            }
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task LogoutAsync(IAuthSession authSession, CancellationToken cancellationToken = default)
    {
        var credentials = await GetClerkCredentialsAsync(authSession, cancellationToken: cancellationToken);
        var jwt = authSession.AuthToken?.AccessToken;

        try
        {
            var (principal, _) = await ValidateToken(jwt, lenientLifetimeValidation: true, cancellationToken)
                ?? throw new Exception("Failed to validate access token.");

            if (principal.FindFirst("sid")?.Value is not { } sessionId)
            {
                throw new Exception("No session ID found in access token.");
            }

            var frontendClient = MakeFrontendApiClient(authSession);
            await frontendClient.EndSessionAsync(sessionId, credentials, cancellationToken);
        }
        catch (Exception)
        {
        }
    }

    public async Task<AuthToken?> RefreshAccessTokenAsync(IAuthSession authSession, CancellationToken cancellationToken = default)
    {
        try
        {
            var token = authSession.AuthToken;
            var credentials = await GetClerkCredentialsAsync(authSession, cancellationToken: cancellationToken);

            var (principal, _) = await ValidateToken(token?.AccessToken, lenientLifetimeValidation: true, cancellationToken)
                ?? throw new Exception("Failed to validate access token during token refresh.");

            if (principal.FindFirst("sid")?.Value is not { } sessionId)
            {
                throw new Exception("No session ID found in access token.");
            }

            var frontendClient = MakeFrontendApiClient(authSession);
            var newToken = await frontendClient.CreateSessionTokenAsync(sessionId, credentials, cancellationToken);
            if (await ValidateToken(newToken.Jwt, lenientLifetimeValidation: false, cancellationToken) == null)
            {
                throw new Exception("New JWT from Clerk is invalid.");
            }
            else
            {
                return new AuthToken(newToken.Jwt!);
            }
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<bool> ValidateAccessTokenAsync(IAuthSession authSession, CancellationToken cancellationToken = default)
    {
        return (await ValidateToken(authSession.AuthToken?.AccessToken, lenientLifetimeValidation: false, cancellationToken)) is not null;
    }

    public async Task<UserInfo?> GetUserInfoAsync(IAuthSession authSession, CancellationToken cancellationToken = default)
    {
        if (await ValidateToken(authSession.AuthToken?.AccessToken, lenientLifetimeValidation: false, cancellationToken) is not var (claims, _))
        {
            return null;
        }

        return new UserInfo(
            claims.FindFirst("sub")?.Value.NullIfEmpty() ?? "",
            claims.FindFirst("email")?.Value.NullIfEmpty() ?? claims.FindFirst("username")?.Value.NullIfEmpty() ?? "",
            claims.FindFirst("full_name")?.Value.NullIfEmpty(),
            claims.FindFirst("has_image")?.Value != "false"
                ? claims.FindFirst("image_url")?.Value.NullIfEmpty()
                : null
        );
    }

    public async Task<TokenLifetime?> GetAccessTokenLifetimeAsync(IAuthSession authSession, CancellationToken cancellationToken = default)
    {
        if (await ValidateToken(authSession.AuthToken?.AccessToken, lenientLifetimeValidation: true, cancellationToken) is var (_, lifetime))
        {
            return lifetime;
        }
        else
        {
            return null;
        }
    }

    public AuthOption[] GetAuthOptions()
    {
        return _authOptions.ToArray();
    }

    public ClerkAuthProvider UseEmailPassword()
    {
        _authOptions.Add(new AuthOption(AuthFlow.EmailPassword));
        return this;
    }

    public ClerkAuthProvider UseGoogle()
    {
        _authOptions.Add(new AuthOption(AuthFlow.OAuth, "Google", "google", Icons.Google));
        return this;
    }

    public ClerkAuthProvider UseGithub()
    {
        _authOptions.Add(new AuthOption(AuthFlow.OAuth, "GitHub", "github", Icons.Github));
        return this;
    }

    public ClerkAuthProvider UseTwitter()
    {
        _authOptions.Add(new AuthOption(AuthFlow.OAuth, "Twitter", "twitter", Icons.Twitter));
        return this;
    }

    public ClerkAuthProvider UseApple()
    {
        _authOptions.Add(new AuthOption(AuthFlow.OAuth, "Apple", "apple", Icons.Apple));
        return this;
    }

    public ClerkAuthProvider UseMicrosoft()
    {
        _authOptions.Add(new AuthOption(AuthFlow.OAuth, "Microsoft", "microsoft", Icons.Microsoft));
        return this;
    }

    private async Task<(ClaimsPrincipal, TokenLifetime)?> ValidateToken(string? jwt, bool lenientLifetimeValidation, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(jwt))
        {
            return null;
        }

        var signingKeys = await GetSigningKeysAsync(cancellationToken);

        var handler = new JwtSecurityTokenHandler
        {
            MapInboundClaims = false
        };
        try
        {
            var principal = handler.ValidateToken(jwt, new TokenValidationParameters
            {
                TryAllIssuerSigningKeys = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKeys = signingKeys,
                ValidAlgorithms = [SecurityAlgorithms.RsaSha256],
                ValidateIssuer = true,
                ValidIssuer = $"https://{_frontendApiDomain}",
                ValidateAudience = false,
                ClockSkew = lenientLifetimeValidation
                    ? TimeSpan.FromDays(1)
                    : TimeSpan.Zero,
            }, out SecurityToken validatedToken);

            return (principal, new TokenLifetime(validatedToken.ValidTo, validatedToken.ValidFrom));
        }
        catch (Exception)
        {
            return null;
        }
    }
}
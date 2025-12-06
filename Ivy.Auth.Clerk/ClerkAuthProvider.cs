using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using Ivy.Hooks;
using Ivy.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Ivy.Auth.Clerk.ApiClient;
using System.Text;
using System.Security.Claims;
using Microsoft.AspNetCore.WebUtilities;

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
    private readonly FrontendApiClient _frontendClient;
    private readonly bool _isProduction;

    // TODO: remove this before merge
    private const string ORIGIN_TEMPORARY_REMOVE_THIS_BEFORE_MERGE = "http://localhost:5010";

    private static (bool IsProduction, string Key) ParseKey(string name, string type, string key)
    {
        var tokens = key.Split('_', 3);
        if (tokens.Length != 3 || tokens[0] != type || (tokens[1] != "test" && tokens[1] != "live"))
        {
            throw new Exception($"{name} is invalid");
        }
        return (tokens[1] == "live", tokens[2]);
    }

    public ClerkAuthProvider()
    {
        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddUserSecrets(Assembly.GetEntryAssembly()!)
            .Build();

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

        _frontendClient = new FrontendApiClient(_frontendApiDomain);
    }

    private async Task<string> GetDevBrowserJwtAsync(IAuthSession authSession, CancellationToken cancellationToken = default)
    {
        if (authSession.AuthSessionData is { } devBrowserJwt && devBrowserJwt.StartsWith("dvb_"))
        {
            return devBrowserJwt;
        }
        var devBrowserTokenResponse = await _frontendClient.CreateDevBrowserTokenAsync(cancellationToken);
        authSession.AuthSessionData = devBrowserTokenResponse.Id;
        return devBrowserTokenResponse.Id;
    }

    public async Task InitializeAsync(IAuthSession authSession, string requestScheme, string requestHost, CancellationToken cancellationToken = default)
    {
        var clientResponse = await _frontendClient.CreateNewClientAsync(cancellationToken);

        var devBrowserJwt = await GetDevBrowserJwtAsync(authSession, cancellationToken);

        var updateEnvironmentResponse = await _frontendClient.UpdateEnvironmentAsync(ClerkApiClientToken.FromDevBrowserToken(devBrowserJwt), ORIGIN_TEMPORARY_REMOVE_THIS_BEFORE_MERGE, cancellationToken);

        var existingClientResponse = await _frontendClient.GetCurrentClientAsync(ClerkApiClientToken.FromDevBrowserToken(devBrowserJwt), cancellationToken);
    }

    private async Task<ICollection<SecurityKey>> GetSigningKeysAsync(CancellationToken cancellationToken = default)
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
            // TODO
            await Task.CompletedTask;
            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<Uri> GetOAuthUriAsync(IAuthSession authSession, AuthOption option, WebhookEndpoint callback, CancellationToken cancellationToken = default)
    {
        var devBrowserJwt = await GetDevBrowserJwtAsync(authSession, cancellationToken);

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
        var clientToken = ClerkApiClientToken.FromDevBrowserToken(devBrowserJwt);
        var signInResponse = await _frontendClient.CreateSignInAsync(clientToken, ORIGIN_TEMPORARY_REMOVE_THIS_BEFORE_MERGE, strategy, redirectUrl, null, cancellationToken);
        var firstFactorVerificationResponse = await _frontendClient.PrepareFirstFactorVerificationAsync(clientToken, ORIGIN_TEMPORARY_REMOVE_THIS_BEFORE_MERGE, signInResponse.Response!.Id, strategy, redirectUrl, null, cancellationToken);
        if (firstFactorVerificationResponse.Response?.FirstFactorVerification?.ExternalVerificationRedirectUrl is not { } oauthUri)
        {
            throw new Exception("Failed to get OAuth redirect URL from Clerk.");
        }
        return new Uri(oauthUri);
    }

    public async Task<AuthToken?> HandleOAuthCallbackAsync(IAuthSession authSession, HttpRequest request, CancellationToken cancellationToken = default)
    {
        var sessionId = request.Query["created_session_id"].ToString();
        var devBrowserJwt = await GetDevBrowserJwtAsync(authSession, cancellationToken);
        try
        {
            var sessionResponse = await _frontendClient.TouchSessionAsync(sessionId, ClerkApiSessionToken.FromDevBrowserToken(devBrowserJwt), cancellationToken);
            var newToken = await _frontendClient.CreateSessionTokenAsync(sessionId, ClerkApiClientToken.FromDevBrowserToken(devBrowserJwt), cancellationToken);
            if (string.IsNullOrEmpty(newToken.Jwt))
            {
                throw new Exception("Failed to get new JWT from Clerk.");
            }
            else
            {
                return new AuthToken(newToken.Jwt);
            }
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task LogoutAsync(IAuthSession authSession, CancellationToken cancellationToken = default)
    {
        var jwt = authSession.AuthToken?.AccessToken;
        var devBrowserJwt = await GetDevBrowserJwtAsync(authSession, cancellationToken);

        try
        {
            var (principal, _) = await ValidateToken(jwt, lenientLifetimeValidation: true, cancellationToken)
                ?? throw new Exception("Failed to validate access token.");

            if (principal.FindFirst("sid")?.Value is not { } sessionId)
            {
                throw new Exception("No session ID found in access token.");
            }

            await _frontendClient.EndSessionAsync(sessionId, ClerkApiSessionToken.FromDevBrowserToken(devBrowserJwt), cancellationToken);
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
            var devBrowserJwt = await GetDevBrowserJwtAsync(authSession, cancellationToken);

            var (principal, _) = await ValidateToken(token?.AccessToken, lenientLifetimeValidation: true, cancellationToken)
                ?? throw new Exception("Failed to validate access token during token refresh.");

            if (principal.FindFirst("sid")?.Value is not { } sessionId)
            {
                throw new Exception("No session ID found in access token.");
            }

            var newToken = await _frontendClient.CreateSessionTokenAsync(sessionId, ClerkApiClientToken.FromDevBrowserToken(devBrowserJwt), cancellationToken);
            if (string.IsNullOrEmpty(newToken.Jwt))
            {
                throw new Exception("Failed to get new JWT from Clerk.");
            }
            else
            {
                return new AuthToken(newToken.Jwt);
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
        try
        {
            var devBrowserJwt = await GetDevBrowserJwtAsync(authSession, cancellationToken);

            var (principal, _) = await ValidateToken(authSession.AuthToken?.AccessToken, lenientLifetimeValidation: true, cancellationToken)
                ?? throw new Exception("Failed to validate access token in GetUserInfoAsync.");

            if (principal.FindFirst("sid")?.Value is not { } sessionId)
            {
                return null;
            }

            var session = await _frontendClient.GetSessionAsync(sessionId, ClerkApiSessionToken.FromDevBrowserToken(devBrowserJwt), cancellationToken);
            var user = session.Response.User;

            string name = user.FirstName ?? "";
            if (!string.IsNullOrEmpty(user.LastName))
            {
                name += " " + user.LastName;
            }

            var email = user.PrimaryEmailAddressId != null
                ? user.EmailAddresses.FirstOrDefault(a => a.Id == user.PrimaryEmailAddressId)?.EmailAddress ?? user.EmailAddresses.FirstOrDefault()?.EmailAddress
                : user.EmailAddresses.FirstOrDefault()?.EmailAddress;

            if (email is null)
            {
                return null;
            }

            return new UserInfo(user.Id, email ?? "", name, user.ProfileImageUrl);
        }
        catch (Exception)
        {
            return null;
        }
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

        var handler = new JwtSecurityTokenHandler();
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
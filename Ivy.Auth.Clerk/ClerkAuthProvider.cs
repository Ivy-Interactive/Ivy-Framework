using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using Ivy.Hooks;
using Ivy.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Ivy.Auth.Clerk.ApiClient;
using System.Text.Json;
using System.Security.Claims;

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

    // TODO: remove this before merge
    private const string ORIGIN_TEMPORARY_REMOVE_THIS_BEFORE_MERGE = "http://localhost:5010";

    public ClerkAuthProvider()
    {
        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddUserSecrets(Assembly.GetEntryAssembly()!)
            .Build();

        _secretKey = configuration.GetValue<string>("Clerk:SecretKey") ?? throw new Exception("Clerk:SecretKey is required");
        _frontendApiDomain = configuration.GetValue<string>("Clerk:FrontendApiDomain") ?? throw new Exception("Clerk:FrontendApiDomain is required");

        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_secretKey}");

        _frontendClient = new FrontendApiClient(_frontendApiDomain);
    }

    private async Task<ICollection<SecurityKey>> GetSigningKeysAsync(CancellationToken cancellationToken = default)
    {
        // Cache keys for 1 hour
        if (_signingKeys != null && DateTime.UtcNow - _signingKeysLastFetched < TimeSpan.FromHours(1))
        {
            return _signingKeys;
        }

        var jwksUrl = $"https://{_frontendApiDomain}.clerk.accounts.dev/.well-known/jwks.json";
        var jwksJson = await _httpClient.GetStringAsync(jwksUrl, cancellationToken);
        var jwks = new JsonWebKeySet(jwksJson);

        _signingKeys = jwks.GetSigningKeys();
        _signingKeysLastFetched = DateTime.UtcNow;

        return _signingKeys;
    }

    public async Task<AuthToken?> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        try
        {
            // Clerk doesn't support direct email/password authentication through the server SDK
            // This would typically be handled on the client side with Clerk's client libraries
            // For server-side authentication, we would need to validate a session token
            // that was created by the client-side Clerk authentication flow

            // For now, return null to indicate this flow is not supported server-side
            await Task.CompletedTask;
            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<Uri> GetOAuthUriAsync(AuthOption option, WebhookEndpoint callback, CancellationToken cancellationToken = default)
    {
        string? devBrowserJwt = null;
        try
        {
            var devBrowserTokenResponse = await _frontendClient.CreateDevBrowserTokenAsync(cancellationToken);
            devBrowserJwt = devBrowserTokenResponse.Id;

            var updateEnvironmentResponse = await _frontendClient.UpdateEnvironmentAsync(devBrowserTokenResponse.Id, ORIGIN_TEMPORARY_REMOVE_THIS_BEFORE_MERGE, cancellationToken);

            var clientResponse = await _frontendClient.GetCurrentClient(devBrowserTokenResponse.Id, cancellationToken);

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
            var signInResponse = await _frontendClient.CreateSignInAsync(devBrowserJwt, ORIGIN_TEMPORARY_REMOVE_THIS_BEFORE_MERGE, strategy, redirectUrl, null, cancellationToken);
            var firstFactorVerificationResponse = await _frontendClient.PrepareFirstFactorVerificationAsync(devBrowserJwt, ORIGIN_TEMPORARY_REMOVE_THIS_BEFORE_MERGE, signInResponse.Response!.Id, strategy, redirectUrl, null, cancellationToken);
            Console.WriteLine($"First factor verification response: {firstFactorVerificationResponse}");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error creating OAuth URI: {e}");
        }

        var redirectUri = callback.GetUri(includeIdInPath: true);

        // TODO: use the correct sign-in URL. This is the default, but it is configurable in Clerk dashboard.
        // Also we may want to just take complete control of the sign-in flow:
        // https://clerk.com/docs/reference/frontend-api/tag/sign-ins/post/v1/client/sign_ins
        var authUrl = $"https://{_frontendApiDomain}.accounts.dev/sign-in?redirect_url={Uri.EscapeDataString(redirectUri.ToString())}";
        if (!string.IsNullOrEmpty(devBrowserJwt))
        {
            authUrl += $"&__clerk_db_jwt={devBrowserJwt}";
        }

        return new Uri(authUrl);
    }

    public async Task<AuthToken> VerifyHandshakeJwtAsync(string jwt, string devBrowserJwt, CancellationToken cancellationToken = default)
    {
        var signingKeys = await GetSigningKeysAsync(cancellationToken);

        var parameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKeys = signingKeys,
            ValidAlgorithms = [SecurityAlgorithms.RsaSha256],

            ValidateLifetime = false,
            ValidateAudience = false,
            ValidateIssuer = false,
        };

        var handler = new JwtSecurityTokenHandler();
        var principal = handler.ValidateToken(jwt, parameters, out var validatedToken);

        // This handshake claim contains HTTP-style cookie assignments.
        var handshakeClaims = principal.FindAll("handshake");
        var cookieValues = new List<(string, string)>();
        foreach (var claim in handshakeClaims)
        {
            var parts = claim.Value.Split(';', StringSplitOptions.TrimEntries);
            if (parts.Length < 2)
            {
                throw new Exception("Invalid handshake claim format.");
            }

            var cookieAssignment = parts[0];
            var cookieParts = cookieAssignment.Split('=', 2, StringSplitOptions.TrimEntries);
            if (cookieParts.Length < 2)
            {
                throw new Exception("Invalid cookie assignment format in handshake claim.");
            }

            cookieValues.Add((cookieParts[0], cookieParts[1]));
        }
        var sessionCookie = cookieValues.First(c => c.Item1 == "__session").Item2;
        var refreshCookie = cookieValues.First(c => c.Item1.StartsWith("__refresh")).Item2;
        return new AuthToken(sessionCookie, refreshCookie, devBrowserJwt);
    }

    public async Task<AuthToken?> HandleOAuthCallbackAsync(HttpRequest request, CancellationToken cancellationToken = default)
    {
        var handshakeJwt = request.Query["__clerk_handshake"].ToString();
        var devBrowserJwt = request.Query["__clerk_db_jwt"].ToString();

        var error = request.Query["error"].ToString();
        var errorDescription = request.Query["error_description"].ToString();

        if (!string.IsNullOrEmpty(error) || !string.IsNullOrEmpty(errorDescription))
        {
            throw new ClerkOAuthException(error, errorDescription);
        }

        if (string.IsNullOrEmpty(handshakeJwt))
        {
            await _frontendClient.RemoveAllSessionsAsync(devBrowserJwt, cancellationToken);
            throw new Exception("Received no handshake JWT from Clerk.");
        }

        var authToken = await VerifyHandshakeJwtAsync(handshakeJwt, devBrowserJwt, cancellationToken);

        try
        {
            var (principal, _) = await ValidateToken(authToken.AccessToken, lenientLifetimeValidation: false, cancellationToken)
                ?? throw new Exception("Failed to validate access token after OAuth callback.");

            if (principal.FindFirst("sid")?.Value is not { } sessionId)
            {
                throw new Exception("No session ID found in access token.");
            }

            await _frontendClient.TouchSessionAsync(sessionId, devBrowserJwt, cancellationToken);
        }
        catch (Exception)
        {
        }

        return authToken;
    }

    public async Task LogoutAsync(string jwt, object? tag, CancellationToken cancellationToken = default)
    {
        string? devBrowserJwt = tag switch
        {
            string s => s,
            JsonElement e when e.ValueKind == JsonValueKind.String => e.GetString(),
            _ => null,
        };

        if (devBrowserJwt is null)
        {
            return;
        }

        try
        {
            var (principal, _) = await ValidateToken(jwt, lenientLifetimeValidation: true, cancellationToken)
                ?? throw new Exception("Failed to validate access token.");

            if (principal.FindFirst("sid")?.Value is not { } sessionId)
            {
                throw new Exception("No session ID found in access token.");
            }

            await _frontendClient.EndSessionAsync(sessionId, devBrowserJwt, cancellationToken);
        }
        catch (Exception)
        {
        }
    }

    public async Task<AuthToken?> RefreshAccessTokenAsync(AuthToken token, CancellationToken cancellationToken = default)
    {
        try
        {
            string? devBrowserJwt = token.Tag switch
            {
                string s => s,
                JsonElement e when e.ValueKind == JsonValueKind.String => e.GetString(),
                _ => null,
            };

            if (devBrowserJwt is null)
            {
                return null;
            }

            var (principal, _) = await ValidateToken(token.AccessToken, lenientLifetimeValidation: true, cancellationToken)
                ?? throw new Exception("Failed to validate access token during token refresh.");

            if (principal.FindFirst("sid")?.Value is not { } sessionId)
            {
                throw new Exception("No session ID found in access token.");
            }

            var newToken = await _frontendClient.CreateSessionTokenAsync(sessionId, devBrowserJwt, cancellationToken);
            if (string.IsNullOrEmpty(newToken.Jwt))
            {
                throw new Exception("Failed to get new JWT from Clerk.");
            }
            else
            {
                return new AuthToken(newToken.Jwt, token.RefreshToken, devBrowserJwt);
            }
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<bool> ValidateAccessTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return (await ValidateToken(token, lenientLifetimeValidation: false, cancellationToken)) is not null;
    }

    public async Task<UserInfo?> GetUserInfoAsync(string token, object? tag, CancellationToken cancellationToken = default)
    {
        try
        {
            string? devBrowserJwt = tag switch
            {
                string s => s,
                JsonElement e when e.ValueKind == JsonValueKind.String => e.GetString(),
                _ => null,
            };

            if (devBrowserJwt is null)
            {
                return null;
            }

            // TODO: cache user info to avoid excessive touch calls
            var (principal, _) = await ValidateToken(token, lenientLifetimeValidation: true, cancellationToken)
                ?? throw new Exception("Failed to validate access token in GetUserInfoAsync.");

            if (principal.FindFirst("sid")?.Value is not { } sessionId)
            {
                return null;
            }

            var session = await _frontendClient.GetSessionAsync(sessionId, devBrowserJwt, cancellationToken);
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

    public async Task<TokenLifetime?> GetTokenLifetimeAsync(AuthToken token, CancellationToken cancellationToken = default)
    {
        if (await ValidateToken(token.AccessToken, lenientLifetimeValidation: true, cancellationToken) is var (_, lifetime))
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

    private async Task<(ClaimsPrincipal, TokenLifetime)?> ValidateToken(string jwt, bool lenientLifetimeValidation, CancellationToken cancellationToken)
    {
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
                ValidIssuer = $"https://{_frontendApiDomain}.clerk.accounts.dev",
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
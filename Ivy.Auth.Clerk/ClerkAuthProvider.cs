using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using Ivy.Hooks;
using Ivy.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Ivy.Auth.Clerk.ApiClient;
using System.Text.Json;

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

        _frontendClient = new FrontendApiClient(_frontendApiDomain, _secretKey);
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
            Console.WriteLine($"got dev browser token: {JsonSerializer.Serialize(devBrowserTokenResponse)}\n");
            devBrowserJwt = devBrowserTokenResponse.Id;

            var updateEnvironmentResponse = await _frontendClient.UpdateEnvironmentAsync(devBrowserTokenResponse.Id, ORIGIN_TEMPORARY_REMOVE_THIS_BEFORE_MERGE, cancellationToken);
            Console.WriteLine($"updated environment: {JsonSerializer.Serialize(updateEnvironmentResponse)}\n");

            var clientResponse = await _frontendClient.GetCurrentClient(devBrowserTokenResponse.Id, cancellationToken);
            Console.WriteLine($"got client: {JsonSerializer.Serialize(clientResponse)}\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during frontend API calls: {ex}");
        }

        var redirectUri = callback.GetUri(includeIdInPath: true);

        // TODO: use the correct sign-in URL. This is the default, but it is configurable in Clerk dashboard.
        // Also we may want to just take complete control of the sign-in flow:
        // https://clerk.com/docs/reference/frontend-api/tag/sign-ins/post/v1/client/sign_ins
        var authUrl = $"https://{_frontendApiDomain}.accounts.dev/sign-in?redirect_url={Uri.EscapeDataString(redirectUri.ToString())}";
        if (!string.IsNullOrEmpty(devBrowserJwt))
        {
            Console.WriteLine($"(not) Appending devBrowserJwt to auth URL:  {devBrowserJwt}.\n");
            // authUrl += $"&__clerk_db_jwt={devBrowserJwt}";
        }
        else
        {
            Console.WriteLine("Warning: devBrowserJwt is null or empty, proceeding without it.\n");
        }

        return new Uri(authUrl);
    }

    public async Task<AuthToken> VerifyHandshakeJwtAsync(string jwt, CancellationToken cancellationToken = default)
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
        return new AuthToken(sessionCookie, refreshCookie);
    }

    public async Task<AuthToken?> HandleOAuthCallbackAsync(HttpRequest request, CancellationToken cancellationToken = default)
    {
        var handshakeJwt = request.Query["__clerk_handshake"].ToString();
        var devBrowserJwt = request.Query["__clerk_db_jwt"].ToString();
        Console.WriteLine($"Received dev browser JWT in callback: {devBrowserJwt}\n");

        var error = request.Query["error"].ToString();
        var errorDescription = request.Query["error_description"].ToString();

        if (!string.IsNullOrEmpty(error) || !string.IsNullOrEmpty(errorDescription))
        {
            throw new ClerkOAuthException(error, errorDescription);
        }

        if (string.IsNullOrEmpty(handshakeJwt))
        {
            throw new Exception("Received no handshake JWT from Clerk.");
        }

        Console.WriteLine("Received handshake JWT!");
        return await VerifyHandshakeJwtAsync(handshakeJwt, cancellationToken);
    }

    public async Task LogoutAsync(string jwt, CancellationToken cancellationToken = default)
    {
        try
        {
            // In Clerk, sessions are typically invalidated on the client side
            // or through the Clerk Dashboard/API
            await Task.CompletedTask;
        }
        catch (Exception)
        {
            // Logout failures are typically not critical
        }
    }

    public async Task<AuthToken?> RefreshAccessTokenAsync(AuthToken token, CancellationToken cancellationToken = default)
    {
        try
        {
            // Clerk handles token refresh automatically in most cases
            // This would typically involve calling Clerk's session refresh APIs
            await Task.CompletedTask;

            // Return the same token or a refreshed one
            return token;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<bool> ValidateAccessTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        try
        {
            var signingKeys = await GetSigningKeysAsync(cancellationToken);

            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                TryAllIssuerSigningKeys = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKeys = signingKeys,
                ValidAlgorithms = [SecurityAlgorithms.RsaSha256],
                ValidateIssuer = true,
                ValidIssuer = $"https://{_frontendApiDomain}.clerk.accounts.dev",
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
            return principal != null;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<UserInfo?> GetUserInfoAsync(string token, CancellationToken cancellationToken = default)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jsonToken = tokenHandler.ReadJwtToken(token);

            var claims = jsonToken.Claims;

            var userId = claims.FirstOrDefault(x => x.Type == "sub")?.Value ?? "";
            var email = claims.FirstOrDefault(x => x.Type == "email")?.Value ?? "";
            var name = claims.FirstOrDefault(x => x.Type == "name")?.Value ?? "";
            var picture = claims.FirstOrDefault(x => x.Type == "picture")?.Value ?? "";

            return new UserInfo(userId, email, name, picture);
        }
        catch (Exception)
        {
            return null;
        }
    }

    public Task<DateTimeOffset?> GetTokenExpiration(AuthToken token, CancellationToken cancellationToken = default)
    {
        // Clerk tokens typically encode expiration in the JWT
        // In a real implementation, you would parse the JWT and extract the exp claim
        // For now, return null to indicate unknown expiration
        return Task.FromResult<DateTimeOffset?>(null);
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
}
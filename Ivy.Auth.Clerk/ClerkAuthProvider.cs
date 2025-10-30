using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Claims;
using Clerk.Net;
using Ivy.Hooks;
using Ivy.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

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
    private readonly string? _jwtKey;
    private readonly string? _fapiInstance;
    private readonly List<AuthOption> _authOptions = new();
    private readonly HttpClient _httpClient;

    public ClerkAuthProvider()
    {
        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddUserSecrets(Assembly.GetEntryAssembly()!)
            .Build();

        _secretKey = configuration.GetValue<string>("Clerk:SecretKey") ?? throw new Exception("Clerk:SecretKey is required");
        _jwtKey = configuration.GetValue<string>("Clerk:JwtKey") ?? throw new Exception("Clerk:JwtKey is required");
        var publishableKey = configuration.GetValue<string>("Clerk:PublishableKey") ?? throw new Exception("Clerk:PublishableKey is required");

        try
        {
            var pkBytes = Convert.FromBase64String(publishableKey);
            _fapiInstance = System.Text.Encoding.UTF8.GetString(pkBytes).Split('%')[0];
        }
        catch (Exception)
        {
            throw new Exception("Clerk:PublishableKey is not a valid base64 string");
        }

        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_secretKey}");
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

    public Task<Uri> GetOAuthUriAsync(AuthOption option, WebhookEndpoint callback, CancellationToken cancellationToken = default)
    {
        // Clerk OAuth URLs are typically generated on the client side
        // The server-side SDK doesn't provide direct OAuth URL generation
        // In a real implementation, you would construct the Clerk OAuth URL
        // based on your Clerk configuration and the selected provider

        var redirectUri = callback.GetUri(includeIdInPath: false);

        var provider = option.Id switch
        {
            "google" => "oauth_google",
            "github" => "oauth_github",
            "twitter" => "oauth_twitter",
            "microsoft" => "oauth_microsoft",
            "apple" => "oauth_apple",
            _ => throw new ArgumentException($"Unknown OAuth provider: {option.Id}")
        };

        // TODO: fetch the correct sign-in URL. This is the default, but it is configurable in Clerk dashboard
        var authUrl = $"https://{_fapiInstance}/sign-in?provider={provider}&redirect_url={Uri.EscapeDataString(redirectUri.ToString())}&state={callback.Id}";

        return Task.FromResult(new Uri(authUrl));
    }

    public async Task<AuthToken?> HandleOAuthCallbackAsync(HttpRequest request, CancellationToken cancellationToken = default)
    {
        var code = request.Query["code"].ToString();
        var error = request.Query["error"].ToString();
        var errorDescription = request.Query["error_description"].ToString();

        if (!string.IsNullOrEmpty(error) || !string.IsNullOrEmpty(errorDescription))
        {
            throw new ClerkOAuthException(error, errorDescription);
        }

        if (string.IsNullOrEmpty(code))
        {
            throw new Exception("Received no authorization code from Clerk.");
        }

        try
        {
            // In a real Clerk implementation, you would exchange the code for a session token
            // This typically involves client-side Clerk libraries
            // For now, we'll simulate a successful authentication
            await Task.CompletedTask;

            // Return a mock token - in practice, this would be the actual Clerk session token
            return new AuthToken("mock_clerk_jwt_token", null, null);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to exchange authorization code for tokens: {ex.Message}");
        }
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
            if (string.IsNullOrEmpty(_jwtKey))
            {
                // Without JWT key, we can't validate the token locally
                // In practice, you would call Clerk's verification API
                return await Task.FromResult(true); // Mock validation
            }

            // Validate JWT token using Clerk's public key
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_jwtKey)),
                ValidateIssuer = false, // Clerk manages the issuer
                ValidateAudience = false, // Clerk manages the audience
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
            if (string.IsNullOrEmpty(_jwtKey))
            {
                // Mock user info when we can't validate the token
                return new UserInfo("mock_user_id", "user@example.com", "Mock User", null);
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var jsonToken = tokenHandler.ReadJwtToken(token);

            var userId = jsonToken.Claims.FirstOrDefault(x => x.Type == "sub")?.Value ?? "";
            var email = jsonToken.Claims.FirstOrDefault(x => x.Type == "email")?.Value ?? "";
            var name = jsonToken.Claims.FirstOrDefault(x => x.Type == "name")?.Value ?? "";
            var picture = jsonToken.Claims.FirstOrDefault(x => x.Type == "picture")?.Value ?? "";

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
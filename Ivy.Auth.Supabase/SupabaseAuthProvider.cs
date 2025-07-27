using System.Reflection;
using Ivy.Hooks;
using Ivy.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Supabase;
using Supabase.Gotrue;

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

    private readonly List<AuthOption> _authOptions = new();

    private string? _pkceCodeVerifier = null;

    public SupabaseAuthProvider()
    {
        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddUserSecrets(Assembly.GetEntryAssembly()!)
            .Build();

        var url = configuration.GetValue<string>("SUPABASE_URL") ?? throw new Exception("SUPABASE_URL is required");
        var key = configuration.GetValue<string>("SUPABASE_API_KEY") ?? throw new Exception("SUPABASE_API_KEY is required");

        var options = new SupabaseOptions
        {
            AutoRefreshToken = false,
            AutoConnectRealtime = false
        };

        _client = new global::Supabase.Client(url, key, options);
    }

    public async Task<AuthToken?> LoginAsync(string email, string password)
    {
        var session = await _client.Auth.SignIn(email, password);
        var authToken = MakeAuthToken(session);
        return authToken;
    }

    public async Task<Uri> GetOAuthUriAsync(AuthOption option, WebhookEndpoint callback)
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

        var providerAuthState = await _client.Auth.SignIn(provider, signInOptions);
        _pkceCodeVerifier = providerAuthState.PKCEVerifier;

        return providerAuthState.Uri;
    }

    public async Task<AuthToken?> HandleOAuthCallbackAsync(HttpRequest request)
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

        var session = await _client.Auth.ExchangeCodeForSession(_pkceCodeVerifier!, code.ToString());
        var authToken = MakeAuthToken(session);
        return authToken;
    }

    public async Task LogoutAsync(string _)
    {
        await _client.Auth.SignOut();
    }

    public async Task<AuthToken?> RefreshJwtAsync(AuthToken jwt)
    {
        if (jwt.ExpiresAt == null || jwt.RefreshToken == null || DateTimeOffset.UtcNow < jwt.ExpiresAt)
        {
            // Refresh not needed (or not possible).
            return jwt;
        }

        try
        {
            var session = await _client.Auth.SetSession(jwt.Jwt, jwt.RefreshToken);
            var authToken = MakeAuthToken(session);
            return authToken;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<bool> ValidateJwtAsync(string jwt)
    {
        try
        {
            // Verify the JWT token with Supabase
            var response = await _client.Auth.GetUser(jwt);

            // If we get a response back, the token is valid
            return response != null;
        }
        catch (Exception)
        {
            // If any exception occurs during validation, consider the token invalid
            return false;
        }
    }

    public async Task<UserInfo?> GetUserInfoAsync(string jwt)
    {
        var user = await _client.Auth.GetUser(jwt);

        if (user == null)
        {
            return null;
        }

        if (user.Id == null || user.Email == null)
        {
            return null;
        }

        return new UserInfo(
            user.Id,
            user.Email,
            user.UserMetadata != null && user.UserMetadata.TryGetValue("full_name", out var value)
                ? value.ToString()
                : string.Empty,
            null
        );
    }

    public AuthOption[] GetAuthOptions()
    {
        return _authOptions.ToArray();
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

    private AuthToken? MakeAuthToken(Session? session) =>
        session?.AccessToken != null
            ? new AuthToken(session.AccessToken, session.RefreshToken, session.ExpiresAt())
            : null;

}

// public async Task<bool> ValidateJwtAsync(string jwt)
// {
//     try
//     {
//         // Get the Supabase JWT secret (this should be your project's JWT secret)
//         string jwtSecret = "your-supabase-jwt-secret"; // Store this securely
//     
//         var tokenHandler = new JwtSecurityTokenHandler();
//         var validationParameters = new TokenValidationParameters
//         {
//             ValidateIssuerSigningKey = true,
//             IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
//             ValidateIssuer = false,     // Set to true with proper issuer in production
//             ValidateAudience = false,   // Set to true with proper audience in production
//             ValidateLifetime = true,    // Check if the token is expired
//             ClockSkew = TimeSpan.Zero   // No tolerance for token expiration
//         };
//
//         // This will throw an exception if validation fails
//         var principal = tokenHandler.ValidateToken(jwt, validationParameters, out _);
//     
//         // Optional: Check for required claims
//         // var userIdClaim = principal.FindFirst("sub")?.Value;
//         // if (string.IsNullOrEmpty(userIdClaim))
//         //     return false;
//         
//         return true;
//     }
//     catch
//     {
//         return false;
//     }
// }
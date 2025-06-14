using System.Reflection;
using Ivy.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Supabase;
using Supabase.Gotrue;

namespace Ivy.Auth.Supabase;

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
        _client.Auth.AddStateChangedListener((_, state) =>
        {
            Console.WriteLine($"[{DateTimeOffset.Now}] Received state change message from Supabase client: {state}");
        });
    }

    public async Task<AuthToken?> LoginAsync(string email, string password)
    {
        var session = await _client.Auth.SignIn(email, password);
        var authToken = MakeAuthToken(session);
        Console.WriteLine($"[{DateTimeOffset.Now}] signed in with email and password, made auth token: {authToken}");
        return authToken;
    }
    
    public async Task<Uri> GetOAuthUriAsync(string optionId, Uri callbackUri)
    {
        var provider = optionId switch
        {
            "google" => Constants.Provider.Google,
            "apple" => Constants.Provider.Apple,
            "discord" => Constants.Provider.Discord,
            "azure" => Constants.Provider.Azure,
            "github" => Constants.Provider.Github,
            "gitlab" => Constants.Provider.Gitlab,
            "bitbucket" => Constants.Provider.Bitbucket,
            _ => throw new ArgumentException($"Unknown OAuth provider: {optionId}"),
        };
        
        var providerAuthState = await _client.Auth.SignIn(provider, new SignInOptions
        {
            RedirectTo = callbackUri.ToString(),
            FlowType = Constants.OAuthFlowType.PKCE,
            Scopes = "email openid",
        });
        _pkceCodeVerifier = providerAuthState.PKCEVerifier;

        Console.WriteLine($"[{DateTimeOffset.Now}] Got URI for OAuth: {providerAuthState.Uri}");

        return providerAuthState.Uri;
    }

    public async Task<AuthToken?> HandleOAuthCallbackAsync(HttpRequest request)
    {
        var code = request.Query["code"];
        Console.WriteLine($"[{DateTimeOffset.Now}] in oauth callback handler. got code {code}");
        var session = await _client.Auth.ExchangeCodeForSession(_pkceCodeVerifier!, code.ToString());
        var authToken = MakeAuthToken(session);
        Console.WriteLine($"[{DateTimeOffset.Now}] exchanged code for token: {authToken}");
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
            Console.WriteLine($"[{DateTimeOffset.Now}] Token refresh not required, or not possible. Reasons:");
            if (jwt.ExpiresAt == null) Console.WriteLine($"    - expiry date is null");
            if (jwt.RefreshToken == null) Console.WriteLine($"    - refresh token is null");
            if (DateTimeOffset.UtcNow < jwt.ExpiresAt) Console.WriteLine($"    - access token is still valid; {DateTimeOffset.UtcNow} < {jwt.ExpiresAt}");
            return jwt;
        }

        try
        {
            Console.WriteLine($"[{DateTimeOffset.Now}] attempting to set session using the existing token. the old token: {jwt}");

            var session = await _client.Auth.SetSession(jwt.Jwt, jwt.RefreshToken);
            var authToken = MakeAuthToken(session);
            Console.WriteLine($"    the new token: {authToken}");
            return authToken;
        }
        catch (Exception e)
        {
            Console.WriteLine($"    setting session failed with exception: {e}");
            return null;
        }
    }

    public async Task<bool> ValidateJwtAsync(string jwt)
    {
        try
        {
            Console.WriteLine($"[{DateTimeOffset.Now}] verifying JWT with Supabase: {jwt}");

            // Verify the JWT token with Supabase
            var response = await _client.Auth.GetUser(jwt);

            Console.WriteLine($"    validation succeeded? {response != null}");

            // If we get a response back, the token is valid
            return response != null;
        }
        catch (Exception e)
        {
            Console.WriteLine($"    validation failed with exception: {e}");

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

    public SupabaseAuthProvider UseAzure()
    {
        _authOptions.Add(new AuthOption(AuthFlow.OAuth, "Azure", nameof(Constants.Provider.Azure).ToLower(), Icons.Azure));
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
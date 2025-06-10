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
            AutoRefreshToken = true,
            AutoConnectRealtime = false
        };
        
        _client = new global::Supabase.Client(url, key, options);
    }

    public async Task<AuthToken?> LoginAsync(string email, string password)
    {
        var session = await _client.Auth.SignIn(email, password);
        var authToken = MakeAuthToken(session);
        Console.WriteLine($"signed in with email and password, made auth token: {authToken}");
        return authToken;
    }
    
    public async Task<Uri> GetOAuthUriAsync(string optionId, Uri callbackUri)
    {
        var provider = optionId switch
        {
            "google" => Constants.Provider.Google,
            "apple" => Constants.Provider.Apple,
            "github" => Constants.Provider.Github,
            _ => throw new ArgumentException($"Unknown OAuth provider: {optionId}"),
        };
        
        var providerAuthState = await _client.Auth.SignIn(provider, new SignInOptions
        {
            RedirectTo = callbackUri.ToString(),
            FlowType = Constants.OAuthFlowType.PKCE,
        });
        _pkceCodeVerifier = providerAuthState.PKCEVerifier;

        Console.WriteLine($"Got URI for OAuth: {providerAuthState.Uri}");

        return providerAuthState.Uri;
    }

    public async Task<AuthToken?> HandleOAuthCallbackAsync(HttpRequest request)
    {
        var code = request.Query["code"];
        Console.WriteLine($"in oauth callback handler. got code {code}");
        var session = await _client.Auth.ExchangeCodeForSession(_pkceCodeVerifier!, code.ToString());
        var authToken = MakeAuthToken(session);
        Console.WriteLine($"exchanged code for token: {authToken}");
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
            Console.WriteLine("Token refresh not required, or not possible. Reasons:");
            if (jwt.ExpiresAt == null)                 Console.WriteLine($"    - expiry date is null");
            if (jwt.RefreshToken == null)              Console.WriteLine($"    - refresh token is null");
            if (DateTimeOffset.UtcNow < jwt.ExpiresAt) Console.WriteLine($"    - access token is still valid; {DateTimeOffset.UtcNow} < {jwt.ExpiresAt}");
            return jwt;
        }
        else
        {
            try
            {
                Console.WriteLine($"attempting to sign in using a refresh token. the old token: {jwt}");
                var session = await _client.Auth.SignIn(Constants.SignInType.RefreshToken, jwt.RefreshToken);
                var authToken = MakeAuthToken(session);
                Console.WriteLine($"    the new token: {authToken}");
                return authToken;
            }
            catch (Exception e)
            {
                Console.WriteLine($"    failed to sign in using refresh token: {e}");
                return null;
            }
        }
    }

    public async Task<bool> ValidateJwtAsync(string jwt)
    {
        try
        {
            Console.WriteLine($"verifying JWT with Supabase: {jwt}");

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
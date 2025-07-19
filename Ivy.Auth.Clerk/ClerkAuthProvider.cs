using System.Reflection;
using Ivy.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using ClerkNet;

namespace Ivy.Auth.Clerk;

public class ClerkOAuthException(string? error, string? errorDescription)
    : Exception($"Clerk error: '{error}' - {errorDescription}")
{
    public string? Error { get; } = error;
    public string? ErrorDescription { get; } = errorDescription;
}

public class ClerkAuthProvider : IAuthProvider
{
    private readonly ClerkClient _client;
    private readonly IConfiguration _configuration;

    private readonly List<AuthOption> _authOptions = new();

    public ClerkAuthProvider()
    {
        _configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddUserSecrets(Assembly.GetEntryAssembly()!)
            .Build();

        var secretKey = _configuration.GetValue<string>("CLERK_SECRET_KEY") ?? throw new Exception("CLERK_SECRET_KEY is required");
        var publishableKey = _configuration.GetValue<string>("CLERK_PUBLISHABLE_KEY") ?? throw new Exception("CLERK_PUBLISHABLE_KEY is required");

        _client = new ClerkClient(secretKey);
    }

    public async Task<AuthToken?> LoginAsync(string email, string password)
    {
        // Clerk doesn't support direct email/password authentication in the same way as Supabase
        // This would typically be handled on the frontend with Clerk's JavaScript SDK
        // For now, we'll return null to indicate this method is not supported
        await Task.CompletedTask;
        return null;
    }

    public async Task<Uri> GetOAuthUriAsync(AuthOption option, Uri callbackUri)
    {
        // Clerk OAuth URLs are typically generated on the frontend
        // For server-side, we would need to construct the OAuth URL manually
        var clerkDomain = _configuration.GetValue<string>("CLERK_DOMAIN") ?? throw new Exception("CLERK_DOMAIN is required");
        
        var provider = option.Id switch
        {
            "google" => "oauth_google",
            "apple" => "oauth_apple",
            "discord" => "oauth_discord",
            "github" => "oauth_github",
            "gitlab" => "oauth_gitlab",
            "microsoft" => "oauth_microsoft",
            "facebook" => "oauth_facebook",
            "twitter" => "oauth_twitter",
            "linkedin" => "oauth_linkedin",
            _ => throw new ArgumentException($"Unknown OAuth provider: {option.Id}"),
        };

        var oauthUrl = $"https://{clerkDomain}/v1/oauth/{provider}/authorize?redirect_url={Uri.EscapeDataString(callbackUri.ToString())}";
        
        await Task.CompletedTask;
        return new Uri(oauthUrl);
    }

    public async Task<AuthToken?> HandleOAuthCallbackAsync(HttpRequest request)
    {
        var code = request.Query["code"];
        var error = request.Query["error"];
        var errorDescription = request.Query["error_description"];
        
        if (error.Count > 0 || errorDescription.Count > 0)
        {
            throw new ClerkOAuthException(error, errorDescription);
        }
        else if (code.Count == 0)
        {
            throw new Exception("Received no recognized query parameters from Clerk.");
        }

        // In a real implementation, you would exchange the code for tokens
        // This is a simplified version - Clerk typically handles this on the frontend
        await Task.CompletedTask;
        return null;
    }

    public async Task LogoutAsync(string jwt)
    {
        if (string.IsNullOrEmpty(jwt))
        {
            return;
        }

        try
        {
            // In Clerk, you would typically revoke the user's sessions
            // This is a simplified implementation
            await Task.CompletedTask;
        }
        catch (Exception)
        {
            // Handle logout errors gracefully
        }
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
            // Clerk handles token refresh differently - typically on the frontend
            // For now, return the original token
            await Task.CompletedTask;
            return jwt;
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
            var sessionClaims = await _client.VerifySessionTokenAsync(jwt);
            return sessionClaims != null;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<UserInfo?> GetUserInfoAsync(string jwt)
    {
        try
        {
            var sessionClaims = await _client.VerifySessionTokenAsync(jwt);
            
            if (sessionClaims == null)
            {
                return null;
            }

            var userId = sessionClaims.Subject;
            var user = await _client.GetUserAsync(userId);

            if (user == null)
            {
                return null;
            }

            return new UserInfo(
                user.Id,
                user.EmailAddresses?.FirstOrDefault()?.EmailAddress ?? string.Empty,
                $"{user.FirstName} {user.LastName}".Trim(),
                user.ImageUrl
            );
        }
        catch (Exception)
        {
            return null;
        }
    }

    public AuthOption[] GetAuthOptions()
    {
        return _authOptions.ToArray();
    }

    public ClerkAuthProvider UseGoogle()
    {
        _authOptions.Add(new AuthOption(AuthFlow.OAuth, "Google", "google", Icons.Google));
        return this;
    }

    public ClerkAuthProvider UseApple()
    {
        _authOptions.Add(new AuthOption(AuthFlow.OAuth, "Apple", "apple", Icons.Apple));
        return this;
    }

    public ClerkAuthProvider UseDiscord()
    {
        _authOptions.Add(new AuthOption(AuthFlow.OAuth, "Discord", "discord", Icons.Discord));
        return this;
    }

    public ClerkAuthProvider UseGithub()
    {
        _authOptions.Add(new AuthOption(AuthFlow.OAuth, "GitHub", "github", Icons.Github));
        return this;
    }

    public ClerkAuthProvider UseGitlab()
    {
        _authOptions.Add(new AuthOption(AuthFlow.OAuth, "GitLab", "gitlab", Icons.Gitlab));
        return this;
    }

    public ClerkAuthProvider UseMicrosoft()
    {
        _authOptions.Add(new AuthOption(AuthFlow.OAuth, "Microsoft", "microsoft", Icons.Microsoft));
        return this;
    }

    public ClerkAuthProvider UseFacebook()
    {
        _authOptions.Add(new AuthOption(AuthFlow.OAuth, "Facebook", "facebook", Icons.Facebook));
        return this;
    }

    public ClerkAuthProvider UseTwitter()
    {
        _authOptions.Add(new AuthOption(AuthFlow.OAuth, "Twitter", "twitter", Icons.Twitter));
        return this;
    }

    public ClerkAuthProvider UseLinkedIn()
    {
        _authOptions.Add(new AuthOption(AuthFlow.OAuth, "LinkedIn", "linkedin", Icons.LinkedIn));
        return this;
    }
}
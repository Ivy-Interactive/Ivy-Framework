using System.Reflection;
using Ivy.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using FirebaseAuthentication.net;
using Google.Apis.Auth.OAuth2;

namespace Ivy.Auth.Firebase;

public class FirebaseOAuthException(string? error, string? errorCode, string? errorDescription)
    : Exception($"Firebase error: '{error}', code '{errorCode}' - {errorDescription}")
{
    public string? Error { get; } = error;
    public string? ErrorCode { get; } = errorCode;
    public string? ErrorDescription { get; } = errorDescription;
}

public class FirebaseAuthProvider : IAuthProvider
{
    private readonly FirebaseAuthClient _authClient;
    private readonly FirebaseAuth _firebaseAuth;
    private readonly List&lt;AuthOption&gt; _authOptions = new();

    public FirebaseAuthProvider()
    {
        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddUserSecrets(Assembly.GetEntryAssembly()!)
            .Build();

        var apiKey = configuration.GetValue&lt;string&gt;("FIREBASE_API_KEY") ?? throw new Exception("FIREBASE_API_KEY is required");
        var authDomain = configuration.GetValue&lt;string&gt;("FIREBASE_AUTH_DOMAIN") ?? throw new Exception("FIREBASE_AUTH_DOMAIN is required");
        var projectId = configuration.GetValue&lt;string&gt;("FIREBASE_PROJECT_ID") ?? throw new Exception("FIREBASE_PROJECT_ID is required");
        var serviceAccountKey = configuration.GetValue&lt;string&gt;("FIREBASE_SERVICE_ACCOUNT_KEY");

        // Initialize Firebase client SDK for authentication
        var config = new FirebaseConfig
        {
            ApiKey = apiKey,
            AuthDomain = authDomain,
            ProjectId = projectId
        };

        _authClient = new FirebaseAuthClient(config);

        // Initialize Firebase Admin SDK for user management
        if (FirebaseApp.DefaultInstance == null)
        {
            FirebaseApp.Create(new AppOptions()
            {
                Credential = !string.IsNullOrEmpty(serviceAccountKey)
                    ? GoogleCredential.FromJson(serviceAccountKey)
                    : GoogleCredential.GetApplicationDefault(),
                ProjectId = projectId
            });
        }

        _firebaseAuth = FirebaseAuth.DefaultInstance;
    }

    public async Task&lt;AuthToken?&gt; LoginAsync(string email, string password)
    {
        try
        {
            var user = await _authClient.SignInWithEmailAndPasswordAsync(email, password);
            return MakeAuthToken(user);
        }
        catch (FirebaseAuthException ex)
        {
            throw new FirebaseOAuthException(ex.Reason.ToString(), ex.ErrorCode, ex.Message);
        }
    }

    public async Task&lt;Uri&gt; GetOAuthUriAsync(AuthOption option, Uri callbackUri)
    {
        var provider = option.Id switch
        {
            "google" =&gt; "google.com",
            "facebook" =&gt; "facebook.com",
            "twitter" =&gt; "twitter.com",
            "github" =&gt; "github.com",
            "microsoft" =&gt; "microsoft.com",
            "apple" =&gt; "apple.com",
            _ =&gt; throw new ArgumentException($"Unknown OAuth provider: {option.Id}"),
        };

        // Firebase OAuth flow requires client-side implementation
        // This is a simplified approach - in a real implementation, you'd use the Firebase Auth SDK
        var redirectUri = callbackUri.ToString();
        var authUrl = $"https://{_authClient.Config.AuthDomain}/v1/authorize?provider={provider}&amp;redirect_uri={redirectUri}";
        
        return new Uri(authUrl);
    }

    public async Task&lt;AuthToken?&gt; HandleOAuthCallbackAsync(HttpRequest request)
    {
        var code = request.Query["code"];
        var error = request.Query["error"];
        var errorCode = request.Query["error_code"];
        var errorDescription = request.Query["error_description"];

        if (error.Count &gt; 0 || errorCode.Count &gt; 0 || errorDescription.Count &gt; 0)
        {
            throw new FirebaseOAuthException(error, errorCode, errorDescription);
        }
        else if (code.Count == 0)
        {
            throw new Exception("Received no recognized query parameters from Firebase.");
        }

        // Note: In a real implementation, you'd exchange the code for tokens
        // This is a simplified version
        throw new NotImplementedException("OAuth callback handling requires additional Firebase OAuth implementation.");
    }

    public async Task LogoutAsync(string jwt)
    {
        try
        {
            // Revoke the refresh token to log out the user
            var decodedToken = await _firebaseAuth.VerifyIdTokenAsync(jwt);
            await _firebaseAuth.RevokeRefreshTokensAsync(decodedToken.Uid);
        }
        catch (Exception)
        {
            // Ignore errors during logout
        }
    }

    public async Task&lt;AuthToken?&gt; RefreshJwtAsync(AuthToken jwt)
    {
        if (jwt.ExpiresAt == null || jwt.RefreshToken == null || DateTimeOffset.UtcNow &lt; jwt.ExpiresAt)
        {
            // Refresh not needed (or not possible).
            return jwt;
        }

        try
        {
            var user = await _authClient.GetLinkedAccountsAsync(jwt.RefreshToken);
            if (user?.FirebaseToken != null)
            {
                return MakeAuthToken(user);
            }
        }
        catch (Exception)
        {
            // Refresh failed
        }

        return null;
    }

    public async Task&lt;bool&gt; ValidateJwtAsync(string jwt)
    {
        try
        {
            var decodedToken = await _firebaseAuth.VerifyIdTokenAsync(jwt);
            return decodedToken != null;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task&lt;UserInfo?&gt; GetUserInfoAsync(string jwt)
    {
        try
        {
            var decodedToken = await _firebaseAuth.VerifyIdTokenAsync(jwt);
            var userRecord = await _firebaseAuth.GetUserAsync(decodedToken.Uid);

            return new UserInfo(
                userRecord.Uid,
                userRecord.Email,
                userRecord.DisplayName ?? string.Empty,
                userRecord.PhotoUrl
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

    public FirebaseAuthProvider UseEmailPassword()
    {
        _authOptions.Add(new AuthOption(AuthFlow.EmailPassword));
        return this;
    }

    public FirebaseAuthProvider UseGoogle()
    {
        _authOptions.Add(new AuthOption(AuthFlow.OAuth, "Google", "google", Icons.Google));
        return this;
    }

    public FirebaseAuthProvider UseFacebook()
    {
        _authOptions.Add(new AuthOption(AuthFlow.OAuth, "Facebook", "facebook", Icons.Facebook));
        return this;
    }

    public FirebaseAuthProvider UseTwitter()
    {
        _authOptions.Add(new AuthOption(AuthFlow.OAuth, "Twitter", "twitter", Icons.Twitter));
        return this;
    }

    public FirebaseAuthProvider UseGithub()
    {
        _authOptions.Add(new AuthOption(AuthFlow.OAuth, "GitHub", "github", Icons.Github));
        return this;
    }

    public FirebaseAuthProvider UseMicrosoft()
    {
        _authOptions.Add(new AuthOption(AuthFlow.OAuth, "Microsoft", "microsoft", Icons.Microsoft));
        return this;
    }

    public FirebaseAuthProvider UseApple()
    {
        _authOptions.Add(new AuthOption(AuthFlow.OAuth, "Apple", "apple", Icons.Apple));
        return this;
    }

    private AuthToken? MakeAuthToken(FirebaseUser? user) =&gt;
        user?.FirebaseToken != null
            ? new AuthToken(user.FirebaseToken, user.RefreshToken, DateTimeOffset.FromUnixTimeSeconds(user.ExpiresIn))
            : null;
}
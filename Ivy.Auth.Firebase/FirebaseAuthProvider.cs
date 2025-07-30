using System.Reflection;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;
using Ivy.Hooks;
using Ivy.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Ivy.Core;
using Ivy.Client;

namespace Ivy.Auth.Firebase;

public class FirebaseOAuthException(string? error, string? errorDescription)
    : Exception($"Firebase error: '{error}' - {errorDescription}")
{
    public string? Error { get; } = error;
    public string? ErrorDescription { get; } = errorDescription;
}

public class FirebaseAuthProvider : IAuthProvider
{
    private readonly HttpClient _httpClient = new();
    private readonly string _apiKey;
    private readonly string _authDomain;
    private readonly string _projectId;
    private readonly List<AuthOption> _authOptions = [];
    private readonly FirebaseAuth _firebaseAuth;

    public FirebaseAuthProvider()
    {
        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddUserSecrets(Assembly.GetEntryAssembly()!)
            .Build();

        _apiKey = configuration.GetValue<string>("FIREBASE_API_KEY") ?? throw new Exception("FIREBASE_API_KEY is required");
        _authDomain = configuration.GetValue<string>("FIREBASE_AUTH_DOMAIN") ?? throw new Exception("FIREBASE_AUTH_DOMAIN is required");
        _projectId = configuration.GetValue<string>("FIREBASE_PROJECT_ID") ?? throw new Exception("FIREBASE_PROJECT_ID is required");
        var serviceAccountKey = configuration.GetValue<string>("FIREBASE_SERVICE_ACCOUNT_KEY") ?? throw new Exception("FIREBASE_SERVICE_ACCOUNT_KEY is required");

        // Initialize FirebaseAdmin
        FirebaseApp app = FirebaseApp.DefaultInstance;
        if (app == null)
        {
            // Create new app if DefaultInstance returned null
            var credential = GoogleCredential.FromJson(serviceAccountKey);
            var options = new AppOptions
            {
                Credential = credential,
                ProjectId = _projectId
            };

            app = FirebaseApp.Create(options);
        }

        _firebaseAuth = FirebaseAuth.GetAuth(app);
    }

    public async Task<AuthToken?> LoginAsync(string email, string password)
    {
        var requestData = new
        {
            email,
            password,
            returnSecureToken = true
        };

        var response = await _httpClient.PostAsJsonAsync(
            $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={_apiKey}",
            requestData
        );

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadFromJsonAsync<FirebaseErrorResponse>();
            throw new FirebaseOAuthException(error?.Error?.Message, error?.Error?.Message);
        }

        var result = await response.Content.ReadFromJsonAsync<FirebaseSignInResponse>();
        if (result == null)
        {
            return null;
        }

        var expiresAt = DateTimeOffset.UtcNow.AddSeconds(int.Parse(result.ExpiresIn));
        return new AuthToken(result.IdToken, result.RefreshToken, expiresAt);
    }

    public bool ShouldUseUnifiedOAuthFlow() => true;

    public async Task<AuthToken?> LoginAsync(IClientProvider client, AuthOption option)
    {
        var result = await client.SignInToFirebaseAsync(
            _apiKey,
            _authDomain,
            _projectId,
            option
        );

        if (!result.Success)
        {
            throw new FirebaseOAuthException(result.ErrorCode, result.ErrorMessage);
        }

        return result.Token;
    }

    public Task<Uri> GetOAuthUriAsync(AuthOption option, WebhookEndpoint callback)
        => throw new InvalidOperationException("GetOAuthUriAsync() should not be called on FirebaseAuthProvider. Use LoginAsync() instead.");

    public Task<AuthToken?> HandleOAuthCallbackAsync(HttpRequest request)
        => throw new InvalidOperationException("HandleOAuthCallbackAsync() should not be called on FirebaseAuthProvider. Use LoginAsync() instead.");

    public async Task LogoutAsync(string jwt)
    {
        try
        {
            // Validate the JWT to get the user ID
            var decodedToken = await _firebaseAuth.VerifyIdTokenAsync(jwt);
            if (decodedToken != null)
            {
                // Revoke all refresh tokens for the user
                await _firebaseAuth.RevokeRefreshTokensAsync(decodedToken.Uid);
            }
        }
        catch (Exception)
        {
            // Logout failures are typically not critical
        }

        await Task.CompletedTask;
    }

    public async Task<AuthToken?> RefreshJwtAsync(AuthToken jwt)
    {
        if (jwt.ExpiresAt == null || jwt.RefreshToken == null || DateTimeOffset.UtcNow < jwt.ExpiresAt)
        {
            return jwt;
        }

        try
        {
            var requestData = new
            {
                grant_type = "refresh_token",
                refresh_token = jwt.RefreshToken
            };

            var response = await _httpClient.PostAsJsonAsync(
                $"https://securetoken.googleapis.com/v1/token?key={_apiKey}",
                requestData
            );

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<FirebaseRefreshResponse>();
            if (result == null)
            {
                return null;
            }

            var expiresAt = DateTimeOffset.UtcNow.AddSeconds(int.Parse(result.ExpiresIn));
            return new AuthToken(result.IdToken, result.RefreshToken, expiresAt);
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
            var decodedToken = await _firebaseAuth.VerifyIdTokenAsync(jwt);
            return decodedToken != null;
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
            // Server-side verification with FirebaseAdmin SDK
            var decodedToken = await _firebaseAuth.VerifyIdTokenAsync(jwt);
            var firebaseUser = await _firebaseAuth.GetUserAsync(decodedToken.Uid);

            return new UserInfo(
                firebaseUser.Uid,
                firebaseUser.Email ?? string.Empty,
                firebaseUser.DisplayName,
                firebaseUser.PhotoUrl
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
}

// Response models for Firebase API responses

public class FirebaseErrorResponse
{
    [JsonPropertyName("error")]
    public FirebaseError? Error { get; set; }

    public class FirebaseError
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }
    }
}

public class FirebaseSignInResponse
{
    [JsonPropertyName("idToken")]
    public string IdToken { get; set; } = "";

    [JsonPropertyName("email")]
    public string Email { get; set; } = "";

    [JsonPropertyName("refreshToken")]
    public string RefreshToken { get; set; } = "";

    [JsonPropertyName("expiresIn")]
    public string ExpiresIn { get; set; } = "";

    [JsonPropertyName("localId")]
    public string LocalId { get; set; } = "";
}

public class FirebaseRefreshResponse
{
    [JsonPropertyName("id_token")]
    public string IdToken { get; set; } = "";

    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; } = "";

    [JsonPropertyName("expires_in")]
    public string ExpiresIn { get; set; } = "";
}

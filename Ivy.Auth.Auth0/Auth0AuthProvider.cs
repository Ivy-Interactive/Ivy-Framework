using System.Reflection;
using Auth0.AuthenticationApi;
using Auth0.AuthenticationApi.Models;
using Ivy.Hooks;
using Ivy.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Ivy.Auth.Auth0;

public class Auth0OAuthException(string? error, string? errorDescription)
    : Exception($"Auth0 error: '{error}' - {errorDescription}")
{
    public string? Error { get; } = error;
    public string? ErrorDescription { get; } = errorDescription;
}

public class Auth0AuthProvider : IAuthProvider
{
    private readonly AuthenticationApiClient _authClient;
    private readonly string _domain;
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly string _audience;
    private readonly List<AuthOption> _authOptions = new();

    public Auth0AuthProvider()
    {
        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddUserSecrets(Assembly.GetEntryAssembly()!)
            .Build();

        _domain = configuration.GetValue<string>("AUTH0_DOMAIN") ?? throw new Exception("AUTH0_DOMAIN is required");
        _clientId = configuration.GetValue<string>("AUTH0_CLIENT_ID") ?? throw new Exception("AUTH0_CLIENT_ID is required");
        _clientSecret = configuration.GetValue<string>("AUTH0_CLIENT_SECRET") ?? throw new Exception("AUTH0_CLIENT_SECRET is required");
        _audience = configuration.GetValue<string>("AUTH0_AUDIENCE") ?? "";

        _authClient = new AuthenticationApiClient(_domain);
    }

    public async Task<AuthToken?> LoginAsync(string email, string password)
    {
        var request = new ResourceOwnerTokenRequest
        {
            ClientId = _clientId,
            ClientSecret = _clientSecret,
            Username = email,
            Password = password,
            Scope = "openid profile email",
            Audience = _audience,
            Realm = "Username-Password-Authentication",
        };

        var response = await _authClient.GetTokenAsync(request);
        return new AuthToken(response.AccessToken, response.RefreshToken, DateTimeOffset.UtcNow.AddSeconds(response.ExpiresIn));
    }

    public Task<Uri> GetOAuthUriAsync(AuthOption option, WebhookEndpoint callback)
    {
        var connection = option.Id switch
        {
            "google-oauth2" => "google-oauth2",
            "github" => "github",
            "twitter" => "twitter",
            "microsoft" => "windowslive",
            "apple" => "apple",
            _ => throw new ArgumentException($"Unknown OAuth provider: {option.Id}"),
        };

        var callbackUri = callback.GetUri(includeIdInPath: false);
        var authorizationUrl = _authClient.BuildAuthorizationUrl()
            .WithResponseType(AuthorizationResponseType.Code)
            .WithClient(_clientId)
            .WithConnection(connection)
            .WithRedirectUrl(callbackUri.ToString())
            .WithScope("openid profile email")
            .WithState(callback.Id);

        if (!string.IsNullOrEmpty(_audience))
        {
            authorizationUrl = authorizationUrl.WithAudience(_audience);
        }

        return Task.FromResult(authorizationUrl.Build());
    }

    public async Task<AuthToken?> HandleOAuthCallbackAsync(HttpRequest request)
    {
        var code = request.Query["code"].ToString();
        var error = request.Query["error"].ToString();
        var errorDescription = request.Query["error_description"].ToString();

        if (error.Length > 0 || errorDescription.Length > 0)
        {
            throw new Auth0OAuthException(error, errorDescription);
        }

        if (code.Length == 0)
        {
            throw new Exception("Received no authorization code from Auth0.");
        }

        try
        {
            var request_ = new AuthorizationCodeTokenRequest
            {
                ClientId = _clientId,
                ClientSecret = _clientSecret,
                Code = code,
                RedirectUri = $"{request.Scheme}://{request.Host}{request.Path}"
            };

            var response = await _authClient.GetTokenAsync(request_);

            return new AuthToken(response.AccessToken, response.RefreshToken, DateTimeOffset.UtcNow.AddSeconds(response.ExpiresIn));
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to exchange authorization code for tokens: {ex.Message}");
        }
    }

    public async Task LogoutAsync(string jwt)
    {
        try
        {
            // Auth0 logout is typically handled on the client side
            // This method can be extended to revoke tokens if needed
            await Task.CompletedTask;
        }
        catch (Exception)
        {
            // Logout failures are typically not critical
        }
    }

    public async Task<AuthToken?> RefreshJwtAsync(AuthToken jwt)
    {
        if (jwt.ExpiresAt == null || jwt.RefreshToken == null || DateTimeOffset.UtcNow < jwt.ExpiresAt)
        {
            return jwt;
        }

        try
        {
            var request = new RefreshTokenRequest
            {
                ClientId = _clientId,
                ClientSecret = _clientSecret,
                RefreshToken = jwt.RefreshToken
            };

            var response = await _authClient.GetTokenAsync(request);
            return new AuthToken(response.AccessToken, response.RefreshToken ?? jwt.RefreshToken, DateTimeOffset.UtcNow.AddSeconds(response.ExpiresIn));
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
            var userInfo = await _authClient.GetUserInfoAsync(jwt);
            return userInfo != null;
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
            var userInfo = await _authClient.GetUserInfoAsync(jwt);
            if (userInfo == null)
            {
                return null;
            }

            return new UserInfo(
                userInfo.UserId,
                userInfo.Email,
                userInfo.FullName,
                userInfo.Picture
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

    public Auth0AuthProvider UseEmailPassword()
    {
        _authOptions.Add(new AuthOption(AuthFlow.EmailPassword));
        return this;
    }

    public Auth0AuthProvider UseGoogle()
    {
        _authOptions.Add(new AuthOption(AuthFlow.OAuth, "Google", "google-oauth2", Icons.Google));
        return this;
    }

    public Auth0AuthProvider UseGithub()
    {
        _authOptions.Add(new AuthOption(AuthFlow.OAuth, "GitHub", "github", Icons.Github));
        return this;
    }

    public Auth0AuthProvider UseTwitter()
    {
        _authOptions.Add(new AuthOption(AuthFlow.OAuth, "Twitter", "twitter", Icons.Twitter));
        return this;
    }

    public Auth0AuthProvider UseApple()
    {
        _authOptions.Add(new AuthOption(AuthFlow.OAuth, "Apple", "apple", Icons.Apple));
        return this;
    }

    public Auth0AuthProvider UseMicrosoft()
    {
        _authOptions.Add(new AuthOption(AuthFlow.OAuth, "Microsoft", "microsoft", Icons.Microsoft));
        return this;
    }
}
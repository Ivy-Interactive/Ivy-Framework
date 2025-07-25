using System.Reflection;
using Ivy.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Octokit;
using Octokit.Internal;

namespace Ivy.Auth.Github;

public class GitHubOAuthException(string? error, string? errorDescription)
    : Exception($"GitHub error: '{error}' - {errorDescription}")
{
    public string? Error { get; } = error;
    public string? ErrorDescription { get; } = errorDescription;
}

public class GitHubAuthProvider : IAuthProvider
{
    private readonly GitHubClient _client;
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly List<AuthOption> _authOptions = new();
    private readonly HttpClient _httpClient;
    private string? _state = null;
    private string[]? _scopes = null;

    public GitHubAuthProvider()
    {
        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddUserSecrets(Assembly.GetEntryAssembly()!)
            .Build();

        _clientId = configuration.GetValue<string>("GITHUB_CLIENT_ID") ?? throw new Exception("GITHUB_CLIENT_ID is required");
        _clientSecret = configuration.GetValue<string>("GITHUB_CLIENT_SECRET") ?? throw new Exception("GITHUB_CLIENT_SECRET is required");

        _client = new GitHubClient(new ProductHeaderValue("Ivy-Framework"));
        _httpClient = new HttpClient();
    }

    public async Task<AuthToken?> LoginAsync(string email, string password)
    {
        throw new NotSupportedException("GitHub does not support email/password authentication. Use OAuth instead.");
    }

    public Task<Uri> GetOAuthUriAsync(AuthOption option, Uri callbackUri)
    {
        _scopes = option.Tag as string[] ?? new[] { "user:email", "read:user" };
        _state = Guid.NewGuid().ToString();

        var request = new OauthLoginRequest(_clientId)
        {
            RedirectUri = callbackUri,
            State = _state
        };

        foreach (var scope in _scopes)
        {
            request.Scopes.Add(scope);
        }

        return Task.FromResult(_client.Oauth.GetGitHubLoginUrl(request));
    }

    public async Task<AuthToken?> HandleOAuthCallbackAsync(HttpRequest request)
    {
        var code = request.Query["code"].ToString();
        var state = request.Query["state"].ToString();
        var error = request.Query["error"].ToString();
        var errorDescription = request.Query["error_description"].ToString();

        if (!string.IsNullOrEmpty(error))
        {
            throw new GitHubOAuthException(error, errorDescription);
        }

        if (string.IsNullOrEmpty(code))
        {
            throw new Exception("No authorization code received from GitHub.");
        }

        if (state != _state)
        {
            throw new Exception("State parameter mismatch. Possible CSRF attack.");
        }

        var tokenRequest = new OauthTokenRequest(_clientId, _clientSecret, code);
        var tokenResponse = await _client.Oauth.CreateAccessToken(tokenRequest);

        if (tokenResponse?.AccessToken == null)
        {
            throw new Exception("Failed to obtain access token from GitHub.");
        }

        return new AuthToken(tokenResponse.AccessToken, null, null);
    }

    public Task LogoutAsync(string jwt)
    {
        // GitHub tokens cannot be revoked via API without additional permissions
        // The token will remain valid until it expires naturally
        return Task.CompletedTask;
    }

    public async Task<AuthToken?> RefreshJwtAsync(AuthToken jwt)
    {
        // GitHub access tokens don't expire and don't have refresh tokens
        // Return the original token if it's still valid
        if (await ValidateJwtAsync(jwt.Jwt))
        {
            return jwt;
        }

        return null;
    }

    public async Task<bool> ValidateJwtAsync(string jwt)
    {
        try
        {
            var client = new GitHubClient(new ProductHeaderValue("Ivy-Framework"))
            {
                Credentials = new Credentials(jwt)
            };

            // Try to get the authenticated user to validate the token
            var user = await client.User.Current();
            return user != null;
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
            var client = new GitHubClient(new ProductHeaderValue("Ivy-Framework"))
            {
                Credentials = new Credentials(jwt)
            };

            var user = await client.User.Current();

            if (user == null || string.IsNullOrEmpty(user.Login))
            {
                return null;
            }

            // Get primary email if available
            var emails = await client.User.Email.GetAll();
            var primaryEmail = emails?.FirstOrDefault(e => e.Primary)?.Email ?? user.Email;

            return new UserInfo(
                user.Id.ToString(),
                primaryEmail ?? user.Login,
                user.Name ?? user.Login,
                user.AvatarUrl
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

    public GitHubAuthProvider UseGithub(string[]? scopes = null)
    {
        var scopesTag = scopes ?? new[] { "user:email", "read:user" };
        _authOptions.Add(new AuthOption(AuthFlow.OAuth, "GitHub", "github", Icons.Github, scopesTag));
        return this;
    }
}
using System.Text.Json;
using Ivy.Auth;
using Ivy.Hooks;
using Ivy.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
namespace Ivy.Auth.GitHub;

/// <summary>GitHub OAuth exception</summary>
public class GitHubOAuthException(string? error, string? errorDescription)
    : Exception($"GitHub OAuth error: '{error}' - {errorDescription}")
{
    public string? Error { get; } = error;
    public string? ErrorDescription { get; } = errorDescription;
}

/// <summary>GitHub OAuth 2.0 authentication provider</summary>
public class GitHubAuthProvider : IAuthProvider
{
    private readonly HttpClient _httpClient;
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly string _redirectUri;
    private readonly List<AuthOption> _authOptions = new();

    /// <summary>Initialize GitHub auth provider</summary>
    public GitHubAuthProvider(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClient = httpClientFactory.CreateClient("GitHubAuth");

        _clientId = configuration.GetValue<string>("GitHub:ClientId") ?? throw new InvalidOperationException(
            "Missing required configuration: 'GitHub:ClientId'. Please set this value in your environment variables or user secrets. See the README setup steps for instructions.");
        _clientSecret = configuration.GetValue<string>("GitHub:ClientSecret") ?? throw new InvalidOperationException(
            "Missing required configuration: 'GitHub:ClientSecret'. Please set this value in your environment variables or user secrets. See the README setup steps for instructions.");
        _redirectUri = configuration.GetValue<string>("GitHub:RedirectUri") ?? throw new InvalidOperationException(
            "Missing required configuration: 'GitHub:RedirectUri'. Please set this value in your environment variables or user secrets. See the README setup steps for instructions.");
    }

    /// <summary>Not supported - use OAuth flow</summary>
    public Task<AuthToken?> LoginAsync(IAuthSession authSession, string email, string password, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("GitHub authentication only supports OAuth flow. Use GetOAuthUriAsync and HandleOAuthCallbackAsync instead.");
    }

    /// <summary>Generate OAuth authorization URI</summary>
    public Task<Uri> GetOAuthUriAsync(IAuthSession authSession, AuthOption option, WebhookEndpoint callback, CancellationToken cancellationToken = default)
    {
        var callbackUri = callback.GetUri(includeIdInPath: false);

        var authUrl = new UriBuilder("https://github.com/login/oauth/authorize")
        {
            Query = string.Join("&", new[]
            {
                $"client_id={Uri.EscapeDataString(_clientId)}",
                $"redirect_uri={Uri.EscapeDataString(callbackUri.ToString())}",
                $"scope={Uri.EscapeDataString("user:email")}",
                $"state={Uri.EscapeDataString(callback.Id)}",
                "allow_signup=true"
            })
        };

        return Task.FromResult(authUrl.Uri);
    }

    /// <summary>Handle OAuth callback and exchange code for token</summary>
    public async Task<AuthToken?> HandleOAuthCallbackAsync(IAuthSession authSession, HttpRequest request, CancellationToken cancellationToken = default)
    {
        var code = request.Query["code"].ToString();
        var error = request.Query["error"].ToString();
        var errorDescription = request.Query["error_description"].ToString();

        if (!string.IsNullOrEmpty(error) || !string.IsNullOrEmpty(errorDescription))
        {
            throw new GitHubOAuthException(error, errorDescription);
        }

        if (string.IsNullOrEmpty(code))
        {
            var details = $"Received no authorization code from GitHub. " +
                $"Possible causes: user denied access, invalid redirect URI, or other OAuth error. " +
                $"Error: '{error}', Error Description: '{errorDescription}'";
            throw new Exception(details);
        }

        try
        {
            var tokenResponse = await ExchangeCodeForTokenAsync(code, cancellationToken);

            if (tokenResponse == null)
            {
                return null;
            }

            return new AuthToken(tokenResponse.AccessToken);
        }
        catch (HttpRequestException ex)
        {
            throw new HttpRequestException($"GitHub OAuth token exchange failed: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Unexpected error during GitHub OAuth token exchange: {ex.Message}", ex);
        }
    }

    /// <summary>No-op logout</summary>
    public Task LogoutAsync(IAuthSession authSession, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    /// <summary>No refresh tokens - returns null</summary>
    public Task<AuthToken?> RefreshAccessTokenAsync(IAuthSession authSession, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<AuthToken?>(null);
    }

    /// <summary>Validate access token via GitHub API</summary>
    public async Task<bool> ValidateAccessTokenAsync(IAuthSession authSession, CancellationToken cancellationToken = default)
    {
        var token = authSession.AuthToken?.AccessToken;
        if (string.IsNullOrWhiteSpace(token))
            return false;

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/user");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
            request.Headers.Add("X-GitHub-Api-Version", "2022-11-28");

            var response = await _httpClient.SendAsync(request, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return false;
        }
    }

    /// <summary>Get user info from GitHub API</summary>
    public async Task<UserInfo?> GetUserInfoAsync(IAuthSession authSession, CancellationToken cancellationToken = default)
    {
        var token = authSession.AuthToken?.AccessToken;
        if (string.IsNullOrWhiteSpace(token))
            return null;

        try
        {
            using var userRequest = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/user");
            userRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            userRequest.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
            userRequest.Headers.Add("X-GitHub-Api-Version", "2022-11-28");

            var userResponse = await _httpClient.SendAsync(userRequest, cancellationToken);
            if (!userResponse.IsSuccessStatusCode)
            {
                return null;
            }

            var userJson = await userResponse.Content.ReadAsStringAsync(cancellationToken);
            using var userDoc = JsonDocument.Parse(userJson);
            var user = userDoc.RootElement;

            var userId = user.GetProperty("id").GetInt64().ToString();
            var login = user.GetProperty("login").GetString() ?? "";
            var name = user.TryGetProperty("name", out var nameProp) ? nameProp.GetString() : null;
            var avatarUrl = user.TryGetProperty("avatar_url", out var avatarProp) ? avatarProp.GetString() : null;

            using var emailRequest = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/user/emails");
            emailRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            emailRequest.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
            emailRequest.Headers.Add("X-GitHub-Api-Version", "2022-11-28");

            var emailResponse = await _httpClient.SendAsync(emailRequest, cancellationToken);
            string? email = null;

            if (emailResponse.IsSuccessStatusCode)
            {
                var emailJson = await emailResponse.Content.ReadAsStringAsync(cancellationToken);
                using var emailDoc = JsonDocument.Parse(emailJson);
                var emails = emailDoc.RootElement.EnumerateArray();

                string? primaryEmail = null;
                string? firstVerifiedEmail = null;

                foreach (var emailObj in emails)
                {
                    if (emailObj.TryGetProperty("primary", out var primaryProp) && primaryProp.GetBoolean())
                    {
                        primaryEmail = emailObj.GetProperty("email").GetString();
                        break;
                    }
                    else if (firstVerifiedEmail == null && emailObj.TryGetProperty("verified", out var verifiedProp) && verifiedProp.GetBoolean())
                    {
                        firstVerifiedEmail = emailObj.GetProperty("email").GetString();
                    }
                }

                email = primaryEmail ?? firstVerifiedEmail;
            }

            email ??= login;

            return new UserInfo(userId, email, name, avatarUrl);
        }
        catch (Exception ex) when (ex is JsonException or HttpRequestException or TaskCanceledException)
        {
            return null;
        }
    }

    /// <summary>Get auth options</summary>
    public AuthOption[] GetAuthOptions()
    {
        return _authOptions.ToArray();
    }

    /// <summary>No expiration - returns null</summary>
    public Task<TokenLifetime?> GetAccessTokenLifetimeAsync(IAuthSession authSession, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<TokenLifetime?>(null);
    }

    /// <summary>Add GitHub auth option</summary>
    public GitHubAuthProvider UseGitHub()
    {
        _authOptions.Add(new AuthOption(AuthFlow.OAuth, "GitHub", "github", Icons.Github));
        return this;
    }

    private async Task<GitHubTokenResponse?> ExchangeCodeForTokenAsync(string code, CancellationToken cancellationToken)
    {
        using var requestBody = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("client_id", _clientId),
            new KeyValuePair<string, string>("client_secret", _clientSecret),
            new KeyValuePair<string, string>("code", code),
            new KeyValuePair<string, string>("redirect_uri", _redirectUri)
        });

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://github.com/login/oauth/access_token");
        request.Content = requestBody;
        request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        request.Headers.Add("X-GitHub-Api-Version", "2022-11-28");

        var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                $"GitHub token exchange failed with status code {(int)response.StatusCode}: {response.ReasonPhrase}. Response: {errorContent}",
                null,
                response.StatusCode
            );
        }

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        try
        {
            using var jsonDoc = JsonDocument.Parse(responseContent);
            var root = jsonDoc.RootElement;

            if (root.TryGetProperty("access_token", out var accessTokenProp))
            {
                return new GitHubTokenResponse(accessTokenProp.GetString()!);
            }

            return null;
        }
        catch (JsonException ex)
        {
            throw new HttpRequestException($"Invalid JSON response from GitHub token endpoint: {ex.Message}", ex);
        }
    }

    private record GitHubTokenResponse(string AccessToken);
}

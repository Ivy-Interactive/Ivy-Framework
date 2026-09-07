using System.Text.Json;
using Ivy.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Ivy.Auth.GitHub;

/// <summary>GitHub OAuth exception</summary>
public class GitHubOAuthException(string? error, string? errorDescription)
    : Exception($"GitHub OAuth error: '{error}' - {errorDescription}")
{
    public string? Error { get; } = error;
    public string? ErrorDescription { get; } = errorDescription;
}

/// <summary>GitHub OAuth 2.0 authentication provider</summary>
public class GitHubAuthProvider : GitHubAuthTokenHandler, IAuthProvider
{
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly string _redirectUri;

    /// <summary>Initialize GitHub auth provider</summary>
    public GitHubAuthProvider(IConfiguration configuration, ILogger<GitHubAuthTokenHandler>? logger = null) : base(configuration, logger)
    {
        _clientId = configuration.GetValue<string>("GitHub:ClientId") ?? throw new InvalidOperationException(
            "Missing required configuration: 'GitHub:ClientId'. Please set this value in your environment variables or user secrets. See the README setup steps for instructions.");
        _clientSecret = configuration.GetValue<string>("GitHub:ClientSecret") ?? throw new InvalidOperationException(
            "Missing required configuration: 'GitHub:ClientSecret'. Please set this value in your environment variables or user secrets. See the README setup steps for instructions.");
        _redirectUri = configuration.GetValue<string>("GitHub:RedirectUri") ?? throw new InvalidOperationException(
            "Missing required configuration: 'GitHub:RedirectUri'. Please set this value in your environment variables or user secrets. See the README setup steps for instructions.");
    }

    /// <summary>Not supported - use OAuth flow</summary>
    public Task<LoginResult> LoginAsync(IAuthSession authSession, string email, string password, CancellationToken cancellationToken = default)
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

    /// <summary>Get auth options</summary>
    public AuthOption[] GetAuthOptions() => [new AuthOption(AuthFlow.OAuth, "GitHub", "github", Icons.Github)];

    /// <summary>Add GitHub auth option</summary>
    [Obsolete("GitHub OAuth is now enabled by default. This method is no longer necessary and will be removed in a future version.")]
    public GitHubAuthProvider UseGitHub() => this;

    /// <summary>Get brokered auth sessions - returns live session references that stay up-to-date</summary>
    public Task<BrokeredSessionsResult> GetBrokeredSessionsAsync(IAuthSession authSession, bool skipCache = false, CancellationToken cancellationToken = default)
        => Task.FromResult(BrokeredSessionsResult.Success([]));

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

        var response = await HttpClient.SendAsync(request, cancellationToken);

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

            if (root.TryGetProperty("error", out var errorProp))
            {
                var error = errorProp.GetString();
                var errorDescription = root.TryGetProperty("error_description", out var descProp) ? descProp.GetString() : null;
                throw new GitHubOAuthException(error, errorDescription);
            }

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

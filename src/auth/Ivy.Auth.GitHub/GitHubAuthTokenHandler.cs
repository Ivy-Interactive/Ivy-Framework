using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace Ivy.Auth.GitHub;

/// <summary>GitHub auth token handler</summary>
[OAuthTokenHandler(OAuthProviders.GitHub)]
public class GitHubAuthTokenHandler : IAuthTokenHandler
{
    protected readonly HttpClient HttpClient;
    private readonly ILogger<GitHubAuthTokenHandler> _logger;

    /// <summary>Initialize GitHub auth token handler</summary>
    public GitHubAuthTokenHandler(IConfiguration configuration, ILogger<GitHubAuthTokenHandler>? logger = null)
    {
        var userAgent = AuthProviderHelpers.GetUserAgent(configuration, "GitHub:UserAgent");
        HttpClient = new HttpClient();
        HttpClient.DefaultRequestHeaders.Add("User-Agent", userAgent);

        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<GitHubAuthTokenHandler>.Instance;
    }

    /// <summary>No refresh tokens - returns existing token if valid</summary>
    public async Task<AuthToken?> RefreshAccessTokenAsync(IAuthTokenHandlerSession authSession, CancellationToken cancellationToken = default)
    {
        // GitHub session tokens cannot be refreshed - validate and return null if invalid
        var isValid = await ValidateAccessTokenAsync(authSession, cancellationToken);
        return isValid ? authSession.AuthToken : null;
    }

    /// <summary>Validate access token via GitHub API</summary>
    public async Task<bool> ValidateAccessTokenAsync(IAuthTokenHandlerSession authSession, CancellationToken cancellationToken = default)
    {
        var token = authSession.AuthToken?.AccessToken;
        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/user");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
            request.Headers.Add("X-GitHub-Api-Version", "2022-11-28");

            var response = await HttpClient.SendAsync(request, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            _logger.LogError(ex, "Exception during GitHub token validation");
            return false;
        }
    }

    /// <summary>Get user info from GitHub API</summary>
    public async Task<UserInfo?> GetUserInfoAsync(IAuthTokenHandlerSession authSession, CancellationToken cancellationToken = default)
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

            var userResponse = await HttpClient.SendAsync(userRequest, cancellationToken);
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

            var emailResponse = await HttpClient.SendAsync(emailRequest, cancellationToken);
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

    /// <summary>No expiration - returns null</summary>
    public Task<TokenLifetime?> GetAccessTokenLifetimeAsync(IAuthTokenHandlerSession authSession, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<TokenLifetime?>(null);
    }
}

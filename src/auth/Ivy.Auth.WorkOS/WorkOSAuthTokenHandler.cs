namespace Ivy.Auth.WorkOS;

/// <summary>WorkOS OAuth token handler</summary>
[OAuthTokenHandler(OAuthProviders.WorkOS)]
public class WorkOSAuthTokenHandler : IAuthTokenHandler
{
    protected readonly HttpClient HttpClient;

    /// <summary>Initialize WorkOS auth token handler</summary>
    public WorkOSAuthTokenHandler(HttpClient httpClient)
    {
        HttpClient = httpClient;
    }

    /// <summary>WorkOS tokens don't support direct refresh</summary>
    public Task<AuthToken?> RefreshAccessTokenAsync(IAuthTokenHandlerSession authSession, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<AuthToken?>(null);
    }

    /// <summary>Validate WorkOS OAuth access token</summary>
    public Task<bool> ValidateAccessTokenAsync(IAuthTokenHandlerSession authSession, CancellationToken cancellationToken = default)
    {
        var token = authSession.AuthToken?.AccessToken;
        return Task.FromResult(!string.IsNullOrWhiteSpace(token));
    }

    /// <summary>WorkOS doesn't provide standard user info endpoint</summary>
    public Task<UserInfo?> GetUserInfoAsync(IAuthTokenHandlerSession authSession, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<UserInfo?>(null);
    }

    /// <summary>Get WorkOS OAuth token lifetime</summary>
    public Task<TokenLifetime?> GetAccessTokenLifetimeAsync(IAuthTokenHandlerSession authSession, CancellationToken cancellationToken = default)
    {
        // WorkOS tokens typically last several hours
        return Task.FromResult<TokenLifetime?>(new TokenLifetime(DateTimeOffset.UtcNow.AddHours(4)));
    }
}

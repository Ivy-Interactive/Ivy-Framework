namespace Ivy.Auth.Notion;

/// <summary>Notion OAuth token handler</summary>
public class NotionAuthTokenHandler : IAuthTokenHandler
{
    protected readonly HttpClient HttpClient;

    /// <summary>Initialize Notion auth token handler</summary>
    public NotionAuthTokenHandler(HttpClient httpClient)
    {
        HttpClient = httpClient;
    }

    /// <summary>Notion tokens don't support refresh</summary>
    public Task<AuthToken?> RefreshAccessTokenAsync(IAuthTokenHandlerSession authSession, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<AuthToken?>(null);
    }

    /// <summary>Validate Notion OAuth access token</summary>
    public Task<bool> ValidateAccessTokenAsync(IAuthTokenHandlerSession authSession, CancellationToken cancellationToken = default)
    {
        var token = authSession.AuthToken?.AccessToken;
        return Task.FromResult(!string.IsNullOrWhiteSpace(token));
    }

    /// <summary>Notion doesn't provide user info endpoint</summary>
    public Task<UserInfo?> GetUserInfoAsync(IAuthTokenHandlerSession authSession, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<UserInfo?>(null);
    }

    /// <summary>Notion tokens don't expire</summary>
    public Task<TokenLifetime?> GetAccessTokenLifetimeAsync(IAuthTokenHandlerSession authSession, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<TokenLifetime?>(null);
    }
}

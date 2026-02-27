using Ivy.Core.Helpers;
using Ivy.Core.Server;
using Microsoft.Extensions.Logging;

namespace Ivy.Core.Auth;

public class OAuthTokenService : IOAuthTokenService
{
    private readonly OAuthProvider _provider;
    private readonly IAuthTokenHandler _handler;
    private readonly IAuthProviderSession _authSession;
    private readonly IClientProvider _client;
    private readonly AppSessionStore _sessionStore;
    private readonly ILogger<OAuthTokenService> _logger;

    public OAuthProvider Provider => _provider;

    public OAuthTokenService(
        OAuthProvider provider,
        IAuthTokenHandler handler,
        IAuthProviderSession authSession,
        IClientProvider client,
        AppSessionStore sessionStore,
        ILogger<OAuthTokenService> logger)
    {
        _provider = provider;
        _handler = handler;
        _authSession = authSession;
        _client = client;
        _sessionStore = sessionStore;
        _logger = logger;
    }

    public bool HasToken()
    {
        return _authSession.OAuthProviderTokens.ContainsKey(_provider);
    }

    public async Task<bool> ValidateAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        if (!_authSession.OAuthProviderTokens.TryGetValue(_provider, out var currentToken))
        {
            return false;
        }

        // Create a temporary auth token handler session for this OAuth token
        var tempAuthSession = new AuthTokenHandlerSession(
            currentToken.AuthToken,
            _authSession.AuthSessionData);

        try
        {
            return await TimeoutHelper.WithTimeoutAsync(
                ct => _handler.ValidateAccessTokenAsync(tempAuthSession, ct),
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating OAuth token for {Provider}", _provider);
            return false;
        }
    }

    public async Task<TokenLifetime?> GetAccessTokenLifetimeAsync(CancellationToken cancellationToken = default)
    {
        if (!_authSession.OAuthProviderTokens.TryGetValue(_provider, out var currentToken))
        {
            return null;
        }

        // Create a temporary auth token handler session for this OAuth token
        var tempAuthSession = new AuthTokenHandlerSession(
            currentToken.AuthToken,
            _authSession.AuthSessionData);

        try
        {
            return await TimeoutHelper.WithTimeoutAsync(
                ct => _handler.GetAccessTokenLifetimeAsync(tempAuthSession, ct),
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting OAuth token lifetime for {Provider}", _provider);
            return null;
        }
    }

    public async Task<AuthToken?> RefreshAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        if (!_authSession.OAuthProviderTokens.TryGetValue(_provider, out var currentToken))
        {
            _logger.LogDebug("No token found for OAuth provider {Provider}", _provider);
            return null;
        }

        // Create a temporary auth token handler session for this OAuth token
        var tempAuthSession = new AuthTokenHandlerSession(
            currentToken.AuthToken,
            _authSession.AuthSessionData);

        try
        {
            _logger.LogInformation("Attempting to refresh OAuth token for {Provider}", _provider);

            var newToken = await TimeoutHelper.WithTimeoutAsync(
                ct => _handler.RefreshAccessTokenAsync(tempAuthSession, ct),
                cancellationToken);

            if (newToken != null)
            {
                _logger.LogInformation("Successfully refreshed OAuth token for {Provider}", _provider);

                // Update the provider token with the new auth token
                var updatedProviderToken = currentToken with { AuthToken = newToken };

                // Update the session
                _authSession.AddOAuthProviderToken(updatedProviderToken);

                // Update cookies (don't reload page for OAuth token refreshes)
                var cookieJarId = _sessionStore.RegisterAuthSessionCookies(_authSession);
                _client.SetAuthCookies(cookieJarId, reloadPage: false, triggerMachineReload: null);

                return newToken;
            }
            else
            {
                _logger.LogWarning("Failed to refresh OAuth token for {Provider}", _provider);
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing OAuth token for {Provider}", _provider);
            return null;
        }
    }

    public async Task<UserInfo?> GetUserInfoAsync(CancellationToken cancellationToken = default)
    {
        if (!_authSession.OAuthProviderTokens.TryGetValue(_provider, out var currentToken))
        {
            return null;
        }

        // Create a temporary auth token handler session for this OAuth token
        var tempAuthSession = new AuthTokenHandlerSession(
            currentToken.AuthToken,
            _authSession.AuthSessionData);

        try
        {
            return await TimeoutHelper.WithTimeoutAsync(
                ct => _handler.GetUserInfoAsync(tempAuthSession, ct),
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user info for OAuth provider {Provider}", _provider);
            return null;
        }
    }

    public AuthToken? GetCurrentToken()
    {
        if (_authSession.OAuthProviderTokens.TryGetValue(_provider, out var currentToken))
        {
            return currentToken.AuthToken;
        }
        return null;
    }

    public string? GetCurrentSessionData() => _authSession.AuthSessionData;

    public IAuthTokenHandlerSession GetAuthTokenHandlerSession()
    {
        if (_authSession.OAuthProviderTokens.TryGetValue(_provider, out var currentToken))
        {
            return new AuthTokenHandlerSession(currentToken.AuthToken, _authSession.AuthSessionData);
        }
        return new AuthTokenHandlerSession(null, _authSession.AuthSessionData);
    }

    public void SetAuthTokenCookies(bool reloadPage = true, bool? triggerMachineReload = null)
    {
        if (_authSession.OAuthProviderTokens.TryGetValue(_provider, out var currentToken))
        {
            var cookieJarId = _sessionStore.RegisterAuthTokenCookies(currentToken.AuthToken);
            _client.SetAuthCookies(cookieJarId, reloadPage, triggerMachineReload);
        }
    }

    public void SetAuthSessionDataCookies(bool reloadPage = false, bool? triggerMachineReload = null)
    {
        var cookieJarId = _sessionStore.RegisterAuthSessionDataCookies(_authSession.AuthSessionData);
        _client.SetAuthCookies(cookieJarId, reloadPage, triggerMachineReload);
    }

    public void RemoveToken()
    {
        _logger.LogInformation("Removing OAuth token for {Provider}", _provider);
        _authSession.RemoveOAuthProviderToken(_provider);

        // Update cookies to reflect removal (don't reload page)
        var cookieJarId = _sessionStore.RegisterAuthSessionCookies(_authSession);
        _client.SetAuthCookies(cookieJarId, reloadPage: false, triggerMachineReload: null);
    }
}

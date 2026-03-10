using Ivy.Core.Helpers;
using Ivy.Core.Server;
using Microsoft.Extensions.Logging;

namespace Ivy.Core.Auth;

public class OAuthTokenService : IOAuthTokenService
{
    private readonly string _provider;
    private readonly IAuthTokenHandler _handler;
    private readonly IAuthTokenHandlerSession _session;
    private readonly IAuthSession _parentSession;
    private readonly IClientProvider _client;
    private readonly AppSessionStore _sessionStore;
    private readonly string _machineId;
    private readonly ILogger<OAuthTokenService> _logger;

    public string Provider => _provider;

    public OAuthTokenService(
        string provider,
        IAuthTokenHandler handler,
        IAuthTokenHandlerSession session,
        IAuthSession parentSession,
        IClientProvider client,
        AppSessionStore sessionStore,
        string machineId,
        ILogger<OAuthTokenService> logger)
    {
        _provider = provider;
        _handler = handler;
        _session = session;
        _parentSession = parentSession;
        _client = client;
        _sessionStore = sessionStore;
        _machineId = machineId;
        _logger = logger;
    }

    public bool HasToken()
    {
        return _session.AuthToken != null;
    }

    public async Task<bool> ValidateAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await TimeoutHelper.WithTimeoutAsync(
                ct => _handler.ValidateAccessTokenAsync(_session, ct),
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
        try
        {
            return await TimeoutHelper.WithTimeoutAsync(
                ct => _handler.GetAccessTokenLifetimeAsync(_session, ct),
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
        try
        {
            _logger.LogInformation("Attempting to refresh OAuth token for {Provider}", _provider);

            var newToken = await TimeoutHelper.WithTimeoutAsync(
                ct => _handler.RefreshAccessTokenAsync(_session, ct),
                cancellationToken);

            if (newToken != null)
            {
                _logger.LogInformation("Successfully refreshed OAuth token for {Provider}", _provider);

                // Update the session's auth token directly
                _session.AuthToken = newToken;

                // Update cookies (don't reload page for OAuth token refreshes)
                var cookieJarId = _sessionStore.RegisterAuthSessionCookies(_parentSession, _machineId);
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
        try
        {
            return await TimeoutHelper.WithTimeoutAsync(
                ct => _handler.GetUserInfoAsync(_session, ct),
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
        return _session.AuthToken;
    }

    public string? GetCurrentSessionData()
    {
        return _session.AuthSessionData;
    }

    public IAuthTokenHandlerSession GetAuthTokenHandlerSession()
    {
        return _session;
    }

    public void SetAuthTokenCookies(bool reloadPage = true, bool? triggerMachineReload = null)
    {
        var cookieJarId = _sessionStore.RegisterAuthTokenCookies(_session.AuthToken);
        _client.SetAuthCookies(cookieJarId, reloadPage, triggerMachineReload);
    }

    public void SetAuthSessionDataCookies(bool reloadPage = false, bool? triggerMachineReload = null)
    {
        var cookieJarId = _sessionStore.RegisterAuthSessionDataCookies(_session.AuthSessionData);
        _client.SetAuthCookies(cookieJarId, reloadPage, triggerMachineReload);
    }

    public void RemoveToken()
    {
        _logger.LogInformation("Removing OAuth token for {Provider}", _provider);
        _parentSession.RemoveOAuthSession(_provider);

        // Update cookies to reflect removal (don't reload page)
        var cookieJarId = _sessionStore.RegisterAuthSessionCookies(_parentSession, _machineId);
        _client.SetAuthCookies(cookieJarId, reloadPage: false, triggerMachineReload: null);
    }
}

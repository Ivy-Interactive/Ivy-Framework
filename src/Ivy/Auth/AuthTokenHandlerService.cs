using Ivy.Core.Auth;
using Ivy.Core.Helpers;
using Ivy.Core.Server;
using Microsoft.Extensions.Logging;

// ReSharper disable once CheckNamespace
namespace Ivy;

public class AuthTokenHandlerService : IAuthTokenHandlerService
{
    protected readonly IAuthTokenHandler _handler;
    protected readonly IAuthTokenHandlerSession _session;
    protected readonly IClientProvider _client;
    protected readonly AppSessionStore _sessionStore;
    protected readonly string? _machineId;
    protected readonly ILogger _logger;

    public AuthTokenHandlerService(
        IAuthTokenHandler handler,
        IAuthTokenHandlerSession session,
        IClientProvider client,
        AppSessionStore sessionStore,
        string? machineId,
        ILogger logger)
    {
        _handler = handler;
        _session = session;
        _client = client;
        _sessionStore = sessionStore;
        _machineId = machineId;
        _logger = logger;
    }

    public virtual async Task<AuthToken?> RefreshAccessTokenAsync(CancellationToken cancellationToken)
    {
        if (_session.AuthToken is null)
        {
            return null;
        }

        var refreshedToken = await TimeoutHelper.WithTimeoutAsync(ct =>
            _handler.RefreshAccessTokenAsync(_session, ct), cancellationToken);
        _session.AuthToken = refreshedToken;

        return refreshedToken;
    }

    public async Task<bool> ValidateAccessTokenAsync(CancellationToken cancellationToken)
    {
        if (_session.AuthToken is null)
        {
            return false;
        }

        return await TimeoutHelper.WithTimeoutAsync(ct =>
            _handler.ValidateAccessTokenAsync(_session, ct), cancellationToken);
    }

    public async Task<UserInfo?> GetUserInfoAsync(CancellationToken cancellationToken)
    {
        var token = _session.AuthToken;

        if (string.IsNullOrWhiteSpace(token?.AccessToken))
        {
            return null;
        }

        return await TimeoutHelper.WithTimeoutAsync(ct =>
            _handler.GetUserInfoAsync(_session, ct), cancellationToken);
    }

    public async Task<TokenLifetime?> GetAccessTokenLifetimeAsync(CancellationToken cancellationToken)
    {
        if (_session.AuthToken is null)
        {
            return null;
        }

        return await TimeoutHelper.WithTimeoutAsync(ct =>
            _handler.GetAccessTokenLifetimeAsync(_session, ct), cancellationToken);
    }

    public AuthToken? GetCurrentToken() => _session.AuthToken;

    public string? GetCurrentSessionData() => _session.AuthSessionData;

    public IAuthTokenHandlerSession GetAuthTokenHandlerSession() => _session;

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
}

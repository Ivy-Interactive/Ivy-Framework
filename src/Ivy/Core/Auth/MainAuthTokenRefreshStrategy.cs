using Ivy.Core.Helpers;
using Ivy.Core.Server;
using Microsoft.Extensions.Logging;

namespace Ivy.Core.Auth;

public class MainAuthTokenRefreshStrategy : ITokenRefreshStrategy
{
    private readonly string _connectionId;
    private readonly IAuthProvider _authProvider;
    private readonly IAuthTokenHandlerService _authService;
    private readonly IAuthSession _authSession;
    private readonly AppSessionStore _sessionStore;
    private readonly IContentBuilder _contentBuilder;
    private readonly ILogger _logger;

    public string LoggingName => "MainAuth";

    public MainAuthTokenRefreshStrategy(
        string connectionId,
        IAuthProvider authProvider,
        IAuthTokenHandlerService authService,
        IAuthSession authSession,
        AppSessionStore sessionStore,
        IContentBuilder contentBuilder,
        ILogger logger)
    {
        _connectionId = connectionId;
        _authProvider = authProvider;
        _authService = authService;
        _authSession = authSession;
        _sessionStore = sessionStore;
        _contentBuilder = contentBuilder;
        _logger = logger;
    }

    public bool HasToken()
    {
        return _authSession.AuthToken != null;
    }

    public async Task<bool> ValidateTokenAsync(CancellationToken cancellationToken = default)
    {
        return await TimeoutHelper.WithTimeoutAsync(
            ct => _authProvider.ValidateAccessTokenAsync(_authSession, ct),
            cancellationToken);
    }

    public async Task<TokenLifetime?> GetTokenLifetimeAsync(CancellationToken cancellationToken = default)
    {
        return await TimeoutHelper.WithTimeoutAsync(
            ct => _authProvider.GetAccessTokenLifetimeAsync(_authSession, ct),
            cancellationToken);
    }

    public async Task<bool> RefreshTokenAsync(CancellationToken cancellationToken = default)
    {
        var oldSession = _authSession.TakeSnapshot();
        await _authService.RefreshAccessTokenAsync(cancellationToken);

        // Check if refresh actually changed the token
        if (_authSession.AuthToken == oldSession.AuthToken)
        {
            // This should only happen if the auth provider implementation is bad
            _logger.LogWarning("AuthRefreshLoop: Token refresh did not change the token for {ConnectionId}", _connectionId);
            return false;
        }

        return _authSession.AuthToken != null;
    }

    public async Task<bool> OnRefreshFailedAsync()
    {
        _logger.LogError("AuthRefreshLoop: Failed to refresh token for {ConnectionId}, abandoning connection", _connectionId);
        await AbandonConnection(resetTokenAndReload: true);
        return false; // Exit the loop
    }

    public async Task<bool> OnTokenLostAsync()
    {
        _logger.LogError("AuthRefreshLoop: Token lost for {ConnectionId}, abandoning connection", _connectionId);
        await AbandonConnection(resetTokenAndReload: true);
        return false; // Exit the loop
    }

    private async Task AbandonConnection(bool resetTokenAndReload)
    {
        var session = _sessionStore.Sessions[_connectionId];
        await SessionHelpers.AbandonSessionAsync(_sessionStore, session, _contentBuilder, resetTokenAndReload, triggerMachineReload: true, _logger, "AuthRefreshLoop");
    }
}

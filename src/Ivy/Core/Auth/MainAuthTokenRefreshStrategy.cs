using Ivy.Core.Apps;
using Ivy.Core.Helpers;
using Microsoft.Extensions.Logging;

namespace Ivy.Core.Auth;

public class MainAuthTokenRefreshStrategy : RefreshStrategy
{
    private readonly IAuthSession _authSession;
    private readonly AppSession _appSession;
    private readonly IContentBuilder _contentBuilder;
    private readonly ILogger _logger;

    public override string LoggingName => "MainAuth";

    public MainAuthTokenRefreshStrategy(
        IAuthTokenHandlerService authService,
        IAuthSession authSession,
        AppSession appSession,
        IContentBuilder contentBuilder,
        ILogger logger) : base(authService)
    {
        _authSession = authSession;
        _appSession = appSession;
        _contentBuilder = contentBuilder;
        _logger = logger;
    }

    public override async Task<bool> RefreshTokenAsync(CancellationToken cancellationToken = default)
    {
        var oldSession = _authSession.TakeSnapshot();
        await _authService.RefreshAccessTokenAsync(cancellationToken);

        // Check if refresh actually changed the token
        if (_authSession.AuthToken == oldSession.AuthToken)
        {
            // This should only happen if the auth provider implementation is bad
            _logger.LogWarning("AuthRefreshLoop: Token refresh did not change the token for {ConnectionId}", _appSession.ConnectionId);
            return false;
        }

        return _authSession.AuthToken != null;
    }

    public override async Task<bool> OnRefreshFailedAsync()
    {
        _logger.LogError("AuthRefreshLoop: Failed to refresh token for {ConnectionId}, abandoning connection", _appSession.ConnectionId);
        await AbandonConnection(resetTokenAndReload: true);
        return false; // Exit the loop
    }

    public override async Task<bool> OnTokenLostAsync()
    {
        _logger.LogError("AuthRefreshLoop: Token lost for {ConnectionId}, abandoning connection", _appSession.ConnectionId);
        await AbandonConnection(resetTokenAndReload: true);
        return false; // Exit the loop
    }

    private async Task AbandonConnection(bool resetTokenAndReload)
    {
        await SessionHelpers.AbandonSessionAsync(_appSession, _contentBuilder, resetTokenAndReload, triggerMachineReload: true, _logger, "AuthRefreshLoop");
    }
}

using Ivy.Core.Helpers;
using Ivy.Core.Server;
using Microsoft.Extensions.Logging;

namespace Ivy.Core.Auth;

public class BrokeredTokenRefreshStrategy : RefreshStrategy
{
    private readonly string _connectionId;
    private readonly string _provider;
    private readonly IAuthSession _parentSession;
    private readonly IAuthService _parentAuthService;
    private readonly AppSessionStore _sessionStore;
    private readonly IContentBuilder _contentBuilder;
    private readonly IClientProvider _client;
    private readonly ILogger _logger;

    // Track consecutive refresh failures across calls to OnRefreshFailedAsync
    private int _consecutiveRefreshFailures = 0;
    private const int MaxConsecutiveRefreshFailures = 3;

    public override string LoggingName { get; }

    public BrokeredTokenRefreshStrategy(
        string connectionId,
        string provider,
        IAuthTokenHandlerService authService,
        IAuthSession parentSession,
        IClientProvider client,
        IAuthService parentAuthService,
        AppSessionStore sessionStore,
        IContentBuilder contentBuilder,
        ILogger logger) : base(authService)
    {
        _connectionId = connectionId;
        _provider = provider;
        _parentSession = parentSession;
        _client = client;
        _parentAuthService = parentAuthService;
        _sessionStore = sessionStore;
        _contentBuilder = contentBuilder;
        _logger = logger;
        LoggingName = $"OAuth[{provider}]";
    }

    public override async Task<bool> ValidateTokenAsync(CancellationToken cancellationToken = default)
    {
        var isValid = await base.ValidateTokenAsync(cancellationToken);
        if (isValid)
        {
            // Reset failure counter on successful validation
            _consecutiveRefreshFailures = 0;
        }
        return isValid;
    }

    public override async Task<bool> RefreshTokenAsync(CancellationToken cancellationToken = default)
    {
        // First, try the token handler's native refresh (uses refresh_token if available)
        _logger.LogInformation("Attempting to refresh OAuth token for {Provider}", _provider);
        var result = await _authService.RefreshAccessTokenAsync(cancellationToken);

        if (result != null)
        {
            _logger.LogInformation("Successfully refreshed OAuth token for {Provider} via token handler", _provider);
            return await OnRefreshSuccessAsync();
        }

        // Fallback: Try to get a fresh token from the auth provider (e.g., Clerk auto-refreshes tokens)
        _logger.LogInformation("Token handler refresh failed for {Provider}, trying GetBrokeredSessionsAsync fallback", _provider);

        try
        {
            var brokeredResult = await _parentAuthService.GetBrokeredSessionsAsync(skipCache: true, cancellationToken);

            // Check if provider is still available
            if (brokeredResult.Sessions == null || !brokeredResult.Sessions.ContainsKey(_provider))
            {
                _logger.LogWarning("GetBrokeredSessionsAsync fallback: {Provider} not in sessions", _provider);
                return false;
            }

            // Validate the token we got back
            var isValid = await _authService.ValidateAccessTokenAsync(cancellationToken);
            if (isValid)
            {
                _logger.LogInformation("Successfully refreshed OAuth token for {Provider} via GetBrokeredSessionsAsync", _provider);
                return await OnRefreshSuccessAsync();
            }

            _logger.LogWarning("GetBrokeredSessionsAsync returned {Provider} but token is invalid", _provider);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "GetBrokeredSessionsAsync fallback failed for {Provider}", _provider);
            return false;
        }
    }

    private async Task<bool> OnRefreshSuccessAsync()
    {
        // Reset consecutive failure counter on success
        _consecutiveRefreshFailures = 0;

        // Update parent session cookies to include the refreshed OAuth token
        var cookieJarId = _sessionStore.RegisterAuthSessionCookies(_parentSession);
        _client.SetAuthCookies(cookieJarId, reloadPage: false, triggerMachineReload: null);

        return true;
    }

    public override async Task<bool> OnRefreshFailedAsync()
    {
        _consecutiveRefreshFailures++;
        _logger.LogWarning("BrokeredTokenRefreshLoop[{Provider}]: Refresh failed for {ConnectionId}, attempt {Attempt}/{MaxAttempts}",
            _provider, _connectionId, _consecutiveRefreshFailures, MaxConsecutiveRefreshFailures);

        // Check if we've exceeded max consecutive failures
        if (_consecutiveRefreshFailures >= MaxConsecutiveRefreshFailures)
        {
            _logger.LogError("BrokeredTokenRefreshLoop[{Provider}]: {MaxAttempts} consecutive refresh failures for {ConnectionId}, abandoning session",
                _provider, MaxConsecutiveRefreshFailures, _connectionId);
            await LogoutAsync();
            return false; // Exit the loop
        }

        // Add a delay before retrying to avoid tight loop
        await Task.Delay(TimeSpan.FromSeconds(5));
        return true; // Continue the loop - will retry RefreshTokenAsync
    }

    public override Task<bool> OnTokenLostAsync()
    {
        _logger.LogInformation("BrokeredTokenRefreshLoop[{Provider}]: Token lost for {ConnectionId}, exiting loop", _provider, _connectionId);
        return Task.FromResult(false); // Exit the loop
    }

    private async Task LogoutAsync()
    {
        try
        {
            // First, properly log out from the auth provider
            await _parentAuthService.LogoutAsync(CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "BrokeredTokenRefreshLoop[{Provider}]: Error during logout for {ConnectionId}",
                _provider, _connectionId);
        }

        // Then abandon the session (show error view)
        if (!_sessionStore.Sessions.TryGetValue(_connectionId, out var session))
        {
            _logger.LogWarning("BrokeredTokenRefreshLoop[{Provider}]: Session already removed for {ConnectionId}, skipping abandon",
                _provider, _connectionId);
            return;
        }

        await SessionHelpers.AbandonSessionAsync(
            session,
            _contentBuilder,
            resetTokenAndReload: true,
            triggerMachineReload: true,
            _logger,
            $"BrokeredTokenRefreshLoop[{_provider}]");
    }
}

using Ivy.Core.Helpers;
using Ivy.Core.Server;
using Microsoft.Extensions.Logging;

namespace Ivy.Core.Auth;

public class OAuthTokenRefreshStrategy : ITokenRefreshStrategy
{
    private readonly string _connectionId;
    private readonly string _provider;
    private readonly IAuthTokenHandlerService _tokenService;
    private readonly IAuthSession _parentSession;
    private readonly string _machineId;
    private readonly IAuthService _authService;
    private readonly AppSessionStore _sessionStore;
    private readonly IContentBuilder _contentBuilder;
    private readonly IClientProvider _client;
    private readonly ILogger _logger;

    public string LoggingName { get; }

    public OAuthTokenRefreshStrategy(
        string connectionId,
        string provider,
        IAuthTokenHandlerService tokenService,
        IAuthSession parentSession,
        string machineId,
        IClientProvider client,
        IAuthService authService,
        AppSessionStore sessionStore,
        IContentBuilder contentBuilder,
        ILogger logger)
    {
        _connectionId = connectionId;
        _provider = provider;
        _tokenService = tokenService;
        _parentSession = parentSession;
        _machineId = machineId;
        _client = client;
        _authService = authService;
        _sessionStore = sessionStore;
        _contentBuilder = contentBuilder;
        _logger = logger;
        LoggingName = $"OAuth[{provider}]";
    }

    public bool HasToken()
    {
        return _tokenService.GetCurrentToken() != null;
    }

    public async Task<bool> ValidateTokenAsync(CancellationToken cancellationToken = default)
    {
        return await _tokenService.ValidateAccessTokenAsync(cancellationToken);
    }

    public async Task<TokenLifetime?> GetTokenLifetimeAsync(CancellationToken cancellationToken = default)
    {
        return await _tokenService.GetAccessTokenLifetimeAsync(cancellationToken);
    }

    public async Task<bool> RefreshTokenAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting to refresh OAuth token for {Provider}", _provider);

        var result = await _tokenService.RefreshAccessTokenAsync(cancellationToken);

        if (result != null)
        {
            _logger.LogInformation("Successfully refreshed OAuth token for {Provider}", _provider);

            // Update parent session cookies to include the refreshed OAuth token
            var cookieJarId = _sessionStore.RegisterAuthSessionCookies(_parentSession, _machineId);
            _client.SetAuthCookies(cookieJarId, reloadPage: false, triggerMachineReload: null);

            return true;
        }
        else
        {
            _logger.LogWarning("Failed to refresh OAuth token for {Provider}", _provider);
            return false;
        }
    }

    public async Task<bool> OnRefreshFailedAsync()
    {
        _logger.LogWarning("OAuthTokenRefreshLoop[{Provider}]: Failed to refresh token for {ConnectionId}, attempting recovery", _provider, _connectionId);

        const int maxRetries = 3;
        const int retryDelaySeconds = 5;

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                _logger.LogInformation("OAuthTokenRefreshLoop[{Provider}]: Recovery attempt {Attempt}/{MaxRetries} for {ConnectionId}",
                    _provider, attempt, maxRetries, _connectionId);

                // Try to re-fetch OAuth provider sessions from the main auth provider (skip cache to force fresh fetch)
                var result = await _authService.GetOAuthSessionsAsync(skipCache: true, CancellationToken.None);

                if (result.Sessions != null && result.Sessions.ContainsKey(_provider))
                {
                    _logger.LogInformation("OAuthTokenRefreshLoop[{Provider}]: Successfully recovered token for {ConnectionId}, continuing loop",
                        _provider, _connectionId);
                    return true; // Continue the loop with the recovered token
                }

                // If the provider signals that retrying won't help, exit immediately
                if (!result.CanRetry)
                {
                    _logger.LogError("OAuthTokenRefreshLoop[{Provider}]: Provider indicates retry will not succeed for {ConnectionId}, abandoning session",
                        _provider, _connectionId);
                    await LogoutAsync();
                    return false; // Exit the loop
                }

                _logger.LogWarning("OAuthTokenRefreshLoop[{Provider}]: Recovery attempt {Attempt} failed - provider not in returned sessions for {ConnectionId}",
                    _provider, attempt, _connectionId);

                if (attempt < maxRetries)
                {
                    await Task.Delay(TimeSpan.FromSeconds(retryDelaySeconds));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OAuthTokenRefreshLoop[{Provider}]: Recovery attempt {Attempt} threw exception for {ConnectionId}",
                    _provider, attempt, _connectionId);

                // If we can't even contact the auth provider, the main session is likely invalid
                _logger.LogError("OAuthTokenRefreshLoop[{Provider}]: Cannot communicate with auth provider for {ConnectionId}, abandoning session",
                    _provider, _connectionId);
                await LogoutAsync();
                return false; // Exit the loop
            }
        }

        // All recovery attempts failed - log out the main auth provider
        _logger.LogError("OAuthTokenRefreshLoop[{Provider}]: All recovery attempts failed for {ConnectionId}, abandoning session",
            _provider, _connectionId);
        await LogoutAsync();
        return false; // Exit the loop
    }

    private async Task LogoutAsync()
    {
        try
        {
            // First, properly log out from the auth provider
            await _authService.LogoutAsync(CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OAuthTokenRefreshLoop[{Provider}]: Error during logout for {ConnectionId}",
                _provider, _connectionId);
        }

        // Then abandon the session (show error view)
        var session = _sessionStore.Sessions[_connectionId];
        await SessionHelpers.AbandonSessionAsync(
            session,
            _contentBuilder,
            resetTokenAndReload: true,
            triggerMachineReload: true,
            _logger,
            $"OAuthTokenRefreshLoop[{_provider}]");
    }

    public Task<bool> OnTokenLostAsync()
    {
        _logger.LogInformation("OAuthTokenRefreshLoop[{Provider}]: Token lost for {ConnectionId}, exiting loop", _provider, _connectionId);
        return Task.FromResult(false); // Exit the loop
    }
}

using Ivy.Core.Helpers;
using Ivy.Core.Server;
using Microsoft.Extensions.Logging;

namespace Ivy.Core.Auth;

public class OAuthTokenRefreshStrategy : ITokenRefreshStrategy
{
    private readonly string _connectionId;
    private readonly IOAuthTokenService _tokenService;
    private readonly IAuthProviderService _authService;
    private readonly AppSessionStore _sessionStore;
    private readonly IContentBuilder _contentBuilder;
    private readonly ILogger _logger;

    public string LoggingName { get; }

    public OAuthTokenRefreshStrategy(
        string connectionId,
        IOAuthTokenService tokenService,
        IAuthProviderService authService,
        AppSessionStore sessionStore,
        IContentBuilder contentBuilder,
        ILogger logger)
    {
        _connectionId = connectionId;
        _tokenService = tokenService;
        _authService = authService;
        _sessionStore = sessionStore;
        _contentBuilder = contentBuilder;
        _logger = logger;
        LoggingName = $"OAuth[{tokenService.Provider}]";
    }

    public bool HasToken()
    {
        return _tokenService.HasToken();
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
        var result = await _tokenService.RefreshAccessTokenAsync(cancellationToken);
        return result != null;
    }

    public async Task<bool> OnRefreshFailedAsync()
    {
        _logger.LogWarning("OAuthTokenRefreshLoop[{Provider}]: Failed to refresh token for {ConnectionId}, attempting recovery", _tokenService.Provider, _connectionId);

        const int maxRetries = 3;
        const int retryDelaySeconds = 5;

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                _logger.LogInformation("OAuthTokenRefreshLoop[{Provider}]: Recovery attempt {Attempt}/{MaxRetries} for {ConnectionId}",
                    _tokenService.Provider, attempt, maxRetries, _connectionId);

                // Try to re-fetch OAuth provider sessions from the main auth provider (skip cache to force fresh fetch)
                var result = await _authService.GetOAuthSessionsAsync(skipCache: true, CancellationToken.None);

                if (result.Sessions != null && result.Sessions.ContainsKey(_tokenService.Provider))
                {
                    _logger.LogInformation("OAuthTokenRefreshLoop[{Provider}]: Successfully recovered token for {ConnectionId}, continuing loop",
                        _tokenService.Provider, _connectionId);
                    return true; // Continue the loop with the recovered token
                }

                // If the provider signals that retrying won't help, exit immediately
                if (!result.CanRetry)
                {
                    _logger.LogError("OAuthTokenRefreshLoop[{Provider}]: Provider indicates retry will not succeed for {ConnectionId}, abandoning session",
                        _tokenService.Provider, _connectionId);
                    await LogoutAsync();
                    return false; // Exit the loop
                }

                _logger.LogWarning("OAuthTokenRefreshLoop[{Provider}]: Recovery attempt {Attempt} failed - provider not in returned sessions for {ConnectionId}",
                    _tokenService.Provider, attempt, _connectionId);

                if (attempt < maxRetries)
                {
                    await Task.Delay(TimeSpan.FromSeconds(retryDelaySeconds));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OAuthTokenRefreshLoop[{Provider}]: Recovery attempt {Attempt} threw exception for {ConnectionId}",
                    _tokenService.Provider, attempt, _connectionId);

                // If we can't even contact the auth provider, the main session is likely invalid
                _logger.LogError("OAuthTokenRefreshLoop[{Provider}]: Cannot communicate with auth provider for {ConnectionId}, abandoning session",
                    _tokenService.Provider, _connectionId);
                await LogoutAsync();
                return false; // Exit the loop
            }
        }

        // All recovery attempts failed - log out the main auth provider
        _logger.LogError("OAuthTokenRefreshLoop[{Provider}]: All recovery attempts failed for {ConnectionId}, abandoning session",
            _tokenService.Provider, _connectionId);
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
                _tokenService.Provider, _connectionId);
        }

        // Then abandon the session (show error view)
        var session = _sessionStore.Sessions[_connectionId];
        await SessionHelpers.AbandonSessionAsync(
            _sessionStore,
            session,
            _contentBuilder,
            resetTokenAndReload: true,
            triggerMachineReload: true,
            _logger,
            $"OAuthTokenRefreshLoop[{_tokenService.Provider}]");
    }

    public Task<bool> OnTokenLostAsync()
    {
        _logger.LogInformation("OAuthTokenRefreshLoop[{Provider}]: Token lost for {ConnectionId}, exiting loop", _tokenService.Provider, _connectionId);
        return Task.FromResult(false); // Exit the loop
    }
}

using Microsoft.Extensions.Logging;

namespace Ivy.Core.Auth;

public class OAuthTokenRefreshStrategy : ITokenRefreshStrategy
{
    private readonly string _connectionId;
    private readonly IOAuthTokenService _tokenService;
    private readonly IAuthTokenHandler _tokenHandler;
    private readonly ILogger _logger;

    public string LoggingName { get; }

    public OAuthTokenRefreshStrategy(
        string connectionId,
        IOAuthTokenService tokenService,
        IAuthTokenHandler tokenHandler,
        ILogger logger)
    {
        _connectionId = connectionId;
        _tokenService = tokenService;
        _tokenHandler = tokenHandler;
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

    public Task<bool> OnRefreshFailedAsync()
    {
        _logger.LogWarning("OAuthTokenRefreshLoop[{Provider}]: Failed to refresh token for {ConnectionId}, removing token and exiting loop", _tokenService.Provider, _connectionId);
        _tokenService.RemoveToken();
        return Task.FromResult(false); // Exit the loop
    }

    public Task<bool> OnTokenLostAsync()
    {
        _logger.LogInformation("OAuthTokenRefreshLoop[{Provider}]: Token lost for {ConnectionId}, exiting loop", _tokenService.Provider, _connectionId);
        return Task.FromResult(false); // Exit the loop
    }
}

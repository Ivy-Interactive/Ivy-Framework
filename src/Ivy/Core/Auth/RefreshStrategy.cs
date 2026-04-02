namespace Ivy.Core.Auth;

public abstract class RefreshStrategy : ITokenRefreshStrategy
{
    protected readonly IAuthTokenHandlerService _authService;

    protected RefreshStrategy(IAuthTokenHandlerService authService)
    {
        _authService = authService;
    }

    public abstract string LoggingName { get; }

    public bool HasToken() => _authService.GetCurrentToken() != null;

    public virtual Task<bool> ValidateTokenAsync(CancellationToken cancellationToken = default)
        => _authService.ValidateAccessTokenAsync(cancellationToken);

    public Task<TokenLifetime?> GetTokenLifetimeAsync(CancellationToken cancellationToken = default)
        => _authService.GetAccessTokenLifetimeAsync(cancellationToken);

    public abstract Task<bool> RefreshTokenAsync(CancellationToken cancellationToken = default);

    public abstract Task<bool> OnRefreshFailedAsync();

    public abstract Task<bool> OnTokenLostAsync();
}
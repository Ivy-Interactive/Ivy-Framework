namespace Ivy.Core.Auth;

public interface ITokenRefreshStrategy
{
    string LoggingName { get; }

    bool HasToken();

    Task<bool> ValidateTokenAsync(CancellationToken cancellationToken = default);

    Task<TokenLifetime?> GetTokenLifetimeAsync(CancellationToken cancellationToken = default);

    Task<bool> RefreshTokenAsync(CancellationToken cancellationToken = default);

    Task<bool> OnRefreshFailedAsync();

    Task<bool> OnTokenLostAsync();
}

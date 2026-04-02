// ReSharper disable once CheckNamespace
namespace Ivy;

public interface IAuthTokenHandler
{
    Task InitializeAsync(IAuthTokenHandlerSession authSession, string requestScheme, string requestHost, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    Task<AuthToken?> RefreshAccessTokenAsync(IAuthTokenHandlerSession authSession, CancellationToken cancellationToken = default);

    Task<bool> ValidateAccessTokenAsync(IAuthTokenHandlerSession authSession, CancellationToken cancellationToken = default);

    Task<UserInfo?> GetUserInfoAsync(IAuthTokenHandlerSession authSession, CancellationToken cancellationToken = default);

    Task<TokenLifetime?> GetAccessTokenLifetimeAsync(IAuthTokenHandlerSession authSession, CancellationToken cancellationToken = default);
}

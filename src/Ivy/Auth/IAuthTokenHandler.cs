// ReSharper disable once CheckNamespace
namespace Ivy;

public interface IAuthTokenHandler
{
    Task<AuthToken?> RefreshAccessTokenAsync(IAuthSession authSession, CancellationToken cancellationToken = default);

    Task<bool> ValidateAccessTokenAsync(IAuthSession authSession, CancellationToken cancellationToken = default);

    Task<UserInfo?> GetUserInfoAsync(IAuthSession authSession, CancellationToken cancellationToken = default);

    Task<TokenLifetime?> GetAccessTokenLifetimeAsync(IAuthSession authSession, CancellationToken cancellationToken = default);
}

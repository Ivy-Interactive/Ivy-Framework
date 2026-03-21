// ReSharper disable once CheckNamespace
namespace Ivy;

public interface IAuthTokenHandlerService
{
    Task<AuthToken?> RefreshAccessTokenAsync(CancellationToken cancellationToken = default);

    Task<bool> ValidateAccessTokenAsync(CancellationToken cancellationToken = default);

    Task<UserInfo?> GetUserInfoAsync(CancellationToken cancellationToken = default);

    Task<TokenLifetime?> GetAccessTokenLifetimeAsync(CancellationToken cancellationToken = default);

    AuthToken? GetCurrentToken();

    string? GetCurrentSessionData();

    IAuthTokenHandlerSession GetAuthTokenHandlerSession();

    internal void SetAuthTokenCookies(bool reloadPage = true, bool? triggerMachineReload = null);

    internal void SetAuthSessionDataCookies(bool reloadPage = false, bool? triggerMachineReload = null);
}

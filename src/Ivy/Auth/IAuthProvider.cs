using Ivy.Hooks;
using Microsoft.AspNetCore.Http;

namespace Ivy.Auth;

public interface IAuthProvider
{
    Task InitializeAsync(IAuthSession authSession, string requestScheme, string requestHost, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    Task<AuthToken?> LoginAsync(IAuthSession authSession, string email, string password, CancellationToken cancellationToken = default);

    Task LogoutAsync(IAuthSession authSession, CancellationToken cancellationToken = default);

    Task<AuthToken?> RefreshAccessTokenAsync(IAuthSession authSession, CancellationToken cancellationToken = default);

    Task<bool> ValidateAccessTokenAsync(IAuthSession authSession, CancellationToken cancellationToken = default);

    Task<UserInfo?> GetUserInfoAsync(IAuthSession authSession, CancellationToken cancellationToken = default);

    AuthOption[] GetAuthOptions();

    Task<Uri> GetOAuthUriAsync(IAuthSession authSession, AuthOption option, WebhookEndpoint callback, CancellationToken cancellationToken = default);

    Task<AuthToken?> HandleOAuthCallbackAsync(IAuthSession authSession, HttpRequest request, CancellationToken cancellationToken = default);

    Task<TokenLifetime?> GetAccessTokenLifetimeAsync(IAuthSession authSession, CancellationToken cancellationToken = default);
}

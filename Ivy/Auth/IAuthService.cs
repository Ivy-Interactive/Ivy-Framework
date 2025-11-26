using Ivy.Hooks;
using Microsoft.AspNetCore.Http;

namespace Ivy.Auth;

public interface IAuthService
{
    Task<AuthToken?> LoginAsync(string email, string password, CancellationToken cancellationToken = default);

    Task<Uri> GetOAuthUriAsync(AuthOption option, WebhookEndpoint callback, CancellationToken cancellationToken = default);

    Task<AuthToken?> HandleOAuthCallbackAsync(HttpRequest request, CancellationToken cancellationToken = default);

    Task LogoutAsync(CancellationToken cancellationToken = default);

    Task<UserInfo?> GetUserInfoAsync(CancellationToken cancellationToken = default);

    AuthOption[] GetAuthOptions();

    Task<AuthToken?> RefreshAccessTokenAsync(CancellationToken cancellationToken = default);

    AuthToken? GetCurrentToken();
}
using Ivy.Core;
using Microsoft.AspNetCore.Http;

// ReSharper disable once CheckNamespace
namespace Ivy;

public interface IAuthProviderService : IAuthTokenHandlerService
{
    Task<AuthToken?> LoginAsync(string email, string password, CancellationToken cancellationToken = default);

    Task<Uri> GetOAuthUriAsync(AuthOption option, WebhookEndpoint callback, CancellationToken cancellationToken = default);

    Task<AuthToken?> HandleOAuthCallbackAsync(HttpRequest request, CancellationToken cancellationToken = default);

    Task LogoutAsync(CancellationToken cancellationToken = default);

    AuthOption[] GetAuthOptions();

    IAuthProviderSession GetAuthProviderSession();

    Task<OAuthProviderSessionsResult> GetOAuthProviderSessionsAsync(bool skipCache = false, CancellationToken cancellationToken = default);

    internal void SetAuthCookies(bool reloadPage = true, bool? triggerMachineReload = null);
}

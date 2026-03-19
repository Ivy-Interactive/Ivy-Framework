using Ivy.Core;
using Microsoft.AspNetCore.Http;

// ReSharper disable once CheckNamespace
namespace Ivy;

public interface IAuthService : IAuthTokenHandlerService
{
    Task<AuthToken?> LoginAsync(string email, string password, CancellationToken cancellationToken = default);

    Task<Uri> GetOAuthUriAsync(AuthOption option, WebhookEndpoint callback, CancellationToken cancellationToken = default);

    Task<AuthToken?> HandleOAuthCallbackAsync(HttpRequest request, CancellationToken cancellationToken = default);

    Task LogoutAsync(CancellationToken cancellationToken = default);

    AuthOption[] GetAuthOptions();

    IAuthSession GetAuthSession();

    Task<BrokeredSessionsResult> GetBrokeredSessionsAsync(bool skipCache = false, CancellationToken cancellationToken = default);

    internal void SetAuthCookies(bool reloadPage = true, bool? triggerMachineReload = null, bool triggerMachineBrokeredRefresh = false);
}

using Ivy.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

// ReSharper disable once CheckNamespace
namespace Ivy;

public static class AuthProviderHelpers
{
    /// <summary>
    /// Gets the User-Agent string to use for HTTP requests from auth providers.
    /// Checks configuration for a custom value, otherwise uses Ivy-Framework/{version}.
    /// </summary>
    public static string GetUserAgent(IConfiguration configuration, string configKey)
    {
        var ivyVersion = typeof(IAuthProvider).Assembly.GetName().Version?.ToString() ?? "1.0.0";
        return configuration.GetValue<string>(configKey) ?? $"Ivy-Framework/{ivyVersion}";
    }
}

public interface IAuthProvider : IAuthTokenHandler
{
    Task<LoginResult> LoginAsync(IAuthSession authSession, string email, string password, CancellationToken cancellationToken = default);

    Task LogoutAsync(IAuthSession authSession, CancellationToken cancellationToken = default);

    AuthOption[] GetAuthOptions();

    Task<Uri> GetOAuthUriAsync(IAuthSession authSession, AuthOption option, WebhookEndpoint callback, CancellationToken cancellationToken = default);

    Task<AuthToken?> HandleOAuthCallbackAsync(IAuthSession authSession, HttpRequest request, CancellationToken cancellationToken = default);

    Task<BrokeredSessionsResult> GetBrokeredSessionsAsync(IAuthSession authSession, bool skipCache = false, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(BrokeredSessionsResult.Failure(canRetry: false));
    }

    bool OpenOAuthLoginInNewTab => false;
}

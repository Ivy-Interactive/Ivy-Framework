using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

public class OAuthProviderSessionsResult
{
    public Dictionary<string, IAuthTokenHandlerSession>? Sessions { get; init; }
    public bool CanRetry { get; init; }

    private OAuthProviderSessionsResult() { }

    public static OAuthProviderSessionsResult Success(Dictionary<string, IAuthTokenHandlerSession> sessions)
    {
        return new OAuthProviderSessionsResult
        {
            Sessions = sessions,
            CanRetry = true
        };
    }

    public static OAuthProviderSessionsResult Failure(bool canRetry = true)
    {
        return new OAuthProviderSessionsResult
        {
            Sessions = null,
            CanRetry = canRetry
        };
    }
}

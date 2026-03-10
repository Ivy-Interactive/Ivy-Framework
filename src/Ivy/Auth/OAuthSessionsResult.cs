using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

public class OAuthSessionsResult
{
    public Dictionary<string, IAuthTokenHandlerSession>? Sessions { get; init; }
    public bool CanRetry { get; init; }

    private OAuthSessionsResult() { }

    public static OAuthSessionsResult Success(Dictionary<string, IAuthTokenHandlerSession> sessions)
    {
        return new OAuthSessionsResult
        {
            Sessions = sessions,
            CanRetry = true
        };
    }

    public static OAuthSessionsResult Failure(bool canRetry = true)
    {
        return new OAuthSessionsResult
        {
            Sessions = null,
            CanRetry = canRetry
        };
    }
}

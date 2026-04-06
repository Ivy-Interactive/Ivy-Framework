// ReSharper disable once CheckNamespace
namespace Ivy;

public class BrokeredSessionsResult
{
    public Dictionary<string, IAuthTokenHandlerSession>? Sessions { get; init; }
    public bool CanRetry { get; init; }

    private BrokeredSessionsResult() { }

    public static BrokeredSessionsResult Success(Dictionary<string, IAuthTokenHandlerSession> sessions)
    {
        return new BrokeredSessionsResult
        {
            Sessions = sessions,
            CanRetry = true
        };
    }

    public static BrokeredSessionsResult Failure(bool canRetry = true)
    {
        return new BrokeredSessionsResult
        {
            Sessions = null,
            CanRetry = canRetry
        };
    }
}

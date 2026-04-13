// ReSharper disable once CheckNamespace
namespace Ivy;

public class LoginResult
{
    public AuthToken? Token { get; init; }
    public LoginStatus Status { get; init; }
    public TimeSpan? RetryAfter { get; init; }

    private LoginResult() { }

    public static LoginResult Success(AuthToken token)
    {
        return new LoginResult
        {
            Token = token,
            Status = LoginStatus.Success,
            RetryAfter = null
        };
    }

    public static LoginResult InvalidCredentials()
    {
        return new LoginResult
        {
            Token = null,
            Status = LoginStatus.InvalidCredentials,
            RetryAfter = null
        };
    }

    public static LoginResult RateLimited(TimeSpan retryAfter)
    {
        return new LoginResult
        {
            Token = null,
            Status = LoginStatus.RateLimited,
            RetryAfter = retryAfter
        };
    }
}

public enum LoginStatus
{
    Success,
    InvalidCredentials,
    RateLimited
}

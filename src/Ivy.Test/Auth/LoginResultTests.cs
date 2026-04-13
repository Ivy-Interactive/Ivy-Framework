namespace Ivy.Test.Auth;

public class LoginResultTests
{
    [Fact]
    public void Success_HasToken()
    {
        var token = new AuthToken("access", "refresh");
        var result = LoginResult.Success(token);

        Assert.Equal(LoginStatus.Success, result.Status);
        Assert.Same(token, result.Token);
        Assert.Null(result.RetryAfter);
    }

    [Fact]
    public void InvalidCredentials_NoToken()
    {
        var result = LoginResult.InvalidCredentials();

        Assert.Equal(LoginStatus.InvalidCredentials, result.Status);
        Assert.Null(result.Token);
        Assert.Null(result.RetryAfter);
    }

    [Fact]
    public void RateLimited_HasRetryAfter()
    {
        var delay = TimeSpan.FromSeconds(30);
        var result = LoginResult.RateLimited(delay);

        Assert.Equal(LoginStatus.RateLimited, result.Status);
        Assert.Null(result.Token);
        Assert.Equal(delay, result.RetryAfter);
    }
}

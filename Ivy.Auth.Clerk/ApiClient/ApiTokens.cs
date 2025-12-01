namespace Ivy.Auth.Clerk.ApiClient;

public enum ClerkApiClientTokenType
{
    DevBrowser,
    ClientJwt,
}

public enum ClerkApiSessionTokenType
{
    DevBrowser,
    SessionJwt,
}

public readonly struct ClerkApiClientToken
{
    public ClerkApiClientTokenType TokenType { get; }
    public string Token { get; }

    private ClerkApiClientToken(ClerkApiClientTokenType tokenType, string token)
    {
        TokenType = tokenType;
        Token = token;
    }

    public static ClerkApiClientToken FromDevBrowserToken(string token)
    {
        return new ClerkApiClientToken(ClerkApiClientTokenType.DevBrowser, token);
    }

    public static ClerkApiClientToken FromClientJwtToken(string token)
    {
        return new ClerkApiClientToken(ClerkApiClientTokenType.ClientJwt, token);
    }
}

public readonly struct ClerkApiSessionToken
{
    public ClerkApiSessionTokenType TokenType { get; }
    public string Token { get; }

    private ClerkApiSessionToken(ClerkApiSessionTokenType tokenType, string token)
    {
        TokenType = tokenType;
        Token = token;
    }

    public static ClerkApiSessionToken FromDevBrowserToken(string token)
    {
        return new ClerkApiSessionToken(ClerkApiSessionTokenType.DevBrowser, token);
    }

    public static ClerkApiSessionToken FromSessionJwtToken(string token)
    {
        return new ClerkApiSessionToken(ClerkApiSessionTokenType.SessionJwt, token);
    }
}

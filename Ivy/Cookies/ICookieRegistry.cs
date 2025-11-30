namespace Ivy.Cookies;

public readonly struct CookieJarId
{
    private readonly string _value;

    internal CookieJarId(string value)
    {
        _value = value ?? throw new ArgumentNullException(nameof(value));
    }

    internal string Value => _value;

    public override string ToString() => _value;
}

public interface IGlobalCookieRegistry : IDisposable
{
    CookieJarId Register(CookieJar cookieJar, string intent);
    bool TryRemove(CookieJarId cookieJarId, string intent, out CookieJar cookieJar);
    bool TryRemove(CookieJarId cookieJarId);
}

public interface ICookieRegistry : IDisposable
{
    CookieJarId Register(CookieJar cookieJar, string intent);
}

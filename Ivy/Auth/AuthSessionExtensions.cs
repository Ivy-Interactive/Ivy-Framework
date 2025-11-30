using System.Text.Json;

namespace Ivy.Auth;

public static class AuthSessionExtensions
{
#if DEBUG
    public static CheckedAuthSessionBuilder WithCheckedAccess(this IAuthSession authSession)
        => new(authSession);
#endif

    public static T? GetAuthSessionData<T>(this IAuthSession authSession)
    {
        if (string.IsNullOrEmpty(authSession.AuthSessionData))
        {
            return default;
        }

        if (typeof(T) == typeof(string))
        {
            return (T)(object)authSession.AuthSessionData;
        }
        else
        {
            return JsonSerializer.Deserialize<T>(authSession.AuthSessionData);
        }
    }

    public static void SetAuthSessionData<T>(this IAuthSession authSession, T? data)
    {
        if (data == null)
        {
            authSession.AuthSessionData = null;
        }
        else if (data is string strData)
        {
            authSession.AuthSessionData = strData;
        }
        else
        {
            authSession.AuthSessionData = JsonSerializer.Serialize(data);
        }
    }
}

using System.Text.Json;

namespace Ivy.Auth;

public interface IAuthSession
{
    public AuthToken? AuthToken { get; set; }
    public string? AuthSessionData { get; set; }

    public T? GetAuthSessionData<T>()
    {
        if (string.IsNullOrEmpty(AuthSessionData))
        {
            return default;
        }

        if (typeof(T) == typeof(string))
        {
            return (T)(object)AuthSessionData;
        }
        else
        {
            return JsonSerializer.Deserialize<T>(AuthSessionData);
        }
    }

    public void SetAuthSessionData<T>(T? data)
    {
        if (data == null)
        {
            AuthSessionData = null;
        }
        else if (data is string strData)
        {
            AuthSessionData = strData;
        }
        else
        {
            AuthSessionData = JsonSerializer.Serialize(data);
        }
    }
}

public class AuthSession(AuthToken? authToken = null, string? authSessionData = null) : IAuthSession
{
    public virtual AuthToken? AuthToken { get; set; } = authToken;
    public virtual string? AuthSessionData { get; set; } = authSessionData;
}

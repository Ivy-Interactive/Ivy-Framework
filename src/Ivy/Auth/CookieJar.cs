using Microsoft.AspNetCore.Http;

namespace Ivy.Auth;

public static class CookieJarIntents
{
    public const string SetAuthCookies = "set-auth-cookies";
}

public class CookieJar
{
    private readonly record struct CookieAssignment(string Name, string Value, CookieOptions Options);

    private readonly List<CookieAssignment> _assignments = [];

    public void Append(string name, string value, CookieOptions options)
    {
        _assignments.Add(new CookieAssignment(name, value, options));
    }

    public bool TryGet(string name, out string? value)
    {
        var assignment = _assignments.LastOrDefault(a => a.Name == name);
        if (assignment != default)
        {
            value = assignment.Value;
            return true;
        }

        value = null;
        return false;
    }

    public void Delete(string name, CookieOptions options)
    {
        options.Expires = DateTimeOffset.UnixEpoch;
        _assignments.Add(new CookieAssignment(name, string.Empty, options));
    }

    public void WriteToResponse(HttpResponse response)
    {
        foreach (var assignment in _assignments)
        {
            response.Cookies.Append(assignment.Name, assignment.Value, assignment.Options);
        }
    }
}

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

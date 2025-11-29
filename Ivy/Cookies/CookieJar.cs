using Microsoft.AspNetCore.Http;

namespace Ivy.Cookies;

public record struct CookieAssignment(string Name, string Value, CookieOptions Options);

public class CookieJar
{
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

    public void Delete(string name)
    {
        _assignments.Add(new CookieAssignment(name, string.Empty, new CookieOptions
        {
            Expires = DateTimeOffset.UnixEpoch,
            Path = "/"
        }));
    }

    public void WriteToResponse(HttpResponse response)
    {
        foreach (var assignment in _assignments)
        {
            response.Cookies.Append(assignment.Name, assignment.Value, assignment.Options);
        }
    }
}

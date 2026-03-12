using System.Text.Json;
using Ivy.Core.Helpers;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// Arguments for the generic error display app. Passed when showing a friendly error page for any framework or app error.
/// Use the static factory methods (<see cref="ForNotFound"/>, <see cref="ForNoApps"/>, <see cref="ForServerError"/>, <see cref="Custom"/>) and <see cref="ToArgsJson"/> to build args from anywhere (e.g. router, auth, middleware). The error app is registered with <see cref="Server.UseErrorNotFound{T}"/> and receives these args via UseArgs&lt;ErrorAppArgs&gt;().
/// </summary>
/// <param name="Title">Heading shown on the error page (e.g. "Ouch! :|").</param>
/// <param name="Message">Explanation shown below the title.</param>
/// <param name="Kind">Optional error kind for styling (e.g. <see cref="KindNotFound"/>, <see cref="KindNoApps"/>, <see cref="KindServerError"/>).</param>
public record ErrorAppArgs(string Title, string Message, string? Kind = null)
{
    /// <summary>Kind for "app/resource not found" (404).</summary>
    public const string KindNotFound = "NotFound";

    /// <summary>Kind for "no apps registered on server".</summary>
    public const string KindNoApps = "NoApps";

    /// <summary>Kind for server/generic errors (500).</summary>
    public const string KindServerError = "ServerError";

    /// <summary>Kind for unauthorized (401).</summary>
    public const string KindUnauthorized = "Unauthorized";

    /// <summary>Kind for forbidden (403).</summary>
    public const string KindForbidden = "Forbidden";

    /// <summary>Build args for a generic "not found" error.</summary>
    public static ErrorAppArgs ForNotFound(string? title = null, string? message = null) =>
        new(title ?? ErrorApp.DefaultTitle, message ?? ErrorApp.DefaultNotFoundMessage, KindNotFound);

    /// <summary>Build args for "no apps registered" error.</summary>
    public static ErrorAppArgs ForNoApps(string? title = null, string? message = null) =>
        new(title ?? ErrorApp.DefaultTitle, message ?? ErrorApp.DefaultNoAppsMessage, KindNoApps);

    /// <summary>Build args for a server/generic error.</summary>
    public static ErrorAppArgs ForServerError(string message, string? title = null) =>
        new(title ?? ErrorApp.DefaultTitle, message, KindServerError);

    /// <summary>Build args for any custom error. Pass kind for optional styling.</summary>
    public static ErrorAppArgs Custom(string title, string message, string? kind = null) =>
        new(title, message, kind);

    /// <summary>Serialize to JSON for use in route result ArgsJson or navigation.</summary>
    public static string ToArgsJson(ErrorAppArgs args) =>
        JsonSerializer.Serialize(args, JsonHelper.DefaultOptions);
}

using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// Generic error display app. Shows a configurable title and message via UseArgs&lt;ErrorAppArgs&gt;().
/// Supports optional <see cref="ErrorAppArgs.Kind"/> for different visuals (e.g. Callout variant, icon).
/// Use for any framework or app error: not found, no apps, server error, unauthorized, or custom.
/// Registered via <see cref="Server.UseErrorNotFound{T}"/>; not intended to be used as a user-facing app.
/// </summary>
public class ErrorApp : ViewBase
{
    public const string DefaultTitle = "Ouch! :|";
    public const string DefaultNotFoundMessage = "Apologies, the app you were looking for was not found.";
    public const string DefaultNoAppsMessage = "No apps are registered on this server.";
    public const string DefaultServerErrorMessage = "Something went wrong. Please try again later.";

    public override object? Build()
    {
        var args = UseArgs<ErrorAppArgs>();
        var title = args?.Title ?? DefaultTitle;
        var message = args?.Message ?? DefaultNotFoundMessage;
        var kind = args?.Kind;

        var (variant, icon) = GetVariantAndIcon(kind);

        return Layout.Center()
               | (Layout.Vertical()
                   .Gap(4)
                   .Center()
                   | CalloutForVariant(variant, title, message, icon)
               );
    }

    private static (CalloutVariant variant, Icons? icon) GetVariantAndIcon(string? kind)
    {
        return kind switch
        {
            ErrorAppArgs.KindNotFound => (CalloutVariant.Warning, Icons.SearchX),
            ErrorAppArgs.KindNoApps => (CalloutVariant.Warning, Icons.LayoutList),
            ErrorAppArgs.KindServerError => (CalloutVariant.Error, Icons.CircleAlert),
            ErrorAppArgs.KindUnauthorized => (CalloutVariant.Warning, Icons.LogIn),
            ErrorAppArgs.KindForbidden => (CalloutVariant.Error, Icons.ShieldAlert),
            _ => (CalloutVariant.Warning, Icons.CircleAlert)
        };
    }

    private static Callout CalloutForVariant(CalloutVariant variant, string title, string message, Icons? icon)
    {
        var callout = variant switch
        {
            CalloutVariant.Error => Callout.Error(message, title),
            CalloutVariant.Success => Callout.Success(message, title),
            CalloutVariant.Warning => Callout.Warning(message, title),
            _ => Callout.Info(message, title)
        };
        return icon != null ? callout.Icon(icon.Value) : callout;
    }
}

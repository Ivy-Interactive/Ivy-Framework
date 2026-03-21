namespace Ivy;

/// <summary>
/// Base class for Ivy apps. Equivalent to <see cref="ViewBase"/> but semantically marks a class as an app.
/// Use the <see cref="AppAttribute"/> to configure app metadata (title, icon, group).
/// </summary>
public abstract class AppBase : ViewBase
{
}

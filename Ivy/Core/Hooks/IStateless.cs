namespace Ivy.Core.Hooks;

/// <summary>
/// Marker interface indicating that a view or component has no state and should not have access to hooks.
/// Views implementing this interface will not receive a ViewContext and cannot use state or effects.
/// </summary>
public interface IStateless
{
}
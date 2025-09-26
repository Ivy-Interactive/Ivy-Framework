namespace Ivy.Core.Hooks;

/// <summary>
/// Interface for objects that own and provide access to a view context for hooks functionality.
/// </summary>
public interface IViewContextOwner
{
    /// <summary>Gets the view context that provides access to hooks like UseState and UseEffect.</summary>
    public IViewContext Context { get; }
}
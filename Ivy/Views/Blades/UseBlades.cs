using System.Collections.Immutable;
using Ivy.Core;
using Ivy.Core.Hooks;
using Ivy.Shared;

namespace Ivy.Views.Blades;

/// <summary>
/// Interface for controlling blade navigation stack operations.
/// </summary>
/// <remarks>
/// Provides methods to manage a stack of blade views with push/pop operations,
/// enabling stacked navigation patterns for master-detail interfaces and drill-down experiences.
/// </remarks>
public interface IBladeController
{
    /// <summary>Gets the reactive state containing all blade items in the navigation stack.</summary>
    /// <value>An immutable array of blade items representing the current navigation stack.</value>
    IState<ImmutableArray<BladeItem>> Blades { get; }

    /// <summary>
    /// Pushes a new blade onto the navigation stack at the specified position.
    /// </summary>
    /// <param name="bladeView">The view to display in the new blade.</param>
    /// <param name="title">Optional title for the blade header.</param>
    /// <param name="toIndex">Optional index to insert the blade at. Defaults to the end of the stack.</param>
    /// <param name="width">Optional width constraint for the blade.</param>
    void Push(IView bladeView, string? title = null, int? toIndex = null, Size? width = null);

    /// <summary>
    /// Pushes a new blade onto the navigation stack after the specified current view.
    /// </summary>
    /// <param name="currentView">The current view to push after.</param>
    /// <param name="bladeView">The view to display in the new blade.</param>
    /// <param name="title">Optional title for the blade header.</param>
    /// <param name="width">Optional width constraint for the blade.</param>
    void Push(IView currentView, IView bladeView, string? title = null, Size? width = null);

    /// <summary>
    /// Pops blades from the navigation stack back to the specified index.
    /// </summary>
    /// <param name="toIndex">Optional index to pop back to. Defaults to removing the last blade.</param>
    /// <param name="refresh">Whether to refresh the target blade after popping.</param>
    void Pop(int? toIndex = null, bool refresh = false);

    /// <summary>
    /// Pops blades from the navigation stack back to the specified current view.
    /// </summary>
    /// <param name="currentView">The current view to pop back to.</param>
    /// <param name="refresh">Whether to refresh the target blade after popping.</param>
    void Pop(IView currentView, bool refresh = false) => Pop(GetIndex(currentView), refresh);

    /// <summary>
    /// Gets the index of the specified blade view in the navigation stack.
    /// </summary>
    /// <param name="bladeView">The blade view to find the index for.</param>
    /// <returns>The zero-based index of the blade view in the stack.</returns>
    int GetIndex(IView bladeView);
}

/// <summary>
/// Default implementation of the blade controller for managing blade navigation stacks.
/// </summary>
/// <remarks>
/// Manages an immutable array of blade items with reactive state updates,
/// providing push/pop operations for stacked navigation patterns.
/// </remarks>
public class BladeController : IBladeController
{
    /// <summary>
    /// Initializes a new blade controller with an empty blade stack.
    /// </summary>
    public BladeController()
    {
        Blades = new State<ImmutableArray<BladeItem>>([]);
    }

    /// <summary>
    /// Initializes a new blade controller with the specified blade state.
    /// </summary>
    /// <param name="blades">The reactive state containing the blade items.</param>
    public BladeController(IState<ImmutableArray<BladeItem>> blades)
    {
        Blades = blades;
    }

    /// <summary>Gets the reactive state containing all blade items in the navigation stack.</summary>
    /// <value>An immutable array of blade items representing the current navigation stack.</value>
    public IState<ImmutableArray<BladeItem>> Blades { get; }

    /// <summary>
    /// Pushes a new blade onto the navigation stack at the specified position.
    /// </summary>
    /// <param name="bladeView">The view to display in the new blade.</param>
    /// <param name="title">Optional title for the blade header.</param>
    /// <param name="toIndex">Optional index to insert the blade at. Defaults to the end of the stack.</param>
    /// <param name="width">Optional width constraint for the blade.</param>
    public void Push(IView bladeView, string? title = null, int? toIndex = null, Size? width = null)
    {
        toIndex ??= Blades.Value.Length - 1;
        //make sure toIndex is within bounds or do nothing if it is not
        if (toIndex < 0 || toIndex >= Blades.Value.Length) return;
        var blade = new BladeItem(bladeView, toIndex.Value + 1, title, width);
        ImmutableArray<BladeItem> immutableArray = [.. Blades.Value.Take(toIndex.Value + 1).Append(blade)];
        Blades.Set(immutableArray);
    }

    /// <summary>
    /// Pushes a new blade onto the navigation stack after the specified current view.
    /// </summary>
    /// <param name="currentView">The current view to push after.</param>
    /// <param name="bladeView">The view to display in the new blade.</param>
    /// <param name="title">Optional title for the blade header.</param>
    /// <param name="width">Optional width constraint for the blade.</param>
    public void Push(IView currentView, IView bladeView, string? title = null, Size? width = null)
    {
        var index = GetIndex(currentView);
        Push(bladeView, title, index, width);
    }

    /// <summary>
    /// Pops blades from the navigation stack back to the specified index.
    /// </summary>
    /// <param name="toIndex">Optional index to pop back to. Defaults to removing the last blade.</param>
    /// <param name="refresh">Whether to refresh the target blade after popping.</param>
    public void Pop(int? toIndex = null, bool refresh = false)
    {
        toIndex ??= Blades.Value.Length - 2;
        //make sure toIndex is within bounds or do nothing if it is not
        if (toIndex < 0 || toIndex >= Blades.Value.Length) return;
        Blades.Set([.. Blades.Value.Take(toIndex.Value + 1)]);
        if (refresh)
        {
            Blades.Value[toIndex.Value].RefreshToken = DateTime.UtcNow.Ticks;
        }
    }

    /// <summary>
    /// Gets the index of the specified blade view in the navigation stack.
    /// </summary>
    /// <param name="bladeView">The blade view to find the index for.</param>
    /// <returns>The zero-based index of the blade view in the stack.</returns>
    public int GetIndex(IView bladeView)
    {
        return Blades.Value.First(e => e.View == bladeView).Index;
    }
}

/// <summary>
/// Represents a single blade item in the navigation stack with its associated metadata.
/// </summary>
/// <remarks>
/// Contains the view, positioning, and display information for a blade in the navigation stack,
/// including refresh token for cache invalidation and unique key for rendering optimization.
/// </remarks>
public class BladeItem(IView view, int index, string? title, Size? width = null)
{
    /// <summary>Gets the unique key for this blade item used for rendering optimization.</summary>
    /// <value>A unique GUID string that identifies this blade instance.</value>
    public string Key { get; } = Guid.NewGuid().ToString();

    /// <summary>Gets or sets the view to display in this blade.</summary>
    /// <value>The view instance that provides the blade's content.</value>
    public IView View { get; set; } = view;

    /// <summary>Gets or sets the zero-based index of this blade in the navigation stack.</summary>
    /// <value>The position of this blade in the stack, starting from 0.</value>
    public int Index { get; set; } = index;

    /// <summary>Gets or sets the refresh token used for cache invalidation and forced re-rendering.</summary>
    /// <value>A timestamp in ticks that changes when the blade needs to refresh its content.</value>
    public long RefreshToken { get; set; } = DateTime.UtcNow.Ticks;

    /// <summary>Gets or sets the optional title displayed in the blade header.</summary>
    /// <value>The title text for the blade, or null to use default display.</value>
    public string? Title { get; set; } = title;

    /// <summary>Gets or sets the optional width constraint for this blade.</summary>
    /// <value>The width constraint, or null to use default blade sizing.</value>
    public Size? Width { get; set; } = width;
}

/// <summary>
/// Extension methods for creating and managing blade navigation systems.
/// </summary>
/// <remarks>
/// Provides convenient methods to set up blade navigation with automatic controller creation
/// and context management for stacked navigation patterns.
/// </remarks>
public static class UseBladesExtensions
{
    /// <summary>
    /// Creates a blade navigation system with the specified root blade for the given view.
    /// </summary>
    /// <typeparam name="TView">The type of view that will use the blade system.</typeparam>
    /// <param name="view">The view that will host the blade navigation.</param>
    /// <param name="rootBlade">A factory function that creates the root blade view.</param>
    /// <param name="title">Optional title for the root blade.</param>
    /// <param name="width">Optional width constraint for the root blade.</param>
    /// <returns>A BladesView that manages the blade navigation interface.</returns>
    public static IView UseBlades<TView>(this TView view, Func<IView> rootBlade, string? title = null, Size? width = null) where TView : ViewBase =>
        view.Context.UseBlades(rootBlade, title, width);

    /// <summary>
    /// Creates a blade navigation system with the specified root blade for the given context.
    /// </summary>
    /// <param name="context">The view context that will host the blade navigation.</param>
    /// <param name="rootBlade">A factory function that creates the root blade view.</param>
    /// <param name="title">Optional title for the root blade.</param>
    /// <param name="width">Optional width constraint for the root blade.</param>
    /// <returns>A BladesView that manages the blade navigation interface.</returns>
    /// <remarks>
    /// This method sets up the complete blade navigation system including:
    /// - Creating reactive state for the blade stack
    /// - Initializing the blade controller
    /// - Setting up the context for blade management
    /// - Returning a BladesView to render the interface
    /// </remarks>
    public static IView UseBlades(this IViewContext context, Func<IView> rootBlade, string? title = null, Size? width = null)
    {
        var blades = context.UseState<ImmutableArray<BladeItem>>(() => [new BladeItem(rootBlade(), 0, title, width)]);
        context.CreateContext<IBladeController>(() => new BladeController(blades));
        IView bladeView = new BladesView();
        return bladeView;
    }
}
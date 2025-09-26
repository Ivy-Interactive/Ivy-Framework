using Ivy.Core;
using Ivy.Core.Hooks;

namespace Ivy.Views;

/// <summary>
/// Represents a function builder delegate that creates view content based on
/// the current view context.
/// </summary>
/// <param name="context">The current view context providing access to
/// state management, services, and other view-related functionality.</param>
/// <returns>The view content object that should be displayed.</returns>
public delegate object? FuncBuilder(IViewContext context);

/// <summary>
/// Represents a function-based view that dynamically creates content
/// using a view factory function.
/// </summary>
public class FuncView(FuncBuilder viewFactory) : ViewBase
{
    /// <summary>
    /// Builds the view content by invoking the view factory function
    /// with the current view context, enabling dynamic content generation.
    /// </summary>
    /// <returns>The dynamically generated view content from the factory function.</returns>
    public override object? Build()
    {
        return viewFactory(Context);
    }
}

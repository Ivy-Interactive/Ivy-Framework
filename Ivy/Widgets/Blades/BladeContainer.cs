using Ivy.Core;
using Ivy.Views.Blades;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>The BladeContainer is responsible for rendering multiple blades side-by-side, creating the characteristic sliding navigation experience where each blade represents a level in the navigation hierarchy.</summary>
public record BladeContainer : WidgetBase<BladeContainer>
{
    /// <param name="blades">The blades are typically managed by an <see cref="IBladeController"/> and represent different levels in the navigation hierarchy, from root blade (index 0) to the deepest navigation level.</param>
    public BladeContainer(params BladeView[] blades) : base(blades.Cast<object>().ToArray())
    {
    }
}
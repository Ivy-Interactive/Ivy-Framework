using Ivy.Core;
using Ivy.Views.Blades;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// Container widget that holds and manages a collection of blade views in a stacked navigation interface.
/// The BladeContainer is responsible for rendering multiple blades side-by-side, creating the characteristic
/// sliding navigation experience where each blade represents a level in the navigation hierarchy.
/// </summary>
public record BladeContainer : WidgetBase<BladeContainer>
{
    /// <summary>
    /// Initializes a new instance of the BladeContainer class with the specified blade views.
    /// Each BladeView represents a memoized wrapper around a Blade widget that manages refresh tokens
    /// and provides optimized rendering for the blade navigation system.
    /// </summary>
    /// <param name="blades">
    /// An array of <see cref="BladeView"/> instances to be contained within this container.
    /// Each BladeView wraps a <see cref="Blade"/> widget and provides memoization for performance optimization.
    /// The blades are typically managed by an <see cref="IBladeController"/> and represent different levels
    /// in the navigation hierarchy, from root blade (index 0) to the deepest navigation level.
    /// </param>
    public BladeContainer(params BladeView[] blades) : base(blades.Cast<object>().ToArray())
    {
    }
}
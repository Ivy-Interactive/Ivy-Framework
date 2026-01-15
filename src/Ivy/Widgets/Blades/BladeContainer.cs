using Ivy.Core;
using Ivy.Views.Blades;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// A container for managing multiple Blade views in a stack.
/// </summary>
public record BladeContainer : WidgetBase<BladeContainer>
{
    public BladeContainer(params BladeView[] blades) : base(blades.Cast<object>().ToArray())
    {
    }

    internal BladeContainer()
    {
    }
}
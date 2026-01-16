using Ivy.Core;
using Ivy.Views.Blades;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// The container managing the stack of active blades.
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
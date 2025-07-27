using Ivy.Core;
using Ivy.Views.Blades;

// ReSharper disable once CheckNamespace
namespace Ivy;

public record BladeContainer : WidgetBase<BladeContainer>
{
    public BladeContainer(params BladeView[] blades) : base(blades.Cast<object>().ToArray())
    {
    }
}
using Ivy.Core.Hooks;

namespace Ivy.Views.Blades;

public class BladeHeader(object header) : ViewBase, IStateless
{
    public override object? Build()
    {
        return new Slot("BladeHeader", header);
    }
}
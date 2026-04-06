// ReSharper disable once CheckNamespace
namespace Ivy;

public class BladeHeader(object header) : ViewBase, IStateless
{
    public override object? Build()
    {
        return new Slot("BladeHeader", header);
    }
}

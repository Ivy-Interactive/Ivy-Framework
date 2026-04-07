// ReSharper disable once CheckNamespace
namespace Ivy;

public class WrapperView(params object[] anything) : ViewBase
{
    public override object? Build()
    {
        if (anything.Length == 0)
        {
            return null;
        }
        if (anything.Length == 1)
        {
            return anything[0];
        }
        return Layout.Vertical().Scroll() | anything;
    }
}

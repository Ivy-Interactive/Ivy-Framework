using Ivy.Core;
using Ivy.Helpers;

namespace Ivy.Views;

/// <summary>
/// A simple container view that wraps content in a vertical scrolling layout if multiple items are provided.
/// Returns null for empty content or the single item if only one is provided.
/// </summary>
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
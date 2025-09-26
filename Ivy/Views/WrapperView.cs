using Ivy.Core;
using Ivy.Helpers;

namespace Ivy.Views;

/// <summary>
/// A flexible view wrapper that handles single or multiple content items with
/// automatic layout decisions. This view automatically determines the appropriate
/// display method based on the number of content items provided.
/// </summary>
public class WrapperView(params object[] anything) : ViewBase
{
    /// <summary>
    /// Builds the wrapped content based on the number of items provided.
    /// </summary>
    /// <returns>Null for empty content, the single item directly, or a Layout.Vertical with scrolling for multiple items.</returns>
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
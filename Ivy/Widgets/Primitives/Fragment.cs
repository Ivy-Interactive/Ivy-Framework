using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// A transparent container widget that groups multiple child elements without adding visual structure or layout constraints.
/// Provides a way to return multiple widgets as a single unit while maintaining clean widget tree organization
/// and avoiding unnecessary wrapper elements that could interfere with layout and styling.
/// </summary>
/// <remarks>
/// The Fragment widget is designed for scenarios where multiple widgets need to be grouped logically:
/// <list type="bullet">
/// <item><description><strong>Multiple returns:</strong> Return multiple widgets from methods or conditional logic without wrapper containers</description></item>
/// <item><description><strong>Layout preservation:</strong> Group elements without affecting parent layout behavior or CSS styling</description></item>
/// <item><description><strong>Conditional grouping:</strong> Conditionally render multiple related widgets as a cohesive unit</description></item>
/// <item><description><strong>Component composition:</strong> Combine multiple widgets into reusable components without visual containers</description></item>
/// </list>
/// <para>Fragment automatically filters out null children to provide clean, safe widget composition without null reference concerns.</para>
/// </remarks>
public record Fragment : WidgetBase<Fragment>
{
    /// <summary>
    /// Initializes a new fragment container with the specified child widgets.
    /// Creates a transparent grouping of widgets that renders the children directly without
    /// adding any wrapper elements, while automatically filtering out null values for safe composition.
    /// </summary>
    /// <param name="children">The child widgets to group within the fragment. Null values are automatically filtered out.</param>
    /// <remarks>
    /// The Fragment constructor provides several key benefits:
    /// <list type="bullet">
    /// <item><description><strong>Null safety:</strong> Automatically filters out null children to prevent rendering issues</description></item>
    /// <item><description><strong>Transparent rendering:</strong> Children are rendered directly without additional wrapper elements</description></item>
    /// <item><description><strong>Flexible composition:</strong> Accepts any number of child widgets for dynamic grouping</description></item>
    /// <item><description><strong>Layout preservation:</strong> Maintains parent layout behavior without interference</description></item>
    /// </list>
    /// <para>This is particularly useful when conditionally including widgets or when methods need to return multiple widgets as a single unit.</para>
    /// </remarks>
    public Fragment(params object?[] children) : base(children.Where(e => e != null).ToArray()!)
    {
    }
}
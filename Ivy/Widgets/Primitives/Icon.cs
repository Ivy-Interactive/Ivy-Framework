using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// A widget for displaying scalable vector icons with customizable appearance and sizing.
/// Provides access to a comprehensive icon library with support for theming, coloring, and responsive sizing
/// to enhance user interface clarity and visual communication throughout the application.
/// </summary>
/// <remarks>
/// The Icon widget is designed for versatile visual communication:
/// <list type="bullet">
/// <item><description><strong>Visual indicators:</strong> Provide clear visual cues for actions, status, and navigation elements</description></item>
/// <item><description><strong>Interface enhancement:</strong> Improve user experience with recognizable symbols and visual hierarchy</description></item>
/// <item><description><strong>Accessibility support:</strong> Offer visual alternatives to text-based labels and descriptions</description></item>
/// <item><description><strong>Responsive design:</strong> Scale appropriately across different screen sizes and device types</description></item>
/// </list>
/// <para>Icons are vector-based for crisp rendering at any size and support theming integration for consistent visual design across the application.</para>
/// </remarks>
public record Icon : WidgetBase<Icon>
{
    /// <summary>
    /// Initializes a new icon widget with the specified icon type and optional color customization.
    /// Creates a scalable vector icon that integrates with the application's design system,
    /// providing consistent visual communication with customizable appearance properties.
    /// </summary>
    /// <param name="name">The specific icon to display from the available icon library.</param>
    /// <param name="color">Optional color override for the icon. If null, uses the default theme color.</param>
    /// <remarks>
    /// The Icon constructor provides flexible icon creation with design system integration:
    /// <list type="bullet">
    /// <item><description><strong>Icon library access:</strong> Choose from a comprehensive set of predefined icons for common UI patterns</description></item>
    /// <item><description><strong>Theme integration:</strong> Automatically inherits appropriate colors from the current theme when no color is specified</description></item>
    /// <item><description><strong>Color customization:</strong> Override default colors for emphasis, branding, or semantic meaning</description></item>
    /// <item><description><strong>Vector rendering:</strong> Ensures crisp, scalable display across all screen densities and sizes</description></item>
    /// </list>
    /// <para>Icons provide essential visual communication elements that enhance usability and create intuitive user interfaces.</para>
    /// </remarks>
    public Icon(Icons name, Colors? color = null)
    {
        Name = name;
        Color = color;
    }

    /// <summary>Gets or sets the specific icon to display from the available icon library.</summary>
    /// <value>The icon type from the <see cref="Icons"/> enumeration that defines which symbol to render.</value>
    /// <remarks>
    /// The Name property determines which icon symbol is displayed:
    /// <list type="bullet">
    /// <item><description><strong>Icon library:</strong> Selected from a curated set of common interface icons and symbols</description></item>
    /// <item><description><strong>Semantic meaning:</strong> Each icon conveys specific meaning and purpose in the user interface</description></item>
    /// <item><description><strong>Consistency:</strong> Ensures visual consistency across the application through standardized iconography</description></item>
    /// <item><description><strong>Recognition:</strong> Uses familiar symbols that users can quickly understand and interact with</description></item>
    /// </list>
    /// <para>Choose icons that clearly communicate their intended function and align with user expectations and interface conventions.</para>
    /// </remarks>
    [Prop] public Icons Name { get; set; }

    /// <summary>Gets or sets the color override for the icon display.</summary>
    /// <value>The color from the <see cref="Colors"/> enumeration to apply to the icon, or null to use the default theme color.</value>
    /// <remarks>
    /// The Color property provides flexible icon appearance customization:
    /// <list type="bullet">
    /// <item><description><strong>Theme integration:</strong> When null, automatically uses appropriate colors from the current theme</description></item>
    /// <item><description><strong>Semantic coloring:</strong> Apply specific colors to convey meaning (success, warning, error, info)</description></item>
    /// <item><description><strong>Brand consistency:</strong> Use brand colors for logo icons or branded interface elements</description></item>
    /// <item><description><strong>Visual hierarchy:</strong> Emphasize important icons with contrasting or accent colors</description></item>
    /// </list>
    /// <para>Consider accessibility and contrast requirements when choosing custom colors to ensure icons remain visible and usable for all users.</para>
    /// </remarks>
    [Prop] public Colors? Color { get; set; }
}

/// <summary>
/// Provides extension methods for configuring icon widgets with fluent syntax and convenient sizing options.
/// Enables streamlined icon creation, color customization, and responsive sizing through method chaining
/// for improved code readability and consistent icon usage patterns throughout the application.
/// </summary>
public static class IconExtensions
{
    /// <summary>
    /// Converts an Icons enumeration value to an Icon widget for convenient icon creation.
    /// </summary>
    /// <param name="icon">The icon type to convert to an Icon widget.</param>
    /// <returns>A new Icon widget displaying the specified icon with default appearance settings.</returns>
    /// <remarks>
    /// This extension method provides a convenient way to create icons directly from the Icons enumeration:
    /// <list type="bullet">
    /// <item><description><strong>Simplified creation:</strong> Reduces boilerplate code when creating basic icons</description></item>
    /// <item><description><strong>Fluent syntax:</strong> Enables method chaining for additional customization</description></item>
    /// <item><description><strong>Default appearance:</strong> Uses theme-appropriate colors and standard sizing</description></item>
    /// <item><description><strong>Type safety:</strong> Ensures only valid icon types can be converted</description></item>
    /// </list>
    /// <para>Use this method as a starting point for icon creation, then chain additional methods for customization.</para>
    /// </remarks>
    public static Icon ToIcon(this Icons icon)
    {
        return new Icon(icon);
    }

    /// <summary>
    /// Sets the color of the icon for visual customization and semantic meaning.
    /// </summary>
    /// <param name="icon">The icon widget to configure.</param>
    /// <param name="color">The color to apply to the icon, or null to use the default theme color.</param>
    /// <returns>The icon widget with the specified color configuration.</returns>
    /// <remarks>
    /// Color customization enables semantic and visual communication through icon appearance:
    /// <list type="bullet">
    /// <item><description><strong>Semantic meaning:</strong> Use colors to convey status, importance, or category</description></item>
    /// <item><description><strong>Visual hierarchy:</strong> Emphasize or de-emphasize icons within the interface</description></item>
    /// <item><description><strong>Brand consistency:</strong> Apply brand colors for consistent visual identity</description></item>
    /// <item><description><strong>Theme integration:</strong> Null values automatically use appropriate theme colors</description></item>
    /// </list>
    /// <para>Consider accessibility guidelines and ensure sufficient contrast when applying custom colors.</para>
    /// </remarks>
    public static Icon Color(this Icon icon, Colors? color = null)
    {
        return icon with { Color = color };
    }

    /// <summary>
    /// Configures the icon with small sizing (4 units) for compact interface elements and secondary content.
    /// </summary>
    /// <param name="icon">The icon widget to configure.</param>
    /// <returns>The icon widget configured with small dimensions (4x4 units).</returns>
    /// <remarks>
    /// Small icons are ideal for specific interface contexts:
    /// <list type="bullet">
    /// <item><description><strong>Compact layouts:</strong> Fit within tight spacing constraints and dense information displays</description></item>
    /// <item><description><strong>Secondary elements:</strong> Provide visual cues without overwhelming primary content</description></item>
    /// <item><description><strong>List items:</strong> Enhance list entries and table cells with minimal space usage</description></item>
    /// <item><description><strong>Form controls:</strong> Add visual indicators to input fields and form elements</description></item>
    /// </list>
    /// <para>Ensure small icons remain legible and accessible across different screen densities and viewing conditions.</para>
    /// </remarks>
    public static Icon Small(this Icon icon)
    {
        return icon with { Width = Size.Units(4), Height = Size.Units(4) };
    }

    /// <summary>
    /// Configures the icon with large sizing (12 units) for prominent display and primary interface elements.
    /// </summary>
    /// <param name="icon">The icon widget to configure.</param>
    /// <returns>The icon widget configured with large dimensions (12x12 units).</returns>
    /// <remarks>
    /// Large icons are suitable for prominent interface elements:
    /// <list type="bullet">
    /// <item><description><strong>Primary actions:</strong> Emphasize important buttons and interactive elements</description></item>
    /// <item><description><strong>Navigation elements:</strong> Provide clear visual targets for main navigation and menu items</description></item>
    /// <item><description><strong>Status indicators:</strong> Display prominent status information and system feedback</description></item>
    /// <item><description><strong>Feature highlights:</strong> Draw attention to key features and capabilities</description></item>
    /// </list>
    /// <para>Large icons improve accessibility by providing bigger touch targets and enhanced visibility for users with visual impairments.</para>
    /// </remarks>
    public static Icon Large(this Icon icon)
    {
        return icon with { Width = Size.Units(12), Height = Size.Units(12) };
    }
}
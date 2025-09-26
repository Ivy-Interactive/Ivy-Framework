namespace Ivy.Views.Blades;

/// <summary>
/// Helper methods for creating blade layouts with custom headers and toolbars.
/// </summary>
public static class BladeHelper
{
    /// <summary>
    /// Creates a blade layout with a custom header that replaces the default blade title.
    /// </summary>
    /// <param name="header">The header content to display in the blade header area (typically toolbars, search inputs, or buttons).</param>
    /// <param name="content">The main content to display in the blade body.</param>
    /// <returns>A fragment containing the blade header slot and content for use within blade views.</returns>
    /// <remarks>
    /// This method creates a blade layout where the header content replaces the default blade title.
    /// Commonly used for adding search inputs, action buttons, or custom toolbars to blade interfaces.
    /// The header content appears in the blade's header area alongside the refresh and close buttons.
    /// </remarks>
    public static object WithHeader(object header, object content)
    {
        return new Fragment()
            | new Slot("BladeHeader", header)
            | content;
    }
}
using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// A user avatar widget that displays a user's profile image with intelligent fallback to text-based representation.
/// Provides a consistent visual representation of users throughout the application with automatic fallback handling
/// when profile images are unavailable, ensuring a polished user experience in all scenarios.
/// </summary>
public record Avatar : WidgetBase<Avatar>
{
    /// <summary>
    /// Initializes a new avatar with fallback text and optional profile image.
    /// Creates a user representation that gracefully handles missing or failed image loads
    /// by displaying the fallback text in a styled container.
    /// </summary>
    /// <param name="fallback">The fallback text displayed when no image is available or when the image fails to load. Typically user initials or a short name.</param>
    /// <param name="image">Optional URL or path to the user's profile image. If null or if the image fails to load, the fallback text will be displayed instead.</param>
    /// <remarks>
    /// The Avatar widget is commonly used in:
    /// <list type="bullet">
    /// <item><description><strong>User profiles:</strong> Display user identity in profile sections and user cards</description></item>
    /// <item><description><strong>Comments and posts:</strong> Show author identity in content lists and discussions</description></item>
    /// <item><description><strong>Navigation:</strong> User identification in headers, menus, and navigation bars</description></item>
    /// <item><description><strong>Team displays:</strong> Member representation in team lists and organizational charts</description></item>
    /// </list>
    /// <para>Best practices for fallback text:</para>
    /// <list type="bullet">
    /// <item><description>Use 1-3 characters (typically initials)</description></item>
    /// <item><description>Use uppercase letters for consistency</description></item>
    /// <item><description>Consider using the first letter of first and last names</description></item>
    /// </list>
    /// </remarks>
    public Avatar(string fallback, string? image = null)
    {
        Fallback = fallback;
        Image = image;
    }

    /// <summary>Gets or sets the fallback text displayed when no image is available or when the image fails to load.</summary>
    /// <value>The fallback text, typically user initials or a short representation of the user's name.</value>
    /// <remarks>
    /// The fallback text is displayed in a styled container with appropriate typography and background color.
    /// It serves as a reliable backup when profile images are unavailable, ensuring users always have a visual representation.
    /// Common patterns include initials (e.g., "JD" for John Doe) or single letters for shorter names.
    /// </remarks>
    [Prop] public string Fallback { get; set; }

    /// <summary>Gets or sets the URL or path to the user's profile image.</summary>
    /// <value>The image URL or file path, or null if no image is specified.</value>
    /// <remarks>
    /// When specified, the avatar will attempt to display this image. If the image fails to load,
    /// is unavailable, or is null, the avatar will automatically fall back to displaying the fallback text.
    /// Supports various image formats and can handle both absolute URLs and relative paths depending on the application configuration.
    /// </remarks>
    [Prop] public string? Image { get; set; }
}

/// <summary>
/// Provides extension methods for configuring avatar widgets with fluent syntax.
/// Enables convenient configuration of avatar properties through method chaining
/// for improved readability and ease of use in avatar creation and customization.
/// </summary>
public static class AvatarExtensions
{
    /// <summary>
    /// Sets the fallback text for the avatar.
    /// </summary>
    /// <param name="avatar">The avatar to configure.</param>
    /// <param name="fallback">The fallback text to display when no image is available, typically user initials.</param>
    /// <returns>The avatar with the specified fallback text.</returns>
    /// <remarks>
    /// The fallback text should be concise and meaningful, typically 1-3 characters representing the user.
    /// Common patterns include initials from first and last names or abbreviations of usernames.
    /// </remarks>
    public static Avatar Fallback(this Avatar avatar, string fallback) => avatar with { Fallback = fallback };

    /// <summary>
    /// Sets the profile image for the avatar.
    /// </summary>
    /// <param name="avatar">The avatar to configure.</param>
    /// <param name="image">The URL or path to the profile image, or null to use only the fallback text.</param>
    /// <returns>The avatar with the specified image.</returns>
    /// <remarks>
    /// If the image fails to load or is unavailable, the avatar will automatically display the fallback text.
    /// Supports various image formats and both absolute URLs and relative paths.
    /// </remarks>
    public static Avatar Image(this Avatar avatar, string? image) => avatar with { Image = image };
}
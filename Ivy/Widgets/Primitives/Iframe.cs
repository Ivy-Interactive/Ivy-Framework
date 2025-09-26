using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// A widget for embedding external web content within the application using HTML iframe elements.
/// Provides secure integration of third-party websites, web applications, and external services
/// while maintaining isolation between the host application and embedded content for security and stability.
/// </summary>
/// <remarks>
/// The Iframe widget is designed for safe external content integration:
/// <list type="bullet">
/// <item><description><strong>External integration:</strong> Embed third-party websites, web applications, and online services seamlessly</description></item>
/// <item><description><strong>Content isolation:</strong> Maintain security boundaries between host application and embedded content</description></item>
/// <item><description><strong>Responsive embedding:</strong> Full-size display that adapts to container dimensions and layout requirements</description></item>
/// <item><description><strong>Refresh control:</strong> Programmatic refresh capabilities for dynamic content updates and state management</description></item>
/// </list>
/// <para><strong>Security Considerations:</strong> Iframe content runs in a separate security context. Ensure embedded URLs are from trusted sources and consider implementing Content Security Policy (CSP) restrictions for enhanced security.</para>
/// </remarks>
public record Iframe : WidgetBase<Iframe>
{
    /// <summary>
    /// Initializes a new iframe widget with the specified source URL and optional refresh control.
    /// Creates a full-size embedded frame that displays external web content within the application,
    /// providing secure isolation and optional programmatic refresh capabilities for dynamic content management.
    /// </summary>
    /// <param name="src">The URL of the external content to embed within the iframe. Must be a valid, accessible web address.</param>
    /// <param name="refreshToken">Optional token for controlling iframe refresh behavior. When changed, triggers content reload.</param>
    /// <remarks>
    /// The Iframe constructor provides secure external content embedding with comprehensive configuration:
    /// <list type="bullet">
    /// <item><description><strong>Full-size display:</strong> Automatically configured to fill available container space (width and height set to 100%)</description></item>
    /// <item><description><strong>Security isolation:</strong> Content runs in separate security context, preventing interference with host application</description></item>
    /// <item><description><strong>Refresh control:</strong> Optional refresh token enables programmatic content reloading for dynamic scenarios</description></item>
    /// <item><description><strong>Responsive design:</strong> Adapts to parent container dimensions and layout changes automatically</description></item>
    /// </list>
    /// <para>Use this widget for integrating external services, documentation, maps, social media content, or any web-based functionality that needs to be embedded within your application.</para>
    /// </remarks>
    public Iframe(string src, long? refreshToken = null)
    {
        Src = src;
        Width = Size.Full();
        Height = Size.Full();
        RefreshToken = refreshToken;
    }

    /// <summary>Gets or sets the URL of the external content to display within the iframe.</summary>
    /// <value>The source URL string pointing to the external web content to be embedded.</value>
    /// <remarks>
    /// The Src property determines what external content is displayed in the iframe:
    /// <list type="bullet">
    /// <item><description><strong>External URLs:</strong> Can point to any accessible web address, including third-party websites and services</description></item>
    /// <item><description><strong>Security context:</strong> Content loads in isolated security context, preventing cross-site scripting attacks</description></item>
    /// <item><description><strong>Dynamic updates:</strong> Changing this property triggers reloading of the iframe with new content</description></item>
    /// <item><description><strong>Protocol support:</strong> Supports HTTP, HTTPS, and other web protocols as allowed by browser security policies</description></item>
    /// </list>
    /// <para>Ensure the URL points to trusted sources and consider implementing Content Security Policy restrictions to enhance security and prevent malicious content embedding.</para>
    /// </remarks>
    [Prop] public string Src { get; set; }

    /// <summary>Gets the refresh token used for controlling programmatic iframe content reloading.</summary>
    /// <value>The refresh token value, or null if no refresh control is configured.</value>
    /// <remarks>
    /// The RefreshToken property enables programmatic control over iframe content refresh:
    /// <list type="bullet">
    /// <item><description><strong>Refresh trigger:</strong> Changing this token value forces the iframe to reload its content</description></item>
    /// <item><description><strong>State management:</strong> Useful for refreshing dynamic content or resetting iframe state</description></item>
    /// <item><description><strong>Cache busting:</strong> Helps ensure fresh content loading when embedded content might be cached</description></item>
    /// <item><description><strong>Immutable property:</strong> Set only during construction to maintain predictable refresh behavior</description></item>
    /// </list>
    /// <para>This property is particularly useful for embedded applications that need periodic refresh or when you need to reset the iframe state programmatically.</para>
    /// </remarks>
    [Prop] public long? RefreshToken { get; }
}
using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// A widget for embedding external content within the application through iframe or similar embedding mechanisms.
/// Provides secure and flexible integration of third-party content, web pages, videos, interactive elements,
/// and other external resources while maintaining application security and user experience standards.
/// </summary>
public record Embed : WidgetBase<Embed>
{
    /// <summary>
    /// Initializes a new embed widget with the specified URL for external content.
    /// Creates an embedded content container that safely displays external resources
    /// within the application interface while maintaining security boundaries.
    /// </summary>
    /// <param name="url">The URL of the external content to embed within the application.</param>
    /// <remarks>
    /// The Embed widget is designed for integrating external content:
    /// <list type="bullet">
    /// <item><description><strong>Video embedding:</strong> Display videos from YouTube, Vimeo, and other platforms</description></item>
    /// <item><description><strong>Interactive content:</strong> Embed maps, forms, widgets, and interactive applications</description></item>
    /// <item><description><strong>Document display:</strong> Show PDFs, presentations, and document viewers</description></item>
    /// <item><description><strong>Third-party tools:</strong> Integrate external dashboards, analytics, and specialized tools</description></item>
    /// </list>
    /// <para>Security considerations include content security policies, iframe sandboxing, and trusted domain validation.</para>
    /// </remarks>
    public Embed(string url)
    {
        Url = url;
    }

    /// <summary>Gets or sets the URL of the external content to embed.</summary>
    /// <value>The URL string pointing to the external resource to be embedded within the application.</value>
    /// <remarks>
    /// The URL should point to content that supports embedding through iframe or similar mechanisms.
    /// Security policies may restrict which domains and content types can be embedded to protect user safety.
    /// Common embedded content includes videos, maps, documents, and interactive web applications.
    /// </remarks>
    [Prop] public string Url { get; set; }
}
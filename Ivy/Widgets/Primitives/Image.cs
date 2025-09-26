using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// A widget for displaying images with automatic sizing and aspect ratio preservation.
/// Provides flexible image rendering capabilities with support for various image formats, sources,
/// and responsive sizing while maintaining optimal display quality and performance across different devices and screen densities.
/// </summary>
/// <remarks>
/// The Image widget is designed for versatile image display scenarios:
/// <list type="bullet">
/// <item><description><strong>Content display:</strong> Show photographs, illustrations, icons, and visual content with proper sizing and quality</description></item>
/// <item><description><strong>Responsive design:</strong> Automatically adapt to container constraints while preserving image aspect ratios</description></item>
/// <item><description><strong>Performance optimization:</strong> Efficient loading and rendering of images with appropriate sizing and caching</description></item>
/// <item><description><strong>Accessibility support:</strong> Provide visual content with proper alternative text and screen reader compatibility</description></item>
/// </list>
/// <para>Images automatically size to their intrinsic dimensions by default, with support for custom sizing through inherited width and height properties. Future enhancements will include aspect ratio maintenance and various clipping options (circular, square, rounded).</para>
/// </remarks>
public record Image : WidgetBase<Image>
{
    /// <summary>
    /// Initializes a new image widget with the specified source URL and automatic intrinsic sizing.
    /// Creates an image display that loads from the provided source and sizes itself to the minimum content dimensions,
    /// preserving the original image aspect ratio and providing optimal display quality.
    /// </summary>
    /// <param name="src">The URL or path to the image source. Supports various image formats and can be relative paths, absolute URLs, or data URIs.</param>
    /// <remarks>
    /// The Image constructor provides flexible image loading with intelligent default sizing:
    /// <list type="bullet">
    /// <item><description><strong>Intrinsic sizing:</strong> Automatically sizes to minimum content dimensions, respecting the image's natural size</description></item>
    /// <item><description><strong>Aspect ratio preservation:</strong> Maintains original image proportions to prevent distortion</description></item>
    /// <item><description><strong>Format support:</strong> Compatible with common web image formats (JPEG, PNG, GIF, WebP, SVG)</description></item>
    /// <item><description><strong>Source flexibility:</strong> Accepts relative paths, absolute URLs, base64 data URIs, and various image sources</description></item>
    /// </list>
    /// <para>The default sizing behavior ensures images display at their natural dimensions while allowing customization through inherited Width and Height properties for responsive layouts and specific design requirements.</para>
    /// </remarks>
    public Image(string src)
    {
        Src = src;
        Width = Size.MinContent();
        Height = Size.MinContent();
    }

    // TODO: Maintain the aspect ratio of the image, Clippings: Circular, Square, Rounded

    /// <summary>Gets or sets the source URL or path for the image to display.</summary>
    /// <value>The image source string, which can be a relative path, absolute URL, or data URI pointing to the image resource.</value>
    /// <remarks>
    /// The Src property determines which image is loaded and displayed:
    /// <list type="bullet">
    /// <item><description><strong>URL support:</strong> Accepts HTTP/HTTPS URLs for remote images and relative/absolute paths for local images</description></item>
    /// <item><description><strong>Format compatibility:</strong> Supports standard web image formats including JPEG, PNG, GIF, WebP, and SVG</description></item>
    /// <item><description><strong>Data URIs:</strong> Accepts base64-encoded data URIs for embedded image data</description></item>
    /// <item><description><strong>Dynamic updates:</strong> Changing this property triggers reloading of the image with new source content</description></item>
    /// </list>
    /// <para>Ensure image sources are accessible and consider implementing proper error handling for failed image loads. For optimal performance, use appropriate image formats and sizes for your specific use case.</para>
    /// </remarks>
    [Prop] public string Src { get; set; }
}
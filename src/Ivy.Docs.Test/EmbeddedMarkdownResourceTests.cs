using System.Reflection;

namespace Ivy.Docs.Test;

public class EmbeddedMarkdownResourceTests
{
    private static readonly Assembly DocsSharedAssembly = typeof(Ivy.Docs.Shared.Middleware.MarkdownMiddlewareExtensions).Assembly;
    private const string ResourcePrefix = "Ivy.Docs.Shared.Generated.";

    [Fact]
    public void DocsShared_ShouldContainEmbeddedMarkdownResources()
    {
        var mdResources = DocsSharedAssembly
            .GetManifestResourceNames()
            .Where(name => name.StartsWith(ResourcePrefix) && name.EndsWith(".md"))
            .ToList();

        Assert.NotEmpty(mdResources);
    }

    [Fact]
    public void EmbeddedMarkdownResources_ShouldHaveContent()
    {
        var mdResources = DocsSharedAssembly
            .GetManifestResourceNames()
            .Where(name => name.StartsWith(ResourcePrefix) && name.EndsWith(".md"))
            .ToList();

        Assert.NotEmpty(mdResources);

        foreach (var resourceName in mdResources.Take(5))
        {
            using var stream = DocsSharedAssembly.GetManifestResourceStream(resourceName);
            Assert.NotNull(stream);
            using var reader = new StreamReader(stream);
            var content = reader.ReadToEnd();
            Assert.False(string.IsNullOrWhiteSpace(content), $"Resource {resourceName} should have content");
        }
    }
}

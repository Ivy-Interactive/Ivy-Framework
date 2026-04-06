using System.Reflection;

namespace Ivy.Docs.Test;

public class EmbeddedApiDocTests
{
    private static readonly Assembly DocsSharedAssembly = typeof(Ivy.Docs.Shared.Middleware.MarkdownMiddlewareExtensions).Assembly;
    private const string ResourcePrefix = "Ivy.Docs.Shared.Generated.";

    [Fact]
    public void WidgetMarkdownResources_ShouldContainApiDocSections()
    {
        // Find embedded .md resources that correspond to widget documentation pages
        var widgetResources = DocsSharedAssembly
            .GetManifestResourceNames()
            .Where(name => name.StartsWith(ResourcePrefix) && name.EndsWith(".md"))
            .Where(name => name.Contains("Widgets"))
            .ToList();

        Assert.NotEmpty(widgetResources);

        var resourcesWithApiSection = 0;
        var resourcesWithPlaceholder = new List<string>();

        foreach (var resourceName in widgetResources)
        {
            using var stream = DocsSharedAssembly.GetManifestResourceStream(resourceName);
            if (stream == null) continue;
            using var reader = new StreamReader(stream);
            var content = reader.ReadToEnd();

            if (content.Contains("## API"))
            {
                resourcesWithApiSection++;

                // Check if it still has the placeholder instead of real docs
                if (content.Contains("(See widget documentation for"))
                {
                    resourcesWithPlaceholder.Add(resourceName);
                }
            }
        }

        Assert.True(resourcesWithApiSection > 0, "At least one widget resource should have an ## API section");

        // The key assertion: no resources should have the placeholder text
        Assert.True(
            resourcesWithPlaceholder.Count == 0,
            $"The following resources still contain placeholder API docs instead of generated content:\n{string.Join("\n", resourcesWithPlaceholder)}"
        );
    }
}

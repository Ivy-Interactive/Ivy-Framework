using System.Reflection;

namespace Ivy.IvyML.Studio.Helpers;

/// <summary>
/// System prompts injected into the Claude agent running inside the Studio chat terminal.
/// </summary>
public static class StudioPrompts
{
    private const string WireframesDirPlaceholder = "{{WIREFRAMES_DIR}}";

    // The prompt body lives in an embedded markdown file (Prompts/SystemPrompt.md) so it can be
    // edited as prose. Resource names are "<RootNamespace>.<folder>.<file>"; match by suffix to stay
    // robust to namespace/folder changes.
    private const string PromptResourceSuffix = "Prompts.SystemPrompt.md";

    private static readonly Lazy<string> PromptTemplate = new(LoadPromptTemplate);

    /// <summary>
    /// Builds the default system prompt explaining IvyML, the <c>ivyml</c> dev CLI (on PATH via the
    /// Studio shim), and the append-only wireframe workflow rooted at <paramref name="wireframesDir"/>.
    /// Appended to Claude's own system prompt via <c>--append-system-prompt-file</c>.
    /// </summary>
    public static string BuildIvyMl(string wireframesDir) =>
        PromptTemplate.Value.Replace(WireframesDirPlaceholder, wireframesDir);

    private static string LoadPromptTemplate()
    {
        var assembly = typeof(StudioPrompts).Assembly;
        var resourceName = assembly.GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith(PromptResourceSuffix, StringComparison.Ordinal))
            ?? throw new InvalidOperationException(
                $"Embedded prompt resource ending in '{PromptResourceSuffix}' was not found.");

        using var stream = assembly.GetManifestResourceStream(resourceName)!;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}

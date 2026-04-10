using Ivy.Tendril.Apps;

namespace Ivy.Tendril.Services;

public static class FileLinkHelper
{
    private static readonly string[] ImageExtensions = [".png", ".jpg", ".jpeg", ".gif", ".svg", ".webp"];

    public static Action<string> CreateFileLinkClickHandler(IState<string?> openFileState)
    {
        return url =>
        {
            if (url.StartsWith("file:///", StringComparison.OrdinalIgnoreCase))
            {
                var filePath = url.Substring("file:///".Length);
                openFileState.Set(filePath);
            }
        };
    }

    public static object? BuildFileLinkSheet(
        string? filePath,
        Action onClose,
        IEnumerable<string> repoPaths,
        string editorCommand = "code",
        string editorLabel = "VS Code")
    {
        if (filePath is null)
            return null;

        var ext = Path.GetExtension(filePath);
        object sheetContent;

        if (ImageExtensions.Contains(ext, StringComparer.OrdinalIgnoreCase))
        {
            var imageUrl = $"/ivy/local-file?path={Uri.EscapeDataString(filePath)}";
            sheetContent = new Image(imageUrl) { ObjectFit = ImageFit.Contain, Alt = Path.GetFileName(filePath) };
        }
        else
        {
            if (File.Exists(filePath))
            {
                var fileContent = FileHelper.ReadAllText(filePath);
                var language = FileApp.GetLanguage(ext);
                sheetContent = new Markdown($"```{language.ToString().ToLowerInvariant()}\n{fileContent}\n```");
            }
            else
            {
                var fileName = Path.GetFileName(filePath);
                var suggestions = MarkdownHelper.FindFilesInRepos(repoPaths, fileName);
                var content = suggestions.Count > 0
                    ? $"File not found.\n\nDid you mean:\n{string.Join("\n", suggestions.Select(s => $"- `{s}`"))}"
                    : "File not found.";
                sheetContent = new Markdown(content);
            }
        }

        var finalContent = File.Exists(filePath)
            ? new HeaderLayout(
                new Button($"Open in {editorLabel}").Icon(Icons.ExternalLink).Outline().OnClick(() =>
                {
                    PlatformHelper.OpenInEditor(editorCommand, filePath);
                }),
                sheetContent
            )
            : sheetContent;

        return new Sheet(
            onClose,
            finalContent,
            Path.GetFileName(filePath)
        ).Width(Size.Half()).Resizable();
    }
}
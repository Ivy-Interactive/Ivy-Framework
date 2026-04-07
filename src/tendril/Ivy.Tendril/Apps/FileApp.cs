using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps;

public record FileAppArgs(string Url);

[App(title: "File", icon: Icons.File, isVisible: false, allowDuplicateTabs: true)]
public class FileApp : ViewBase
{
    private static readonly HashSet<string> ImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".png", ".jpg", ".jpeg", ".gif", ".bmp", ".svg", ".webp", ".ico"
    };

    private static readonly Dictionary<string, Languages> LanguageMap = new(StringComparer.OrdinalIgnoreCase)
    {
        { ".cs", Languages.Csharp },
        { ".js", Languages.Javascript },
        { ".ts", Languages.Typescript },
        { ".tsx", Languages.Typescript },
        { ".jsx", Languages.Javascript },
        { ".py", Languages.Python },
        { ".sql", Languages.Sql },
        { ".html", Languages.Html },
        { ".htm", Languages.Html },
        { ".css", Languages.Css },
        { ".json", Languages.Json },
        { ".md", Languages.Markdown },
        { ".xml", Languages.Xml },
        { ".yaml", Languages.Yaml },
        { ".yml", Languages.Yaml },
        { ".csv", Languages.Csv },
        { ".txt", Languages.Text },
        { ".log", Languages.Text },
        { ".env", Languages.Text },
        { ".config", Languages.Xml },
        { ".csproj", Languages.Xml },
        { ".sln", Languages.Text },
        { ".ps1", Languages.Powershell },
        { ".psm1", Languages.Powershell },
        { ".sh", Languages.Bash },
        { ".bash", Languages.Bash }
    };

    public static Languages GetLanguage(string extension)
    {
        return LanguageMap.GetValueOrDefault(extension, Languages.Text);
    }

    public override object? Build()
    {
        var args = UseArgs<FileAppArgs>();
        var contentState = UseState("");
        var errorState = UseState<string?>(null);
        var previousUrl = UseRef<string?>(null);

        if (args?.Url is not { } url || string.IsNullOrWhiteSpace(url))
            return Text.P("No file URL provided.");

        var extension = Path.GetExtension(url);
        var isImage = ImageExtensions.Contains(extension);

        // Load file content when URL changes (for text files only)
        if (!isImage && previousUrl.Value != url)
        {
            try
            {
                var content = FileHelper.ReadAllText(url);
                contentState.Set(content);
                errorState.Set(null);
            }
            catch (Exception ex)
            {
                errorState.Set($"Failed to read file: {ex.Message}");
            }

            previousUrl.Value = url;
        }

        // Show error if file read failed
        if (errorState.Value != null)
            return Text.P(errorState.Value);

        // Display image
        if (isImage)
            return new Image(url)
            {
                ObjectFit = ImageFit.Contain,
                Alt = Path.GetFileName(url)
            };

        // Display code editor
        var language = GetLanguage(extension);
        return contentState.ToCodeInput(language: language)
            .Height(Size.Full());
    }
}
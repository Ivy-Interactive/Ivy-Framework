using System.Reflection;
using YamlDotNet.Serialization;

namespace Ivy.IvyML;

/// <summary>
/// Reads an Ivy <see cref="Theme"/> from YAML.
///
/// The mapping is deliberately forgiving: keys are matched case-insensitively and ignoring
/// <c>-</c> and <c>_</c>, so <c>fontFamily</c>, <c>font-family</c> and <c>font_family</c> are the
/// same key. Anything the file leaves out stays null on the theme, which means the framework's own
/// defaults still apply -- a theme file only has to state what it changes.
/// </summary>
public static class ThemeLoader
{
    private const string BuiltInResourcePrefix = "Ivy.IvyML.Themes.";

    /// <summary>
    /// Resolves a <c>--theme</c> value: a path to a YAML file, or the name of a theme built into
    /// the CLI (see <see cref="BuiltInNames"/>).
    /// </summary>
    public static Theme Load(string pathOrName)
    {
        if (string.IsNullOrWhiteSpace(pathOrName))
            throw new ArgumentException("Theme must be a file path or a built-in theme name.", nameof(pathOrName));

        if (File.Exists(pathOrName))
        {
            var fullPath = Path.GetFullPath(pathOrName);
            return Parse(File.ReadAllText(fullPath), Path.GetDirectoryName(fullPath));
        }

        var builtIn = ReadBuiltIn(pathOrName);
        if (builtIn != null)
            return Parse(builtIn, baseDirectory: null);

        throw new FileNotFoundException(
            $"Theme '{pathOrName}' is neither an existing file nor a built-in theme "
            + $"({string.Join(", ", BuiltInNames())}).");
    }

    /// <summary>Names of the themes shipped inside the CLI, usable directly as <c>--theme</c>.</summary>
    public static IEnumerable<string> BuiltInNames() =>
        typeof(ThemeLoader).Assembly.GetManifestResourceNames()
            .Where(n => n.StartsWith(BuiltInResourcePrefix, StringComparison.Ordinal)
                        && n.EndsWith(".yaml", StringComparison.Ordinal))
            .Select(n => n[BuiltInResourcePrefix.Length..^".yaml".Length])
            .OrderBy(n => n, StringComparer.Ordinal);

    private static string? ReadBuiltIn(string name)
    {
        var resource = BuiltInResourcePrefix + name.Trim() + ".yaml";
        var assembly = typeof(ThemeLoader).Assembly;
        var match = assembly.GetManifestResourceNames()
            .FirstOrDefault(n => string.Equals(n, resource, StringComparison.OrdinalIgnoreCase));
        if (match is null) return null;

        using var stream = assembly.GetManifestResourceStream(match)!;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    /// <param name="baseDirectory">
    /// Directory that relative font paths in the document resolve against -- normally the folder the
    /// YAML file itself lives in.
    /// </param>
    public static Theme Parse(string yaml, string? baseDirectory = null)
    {
        var root = new DeserializerBuilder().Build().Deserialize<object?>(yaml);
        if (root is null) return new Theme();

        if (root is not IDictionary<object, object> map)
            throw new InvalidDataException("A theme file must be a YAML mapping of theme properties.");

        // Resolved up front rather than in key order, so `extends` works wherever it is written.
        var baseName = map.FirstOrDefault(e => Normalize(e.Key) is "extends" or "base").Value;
        var theme = baseName is null ? new Theme() : Load(Scalar(baseName)!);

        foreach (var (rawKey, value) in map)
        {
            var key = Normalize(rawKey);
            switch (key)
            {
                case "extends":
                case "base": break; // already applied above

                case "name": theme.Name = Scalar(value) ?? theme.Name; break;

                case "font":
                case "fontfamily":
                case "fontfamilysans": theme.FontFamily = Scalar(value); break;
                case "fontfamilymono": theme.FontFamilyMono = Scalar(value); break;
                case "fontfamilyserif": theme.FontFamilySerif = Scalar(value); break;
                case "fontsize": theme.FontSize = Scalar(value); break;
                case "fontfaces": theme.FontFaces = ReadFontFaces(value, baseDirectory); break;

                case "borderradius": ApplyBorderRadius(theme, value); break;
                case "borderradiusboxes": theme.BorderRadiusBoxes = Scalar(value); break;
                case "borderradiusfields": theme.BorderRadiusFields = Scalar(value); break;
                case "borderradiusselectors": theme.BorderRadiusSelectors = Scalar(value); break;

                case "shadow":
                case "shadows": ApplyShadows(theme, value); break;
                case "shadowboxes": theme.ShadowBoxes = Bool(value); break;
                case "shadowfields": theme.ShadowFields = Bool(value); break;
                case "shadowselectors": theme.ShadowSelectors = Bool(value); break;

                case "colors": ApplyColors(theme, value); break;

                default:
                    throw new InvalidDataException($"Unknown theme property '{rawKey}'.");
            }
        }

        return theme;
    }

    private static void ApplyBorderRadius(Theme theme, object? value)
    {
        // A scalar sets every group at once; a mapping addresses them individually.
        if (value is IDictionary<object, object> map)
        {
            foreach (var (rawKey, v) in map)
            {
                switch (Normalize(rawKey))
                {
                    case "boxes": theme.BorderRadiusBoxes = Scalar(v); break;
                    case "fields": theme.BorderRadiusFields = Scalar(v); break;
                    case "selectors": theme.BorderRadiusSelectors = Scalar(v); break;
                    default: throw new InvalidDataException($"Unknown borderRadius key '{rawKey}'. Expected boxes, fields or selectors.");
                }
            }
            return;
        }

        var all = Scalar(value);
        theme.BorderRadiusBoxes = all;
        theme.BorderRadiusFields = all;
        theme.BorderRadiusSelectors = all;
    }

    private static void ApplyShadows(Theme theme, object? value)
    {
        if (value is IDictionary<object, object> map)
        {
            foreach (var (rawKey, v) in map)
            {
                switch (Normalize(rawKey))
                {
                    case "boxes": theme.ShadowBoxes = Bool(v); break;
                    case "fields": theme.ShadowFields = Bool(v); break;
                    case "selectors": theme.ShadowSelectors = Bool(v); break;
                    default: throw new InvalidDataException($"Unknown shadows key '{rawKey}'. Expected boxes, fields or selectors.");
                }
            }
            return;
        }

        var all = Bool(value);
        theme.ShadowBoxes = all;
        theme.ShadowFields = all;
        theme.ShadowSelectors = all;
    }

    private static void ApplyColors(Theme theme, object? value)
    {
        if (value is not IDictionary<object, object> map)
            throw new InvalidDataException("'colors' must be a mapping with 'light' and/or 'dark' sections.");

        foreach (var (rawKey, v) in map)
        {
            switch (Normalize(rawKey))
            {
                case "light": ApplyColorScheme(theme.Colors.Light, v); break;
                case "dark": ApplyColorScheme(theme.Colors.Dark, v); break;
                default:
                    throw new InvalidDataException($"Unknown colors key '{rawKey}'. Expected light or dark.");
            }
        }
    }

    /// <summary>
    /// <see cref="ThemeColors"/> is a flat bag of ~25 nullable strings, so reflecting over it keeps
    /// this in step with the framework instead of duplicating the list here.
    /// </summary>
    private static readonly Dictionary<string, PropertyInfo> ColorProperties =
        typeof(ThemeColors).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite && p.PropertyType == typeof(string))
            .ToDictionary(p => Normalize(p.Name));

    private static void ApplyColorScheme(ThemeColors colors, object? value)
    {
        if (value is not IDictionary<object, object> map)
            throw new InvalidDataException("A colors section must be a mapping of color names to values.");

        foreach (var (rawKey, v) in map)
        {
            if (!ColorProperties.TryGetValue(Normalize(rawKey), out var property))
                throw new InvalidDataException($"Unknown color '{rawKey}'.");
            property.SetValue(colors, Scalar(v));
        }
    }

    private static List<ThemeFontFace> ReadFontFaces(object? value, string? baseDirectory)
    {
        if (value is not IEnumerable<object> items)
            throw new InvalidDataException("'fontFaces' must be a list of font face mappings.");

        var faces = new List<ThemeFontFace>();
        foreach (var item in items)
        {
            if (item is not IDictionary<object, object> map)
                throw new InvalidDataException("Each entry under 'fontFaces' must be a mapping.");

            var face = new ThemeFontFace();
            foreach (var (rawKey, v) in map)
            {
                switch (Normalize(rawKey))
                {
                    case "family": face.Family = Scalar(v) ?? ""; break;
                    case "src":
                    case "url": face.Src = ResolveSrc(Scalar(v) ?? "", baseDirectory); break;
                    case "weight":
                    case "fontweight": face.Weight = Scalar(v); break;
                    case "style":
                    case "fontstyle": face.Style = Scalar(v); break;
                    case "display":
                    case "fontdisplay": face.Display = Scalar(v); break;
                    case "unicoderange": face.UnicodeRange = Scalar(v); break;
                    default: throw new InvalidDataException($"Unknown font face property '{rawKey}'.");
                }
            }

            if (string.IsNullOrWhiteSpace(face.Family) || string.IsNullOrWhiteSpace(face.Src))
                throw new InvalidDataException("Each entry under 'fontFaces' needs both 'family' and 'src'.");

            faces.Add(face);
        }

        return faces;
    }

    private static readonly Dictionary<string, string> FontMimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        [".woff2"] = "font/woff2",
        [".woff"] = "font/woff",
        [".ttf"] = "font/ttf",
        [".otf"] = "font/otf",
    };

    /// <summary>
    /// Leaves URLs, data URIs and complete CSS <c>src</c> values alone, and inlines a local font
    /// file as a data URI so a theme that ships its own font renders without network access.
    /// </summary>
    private static string ResolveSrc(string src, string? baseDirectory)
    {
        var trimmed = src.Trim();
        if (trimmed.Length == 0) return trimmed;

        if (trimmed.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || trimmed.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
            || trimmed.StartsWith("data:", StringComparison.OrdinalIgnoreCase)
            || trimmed.StartsWith("//", StringComparison.Ordinal)
            || trimmed.StartsWith("/", StringComparison.Ordinal)
            || trimmed.Contains("url(", StringComparison.OrdinalIgnoreCase)
            || trimmed.StartsWith("local(", StringComparison.OrdinalIgnoreCase))
            return trimmed;

        var path = Path.IsPathRooted(trimmed) || baseDirectory is null
            ? trimmed
            : Path.Combine(baseDirectory, trimmed);

        if (!File.Exists(path))
            throw new FileNotFoundException($"Font file not found: {trimmed}");

        var ext = Path.GetExtension(path);
        if (!FontMimeTypes.TryGetValue(ext, out var mime))
            throw new InvalidDataException($"Unsupported font file '{trimmed}'. Supported: {string.Join(", ", FontMimeTypes.Keys)}.");

        return $"data:{mime};base64,{Convert.ToBase64String(File.ReadAllBytes(path))}";
    }

    private static string Normalize(object? key) =>
        (key?.ToString() ?? "").Replace("-", "").Replace("_", "").ToLowerInvariant();

    private static string? Scalar(object? value) => value switch
    {
        null => null,
        string s => s,
        _ => Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture),
    };

    private static bool Bool(object? value)
    {
        var text = Scalar(value);
        if (bool.TryParse(text, out var parsed)) return parsed;
        throw new InvalidDataException($"Expected true or false, got '{text}'.");
    }
}

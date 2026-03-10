namespace Ivy.Core.Server;

public class ManifestOptions
{
    public string Name { get; set; } = "Ivy App";
    public string ShortName { get; set; } = "Ivy";
    public string StartUrl { get; set; } = "/";
    public string Display { get; set; } = "standalone";
    public string BackgroundColor { get; set; } = "#ffffff";
    public string ThemeColor { get; set; } = "#000000";
    public List<ManifestIcon>? Icons { get; set; }

    public object ToManifest() => new
    {
        name = Name,
        short_name = ShortName,
        start_url = StartUrl,
        display = Display,
        background_color = BackgroundColor,
        theme_color = ThemeColor,
        icons = Icons?.Select(i => new
        {
            src = i.Src,
            sizes = i.Sizes,
            type = i.Type
        })
    };
}

public class ManifestIcon
{
    public string Src { get; set; } = "";
    public string Sizes { get; set; } = "";
    public string Type { get; set; } = "image/png";
}

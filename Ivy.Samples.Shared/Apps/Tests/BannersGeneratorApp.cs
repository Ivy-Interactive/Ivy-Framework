using Ivy.Shared;
using Ivy.Widgets.Inputs;

namespace Ivy.Samples.Shared.Apps.Tests;

[App(icon: Icons.Image, path: ["Tests"], isVisible: true)]
public class LongSidebarTestApp : SampleBase
{
    protected override object? BuildSample()
    {
        // State management for all form inputs
        var endpoint = UseState(() => "https://automation-ivy-staging.azurewebsites.net/api/BannerFunction");
        var text = UseState(() => "Hello World");
        var width = UseState<int?>((int?)null, false);
        var height = UseState<int?>((int?)null, false);
        var widthHeightPreset = UseState<string?>((string?)null, false);
        var showLogo = UseState(() => true);
        var seed = UseState<int?>(() => 1);
        var primaryBackgroundColor = UseState<string?>((string?)null, false);
        var secondaryBackgroundColor = UseState<string?>((string?)null, false);
        var textColor = UseState<string?>((string?)null, false);
        var bannerRadius = UseState<int?>(() => 15);
        var targetQuadrantSize = UseState<int?>(() => 38);
        var theme = UseState(() => "Dark");
        var fileFormat = UseState(() => "Svg");

        // Sidebar content with form inputs
        var sidebarContent = Layout.Vertical(
            // Section header
            Text.H3("Long Sidebar Test"),

            // Endpoint
            Layout.Vertical(
                Text.Label("Endpoint *"),
                endpoint.ToTextInput("API Endpoint")
            ),

            // Text
            Layout.Vertical(
                Text.Label("Text"),
                text.ToTextInput("Banner text")
            ),

            // Width & Height in same row
            Layout.Horizontal(
                Layout.Vertical(
                    Text.Label("Width"),
                    width.ToInput("Width")
                ).Width(Size.Grow()),
                Layout.Vertical(
                    Text.Label("Height"),
                    height.ToInput("Height")
                ).Width(Size.Grow())
            ).Gap(4),

            // Width Height Preset
            Layout.Vertical(
                Text.Label("Width Height Preset"),
                widthHeightPreset.ToTextInput("e.g., 1920x1080")
            ),

            // Show Logo
            showLogo.ToBoolInput("Show Logo").Variant(BoolInputs.Checkbox),

            // Seed
            Layout.Vertical(
                Text.Label("Seed"),
                seed.ToInput("Random seed")
            ),

            // Primary Background Color
            Layout.Vertical(
                Text.Label("Primary Background Color"),
                primaryBackgroundColor.ToColorInput("#000000")
            ),

            // Secondary Background Color
            Layout.Vertical(
                Text.Label("Secondary Background Color"),
                secondaryBackgroundColor.ToColorInput("#333333")
            ),

            // Text Color
            Layout.Vertical(
                Text.Label("Text Color"),
                textColor.ToColorInput("#ffffff")
            ),

            // Banner Radius & Target Quadrant Size in same row
            Layout.Horizontal(
                Layout.Vertical(
                    Text.Label("Banner Radius"),
                    bannerRadius.ToInput("Border radius")
                ).Width(Size.Grow()),
                Layout.Vertical(
                    Text.Label("Target Quadrant Size"),
                    targetQuadrantSize.ToInput("Quadrant size")
                ).Width(Size.Grow())
            ).Gap(4),

            // Theme
            Layout.Vertical(
                Text.Label("Theme"),
                theme.ToSelectInput(new IAnyOption[]
                {
                    new Option<string>("Dark", "Dark"),
                    new Option<string>("Light", "Light")
                })
            ),

            // File Format
            Layout.Vertical(
                Text.Label("File Format"),
                fileFormat.ToSelectInput(new IAnyOption[]
                {
                    new Option<string>("Svg", "Svg"),
                    new Option<string>("Png", "Png"),
                    new Option<string>("Jpg", "Jpg")
                })
            )
        ).Gap(4);

        // Preview section on the right  
        var bannerUrl = GetBannerUrl(endpoint.Value, text.Value, width.Value, height.Value,
                                   widthHeightPreset.Value, showLogo.Value, seed.Value,
                                   primaryBackgroundColor.Value, secondaryBackgroundColor.Value,
                                   textColor.Value, bannerRadius.Value, targetQuadrantSize.Value,
                                   theme.Value, fileFormat.Value);

        // Main content area with preview
        var mainContent = Layout.Vertical(
            Text.H1("Long Sidebar Test"),

            // URL display (copyable)
            Layout.Vertical(
                Text.Label("Generated URL"),
                Text.Code(bannerUrl)
            ),

            // Preview image
            fileFormat.Value == "Svg" ?
                new Image(bannerUrl)
                : Text.Muted("Preview available for SVG format only"),

            // Download button
            new Button("Download Banner", onClick: _ =>
                {
                    // Generate download URL with current parameters
                    var downloadUrl = GetBannerUrl(endpoint.Value, text.Value, width.Value, height.Value,
                                                widthHeightPreset.Value, showLogo.Value, seed.Value,
                                                primaryBackgroundColor.Value, secondaryBackgroundColor.Value,
                                                textColor.Value, bannerRadius.Value, targetQuadrantSize.Value,
                                                theme.Value, fileFormat.Value);
                    // Open URL in new tab for download
                    UseService<IClientProvider>().OpenUrl(downloadUrl);
                }
            ).Icon(Icons.Download).Variant(ButtonVariant.Primary),

            Text.P("egg"),
            Text.P("egg"),
            Text.P("egg"),
            Text.P("egg"),
            Text.P("egg"),
            Text.P("egg"),
            Text.P("egg"),
            Text.P("egg"),
            Text.P("egg"),
            Text.P("egg"),
            Text.P("egg"),
            Text.P("egg"),
            Text.P("egg"),
            Text.P("egg"),
            Text.P("egg")
        ).Gap(6);

        // Return SidebarLayout with form inputs in sidebar and preview in main content
        return new SidebarLayout(
            mainContent,
            sidebarContent,
            sidebarHeader: null,
            sidebarFooter: null
        );
    }

    private string GetBannerUrl(string endpoint, string text, int? width, int? height,
                              string? widthHeightPreset, bool showLogo, int? seed,
                              string? primaryColor, string? secondaryColor, string? textColor,
                              int? bannerRadius, int? targetQuadrantSize, string theme, string fileFormat)
    {
        var queryParams = new List<string>();

        if (!string.IsNullOrEmpty(text))
            queryParams.Add($"text={Uri.EscapeDataString(text)}");

        if (width.HasValue)
            queryParams.Add($"width={width}");

        if (height.HasValue)
            queryParams.Add($"height={height}");

        if (!string.IsNullOrEmpty(widthHeightPreset))
            queryParams.Add($"widthHeightPreset={Uri.EscapeDataString(widthHeightPreset)}");

        queryParams.Add($"showLogo={showLogo.ToString().ToLower()}");

        if (seed.HasValue)
            queryParams.Add($"seed={seed}");

        if (!string.IsNullOrEmpty(primaryColor))
            queryParams.Add($"primaryBackgroundColor={Uri.EscapeDataString(primaryColor)}");

        if (!string.IsNullOrEmpty(secondaryColor))
            queryParams.Add($"secondaryBackgroundColor={Uri.EscapeDataString(secondaryColor)}");

        if (!string.IsNullOrEmpty(textColor))
            queryParams.Add($"textColor={Uri.EscapeDataString(textColor)}");

        if (bannerRadius.HasValue)
            queryParams.Add($"bannerRadius={bannerRadius}");

        if (targetQuadrantSize.HasValue)
            queryParams.Add($"targetQuadrantSize={targetQuadrantSize}");

        queryParams.Add($"theme={Uri.EscapeDataString(theme)}");
        queryParams.Add($"fileFormat={Uri.EscapeDataString(fileFormat)}");

        var queryString = string.Join("&", queryParams);
        return $"{endpoint}?{queryString}";
    }
}

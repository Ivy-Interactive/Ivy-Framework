using System.Collections.Immutable;
using Ivy.Widgets.Internal;

namespace Ivy.Samples.Shared.Apps.Demos;

[App(icon: Icons.Palette, searchHints: ["theming", "customization", "branding", "styling", "appearance", "design"])]
public class ThemeCustomizer : SampleBase
{
    protected override object? BuildSample()
    {
        var currentTheme = UseState(Theme.Default);
        var isExportOpen = UseState(false);
        var client = UseService<IClientProvider>();
        var selectedMode = UseState("light"); // "light" or "dark"
        var editingTheme = UseState(CloneTheme(Theme.Default));
        var themeQuery = UseQuery(
            key: editingTheme.Value,
            fetcher: async ct =>
            {
                var themeService = new ThemeService();
                themeService.SetTheme(editingTheme.Value);
                var css = themeService.GenerateThemeCss();
                client.ApplyTheme(css);
                await Task.CompletedTask;
                return true;
            });

        void LoadPreset(Theme preset)
        {
            editingTheme.Set(CloneTheme(preset));
            client.Toast($"Loaded {preset.Name} theme", "Theme");
        }

        var presets = new Dictionary<string, Theme>
        {
            ["Default"] = Theme.Default,
            ["Ocean"] = GetOceanTheme(),
            ["Forest"] = GetForestTheme(),
            ["Sunset"] = GetSunsetTheme(),
            ["Midnight"] = GetMidnightTheme()
        };

        // Sidebar Content (Presets, Mode, Colors)
        var editorContent = new ThemeSidebarContent(editingTheme, selectedMode, presets, LoadPreset);

        // Sidebar Header
        var sidebarHeader = Layout.Vertical()
            | Text.H2("Theme Editor")
            | Text.P("Customize your theme").Small().Muted();

        // Sidebar Footer
        var sidebarFooter = Layout.Vertical()
            | new Button("Copy Configuration")
                .Primary()
                .Icon(Icons.Copy)
                .OnClick(() => isExportOpen.Set(true))
                .Width(Size.Full());

        // Right side - Live Preview
        var previewPanel = new LivePreviewPanel(editingTheme.Value);

        // Export dialog
        var exportDialog = isExportOpen.Value
            ? new Dialog(
                _ => isExportOpen.Set(false),
                new DialogHeader("Export Theme Configuration"),
                new DialogBody(
                    Layout.Tabs(
                        new Tab(
                            "C#",
                            Layout.Vertical()
                                | Text.P("Copy this C# configuration into your server setup.").Small()
                                | new CodeBlock(GenerateCSharpCode(editingTheme.Value), Languages.Csharp)
                                | new Button("Copy C# Code")
                                    .Primary()
                                    .Icon(Icons.ClipboardCopy, Align.Right)
                                    .OnClick(() =>
                                    {
                                        client.CopyToClipboard(GenerateCSharpCode(editingTheme.Value));
                                        client.Toast("C# theme configuration copied to clipboard!", "Export");
                                    })
                        ).Icon(Icons.Code),
                        new Tab(
                            "JSON",
                            Layout.Vertical()
                                | Text.P("Use this JSON to persist or share the theme.").Small()
                                | new CodeBlock(System.Text.Json.JsonSerializer.Serialize(
                                        editingTheme.Value,
                                        new System.Text.Json.JsonSerializerOptions { WriteIndented = true }),
                                    Languages.Json)
                                | new Button("Copy JSON")
                                    .Primary()
                                    .Icon(Icons.ClipboardCopy, Align.Right)
                                    .OnClick(() =>
                                    {
                                        var json = System.Text.Json.JsonSerializer.Serialize(
                                            editingTheme.Value,
                                            new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                                        client.CopyToClipboard(json);
                                        client.Toast("JSON theme configuration copied to clipboard!", "Export");
                                    })
                        ).Icon(Icons.FileBraces)
                    )
                ),
                new DialogFooter(
                    new Button("Close", _ => isExportOpen.Set(false), variant: ButtonVariant.Secondary)
                )
            ).Width(Size.Units(150))
            : null;

        return new SidebarLayout(
                mainContent: Layout.Vertical()
                    | previewPanel
                    | exportDialog,
                sidebarContent: editorContent,
                sidebarHeader: sidebarHeader,
                sidebarFooter: sidebarFooter,
                width: Size.Px(380)
            ).Height(Size.Full());
    }

    private static Theme CloneTheme(Theme source)
    {
        return new Theme
        {
            Name = source.Name,
            FontFamily = source.FontFamily,
            FontSize = source.FontSize,
            BorderRadiusBoxes = source.BorderRadiusBoxes,
            BorderRadiusFields = source.BorderRadiusFields,
            BorderRadiusSelectors = source.BorderRadiusSelectors,
            Colors = new ThemeColorScheme
            {
                Light = CloneThemeColors(source.Colors.Light),
                Dark = CloneThemeColors(source.Colors.Dark)
            }
        };
    }

    private static ThemeColors CloneThemeColors(ThemeColors source)
    {
        return new ThemeColors
        {
            Primary = source.Primary,
            PrimaryForeground = source.PrimaryForeground,
            Secondary = source.Secondary,
            SecondaryForeground = source.SecondaryForeground,
            Background = source.Background,
            Foreground = source.Foreground,
            Destructive = source.Destructive,
            DestructiveForeground = source.DestructiveForeground,
            Success = source.Success,
            SuccessForeground = source.SuccessForeground,
            Warning = source.Warning,
            WarningForeground = source.WarningForeground,
            Info = source.Info,
            InfoForeground = source.InfoForeground,
            Border = source.Border,
            Input = source.Input,
            Ring = source.Ring,
            Muted = source.Muted,
            MutedForeground = source.MutedForeground,
            Accent = source.Accent,
            AccentForeground = source.AccentForeground,
            Card = source.Card,
            CardForeground = source.CardForeground,
            Popover = source.Popover,
            PopoverForeground = source.PopoverForeground
        };
    }

    /// <summary>
    /// Sidebar content with presets, mode toggle, and color editors
    /// </summary>
    private class ThemeSidebarContent(
        IState<Theme> editingTheme,
        IState<string> selectedMode,
        Dictionary<string, Theme> presets,
        Action<Theme> loadPreset) : ViewBase
    {
        public override object Build()
        {
            var client = UseService<IClientProvider>();
            var selectedPreset = UseState(editingTheme.Value.Name);
            var currentColors = selectedMode.Value == "light"
                ? editingTheme.Value.Colors.Light
                : editingTheme.Value.Colors.Dark;

            void UpdateColor(Action<ThemeColors> updater)
            {
                var newTheme = CloneTheme(editingTheme.Value);
                var colors = selectedMode.Value == "light" ? newTheme.Colors.Light : newTheme.Colors.Dark;
                updater(colors);
                editingTheme.Set(newTheme);
            }


            void UpdateThemeProperty(Action<Theme> updater)
            {
                var newTheme = CloneTheme(editingTheme.Value);
                updater(newTheme);
                editingTheme.Set(newTheme);
            }

            var presetOptions = presets.Select(kv => new Option<string>(kv.Key, kv.Key)).ToArray();

            return Layout.Vertical()
                | Text.H3("Theme Preset").Small()
                | new SelectInput<string>(
                    value: selectedPreset.Value,
                    onChange: e =>
                    {
                        selectedPreset.Set(e.Value);
                        if (presets.TryGetValue(e.Value, out var preset))
                        {
                            loadPreset(preset);
                        }
                    },
                    options: presetOptions
                )
                | new Separator()
                // Mode toggle
                | Text.H3("Theme Mode").Small()
                | (Layout.Horizontal()
                    | new Button("Light")
                        .Variant(selectedMode.Value == "light" ? ButtonVariant.Primary : ButtonVariant.Outline)
                        .Icon(Icons.Sun)
                        .OnClick(() =>
                        {
                            selectedMode.Set("light");
                            client.SetThemeMode(ThemeMode.Light);
                        })
                        .Width(Size.Full())
                    | new Button("Dark")
                        .Variant(selectedMode.Value == "dark" ? ButtonVariant.Primary : ButtonVariant.Outline)
                        .Icon(Icons.Moon)
                        .OnClick(() =>
                        {
                            selectedMode.Set("dark");
                            client.SetThemeMode(ThemeMode.Dark);
                        })
                        .Width(Size.Full()))

                | new Separator()

                | new Expandable(
                    header: Text.Block("Colors").Bold(),
                    content: Layout.Vertical()
                        | Text.Block("Main Colors").Small()
                        | (Layout.Grid().Columns(4).Gap(2)
                            | new ThemeColorPicker(currentColors.Primary ?? "#000000", e => UpdateColor(c => c.Primary = e.Value), placeholder: "Primary").AllowAlpha().WithField().Medium().Description("Primary").WithTooltip($"Primary: {currentColors.Primary ?? "#000000"}")
                            | new ThemeColorPicker(currentColors.PrimaryForeground ?? "#000000", e => UpdateColor(c => c.PrimaryForeground = e.Value), placeholder: "Primary Foreground").Foreground(true).AllowAlpha().WithField().Medium().Description("\u00A0").WithTooltip($"Primary Foreground: {currentColors.PrimaryForeground ?? "#000000"}")
                            | new ThemeColorPicker(currentColors.Secondary ?? "#000000", e => UpdateColor(c => c.Secondary = e.Value), placeholder: "Secondary").AllowAlpha().WithField().Medium().Description("Secondary").WithTooltip($"Secondary: {currentColors.Secondary ?? "#000000"}")
                            | new ThemeColorPicker(currentColors.SecondaryForeground ?? "#000000", e => UpdateColor(c => c.SecondaryForeground = e.Value), placeholder: "Secondary Foreground").Foreground(true).AllowAlpha().WithField().Medium().Description("\u00A0").WithTooltip($"Secondary Foreground: {currentColors.SecondaryForeground ?? "#000000"}")
                            | new ThemeColorPicker(currentColors.Background ?? "#000000", e => UpdateColor(c => c.Background = e.Value), placeholder: "Background").AllowAlpha().WithField().Medium().Description("Background").WithTooltip($"Background: {currentColors.Background ?? "#000000"}")
                            | new ThemeColorPicker(currentColors.Foreground ?? "#000000", e => UpdateColor(c => c.Foreground = e.Value), placeholder: "Foreground").Foreground(true).AllowAlpha().WithField().Medium().Description("\u00A0").WithTooltip($"Foreground: {currentColors.Foreground ?? "#000000"}"))

                        | new Separator()

                        | Text.Block("Semantic Colors").Small()
                        | (Layout.Grid().Columns(4).Gap(2)
                            | new ThemeColorPicker(currentColors.Success ?? "#000000", e => UpdateColor(c => c.Success = e.Value), placeholder: "Success").AllowAlpha().WithField().Medium().Description("Success").WithTooltip($"Success: {currentColors.Success ?? "#000000"}")
                            | new ThemeColorPicker(currentColors.SuccessForeground ?? "#000000", e => UpdateColor(c => c.SuccessForeground = e.Value), placeholder: "Success Foreground").Foreground(true).AllowAlpha().WithField().Medium().Description("\u00A0").WithTooltip($"Success Foreground: {currentColors.SuccessForeground ?? "#000000"}")
                            | new ThemeColorPicker(currentColors.Destructive ?? "#000000", e => UpdateColor(c => c.Destructive = e.Value), placeholder: "Destructive").AllowAlpha().WithField().Medium().Description("Destructive").WithTooltip($"Destructive: {currentColors.Destructive ?? "#000000"}")
                            | new ThemeColorPicker(currentColors.DestructiveForeground ?? "#000000", e => UpdateColor(c => c.DestructiveForeground = e.Value), placeholder: "Destructive Foreground").Foreground(true).AllowAlpha().WithField().Medium().Description("\u00A0").WithTooltip($"Destructive Foreground: {currentColors.DestructiveForeground ?? "#000000"}")
                            | new ThemeColorPicker(currentColors.Warning ?? "#000000", e => UpdateColor(c => c.Warning = e.Value), placeholder: "Warning").AllowAlpha().WithField().Medium().Description("Warning").WithTooltip($"Warning: {currentColors.Warning ?? "#000000"}")
                            | new ThemeColorPicker(currentColors.WarningForeground ?? "#000000", e => UpdateColor(c => c.WarningForeground = e.Value), placeholder: "Warning Foreground").Foreground(true).AllowAlpha().WithField().Medium().Description("\u00A0").WithTooltip($"Warning Foreground: {currentColors.WarningForeground ?? "#000000"}")
                            | new ThemeColorPicker(currentColors.Info ?? "#000000", e => UpdateColor(c => c.Info = e.Value), placeholder: "Info").AllowAlpha().WithField().Medium().Description("Info").WithTooltip($"Info: {currentColors.Info ?? "#000000"}")
                            | new ThemeColorPicker(currentColors.InfoForeground ?? "#000000", e => UpdateColor(c => c.InfoForeground = e.Value), placeholder: "Info Foreground").Foreground(true).AllowAlpha().WithField().Medium().Description("\u00A0").WithTooltip($"Info Foreground: {currentColors.InfoForeground ?? "#000000"}")
      )

                        | new Separator()

                        | Text.Block("UI Element Colors").Small()
                        | (Layout.Grid().Columns(4).Gap(2)
                            | new ThemeColorPicker(currentColors.Muted ?? "#000000", e => UpdateColor(c => c.Muted = e.Value), placeholder: "Muted").AllowAlpha().WithField().Medium().Description("Muted").WithTooltip($"Muted: {currentColors.Muted ?? "#000000"}")
                            | new ThemeColorPicker(currentColors.MutedForeground ?? "#000000", e => UpdateColor(c => c.MutedForeground = e.Value), placeholder: "Muted Foreground").Foreground(true).AllowAlpha().WithField().Medium().Description("\u00A0").WithTooltip($"Muted Foreground: {currentColors.MutedForeground ?? "#000000"}")
                            | new ThemeColorPicker(currentColors.Accent ?? "#000000", e => UpdateColor(c => c.Accent = e.Value), placeholder: "Accent").AllowAlpha().WithField().Medium().Description("Accent").WithTooltip($"Accent: {currentColors.Accent ?? "#000000"}")
                            | new ThemeColorPicker(currentColors.AccentForeground ?? "#000000", e => UpdateColor(c => c.AccentForeground = e.Value), placeholder: "Accent Foreground").Foreground(true).AllowAlpha().WithField().Medium().Description("\u00A0").WithTooltip($"Accent Foreground: {currentColors.AccentForeground ?? "#000000"}")
                            | new ThemeColorPicker(currentColors.Card ?? "#000000", e => UpdateColor(c => c.Card = e.Value), placeholder: "Card").AllowAlpha().WithField().Medium().Description("Card").WithTooltip($"Card: {currentColors.Card ?? "#000000"}")
                            | new ThemeColorPicker(currentColors.CardForeground ?? "#000000", e => UpdateColor(c => c.CardForeground = e.Value), placeholder: "Card Foreground").Foreground(true).AllowAlpha().WithField().Medium().Description("\u00A0").WithTooltip($"Card Foreground: {currentColors.CardForeground ?? "#000000"}")
                            | new ThemeColorPicker(currentColors.Popover ?? "#000000", e => UpdateColor(c => c.Popover = e.Value), placeholder: "Popover").AllowAlpha().WithField().Medium().Description("Popover").WithTooltip($"Popover: {currentColors.Popover ?? "#000000"}")
                            | new ThemeColorPicker(currentColors.PopoverForeground ?? "#000000", e => UpdateColor(c => c.PopoverForeground = e.Value), placeholder: "Popover Foreground").Foreground(true).AllowAlpha().WithField().Medium().Description("\u00A0").WithTooltip($"Popover Foreground: {currentColors.PopoverForeground ?? "#000000"}")
                            | new ThemeColorPicker(currentColors.Border ?? "#000000", e => UpdateColor(c => c.Border = e.Value), placeholder: "Border").AllowAlpha().WithField().Medium().Description("Border").WithTooltip($"Border: {currentColors.Border ?? "#000000"}")
                            | new ThemeColorPicker(currentColors.Input ?? "#000000", e => UpdateColor(c => c.Input = e.Value), placeholder: "Input").AllowAlpha().WithField().Medium().Description("Input").WithTooltip($"Input: {currentColors.Input ?? "#000000"}")
                            | new ThemeColorPicker(currentColors.Ring ?? "#000000", e => UpdateColor(c => c.Ring = e.Value), placeholder: "Ring").AllowAlpha().WithField().Medium().Description("Ring").WithTooltip($"Ring: {currentColors.Ring ?? "#000000"}")
      )
                ).Height(Size.Fit()).Open()

                // Typography & Layout
                | new Expandable(
                    "Typography & Layout",
                    Layout.Vertical()
                        | new TextInput(
                            value: editingTheme.Value.FontFamily ?? "",
                            onChange: e => UpdateThemeProperty(t => t.FontFamily = string.IsNullOrWhiteSpace(e.Value) ? null : e.Value),
                            placeholder: "e.g., Inter, system-ui, sans-serif"
                        ).WithField().Label("Font Family")
                        | new TextInput(
                            value: editingTheme.Value.FontSize ?? "",
                            onChange: e => UpdateThemeProperty(t => t.FontSize = string.IsNullOrWhiteSpace(e.Value) ? null : e.Value),
                            placeholder: "e.g., 16px, 1rem"
                        ).WithField().Label("Font Size")
                        | new Separator()
                        | new BorderRadiusSelector(
                            editingTheme,
                            UpdateThemeProperty
                        )
                );

        }
    }

    /// <summary>
    /// Color editor component with label and color picker
    /// </summary>
    private class ColorEditor(string label, string? color, Action<string> onChange) : ViewBase
    {
        public override object Build()
        {
            var colorState = UseState(color ?? "#000000");

            // Sync internal state when the color prop changes using UseQuery
            UseQuery(
                key: color,
                fetcher: async ct =>
                {
                    if (color != null && colorState.Value != color)
                    {
                        colorState.Set(color);
                    }
                    await Task.CompletedTask;
                    return true;
                },
                options: new QueryOptions { RevalidateOnMount = true });

            return Layout.Horizontal().Align(Align.Center)
                | Text.P(label).Small().Width(Size.Px(180))
                | new ColorInput(
                    value: colorState.Value,
                    onChange: e =>
                    {
                        colorState.Set(e.Value);
                        onChange(e.Value);
                    },
                    variant: ColorInputVariant.TextAndPicker
                );
        }
    }

    /// <summary>
    /// Border radius selector with visual preview boxes
    /// </summary>
    private class BorderRadiusSelector(IState<Theme> editingTheme, Action<Action<Theme>> updateThemeProperty) : ViewBase
    {
        // Constants for consistent sizing
        private const int PreviewSize = 35;
        private const int CardSize = 60;
        private const int SvgViewBox = 32;

        // Available border radius options: (cssValue, pxRadius)
        private static readonly (string Value, int Pixels)[] RadiusOptions =
        [
            ("0px", 0),
            ("0.5rem", 8),
            ("1rem", 16),
            ("1.5rem", 24),
            ("2rem", 32)
        ];

        public override object Build()
        {
            return Layout.Vertical().Gap(3)
                | Text.Block("Radius").Small().Bold()
                | BuildRadiusCategory(
                    "Boxes",
                    "card, modal, alert",
                    editingTheme.Value.BorderRadiusBoxes,
                    value => updateThemeProperty(t => t.BorderRadiusBoxes = value))
                | BuildRadiusCategory(
                    "Fields",
                    "button, input, select, tab",
                    editingTheme.Value.BorderRadiusFields,
                    value => updateThemeProperty(t => t.BorderRadiusFields = value))
                | BuildRadiusCategory(
                    "Selectors",
                    "checkbox, toggle, badge",
                    editingTheme.Value.BorderRadiusSelectors,
                    value => updateThemeProperty(t => t.BorderRadiusSelectors = value));
        }

        private static object BuildRadiusCategory(
            string title,
            string subtitle,
            string? currentValue,
            Action<string?> onUpdate)
        {
            var options = Layout.Horizontal().Gap(2);
            foreach (var (value, pixels) in RadiusOptions)
            {
                options = options | CreateOption(value, pixels, currentValue, onUpdate);
            }

            return Layout.Vertical()
                | Text.Block(title).Bold().Small()
                | Text.Block(subtitle).Muted().Italic()
                | options;
        }

        private static object CreateOption(
            string remValue,
            int pxRadius,
            string? currentValue,
            Action<string?> onUpdate)
        {
            // Normalize both values to compare (treat null/empty as "0px")
            var normalizedCurrent = string.IsNullOrWhiteSpace(currentValue) ? "0px" : currentValue;
            var normalizedOption = remValue == "0px" ? "0px" : remValue;
            var isSelected = normalizedCurrent == normalizedOption;
            var fillColor = isSelected ? "var(--primary)" : "var(--secondary)";

            // Allow rectangle to be larger than viewbox to support large radii without capping
            // If rect is 32x32, max radius is 16. If rect is 64x64, max radius is 32.
            var rectSize = SvgViewBox * 2;

            // ViewBox shows the top-left 32x32 area
            // Radii: 0, 8, 16, 24, 32 will now be visually distinct
            var svgContent = $@"<svg width='100%' height='100%' viewBox='0 0 {SvgViewBox} {SvgViewBox}' xmlns='http://www.w3.org/2000/svg'>
                <rect width='{rectSize}' height='{rectSize}' rx='{pxRadius}' fill='{fillColor}' />
            </svg>";

            return new Card(
                    Layout.Center()
                        | new Svg(svgContent)
                            .Width(Size.Px(PreviewSize))
                            .Height(Size.Px(PreviewSize))
                )
                .Width(Size.Px(CardSize))
                .Height(Size.Px(CardSize))
                .OnClick(() => onUpdate(remValue == "0px" ? null : remValue))
                .WithTooltip($"{remValue} ({pxRadius}px)");
        }
    }

    /// <summary>
    /// Right panel showing live preview of the theme
    /// </summary>
    private class LivePreviewPanel(Theme theme) : ViewBase
    {
        public override object Build()
        {
            return Layout.Vertical()
                    | Text.H2("Live Preview")
                    | Text.P("See your theme changes in real-time").Small().Muted()
                    | Layout.Tabs(
                        new Tab("Components", new InteractiveThemePreview(theme)).Icon(Icons.LayoutPanelLeft),
                        new Tab("Dashboard", new DashboardApp()).Icon(Icons.LayoutDashboard)
                    // new Tab("Colors", new ColorPalettePreview(theme)).Icon(Icons.Palette)
                    );
        }
    }

    /// <summary>
    /// Shows color palette for both light and dark modes
    /// </summary>
    private class ColorPalettePreview(Theme theme) : ViewBase
    {
        public override object Build()
        {
            return Layout.Vertical()
                | Text.H3("Light Theme Colors")
                | (Layout.Grid().Columns(2)
                    | new ColorPreview("Primary", theme.Colors.Light.Primary, theme.Colors.Light.PrimaryForeground)
                    | new ColorPreview("Secondary", theme.Colors.Light.Secondary, theme.Colors.Light.SecondaryForeground)
                    | new ColorPreview("Success", theme.Colors.Light.Success, theme.Colors.Light.SuccessForeground)
                    | new ColorPreview("Destructive", theme.Colors.Light.Destructive, theme.Colors.Light.DestructiveForeground)
                    | new ColorPreview("Warning", theme.Colors.Light.Warning, theme.Colors.Light.WarningForeground)
                    | new ColorPreview("Info", theme.Colors.Light.Info, theme.Colors.Light.InfoForeground)
                    | new ColorPreview("Muted", theme.Colors.Light.Muted, theme.Colors.Light.MutedForeground)
                    | new ColorPreview("Accent", theme.Colors.Light.Accent, theme.Colors.Light.AccentForeground))

                | Text.H3("Dark Theme Colors")
                | (Layout.Grid().Columns(2)
                    | new ColorPreview("Primary", theme.Colors.Dark.Primary, theme.Colors.Dark.PrimaryForeground)
                    | new ColorPreview("Secondary", theme.Colors.Dark.Secondary, theme.Colors.Dark.SecondaryForeground)
                    | new ColorPreview("Success", theme.Colors.Dark.Success, theme.Colors.Dark.SuccessForeground)
                    | new ColorPreview("Destructive", theme.Colors.Dark.Destructive, theme.Colors.Dark.DestructiveForeground)
                    | new ColorPreview("Warning", theme.Colors.Dark.Warning, theme.Colors.Dark.WarningForeground)
                    | new ColorPreview("Info", theme.Colors.Dark.Info, theme.Colors.Dark.InfoForeground)
                    | new ColorPreview("Muted", theme.Colors.Dark.Muted, theme.Colors.Dark.MutedForeground)
                    | new ColorPreview("Accent", theme.Colors.Dark.Accent, theme.Colors.Dark.AccentForeground));
        }
    }

    /// <summary>
    /// Compact demo form that visually reacts to the currently selected theme.
    /// </summary>
    private class InteractiveThemePreview(Theme theme) : ViewBase
    {
        private readonly Theme _theme = theme;

        public override object Build()
        {
            var client = UseService<IClientProvider>();

            // --- Form state ----------------------------------------------------
            var payment = UseState(() => new PaymentModel(
                NameOnCard: "John Doe",
                CardNumber: "1234 5678 9012 3456",
                Cvv: "123",
                Month: "MM",
                Year: "YYYY",
                BillingAddress: "",
                SameAsShipping: true,
                Comments: string.Empty
            ));

            var price = UseState(500);

            // --- Settings / inputs / misc state -------------------------------
            var agreeTerms = UseState(true);
            var themeSatisfaction = UseState(4);
            var uxSatisfaction = UseState((int?)null);

            var paginationPage = UseState(1);
            var passwordText = UseState("");
            var notesText = UseState("");
            var searchText = UseState("");
            var domain = UseState("ivy.app");
            var email = UseState("");
            var selectedCategory = UseState<string?>("Primary");
            var badgeVariant = UseState(new[] { "Success", "Warning", "Info" });
            var buttonVariant = UseState(new[] { "Primary" });
            var disableButtons = UseState(false);
            var disableInputs = UseState(false);
            var dateTimeState = UseState(DateTime.Now);
            var dateRangeState = UseState(() => (from: DateTime.Today.AddDays(-7), to: DateTime.Today));

            // --- Chat state ----------------------------------------------------
            var chatMessages = UseState(ImmutableArray.Create<ChatMessage>(
                new ChatMessage(ChatSender.Assistant,
                    $"You're previewing the '{_theme.Name}' theme. Type a message to see how chat looks in this theme.")
            ));

            UseEffect(() =>
            {
                if (!string.IsNullOrWhiteSpace(payment.Value.NameOnCard) &&
                    !string.IsNullOrWhiteSpace(payment.Value.CardNumber))
                {
                    client.Toast($"Payment form submitted for {payment.Value.NameOnCard}", "Form");
                }
            }, payment);

            const int totalPages = 5;
            var themeIcon = GetThemeIcon(_theme.Name);
            var statusVariant = GetStatusVariant(_theme.Name);

            // --- Helpers -------------------------------------------------------
            ValueTask OnChatSend(Event<Chat, string> e)
            {
                var trimmed = e.Value.Trim();
                if (string.IsNullOrEmpty(trimmed))
                {
                    return ValueTask.CompletedTask;
                }

                var withUser = chatMessages.Value.Add(new ChatMessage(ChatSender.User, trimmed));
                var withAssistant = withUser.Add(
                    new ChatMessage(ChatSender.Assistant, $"You said: {trimmed}")
                );
                chatMessages.Set(withAssistant);
                return ValueTask.CompletedTask;
            }

            // Build Ivy Form from the payment state
            var paymentForm = payment.ToForm("Submit payment")
                .SubmitBuilder(isLoading => new Button("Submit payment").Loading(isLoading).Disabled(isLoading || disableButtons.Value))
                .Clear()
                .Place(m => m.NameOnCard)
                .Place(m => m.CardNumber)
                .Place(m => m.Cvv)
                .PlaceHorizontal(m => m.Month, m => m.Year)
                .Place(m => m.BillingAddress)
                .Place(m => m.SameAsShipping)
                .Place(m => m.Comments)
                .Label(m => m.NameOnCard, "Name on card")
                .Label(m => m.CardNumber, "Card number")
                .Label(m => m.Cvv, "CVV")
                .Label(m => m.Month, "Month")
                .Label(m => m.Year, "Year")
                .Label(m => m.BillingAddress, "Billing address")
                .Label(m => m.SameAsShipping, "Same as shipping address")
                .Label(m => m.Comments, "Comments")
                .Builder(m => m.NameOnCard, s => s.ToTextInput().Disabled(disableInputs.Value))
                .Builder(m => m.CardNumber, s => s.ToTextInput().Disabled(disableInputs.Value))
                .Builder(m => m.Cvv, s => s.ToPasswordInput().Placeholder("CVV").Disabled(disableInputs.Value))
                .Builder(m => m.Comments, s => s.ToTextareaInput().Placeholder("Add any additional comments").Disabled(disableInputs.Value))
                .Builder(m => m.Month, s => s.ToTextInput().Disabled(disableInputs.Value))
                .Builder(m => m.Year, s => s.ToTextInput().Disabled(disableInputs.Value))
                .Builder(m => m.BillingAddress, s => s.ToTextInput().Disabled(disableInputs.Value))
                .Builder(m => m.SameAsShipping, s => s.ToBoolInput().Disabled(disableInputs.Value))
                .Required(m => m.NameOnCard, m => m.CardNumber, m => m.Cvv);

            QueryResult<Option<string>[]> QueryCategories(IViewContext context, string query)
            {
                var categories = new[] { "Primary", "Secondary", "Outline", "Destructive", "Success", "Warning", "Info" };
                return context.UseQuery<Option<string>[], (string, string)>(
                    key: (nameof(QueryCategories), query),
                    fetcher: ct => Task.FromResult(categories
                        .Where(c => c.Contains(query, StringComparison.OrdinalIgnoreCase))
                        .Select(c => new Option<string>(c))
                        .ToArray()));
            }

            QueryResult<Option<string>?> LookupCategory(IViewContext context, string? category)
            {
                return context.UseQuery<Option<string>?, (string, string?)>(
                    key: (nameof(LookupCategory), category),
                    fetcher: ct => Task.FromResult(category != null ? new Option<string>(category) : null));
            }

            Button CreateLoadingButton(string name, ButtonVariant variant) =>
                new Button(name, variant: variant)
                {
                    OnClick = new(_ =>
                    {
                        client.Toast($"{name} button clicked", "Action");
                        return ValueTask.CompletedTask;
                    })
                }.Width(Size.Full()).Disabled(disableButtons.Value);

            static object GetPaginationContent(int page, int total) =>
                new Card(
                    Layout.Vertical().Align(Align.Center)
                        | Text.Block("Theme insight").Small()
                        | Text.P(page switch
                        {
                            1 => "Discover how primary and accent colors shape the whole experience.",
                            2 => "Badges, borders and subtle shadows adapt instantly to your theme.",
                            3 => "Form controls, switches and sliders stay readable in every palette.",
                            4 => "Try a different theme and see how this card transforms.",
                            _ => "You've reached the end of the tour - tweak settings and explore freely."
                        }).Small()
                ).Height(Size.Fit());

            // --- Column builders ----------------------------------------------
            object BuildFirstColumn() =>
                Layout.Vertical()
                    | new Card(
                        Layout.Vertical()
                            | paymentForm).Height(Size.Fit())
                    | new Card(Layout.Vertical()
                        | Text.Block("Category Selector").Bold()
                        | Text.P("Select a category to see the corresponding action button.").Small()
                        | selectedCategory.ToAsyncSelectInput(QueryCategories, LookupCategory, placeholder: "Select Category").Disabled(disableInputs.Value)
                        | (selectedCategory.Value switch
                        {
                            "Primary" => CreateLoadingButton("Primary", ButtonVariant.Primary),
                            "Secondary" => CreateLoadingButton("Secondary", ButtonVariant.Secondary),
                            "Outline" => CreateLoadingButton("Outline", ButtonVariant.Outline),
                            "Destructive" => CreateLoadingButton("Destructive", ButtonVariant.Destructive),
                            "Success" => CreateLoadingButton("Success", ButtonVariant.Success),
                            "Warning" => CreateLoadingButton("Warning", ButtonVariant.Warning),
                            "Info" => CreateLoadingButton("Info", ButtonVariant.Info),
                            _ => CreateLoadingButton("Primary", ButtonVariant.Primary)
                        }));

            object BuildSecondColumn() =>
                Layout.Vertical()
                    | new Card(
                        Layout.Vertical()
                            | Text.Block("Badge Variant Selector").Bold()
                            | Text.P("Select one or multiple badge variants to see them displayed below.").Small()
                            | badgeVariant.ToSelectInput(new[]
                            {
                                new Option<string>("Primary", "Primary"),
                                new Option<string>("Destructive", "Destructive"),
                                new Option<string>("Secondary", "Secondary"),
                                new Option<string>("Outline", "Outline"),
                                new Option<string>("Success", "Success"),
                                new Option<string>("Warning", "Warning"),
                                new Option<string>("Info", "Info")
                            }).Variant(SelectInputVariant.Toggle).Disabled(disableInputs.Value)
                            | Text.Block("Selected badges:").Small()
                            | (Layout.Horizontal().Align(Align.Center)
                                | badgeVariant.Value.Select(variant => variant switch
                                {
                                    "Primary" => new Badge("Primary").Primary(),
                                    "Destructive" => new Badge("Destructive").Destructive(),
                                    "Secondary" => new Badge("Secondary").Secondary(),
                                    "Outline" => new Badge("Outline").Outline(),
                                    "Success" => new Badge("Success").Success(),
                                    "Warning" => new Badge("Warning").Warning(),
                                    "Info" => new Badge("Info").Info(),
                                    _ => new Badge("Primary").Primary()
                                }).ToArray())).Height(Size.Fit())
                     | new Box(
                        Layout.Vertical().Align(Align.Center)
                        | Text.Block("Pagination demo").Bold()
                            | GetPaginationContent(paginationPage.Value, totalPages)
                            | new Pagination(paginationPage.Value, totalPages, e =>
                            {
                                paginationPage.Set(e.Value);
                                return ValueTask.CompletedTask;
                            }).Disabled(disableInputs.Value)
                    )
                    | new Card(Layout.Vertical()
                        | Text.Block("Buttons & Actions").Bold()
                        | (Layout.Horizontal().Height(Size.Fit())
                            | CreateLoadingButton("Primary", ButtonVariant.Primary).Loading()
                            | CreateLoadingButton("Secondary", ButtonVariant.Secondary).Loading()
                            | CreateLoadingButton("Outline", ButtonVariant.Outline).Loading())
                        | (Layout.Horizontal().Width(Size.Full())
                            | (Layout.Vertical().Align(Align.Left)
                                | themeSatisfaction.ToFeedbackInput().Stars().Disabled(disableInputs.Value))
                            | (Layout.Vertical().Align(Align.Right)
                                | uxSatisfaction.ToFeedbackInput().Thumbs().Disabled(disableInputs.Value)))
                        | new Box((Layout.Horizontal().Height(Size.Fit())
                            | agreeTerms.ToBoolInput().Disabled(disableInputs.Value)
                            | Text.Block("I agree to the terms and conditions")))
                        | new Embed("https://github.com/Ivy-Interactive/Ivy-Framework")
                        | (Layout.Horizontal().Height(Size.Fit())
                            | (Layout.Vertical() | new Box((Layout.Horizontal()
                                    | (Layout.Vertical().Align(Align.Left) | Text.Block("Disable all buttons"))
                                    | disableButtons.ToSwitchInput())))
                            | (Layout.Vertical() | new Box((Layout.Horizontal()
                                | (Layout.Vertical().Align(Align.Left) | Text.Block("Disable all inputs"))
                                | disableInputs.ToSwitchInput()))))
                        | (Layout.Vertical().Align(Align.Center) | new Badge($"{_theme.Name} theme active", statusVariant, themeIcon).Primary())
                    );

            object BuildThirdColumn() =>
                Layout.Vertical()
                    | new Card((Layout.Vertical() | new Chat(chatMessages.Value.ToArray(), OnChatSend).Height(Size.Px(330))).Height(Size.Fit()))
                    | new Card(Layout.Vertical()
                        | Text.Block("Fields").Bold()
                        | searchText.ToSearchInput().Placeholder("Search in settings").Disabled(disableInputs.Value)
                        | dateRangeState.ToDateRangeInput()
                            .Disabled(disableInputs.Value)
                            .WithField()
                            .Label($"Date Range ({(dateRangeState.Value.to - dateRangeState.Value.from).Days} days)")
                            .Height(Size.Fit())
                        | dateTimeState.ToDateTimeInput()
                            .Format("dd/MM/yyyy HH:mm:ss")
                            .Disabled(disableInputs.Value)
                            .WithField()
                            .Label("DateTime")
                            .Height(Size.Fit())
                        | domain.ToTextInput().Prefix("https://").Disabled(disableInputs.Value)
                        | email.ToTextInput()
                            .Placeholder("Email (Ctrl+E)")
                            .ShortcutKey("Ctrl+E")
                            .Variant(TextInputVariant.Email)
                            .Disabled(disableInputs.Value)
                        | Text.Block("Price range").Bold()
                        | Text.P($"Estimated monthly budget: ${price.Value}").Small()
                        | price.ToSliderInput().Min(0).Max(2000).Step(50).Disabled(disableInputs.Value)

                    );

            // --- Layout -------------------------------------------------------
            return Layout.Horizontal()
                | BuildFirstColumn()
                | BuildSecondColumn()
                | BuildThirdColumn();
        }

        private record PaymentModel(
            string NameOnCard,
            string CardNumber,
            string Cvv,
            string Month,
            string Year,
            string BillingAddress,
            bool SameAsShipping,
            string Comments
        );
    }

    private static Icons GetThemeIcon(string themeName)
    {
        return themeName.ToLowerInvariant() switch
        {
            "ocean" => Icons.Waves,
            "forest" => Icons.TreePine,
            "sunset" => Icons.Sunset,
            "midnight" => Icons.Moon,
            _ => Icons.Palette
        };
    }

    private static BadgeVariant GetStatusVariant(string themeName)
    {
        return themeName.ToLowerInvariant() switch
        {
            "ocean" => BadgeVariant.Info,
            "forest" => BadgeVariant.Success,
            "sunset" => BadgeVariant.Warning,
            "midnight" => BadgeVariant.Secondary,
            _ => BadgeVariant.Primary
        };
    }

    private class ColorPreview(string label, string? bgColor, string? fgColor) : ViewBase
    {
        public override object Build()
        {
            var bgState = UseState(bgColor ?? "#000000");
            var fgState = UseState(fgColor ?? "#FFFFFF");

            // Map label to appropriate predefined color
            var previewColor = label switch
            {
                "Primary" => Colors.Primary,
                "Secondary" => Colors.Secondary,
                "Success" => Colors.Green,
                "Destructive" => Colors.Red,
                "Warning" => Colors.Orange,
                "Info" => Colors.Blue,
                "Muted" => Colors.Gray,
                "Accent" => Colors.Purple,
                _ => Colors.Primary
            };

            return Layout.Vertical()
                | Text.P(label).Small()
                | Layout.Horizontal(
                    // Color preview box using appropriate predefined color
                    new Box("Preview")
                        .Width(Size.Px(100))
                        .Height(Size.Px(60))
                        .Background(previewColor)
                        .BorderRadius(BorderRadius.Rounded)
                        .ContentAlign(Align.Center),
                    Layout.Vertical()
                        | Text.P("Background:").Small()
                        | bgState.ToColorInput().Variant(ColorInputVariant.TextAndPicker).Disabled()
                        | Text.P("Foreground:").Small()
                        | fgState.ToColorInput().Variant(ColorInputVariant.TextAndPicker).Disabled()
                );
        }
    }

    private string GenerateCSharpCode(Theme theme)
    {
        var lightColors = theme.Colors.Light;
        var darkColors = theme.Colors.Dark;
        return $@"// Add this to your server configuration:
var server = new Server()
    .UseTheme(theme => {{
        theme.Name = ""{theme.Name}"";
        theme.Colors = new ThemeColorScheme
        {{
            Light = new ThemeColors
            {{
                Primary = ""{lightColors.Primary}"",
                PrimaryForeground = ""{lightColors.PrimaryForeground}"",
                Secondary = ""{lightColors.Secondary}"",
                SecondaryForeground = ""{lightColors.SecondaryForeground}"",
                Background = ""{lightColors.Background}"",
                Foreground = ""{lightColors.Foreground}"",
                Destructive = ""{lightColors.Destructive}"",
                DestructiveForeground = ""{lightColors.DestructiveForeground}"",
                Success = ""{lightColors.Success}"",
                SuccessForeground = ""{lightColors.SuccessForeground}"",
                Warning = ""{lightColors.Warning}"",
                WarningForeground = ""{lightColors.WarningForeground}"",
                Info = ""{lightColors.Info}"",
                InfoForeground = ""{lightColors.InfoForeground}"",
                Border = ""{lightColors.Border}"",
                Input = ""{lightColors.Input}"",
                Ring = ""{lightColors.Ring}"",
                Muted = ""{lightColors.Muted}"",
                MutedForeground = ""{lightColors.MutedForeground}"",
                Accent = ""{lightColors.Accent}"",
                AccentForeground = ""{lightColors.AccentForeground}"",
                Card = ""{lightColors.Card}"",
                CardForeground = ""{lightColors.CardForeground}"",
                Popover = ""{lightColors.Popover}"",
                PopoverForeground = ""{lightColors.PopoverForeground}""
            }},
            Dark = new ThemeColors
            {{
                Primary = ""{darkColors.Primary}"",
                PrimaryForeground = ""{darkColors.PrimaryForeground}"",
                Secondary = ""{darkColors.Secondary}"",
                SecondaryForeground = ""{darkColors.SecondaryForeground}"",
                Background = ""{darkColors.Background}"",
                Foreground = ""{darkColors.Foreground}"",
                Destructive = ""{darkColors.Destructive}"",
                DestructiveForeground = ""{darkColors.DestructiveForeground}"",
                Success = ""{darkColors.Success}"",
                SuccessForeground = ""{darkColors.SuccessForeground}"",
                Warning = ""{darkColors.Warning}"",
                WarningForeground = ""{darkColors.WarningForeground}"",
                Info = ""{darkColors.Info}"",
                InfoForeground = ""{darkColors.InfoForeground}"",
                Border = ""{darkColors.Border}"",
                Input = ""{darkColors.Input}"",
                Ring = ""{darkColors.Ring}"",
                Muted = ""{darkColors.Muted}"",
                MutedForeground = ""{darkColors.MutedForeground}"",
                Accent = ""{darkColors.Accent}"",
                AccentForeground = ""{darkColors.AccentForeground}"",
                Card = ""{darkColors.Card}"",
                CardForeground = ""{darkColors.CardForeground}"",
                Popover = ""{darkColors.Popover}"",
                PopoverForeground = ""{darkColors.PopoverForeground}""
            }}
        }}
        theme.FontFamily = ""{theme.FontFamily}"";
        theme.FontSize = ""{theme.FontSize}"";
        theme.BorderRadiusBoxes = ""{theme.BorderRadiusBoxes}"";
        theme.BorderRadiusFields = ""{theme.BorderRadiusFields}"";
        theme.BorderRadiusSelectors = ""{theme.BorderRadiusSelectors}""; 
    }});";
    }

    // Theme presets
    private static Theme GetOceanTheme() => new()
    {
        Name = "Ocean",
        FontFamily = "Geist",
        FontSize = "16px",
        BorderRadiusBoxes = Theme.Default.BorderRadiusBoxes,
        BorderRadiusFields = Theme.Default.BorderRadiusFields,
        BorderRadiusSelectors = Theme.Default.BorderRadiusSelectors,
        Colors = new ThemeColorScheme
        {
            Light = new ThemeColors
            {
                Primary = "#0077BE",
                PrimaryForeground = "#FFFFFF",
                Secondary = "#5B9BD5",
                SecondaryForeground = "#FFFFFF",
                Background = "#F0F8FF",
                Foreground = "#1A1A1A",
                Destructive = "#DC143C",
                DestructiveForeground = "#FFFFFF",
                Success = "#20B2AA",
                SuccessForeground = "#FFFFFF",
                Warning = "#FFD700",
                WarningForeground = "#1A1A1A",
                Info = "#4682B4",
                InfoForeground = "#FFFFFF",
                Border = "#B0C4DE",
                Input = "#E6F2FF",
                Ring = "#0077BE",
                Muted = "#E0E8F0",
                MutedForeground = "#5A6A7A",
                Accent = "#87CEEB",
                AccentForeground = "#1A1A1A",
                Card = "#FFFFFF",
                CardForeground = "#1A1A1A",
                Popover = "#F0F8FF",
                PopoverForeground = "#1A1A1A"
            },
            Dark = new ThemeColors
            {
                Primary = "#4A9EFF",
                PrimaryForeground = "#001122",
                Secondary = "#2D4F70",
                SecondaryForeground = "#E8F4FD",
                Background = "#001122",
                Foreground = "#E8F4FD",
                Destructive = "#FF6B7D",
                DestructiveForeground = "#FFFFFF",
                Success = "#4ECDC4",
                SuccessForeground = "#001122",
                Warning = "#FFE066",
                WarningForeground = "#001122",
                Info = "#87CEEB",
                InfoForeground = "#001122",
                Border = "#1A3A5C",
                Input = "#0F2A4A",
                Ring = "#4A9EFF",
                Muted = "#0F2A4A",
                MutedForeground = "#8BB3D9",
                Accent = "#1A3A5C",
                AccentForeground = "#E8F4FD",
                Card = "#0F2A4A",
                CardForeground = "#E8F4FD",
                Popover = "#001122",
                PopoverForeground = "#E8F4FD"
            }
        }
    };

    private static Theme GetForestTheme() => new()
    {
        Name = "Forest",
        FontFamily = "Geist",
        FontSize = "16px",
        BorderRadiusBoxes = Theme.Default.BorderRadiusBoxes,
        BorderRadiusFields = Theme.Default.BorderRadiusFields,
        BorderRadiusSelectors = Theme.Default.BorderRadiusSelectors,
        Colors = new ThemeColorScheme
        {
            Light = new ThemeColors
            {
                Primary = "#228B22",
                PrimaryForeground = "#FFFFFF",
                Secondary = "#8FBC8F",
                SecondaryForeground = "#1A1A1A",
                Background = "#F0FFF0",
                Foreground = "#1A1A1A",
                Destructive = "#B22222",
                DestructiveForeground = "#FFFFFF",
                Success = "#32CD32",
                SuccessForeground = "#FFFFFF",
                Warning = "#FFA500",
                WarningForeground = "#1A1A1A",
                Info = "#4169E1",
                InfoForeground = "#FFFFFF",
                Border = "#90EE90",
                Input = "#E8F5E8",
                Ring = "#228B22",
                Muted = "#E0F0E0",
                MutedForeground = "#4A5A4A",
                Accent = "#98FB98",
                AccentForeground = "#1A1A1A",
                Card = "#FFFFFF",
                CardForeground = "#1A1A1A",
                Popover = "#F0FFF0",
                PopoverForeground = "#1A1A1A"
            },
            Dark = new ThemeColors
            {
                Primary = "#4AFF4A",
                PrimaryForeground = "#001100",
                Secondary = "#2D4A2D",
                SecondaryForeground = "#E8FFE8",
                Background = "#001100",
                Foreground = "#E8FFE8",
                Destructive = "#FF4444",
                DestructiveForeground = "#FFFFFF",
                Success = "#66FF66",
                SuccessForeground = "#001100",
                Warning = "#FFB84D",
                WarningForeground = "#001100",
                Info = "#6A9BFF",
                InfoForeground = "#001100",
                Border = "#1A3A1A",
                Input = "#0F2A0F",
                Ring = "#4AFF4A",
                Muted = "#0F2A0F",
                MutedForeground = "#8BC98B",
                Accent = "#1A3A1A",
                AccentForeground = "#E8FFE8",
                Card = "#0F2A0F",
                CardForeground = "#E8FFE8",
                Popover = "#001100",
                PopoverForeground = "#E8FFE8"
            }
        }
    };

    private static Theme GetSunsetTheme() => new()
    {
        Name = "Sunset",
        FontFamily = "Geist",
        FontSize = "16px",
        BorderRadiusBoxes = Theme.Default.BorderRadiusBoxes,
        BorderRadiusFields = Theme.Default.BorderRadiusFields,
        BorderRadiusSelectors = Theme.Default.BorderRadiusSelectors,
        Colors = new ThemeColorScheme
        {
            Light = new ThemeColors
            {
                Primary = "#FF6347",
                PrimaryForeground = "#FFFFFF",
                Secondary = "#FFB6C1",
                SecondaryForeground = "#1A1A1A",
                Background = "#FFF5EE",
                Foreground = "#1A1A1A",
                Destructive = "#DC143C",
                DestructiveForeground = "#FFFFFF",
                Success = "#90EE90",
                SuccessForeground = "#1A1A1A",
                Warning = "#FFD700",
                WarningForeground = "#1A1A1A",
                Info = "#87CEEB",
                InfoForeground = "#1A1A1A",
                Border = "#FFE4E1",
                Input = "#FFF0E6",
                Ring = "#FF6347",
                Muted = "#FFDAB9",
                MutedForeground = "#8B4513",
                Accent = "#FFA07A",
                AccentForeground = "#1A1A1A",
                Card = "#FFFFFF",
                CardForeground = "#1A1A1A",
                Popover = "#FFF5EE",
                PopoverForeground = "#1A1A1A"
            },
            Dark = new ThemeColors
            {
                Primary = "#FF8A65",
                PrimaryForeground = "#2A1100",
                Secondary = "#8D4A47",
                SecondaryForeground = "#FFE8E1",
                Background = "#2A1100",
                Foreground = "#FFE8E1",
                Destructive = "#FF5252",
                DestructiveForeground = "#FFFFFF",
                Success = "#81C784",
                SuccessForeground = "#2A1100",
                Warning = "#FFB74D",
                WarningForeground = "#2A1100",
                Info = "#64B5F6",
                InfoForeground = "#2A1100",
                Border = "#5D2A1A",
                Input = "#3D1F0F",
                Ring = "#FF8A65",
                Muted = "#3D1F0F",
                MutedForeground = "#C19A8A",
                Accent = "#5D2A1A",
                AccentForeground = "#FFE8E1",
                Card = "#3D1F0F",
                CardForeground = "#FFE8E1",
                Popover = "#2A1100",
                PopoverForeground = "#FFE8E1"
            }
        }
    };

    private static Theme GetMidnightTheme() => new()
    {
        Name = "Midnight",
        FontFamily = "Geist",
        FontSize = "16px",
        BorderRadiusBoxes = Theme.Default.BorderRadiusBoxes,
        BorderRadiusFields = Theme.Default.BorderRadiusFields,
        BorderRadiusSelectors = Theme.Default.BorderRadiusSelectors,
        Colors = new ThemeColorScheme
        {
            Light = new ThemeColors
            {
                Primary = "#7C3AED",
                PrimaryForeground = "#FFFFFF",
                Secondary = "#DDD6FE",
                SecondaryForeground = "#1A1A1A",
                Background = "#FAFAFA",
                Foreground = "#1A1A1A",
                Destructive = "#EF4444",
                DestructiveForeground = "#FFFFFF",
                Success = "#10B981",
                SuccessForeground = "#FFFFFF",
                Warning = "#F59E0B",
                WarningForeground = "#000000",
                Info = "#3B82F6",
                InfoForeground = "#FFFFFF",
                Border = "#E5E7EB",
                Input = "#F3F4F6",
                Ring = "#7C3AED",
                Muted = "#F9FAFB",
                MutedForeground = "#6B7280",
                Accent = "#F3F0FF",
                AccentForeground = "#1A1A1A",
                Card = "#FFFFFF",
                CardForeground = "#1A1A1A",
                Popover = "#FAFAFA",
                PopoverForeground = "#1A1A1A"
            },
            Dark = new ThemeColors
            {
                Primary = "#A78BFA",
                PrimaryForeground = "#1A1A2E",
                Secondary = "#4C1D95",
                SecondaryForeground = "#E5E5E5",
                Background = "#0F0F23",
                Foreground = "#E5E5E5",
                Destructive = "#EF4444",
                DestructiveForeground = "#FFFFFF",
                Success = "#10B981",
                SuccessForeground = "#FFFFFF",
                Warning = "#F59E0B",
                WarningForeground = "#000000",
                Info = "#3B82F6",
                InfoForeground = "#FFFFFF",
                Border = "#374151",
                Input = "#1F2937",
                Ring = "#A78BFA",
                Muted = "#1F2937",
                MutedForeground = "#9CA3AF",
                Accent = "#6366F1",
                AccentForeground = "#FFFFFF",
                Card = "#1A1A2E",
                CardForeground = "#E5E5E5",
                Popover = "#0F0F23",
                PopoverForeground = "#E5E5E5"
            }
        }
    };


}

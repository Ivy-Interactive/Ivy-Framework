using Ivy.Shared;
using Ivy.Themes;
using Ivy.Hooks;
using Ivy.Samples.Shared.Helpers;
using Ivy.Views.Forms;

namespace Ivy.Samples.Shared.Apps.Demos;

[App(icon: Icons.Palette, searchHints: ["theming", "customization", "branding", "styling", "appearance", "design"])]
public class ThemeCustomizer : SampleBase
{
    protected override object? BuildSample()
    {
        var selectedPreset = UseState("default");
        var currentTheme = UseState(Theme.Default);
        var showJson = UseState(false);
        var showCode = UseState(false);
        var client = UseService<IClientProvider>();
        var themeService = UseService<IThemeService>();

        var presets = new Dictionary<string, Theme>
        {
            ["default"] = Theme.Default,
            ["ocean"] = GetOceanTheme(),
            ["forest"] = GetForestTheme(),
            ["sunset"] = GetSunsetTheme(),
            ["midnight"] = GetMidnightTheme()
        };

        void ApplyTheme()
        {
            try
            {
                // Apply theme directly to the service
                themeService.SetTheme(currentTheme.Value);

                // Generate and apply the CSS to the frontend immediately
                var css = themeService.GenerateThemeCss();
                client.ApplyTheme(css);

                client.Toast("Theme applied successfully!", "Success");
            }
            catch (Exception ex)
            {
                client.Toast($"Error: {ex.Message}", "Error");
            }
        }

        return Layout.Vertical()
            | Text.H1("Theme Customizer")
            | Text.Block("Customize and apply themes dynamically to your Ivy application.")
            | new Card(
                Layout.Vertical()
                    | Text.Block("🎨 Live Theme Application")
                    | Text.P("Select a preset and click 'Apply Selected Theme' to see changes instantly - no page refresh needed!").Small()
            ).BorderColor(Colors.Primary)

            // Preset selector
            | Text.H2("Theme Presets")
            | Layout.Horizontal(
                Text.Block("Select Theme"),
                selectedPreset.ToSelectInput(
                    presets.Select(kv => new Option<string>(kv.Value.Name, kv.Key))
                )
            )

            // Apply button
            | new Button("Apply Selected Theme")
            {
                OnClick = _ =>
                {
                    // Update current theme from selected preset first
                    if (presets.TryGetValue(selectedPreset.Value, out var theme))
                    {
                        currentTheme.Set(theme);
                    }
                    ApplyTheme();
                    return ValueTask.CompletedTask;
                },
                Icon = Icons.Sparkles
            }
            // Interactive preview form that reacts to the current theme
            | Text.H2("Interactive Preview")
            | Text.Block("Common form elements below use the active theme tokens for colors, borders and accents.")
            | new InteractiveThemePreview(currentTheme.Value)

            // Theme preview with actual colors
            | Text.H2("Color Preview")
            | Layout.Horizontal(
                // Light theme colors
                new Card(
                    Layout.Grid().Columns(1)
                        | new ColorPreview("Primary", currentTheme.Value.Colors.Light.Primary, currentTheme.Value.Colors.Light.PrimaryForeground)
                        | new ColorPreview("Secondary", currentTheme.Value.Colors.Light.Secondary, currentTheme.Value.Colors.Light.SecondaryForeground)
                        | new ColorPreview("Success", currentTheme.Value.Colors.Light.Success, currentTheme.Value.Colors.Light.SuccessForeground)
                        | new ColorPreview("Destructive", currentTheme.Value.Colors.Light.Destructive, currentTheme.Value.Colors.Light.DestructiveForeground)
                        | new ColorPreview("Warning", currentTheme.Value.Colors.Light.Warning, currentTheme.Value.Colors.Light.WarningForeground)
                        | new ColorPreview("Info", currentTheme.Value.Colors.Light.Info, currentTheme.Value.Colors.Light.InfoForeground)
                        | new ColorPreview("Muted", currentTheme.Value.Colors.Light.Muted, currentTheme.Value.Colors.Light.MutedForeground)
                        | new ColorPreview("Accent", currentTheme.Value.Colors.Light.Accent, currentTheme.Value.Colors.Light.AccentForeground)
                ).Title("Light Theme"),
                // Dark theme colors  
                new Card(
                    Layout.Grid().Columns(1)
                        | new ColorPreview("Primary", currentTheme.Value.Colors.Dark.Primary, currentTheme.Value.Colors.Dark.PrimaryForeground)
                        | new ColorPreview("Secondary", currentTheme.Value.Colors.Dark.Secondary, currentTheme.Value.Colors.Dark.SecondaryForeground)
                        | new ColorPreview("Success", currentTheme.Value.Colors.Dark.Success, currentTheme.Value.Colors.Dark.SuccessForeground)
                        | new ColorPreview("Destructive", currentTheme.Value.Colors.Dark.Destructive, currentTheme.Value.Colors.Dark.DestructiveForeground)
                        | new ColorPreview("Warning", currentTheme.Value.Colors.Dark.Warning, currentTheme.Value.Colors.Dark.WarningForeground)
                        | new ColorPreview("Info", currentTheme.Value.Colors.Dark.Info, currentTheme.Value.Colors.Dark.InfoForeground)
                        | new ColorPreview("Muted", currentTheme.Value.Colors.Dark.Muted, currentTheme.Value.Colors.Dark.MutedForeground)
                        | new ColorPreview("Accent", currentTheme.Value.Colors.Dark.Accent, currentTheme.Value.Colors.Dark.AccentForeground)
                ).Title("Dark Theme")
            )

            // Export options
            | Text.H2("Export Options")
            | Layout.Horizontal(
                new Button("Show C# Code")
                {
                    OnClick = _ =>
                    {
                        showCode.Set(!showCode.Value);
                        showJson.Set(false);
                        return ValueTask.CompletedTask;
                    }
                },
                new Button("Show JSON")
                {
                    OnClick = _ =>
                    {
                        showJson.Set(!showJson.Value);
                        showCode.Set(false);
                        return ValueTask.CompletedTask;
                    }
                }.Variant(ButtonVariant.Secondary),
                                new Button("Copy to Clipboard")
                                {
                                    OnClick = _ =>
                                    {
                                        var content = showCode.Value
                                            ? GenerateCSharpCode(currentTheme.Value)
                                            : System.Text.Json.JsonSerializer.Serialize(currentTheme.Value, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                                        client.CopyToClipboard(content);
                                        client.Toast("Theme configuration copied to clipboard!");
                                        return ValueTask.CompletedTask;
                                    }
                                }.Variant(ButtonVariant.Outline)
            )

            | (showCode.Value ? new Code(GenerateCSharpCode(currentTheme.Value), Languages.Csharp) : null)
            | (showJson.Value ? new Code(System.Text.Json.JsonSerializer.Serialize(currentTheme.Value, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }), Languages.Json) : null)

            // Usage instructions
            | Text.H2("Usage")
            | new Card(
                Layout.Vertical()
                    | Text.H3("Dynamic Theme Application")
                    | Text.Block("Click 'Apply Selected Theme' to instantly apply the theme with live CSS updates - no server restart required!")
                    | Text.H3("Server Configuration")
                    | Text.Block("You can also configure themes at server startup:")
                    | new Code(@"var server = new Server()
    .UseTheme(theme => 
    {
        // Your theme configuration here
    });", Languages.Csharp)
            )
        ;
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

            // Payment form model (left column)
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

            // Settings / chat preview
            var agreeTerms = UseState(true);
            var themeSatisfaction = UseState(4); // 1–5 stars
            var uxSatisfaction = UseState((int?)null);

            var paginationPage = UseState(1);
            const int totalPages = 5;

            // Text input demo state
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

            var themeIcon = GetThemeIcon(_theme.Name);
            var statusVariant = GetStatusVariant(_theme.Name);

            // Chat messages for environment preview
            var chatMessages = UseState(ImmutableArray.Create<ChatMessage>(
                new ChatMessage(ChatSender.Assistant,
                    $"You're previewing the '{_theme.Name}' theme. Type a message to see how chat looks in this theme.")
            ));

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
                .Add(m => m.NameOnCard)
                .Add(m => m.CardNumber)
                .PlaceHorizontal(m => m.Cvv, m => m.Month, m => m.Year)
                .Add(m => m.BillingAddress)
                .Add(m => m.SameAsShipping)
                .Add(m => m.Comments)
                .Label(m => m.NameOnCard, "Name on card")
                .Label(m => m.CardNumber, "Card number")
                .Label(m => m.Cvv, "CVV")
                .Label(m => m.Month, "Month")
                .Label(m => m.Year, "Year")
                .Label(m => m.BillingAddress, "Billing address")
                .Label(m => m.SameAsShipping, "Same as shipping address")
                .Label(m => m.Comments, "Comments")
                .Builder(m => m.Cvv, s => s.ToPasswordInput().Placeholder("CVV").Disabled(disableInputs.Value))
                .Builder(m => m.Comments, s => s.ToTextAreaInput().Placeholder("Add any additional comments").Disabled(disableInputs.Value))
                .Builder(m => m.NameOnCard, s => s.ToTextInput().Disabled(disableInputs.Value))
                .Builder(m => m.CardNumber, s => s.ToTextInput().Disabled(disableInputs.Value))
                .Builder(m => m.Month, s => s.ToTextInput().Disabled(disableInputs.Value))
                .Builder(m => m.Year, s => s.ToTextInput().Disabled(disableInputs.Value))
                .Builder(m => m.BillingAddress, s => s.ToTextInput().Disabled(disableInputs.Value))
                .Builder(m => m.SameAsShipping, s => s.ToBoolInput().Disabled(disableInputs.Value))
                .Required(m => m.NameOnCard, m => m.CardNumber, m => m.Cvv);

            UseEffect(() =>
            {
                if (!string.IsNullOrWhiteSpace(payment.Value.NameOnCard) &&
                    !string.IsNullOrWhiteSpace(payment.Value.CardNumber))
                {
                    client.Toast($"Payment form submitted for {payment.Value.NameOnCard}", "Form");
                }
            }, payment);

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
                    OnClick = _ =>
                    {
                        client.Toast($"{name} button clicked", "Action");
                        return ValueTask.CompletedTask;
                    }
                }.Width(Size.Full()).Disabled(disableButtons.Value);

            var firstColumn = Layout.Vertical()
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
                    }))
                    | (Layout.Vertical().Align(Align.Center) | new Badge($"{_theme.Name} theme active", statusVariant, themeIcon).Primary());

            var secondColumn =
                    Layout.Vertical().Gap(5)
                        | new Embed("https://github.com/Ivy-Interactive/Ivy-Framework")
                        | Text.Block("Price range").Bold()
                        | Text.P($"Estimated monthly budget: ${price.Value}").Small()
                        | price.ToSliderInput().Min(0).Max(2000).Step(50).Disabled(disableInputs.Value)
                        | (Layout.Horizontal().Height(Size.Fit())
                            | CreateLoadingButton("Primary", ButtonVariant.Primary).Loading()
                            | CreateLoadingButton("Secondary", ButtonVariant.Secondary).Loading()
                            | CreateLoadingButton("Outline", ButtonVariant.Outline).Loading())
                        | domain.ToTextInput().Prefix("https://").Disabled(disableInputs.Value)
                        | dateTimeState.ToDateTimeInput()
                            .Format("dd/MM/yyyy HH:mm:ss")
                            .Disabled(disableInputs.Value)
                            .WithField()
                            .Label("DateTime")
                            .Height(Size.Fit())
                        | new Card(
                            Layout.Vertical().Gap(3)
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
                                }).Variant(SelectInputs.Toggle).Disabled(disableInputs.Value)
                                | Text.Block("Selected badges:").Small()
                                | (Layout.Horizontal().Gap(2).Align(Align.Center)
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
                        | email.ToTextInput()
                            .Placeholder("Email (Ctrl+E)")
                            .ShortcutKey("Ctrl+E")
                            .Variant(TextInputs.Email)
                            .Disabled(disableInputs.Value)
                        | (Layout.Grid().Columns(2).Gap(3).Width(Size.Full())
                            | (Layout.Vertical()
                                | themeSatisfaction.ToFeedbackInput().Variant(FeedbackInputs.Stars).Disabled(disableInputs.Value))
                            | (Layout.Vertical().Align(Align.Right)
                                | uxSatisfaction.ToFeedbackInput().Variant(FeedbackInputs.Thumbs).Disabled(disableInputs.Value)));
                        

            var thirdColumn = Layout.Vertical()
                        | new Card((Layout.Vertical() | new Chat(chatMessages.Value.ToArray(), OnChatSend).Height(Size.Px(330))).Height(Size.Fit()))

                        | (Layout.Horizontal().Height(Size.Fit())
                            | (Layout.Vertical().Gap(2) | new Box((Layout.Horizontal()
                                    | (Layout.Vertical().Align(Align.Left) | Text.Block("Disable all buttons"))
                                    | disableButtons.ToSwitchInput())))
                            | (Layout.Vertical().Gap(2) | new Box((Layout.Horizontal()
                                | (Layout.Vertical().Align(Align.Left) | Text.Block("Disable all inputs"))
                                | disableInputs.ToSwitchInput()))))
                        | searchText.ToSearchInput().Placeholder("Search in settings").Disabled(disableInputs.Value)
                        | dateRangeState.ToDateRangeInput()
                            .Disabled(disableInputs.Value)
                            .WithField()
                            .Label($"Date Range ({(dateRangeState.Value.to - dateRangeState.Value.from).Days} days)")
                            .Height(Size.Fit())
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
                        |new Box((Layout.Horizontal().Height(Size.Fit())
                            | agreeTerms.ToBoolInput().Disabled(disableInputs.Value)
                            | Text.Block("I agree to the terms and conditions")))
                       ;

            static object GetPaginationContent(int page, int total) =>
                new Card(
                    Layout.Vertical().Align(Align.Center).Gap(2)
                        | Text.Block("Theme insight").Small()
                        | Text.P(page switch
                        {
                            1 => "Discover how primary and accent colors shape the whole experience.",
                            2 => "Badges, borders and subtle shadows adapt instantly to your theme.",
                            3 => "Form controls, switches and sliders stay readable in every palette.",
                            4 => "Try a different theme and see how this card transforms.",
                            _ => "You’ve reached the end of the tour — tweak settings and explore freely."
                        }).Small()
                ).Height(Size.Fit());

            return Layout.Horizontal()
                | firstColumn
                | secondColumn
                | thirdColumn;
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
                        .Color(previewColor)
                        .BorderRadius(BorderRadius.Rounded)
                        .Padding(3)
                        .ContentAlign(Align.Center),
                    Layout.Vertical()
                        | Text.P("Background:").Small()
                        | bgState.ToColorInput().Variant(ColorInputs.TextAndPicker).Disabled()
                        | Text.P("Foreground:").Small()
                        | fgState.ToColorInput().Variant(ColorInputs.TextAndPicker).Disabled()
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
        }};
    }});";
    }

    // Theme presets
    private static Theme GetOceanTheme() => new()
    {
        Name = "Ocean",
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
# Ivy Framework Weekly Notes - Week of 2026-02-19

## UI/UX Improvements

### Enhanced Sidebar Navigation

The sidebar menu now provides better visual feedback and navigation:

- **Active Item Highlighting**: Clearly see your current location with highlighted active items.
- **Auto-scroll to Active Item**: The sidebar now automatically scrolls to bring the active item into partial view when navigating or loading a page.
- **Improved Keyboard Navigation**: Sidebar search results now support full keyboard navigation with arrow keys and automatic scrolling.
- **Forgiving Search**: Search is now smarter, ignoring spaces and special characters for easier matching (e.g., "datatables" finds "Data Tables").

### Button Links with Configurable Target

Buttons with URLs now provide flexible control over link navigation behavior. By default, buttons navigate in the same tab, but you can easily configure them to open in new tabs using the `.OpenInNewTab()` method.

```csharp
// Opens in a new tab
new Button("External Link").Secondary()
    .Url("https://github.com/Ivy-Interactive/Ivy-Framework")
    .OpenInNewTab()
    .Icon(Icons.ExternalLink, Align.Right);

// Control target explicitly
new Button("View Details")
    .Url("/details")
    .Target(LinkTarget.Self);  // Same tab
```

### Icon Support for Switch Inputs

Switch inputs now support icons, allowing you to add visual indicators inside the switch thumb.

```csharp
var darkMode = UseState(false);
darkMode.ToSwitchInput(Icons.Moon).Label("Dark Mode");
```

### Clickable Links in DataTable

To display a column as clickable links, use the `LinkDisplayRenderer`:

```csharp
dataTable
    .Column(x => x.WebsiteUrl)
    .Renderer(x => x.WebsiteUrl, new LinkDisplayRenderer { Type = LinkDisplayType.Url });
```

### Tooltips for DataTable Row Actions

DataTable row action buttons now support tooltips.

To add a tooltip to a row action, use the `Tooltip` property:

```csharp
dataTable
    .RowAction("edit", action => action
        .Label("Edit")
        .Icon("pencil")
        .Tooltip("Edit this record")
        .OnClick(row => EditRecord(row.Id)));
```

### Improved Multiselect Component

You can control how many badges are visible before overflow using the `maxVisibleBadges` property (defaults to 2):

```csharp
multiselect
    .MaxVisibleBadges(3)  // Show up to 3 badges before showing "+N"
    .Options(myOptions);
```

### Icon Picker Input

A new `IconInput` widget allows you to select icons from the full Lucide icon library. The input provides a searchable dropdown with visual icon previews.

```csharp
var iconState = UseState<Icons>(Icons.Star);
iconState.ToIconInput().Placeholder("Pick an icon");
```

### Icon Animations

Icons now support animations using the `Animation` widget with the `.WithAnimation()` extension method.

```csharp
Icons.LoaderCircle
    .ToIcon()
    .Color(Colors.Blue)
    .WithAnimation(AnimationType.Rotate)
    .Trigger(AnimationTrigger.Auto)
    .Duration(1);
```

link to animation docs with sentence.

## Theming & Customization

### Enhanced Theme Color Picker

A powerful `ThemeColorPicker` widget designed for theme customization provides an intuitive interface for selecting and adjusting theme colors with both palette and slider-based controls.

Instead of using `ColorInput` with the `ThemePicker` variant, you now use the `ThemeColorPicker` widget directly from the `Ivy.Widgets.Internal` namespace.

- **Theme Color Palette**: A grid of 160 colors (8 rows × 20 columns) with automatic detection and labeling of active theme colors.
- **Dual View Modes**: Toggle between a Palette view for quick color selection and a Picker view with RGB sliders for precise control
- **RGB Sliders with Hex Display**: The Picker view shows RGB sliders with hexadecimal values for each channel, with gradient backgrounds showing the color range
- **Live Format Preview**: Footer displays the selected color in HEX format with a visual preview swatch

Basic usage:

```csharp
using Ivy.Widgets.Internal;

new ThemeColorPicker(
    currentColor ?? "#000000",
    e => UpdateColor(e.Value),
    placeholder: "Primary"
);
```

Mention about need in update to new way of using ThemePicker.

### Granular Border Radius Control

Themes now support separate border radius values for different types of UI elements, giving you more precise control over your app's visual style. Instead of a single border radius that applies everywhere, you can now customize:

- **Boxes**: Cards, containers, data tables, and other box-like elements
- **Fields**: Text inputs, selects, and other form fields
- **Selectors**: Dropdowns, comboboxes, and other selector controls

To customize border radius in your theme:

```csharp
var customTheme = new Theme
{
    Name = "Custom",
    Colors = ThemeColorScheme.Default,
    BorderRadiusBoxes = "0.75rem",    // Larger radius for cards/containers
    BorderRadiusFields = "0.25rem",   // Smaller radius for inputs
    BorderRadiusSelectors = "0.5rem"  // Medium radius for dropdowns
};
```

The default theme uses semantic values from the design system tokens, providing a balanced appearance across all UI elements.

## Layout & Components

### Fluent Button Variant API

The Button widget now includes convenient extension methods for setting contextual variants. Instead of using the verbose `variant: ButtonVariant.Success` parameter syntax, you can now use fluent methods that chain naturally with other button configuration.

New extension methods:

```csharp
new Button("Save Changes").Success();
```

These join the existing variant methods like `.Primary()`, `.Secondary()`, `.Destructive()`, `.Outline()`, `.Ghost()`, `.Link()`, and `.Ai()`, providing a complete and consistent API for button styling.

### Improved ResizablePanel API

The `ResizablePanelGroup` component now uses a more structured and type-safe API for defining panel sizes. The old integer-based sizing has been replaced with a `Size` API that provides better control and clarity.

```csharp
new ResizablePanelGroup(
    new ResizablePanel(
        Size.Fraction(0.3f).Min(0.15f).Max(0.5f),
        new Card("Sidebar")
    ),
    new ResizablePanel(
        Size.Fraction(0.7f).Min(0.5f).Max(0.85f),
        new Card("Main Content")
    )
)
```

## Authentication & Security

### Simplified Auth Provider Setup

**Simplified GitHub Authentication**: GitHub authentication setup is now much simpler. You no longer need to manually register an HttpClient factory or call configuration methods:

```csharp
// Old approach (still works but deprecated)
server.Services.AddHttpClient("GitHubAuth", client =>
{
    client.DefaultRequestHeaders.Add("User-Agent", "Ivy-Framework");
    client.DefaultRequestHeaders.Add("Accept", "application/vnd.github+json");
    client.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");
});
server.Services.AddSingleton(server.Configuration);
server.UseAuth<GitHubAuthProvider>(c => c.UseGitHub());

// New simplified approach
server.UseAuth<GitHubAuthProvider>();
```

The framework now handles HttpClient configuration automatically. Just configure your GitHub credentials in user secrets or environment variables and you're ready to go.

**Simplified Microsoft Entra Authentication**: Similarly, Microsoft Entra (Azure AD) authentication no longer requires the `.UseMicrosoftEntra()` call:

```csharp
// Old approach (still works but deprecated)
server.UseAuth<MicrosoftEntraAuthProvider>(c => c.UseMicrosoftEntra());

// New simplified approach
server.UseAuth<MicrosoftEntraAuthProvider>();
```

**Configurable User-Agent Headers**: All auth providers now support customizable User-Agent headers for HTTP requests. This is useful for identifying your application in provider logs or when working with rate limits:

```csharp
// Configure via user secrets or environment variables
// GitHub:UserAgent
// Clerk:UserAgent
// Authelia:UserAgent
// Supabase:UserAgent
```

If not specified, providers default to `Ivy-Framework/{version}` where version is the Ivy assembly version.

### Customizable Authentication Cookie Settings

You now have full control over authentication cookie settings in your Ivy applications. Using the new `Server.

By default, Ivy authentication cookies are configured with secure defaults:

- **HttpOnly**: `true` (prevents JavaScript access)
- **Secure**: `true` in production (requires HTTPS)
- **SameSite**: `Lax` (provides CSRF protection)
- **Expires**: 1 year from creation

To customize these settings, add the following to your `Program.cs` before calling `server.RunAsync()`:

```csharp
Server.ConfigureAuthCookieOptions = options =>
{
    options.Expires = DateTimeOffset.UtcNow.AddDays(30);
    // Override any other cookie settings as needed
};
```

## Developer Tools

### Enhanced `ivy init` Command Options

**Quick Scripts**: Create a simple single-file Ivy application for rapid prototyping:

```terminal
ivy init --script
```

**Project Templates**: Use a specific template or interactively select from available options:

```terminal
ivy init --template my-template
ivy init --select-template
```

**IDE Integration**: Automatically install Cursor or Claude Code MCP integration after project creation:

```terminal
ivy init --cursor
ivy init --claude
```

**Automation-Friendly**: Skip all prompts and use defaults for automated scripts:

```terminal
ivy init --yes-to-all
```

Other available options include `--hello` for demo apps, `--verbose` for detailed output, `--ignore-git` to skip Git operations, and `--prerelease` to include prerelease framework versions.

## Bug Fixes

- **DataTable**: Fixed cell click events not firing for link cells and actions.
- **DataTable**: Resolved an issue where the last row could be cropped by the footer.
- **Textarea**: Improved resize handle appearance to match theme; added `.Rows()` for initial height control.
- **Code Block**: Fixed indentation inconsistency on the first line.
- **Avatar**: Fixed a crash when rendering avatars with empty user information.

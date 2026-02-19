# Ivy Framework Weekly Notes - Week of 2026-02-19

## UI/UX Improvements

### Enhanced Sidebar Navigation

The sidebar menu now provides better visual feedback and navigation:

- **Active Item Highlighting**: The current page is now highlighted in the sidebar menu, making it easy to see where you are in your app
- **Auto-scroll to Active Item**: When you navigate to a new page, the sidebar automatically scrolls to show the active item and expands parent sections as needed
- **Improved Visual States**: Better hover and active states for menu items, with distinct styling for the current page vs hovered items
- **Keyboard Navigation in Search**: When searching in the sidebar, you can now use arrow keys to navigate through results. The selected result automatically scrolls into view with smooth scrolling, making keyboard-only navigation effortless. Recent improvements ensure that arrow navigation works reliably across the entire sidebar, with the scroll container properly detecting when to bring items into view. Search results are also better organized, grouped by their path for easier scanning
- **Smarter Search Matching**: The sidebar search now ignores spaces, hyphens, and underscores when matching, making it much more forgiving. You can type "datatables" to find "Data Tables", "footerlayout" to match "Footer Layout", or search continuously without worrying about exact spacing. This works for both menu labels and search hints, making keyboard navigation even faster

These improvements make navigation more intuitive, especially in apps with deep menu hierarchies.

### Better Alignment for Boolean Inputs with Descriptions

Boolean input controls (checkboxes, switches, and toggles) now align properly when they include a description field. Previously, the control would center-align with the entire label+description block, which looked awkward for multi-line descriptions. Now the control aligns to the top, staying level with the first line of the label for a cleaner, more professional appearance.

Additionally, boolean inputs now maintain consistent heights with text inputs across all scales (Small, Medium, Large), ensuring perfect vertical alignment when mixing different input types in your forms. The controls are also prevented from shrinking, maintaining their proper size and spacing regardless of content.

This improvement applies automatically to all `BoolInput` widgets that have a description set.

### Button Links with Configurable Target

Buttons with URLs now provide flexible control over link navigation behavior. By default, buttons navigate in the same tab, but you can easily configure them to open in new tabs using the `.OpenInNewTab()` method.

```csharp
// Opens in the same tab (default)
new Button("Documentation")
    .Url("/docs/getting-started");

// Opens in a new tab
new Button("External Link", variant: ButtonVariant.Secondary)
    .Url("https://github.com/Ivy-Interactive/Ivy-Framework")
    .OpenInNewTab()
    .Icon(Icons.ExternalLink, Align.Right);

// Control target explicitly
new Button("View Details")
    .Url("/details")
    .Target(LinkTarget.Self);  // Same tab
```

**Breaking Change**: Previous versions always opened button links in new tabs. If you relied on this behavior, add `.OpenInNewTab()` to maintain the same functionality.

Buttons with URLs support right-click actions like "Copy Link" and "Open in New Tab", providing a better user experience than programmatic navigation.

### Icon Support for Switch Inputs

Switch inputs now support icons, allowing you to add visual indicators inside the switch thumb. This makes switches more expressive and helps communicate their purpose at a glance - perfect for common toggles like dark mode, notifications, or feature flags.

To add an icon to a switch, simply pass it to the `ToSwitchInput` extension method:

```csharp
var darkMode = UseState(false);
darkMode.ToSwitchInput(Icons.Moon).Label("Dark Mode");

var notifications = UseState(true);
notifications.ToSwitchInput(Icons.Bell).Label("Notifications");
```

Icons automatically display centered inside the switch thumb and scale appropriately with the switch size. This feature brings switches in line with toggle inputs, which already supported icons.

### Clickable Links in DataTable

Links in DataTable cells are now much easier to use. Simply click on a link to navigate - no more need for Ctrl/Cmd + click. The cursor also changes to a pointer when hovering over links, providing clear visual feedback that the cell is clickable.

To display a column as clickable links, use the `LinkDisplayRenderer`:

```csharp
dataTable
    .Column(x => x.WebsiteUrl)
    .Renderer(x => x.WebsiteUrl, new LinkDisplayRenderer { Type = LinkDisplayType.Url });
```

External links (http/https) open in a new tab, while relative URLs navigate in the same tab.

### Tooltips for DataTable Row Actions

DataTable row action buttons now support tooltips, making it easier to provide helpful hints about what each action does. When you hover over an action button, a tooltip appears with the text you've specified.

To add a tooltip to a row action, use the `Tooltip` property:

```csharp
dataTable
    .RowAction("edit", action => action
        .Label("Edit")
        .Icon("pencil")
        .Tooltip("Edit this record")
        .OnClick(row => EditRecord(row.Id)));
```

This is particularly useful for icon-only actions where the purpose might not be immediately clear, or when you want to provide additional context about what an action will do.

### Fixed Last Row Cropping in DataTable

Fixed an issue where the last row in DataTable was being cropped by an overlay footer. Tables now display all rows completely, ensuring your data is fully visible without any visual clipping at the bottom of the table.

### Improved Multiselect Component

The Multiselect component has been enhanced with better badge display and interaction:

- **Cleaner Badge Appearance**: Fixed badge sizing issues by adjusting the remove button height, resulting in more visually consistent badges
- **Badge Overflow Handling**: When you have many selected items, only the first few badges are displayed with a "+N" indicator showing how many more items are selected. This prevents the control from becoming cluttered
- **Enhanced Dropdown Interaction**: The dropdown now shows all available options, with visual indicators (X icons) marking which items are currently selected. You can toggle selections directly from the dropdown by clicking any option

You can control how many badges are visible before overflow using the `maxVisibleBadges` property (defaults to 2):

```csharp
multiselect
    .MaxVisibleBadges(3)  // Show up to 3 badges before showing "+N"
    .Options(myOptions);
```

These improvements make multiselect controls much cleaner and easier to use, especially when working with many selected items.

### Icon Picker Input

A new `IconInput` widget allows you to select icons from the full Lucide icon library. The input provides a searchable dropdown with visual icon previews, making it easy to find and select the perfect icon for your UI.

Key features:
- **Searchable Icon Grid**: Browse and search through all available Lucide icons with a visual grid interface
- **Multiple Sizes**: Supports Small, Medium (default), and Large sizes
- **Nullable Support**: Works with both `Icons` and `Icons?` types for optional icon selection
- **Form Integration**: Automatically scaffolded for `Icons` properties in forms

Basic usage with state binding:

```csharp
var iconState = UseState<Icons>(Icons.Star);
iconState.ToIconInput().Placeholder("Pick an icon");
```

For nullable icons:

```csharp
var iconState = UseState<Icons?>(null);
iconState.ToIconInput().Nullable();
```

The selected icon can be displayed using the `Icon` widget, which now includes a new `Medium()` size option:

```csharp
new Icon(iconState.Value).Medium();
```

Icon properties are automatically rendered as icon pickers in form scaffolding, making it easy to add icon selection to your models without any additional configuration.

### Icon Animations

Icons now support animations using the `Animation` widget with the `.WithAnimation()` extension method. You can add visual effects like rotation, pulsing, bouncing, and shaking to icons, triggered automatically, on click, or on hover.

Available animation types include:
- **Rotate**: Continuous spinning (perfect for loading indicators)
- **Pulse**: Scaling effect that draws attention
- **Hover**: Subtle elevation effect on mouse over
- **Shake**: Quick back-and-forth motion
- **Bounce**: Playful bouncing effect

Basic usage:

```csharp
// Spinning loader icon
Icons.LoaderCircle
    .ToIcon()
    .Color(Colors.Blue)
    .WithAnimation(AnimationType.Rotate)
    .Trigger(AnimationTrigger.Auto)
    .Duration(1);

// Heart that pulses on click
Icons.Heart
    .ToIcon()
    .Color(Colors.Red)
    .WithAnimation(AnimationType.Pulse)
    .Trigger(AnimationTrigger.Click);

// Bell that shakes on click
Icons.Bell
    .ToIcon()
    .Color(Colors.Orange)
    .WithAnimation(AnimationType.Shake)
    .Trigger(AnimationTrigger.Click)
    .Duration(0.6);
```

Animation triggers can be set to:
- `AnimationTrigger.Auto` - Starts immediately and loops continuously
- `AnimationTrigger.Click` - Plays once when clicked
- `AnimationTrigger.Hover` - Plays when mouse hovers over the icon

The `.Duration()` method controls animation speed in seconds. This makes it easy to add polish and interactivity to your UI with animated icons for loading states, interactive feedback, and visual emphasis.

## Theming & Customization

### Enhanced Theme Color Picker

A powerful `ThemeColorPicker` widget designed for theme customization provides an intuitive interface for selecting and adjusting theme colors with both palette and slider-based controls.

**Breaking Change**: The theme picker has been refactored into its own dedicated widget. Instead of using `ColorInput` with the `ThemePicker` variant, you now use the `ThemeColorPicker` widget directly from the `Ivy.Widgets.Internal` namespace.

**Key features:**

- **Theme Color Palette**: A grid of 160 colors (8 rows × 20 columns) with automatic detection and labeling of active theme colors. When a color in the palette matches one of your theme's color variables, it displays abbreviated labels (P for Primary, SF for Secondary Foreground, etc.) making it easy to see which colors are currently in use
- **Improved Foreground Color Display**: Color inputs for foreground colors now show the selected foreground color accurately by displaying it on the appropriate background color. For example, when selecting "Primary Foreground", the color swatch displays the foreground color on the primary color background, making it easy to see how the colors work together. The "A" indicator character also displays in the correct foreground color for better visual feedback
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

For foreground colors, use the `.Foreground()` method to add the "A" indicator:

```csharp
new ThemeColorPicker(
    currentForegroundColor ?? "#000000",
    e => UpdateColor(e.Value),
    placeholder: "Primary Foreground"
).Foreground(true);
```

**Migration from old API:**

```csharp
// Old way (no longer supported)
new ColorInput(
    currentColor ?? "#000000",
    e => UpdateColor(e.Value),
    placeholder: "Primary",
    variant: ColorInputs.ThemePicker
);

// New way
using Ivy.Widgets.Internal;

new ThemeColorPicker(
    currentColor ?? "#000000",
    e => UpdateColor(e.Value),
    placeholder: "Primary"
);
```

The ThemePicker variant automatically detects your current theme colors and highlights them in the palette, making it perfect for building theme customization UIs.

**Copy to Clipboard**: A new copy button next to the hex input field makes it easy to copy color values with a single click. This is particularly useful when you want to share or reuse color values across your theme or with other designers.

### Granular Border Radius Control

Themes now support separate border radius values for different types of UI elements, giving you more precise control over your app's visual style. Instead of a single border radius that applies everywhere, you can now customize:

- **Boxes**: Cards, containers, data tables, and other box-like elements
- **Fields**: Text inputs, selects, and other form fields
- **Selectors**: Dropdowns, comboboxes, and other selector controls

This allows you to create more nuanced designs - for example, keeping form fields crisp with smaller radii while using larger, softer radii for cards and containers.

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

The default theme uses semantic values from the design system tokens, providing a balanced appearance across all UI elements. If you don't specify these values, the framework applies sensible defaults.

**Visual Border Radius Configurator**: The theme customizer now includes an intuitive visual selector for configuring border radius. Instead of typing CSS values, you can click visual previews to choose from five preset options (0px, 0.5rem, 1rem, 1.5rem, 2rem) for each category. Each preview shows an SVG representation of the border radius, making it easy to see exactly how different radii will look before applying them to your theme.

**CSS Utility Classes**: Three new utility classes are now available for custom styling:
- `.rounded-box` - Uses the theme's box border radius
- `.rounded-field` - Uses the theme's field border radius
- `.rounded-selector` - Uses the theme's selector border radius

These classes automatically adapt to your theme's configured border radius values, ensuring consistency across custom components.

**Breaking Change**: The legacy `BorderRadius` property has been removed from the `Theme` class. If you were using this property, you'll need to migrate to the semantic properties (`BorderRadiusBoxes`, `BorderRadiusFields`, `BorderRadiusSelectors`). In most cases, you can simply set all three properties to the same value that you previously used for `BorderRadius`.

**Improved Checkbox Border Radius**: Checkboxes now use a refined border radius calculation (half of the selector radius) instead of the full selector radius. This prevents checkboxes from appearing as circles when larger selector radii are used, maintaining their characteristic square appearance with appropriately rounded corners. This enhancement ensures checkboxes maintain proper visual proportions across all theme configurations.

## Layout & Components

### Fluent Button Variant API

The Button widget now includes convenient extension methods for setting contextual variants, making your code more concise and readable. Instead of using the verbose `variant: ButtonVariant.Success` parameter syntax, you can now use fluent methods that chain naturally with other button configuration.

New extension methods:

```csharp
// Success variant - for positive actions
new Button("Save Changes").Success();

// Warning variant - for actions that require caution
new Button("Proceed with Caution").Warning();

// Info variant - for informational actions
new Button("Learn More").Info();
```

These join the existing variant methods like `.Primary()`, `.Secondary()`, `.Destructive()`, `.Outline()`, `.Ghost()`, `.Link()`, and `.Ai()`, providing a complete and consistent API for button styling.

Before:
```csharp
new Button("Delete", variant: ButtonVariant.Destructive)
```

After:
```csharp
new Button("Delete").Destructive()
```

This improvement makes the API more discoverable through IntelliSense and creates more readable, chainable button configurations.

### Improved ResizablePanel API

The `ResizablePanelGroup` component now uses a more structured and type-safe API for defining panel sizes. The old integer-based sizing has been replaced with a `Size` API that provides better control and clarity.

**Key improvements:**
- **Fraction-based sizing**: Use `Size.Fraction()` for percentage-based panel sizes (0-1 range)
- **Min/Max constraints**: Set minimum and maximum size limits with `.Min()` and `.Max()` methods
- **Type safety**: The structured API prevents common sizing mistakes and provides better IntelliSense support

Example of the new API:

```csharp
new ResizablePanelGroup(
    new ResizablePanel(
        Size.Fraction(0.3f).Min(0.15f).Max(0.5f),  // 30% default, min 15%, max 50%
        new Card("Sidebar")
    ),
    new ResizablePanel(
        Size.Fraction(0.7f).Min(0.5f).Max(0.85f),  // 70% default, min 50%, max 85%
        new Card("Main Content")
    )
)
```

The old API using integer percentages (`new ResizablePanel(30, ...)`) has been replaced with this more explicit approach. Note: The component was also renamed from `ResizeablePanel` to `ResizablePanel` (fixing the spelling).

## Authentication & Security

### Same-Tab OAuth Authentication Flow

OAuth authentication now completes in the same browser tab, providing a smoother and more native authentication experience. Previously, OAuth redirects would open in new tabs or windows, which could be blocked by popup blockers and felt disjointed. The new flow keeps users in the same context throughout the authentication process.

This enhancement particularly improves the mobile authentication experience, where multiple tabs can be confusing and harder to manage.

**Important: Callback URL Update Required**

With this change, the OAuth callback URL structure has been updated. You'll need to update your OAuth application configurations with the new callback URL:

**Old callback URL:** `http://localhost:5010/ivy/webhook`
**New callback URL:** `http://localhost:5010/ivy/auth/callback`

For production deployments, replace `http://localhost:5010` with your actual application URL.

This affects all OAuth providers:
- **Auth0**: Update "Allowed Callback URLs" in your application settings
- **GitHub**: Update "Authorization callback URL" in your OAuth app settings
- **Microsoft Entra**: Update "Redirect URI" in your app registration
- **Supabase**: Update redirect URLs in your project's authentication settings

The authentication functionality is unchanged; only the callback endpoint path is different. Update your OAuth app configurations to continue using OAuth authentication.

### Simplified Auth Provider Setup

Auth provider configuration has been significantly streamlined, making it easier to get authentication up and running in your applications. Several authentication providers now require less boilerplate code and manual configuration.

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

**Example Projects**: The framework now includes complete example projects for all auth providers (Auth0, Authelia, BasicAuth, Clerk, GitHub, Microsoft Entra, and Supabase), making it easier to see working authentication implementations and get started quickly.


### Customizable Authentication Cookie Settings

You now have full control over authentication cookie settings in your Ivy applications. Using the new `Server.ConfigureAuthCookieOptions` static property, you can customize cookie expiration, SameSite policy, Secure flag, and other settings to meet your application's specific security requirements.

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

Your custom configuration is applied after the defaults, so you can override any setting while keeping the secure defaults for other properties.

### Security: Lodash-ES Prototype Pollution Fix

Updated the `lodash-es` dependency from version 4.17.21 to 4.17.23 to resolve a prototype pollution vulnerability. This security fix is automatically included in the framework - no action needed from users.

## Developer Tools

### External Widgets Documentation

Comprehensive documentation is now available for creating External Widgets - custom React components that extend the Ivy Framework. External widgets allow you to build domain-specific UI components (like diagrams, charts, or rich editors) separately from the core framework and integrate them seamlessly into your Ivy applications.

The new documentation covers the complete workflow:

**Architecture**: External widgets consist of three parts - a C# proxy record with `[ExternalWidget]` attribute, a React component built with standard tooling (Vite), and an MSBuild pipeline that embeds the compiled JS/CSS as resources in your assembly.

**CLI Scaffolding**: Generate new external widgets quickly using the Ivy CLI:

```terminal
ivy widget
Namespace: ExternalWidget
Widget: MyWidget
```

**C# Backend**: Define your widget with props and events using the `[ExternalWidget]` attribute:

```csharp
[ExternalWidget(
    "frontend/dist/ExternalWidget.js",
    StylePath = "frontend/dist/style.css",
    ExportName = "MyWidget",
    GlobalName = "MyProject_Widgets_MyWidget")]
public record MyWidget : WidgetBase<MyWidget>
{
    [Prop] public string? Label { get; set; }
    [Event] public Func<Event<MyWidget>, ValueTask>? OnClick { get; set; }
}
```

**Frontend Build**: Configure Vite to build an IIFE bundle that works with the Ivy host, externalizing React and ReactDOM. The documentation includes complete Vite configuration examples and explains how to structure your React components to receive props and fire events back to C#.

**Project Patterns**: Choose between standalone widget projects (for reusable NuGet packages) or integrated patterns (widgets inside your host app), with guidance on project structure, MSBuild targets for frontend builds, and embedded resource configuration.

**Theme Integration**: External widgets can use Ivy theme variables (`--primary`, `--background`, `--foreground`) to match the host app's appearance automatically.

The documentation also includes a comprehensive troubleshooting guide covering common issues like resource paths, global variable naming, React duplication, and build configuration.

This makes it much easier to extend Ivy with specialized UI components while keeping them decoupled from the framework core.

### File-Based Apps

You can now run Ivy applications from a single `.cs` file without any project scaffolding. This makes it incredibly easy to experiment with Ivy, create quick demos, or prototype ideas without running `ivy init` or setting up a full project structure.

**Creating a file-based app:**

```terminal
ivy init --script
```

This generates a single-file app that you can customize. Here's a minimal example:

```csharp
#: package Ivy@*

using Ivy;
using Ivy.Views;

var server = new Server();
server.AddApp<HelloApp>();
await server.RunAsync();

[App]
class HelloApp : ViewBase
{
    public override object? Build()
    {
        return Layout.Center(
            new Card(
                Text.P("Hello from Ivy!")
            ).Width(60)
        );
    }
}
```

**Running your file-based app:**

```terminal
dotnet run HelloApp.cs
```

That's it! The app starts immediately, typically on port 5010. No project files, no solution structure—just pure Ivy code in a single file.

**Key features:**
- **Package directive**: The `#: package Ivy@*` directive automatically references the Ivy NuGet package
- **Prerequisites**: Requires .NET 10 or later (which supports enhanced file-based apps)
- **Full Ivy capabilities**: Despite being a single file, you have access to all Ivy features—hooks, views, widgets, layouts, and more

File-based apps are perfect for learning Ivy, sharing code examples, creating quick prototypes, or building simple utilities without the overhead of a full project structure.

### Enhanced `ivy init` Command Options

The `ivy init` command now has comprehensive documentation for all available options, making it easier to customize your project setup from the start. Here are some particularly useful options you may not have known about:

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

### More Reliable `--describe` Command

The `--describe` CLI command is now more robust in environments where multiple applications are running. If the default port is unavailable, the command automatically finds an available port instead of failing. This change makes the describe functionality work reliably without requiring manual port configuration.

## Bug Fixes

### Fixed DataTable Cell Click Events

Fixed a critical bug where `OnCellClick` events were not firing correctly in DataTables, particularly when used with link cells or cell actions:

- **Link Cells**: The `OnCellClick` event now fires properly when clicking link cells. Previously, link navigation would prevent the click event from being triggered, making it impossible to track or react to clicks on links within your tables
- **Cell Actions**: Cell actions are now correctly bound to the `OnCellClick` handler instead of `OnCellActivated`, ensuring your cell action callbacks execute as expected

If you've been using `OnCellClick` events with DataTables and noticed they weren't working reliably, this fix resolves those issues. Both the click event and any link navigation or cell actions now work together properly.

### Fixed Code Block First-Line Indentation

Code blocks now display with proper alignment across all lines. Previously, the first line of code would have extra indentation compared to subsequent lines, creating a visual inconsistency. This has been fixed by separating the container padding from the code text styling, ensuring all lines align perfectly.

This improvement automatically applies to all `Code` widgets throughout your application, providing cleaner and more professional code display.

### Improved Textarea Resize Handle

The resize handle on textarea inputs has been redesigned to better match your theme. The drag indicator now uses theme colors instead of browser defaults, providing a more cohesive and polished appearance. This visual enhancement applies automatically to all resizable textarea inputs in your application.

### Textarea Row Control

Textarea inputs now support explicit row height control via the `.Rows()` method, giving you precise control over the initial height of multiline text inputs. This is particularly useful when you want to provide more vertical space for longer text entries without requiring users to manually resize the field.

```csharp
var description = UseState("");
description.ToTextAreaInput()
    .Label("Description")
    .Rows(5)  // Display 5 rows initially
    .Placeholder("Enter a detailed description...");
```

The textarea remains resizable by users, but starts with the specified number of rows. This helps create more predictable and comfortable form layouts for fields that typically contain longer text.

### Fixed Microsoft Entra Authentication

Resolved an issue with Microsoft Entra (formerly Azure AD) authentication where OAuth callbacks were not being handled correctly. The authentication callback URL structure has been fixed internally to ensure proper OAuth flow completion. If you experienced issues signing in with Microsoft Entra, this update resolves those problems.

### Fixed Avatar Widget Exception with Empty User Info

Fixed a frontend exception that occurred when Avatar components received empty or null user information. Previously, if an avatar's fallback text was undefined or null, the widget would throw an error when trying to access the `.length` property. The Avatar widget now handles these cases gracefully by using optional chaining and providing safe defaults, ensuring your UI remains stable even when user data is incomplete.

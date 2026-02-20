# Ivy Framework Weekly Notes - Week of 2026-02-19

> [!NOTE]
> We usually release on Fridays every week. Sign up on [https://ivy.app/](https://ivy.app/auth/sign-up) to get release notes directly to your inbox.

## UI/UX Improvements

### Button Links with Configurable Target

[Buttons](https://docs.ivy.app/widgets/common/button) with URLs now provide flexible control over link navigation behavior. By default, buttons navigate in the same tab, but you can easily configure them to open in new tabs using the `.OpenInNewTab()` method.

```csharp
// Opens in a new tab
new Button("External Link").Secondary()
    .Url("https://github.com/Ivy-Interactive/Ivy-Framework")
    .OpenInNewTab()
    .Icon(Icons.ExternalLink, Align.Right);
```

### Icon Support for Switch Inputs

[Switch inputs](https://docs.ivy.app/widgets/inputs/bool-input) now support icons, allowing you to add visual indicators inside the switch thumb.

```csharp
var darkMode = UseState(false);
darkMode.ToSwitchInput(Icons.Moon).Label("Dark Mode");
```

### Clickable Links in DataTable

To display a column as clickable links in a [DataTable](https://docs.ivy.app/widgets/advanced/data-table), use the `LinkDisplayRenderer`:

```csharp
dataTable
    .Column(x => x.WebsiteUrl)
    .Renderer(x => x.WebsiteUrl, new LinkDisplayRenderer { Type = LinkDisplayType.Url });
```

### Tooltips for DataTable Row Actions

[DataTable](https://docs.ivy.app/widgets/advanced/data-table) row action buttons now support [tooltips](https://docs.ivy.app/widgets/common/tooltip).

```csharp
dataTable
    .RowAction("edit", action => action
        .Label("Edit")
        .Icon("pencil")
        .Tooltip("Edit this record")
        .OnClick(row => EditRecord(row.Id)));
```

### Improved Multiselect Component

You can control how many badges are visible before overflow using the `maxVisibleBadges` property (defaults to 2) on [multiselect](https://docs.ivy.app/widgets/inputs/select-input) inputs:

```csharp
multiselect
    .MaxVisibleBadges(3)  // Show up to 3 badges before showing "+N"
    .Options(myOptions);
```

### Icon Picker Input

A new IconInput widget allows you to select icons from the full Lucide icon library. The input provides a searchable dropdown with visual icon previews.

```csharp
var iconState = UseState<Icons>(Icons.Star);
iconState.ToIconInput().Placeholder("Pick an icon");
```

## Theming & Customization

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

The default theme uses semantic values from the design system tokens documented in the [Colors API](https://docs.ivy.app/api-reference/ivy-shared/colors), providing a balanced appearance across all UI elements.

## Layout & Components

### Fluent Button Variant API

The [Button](https://docs.ivy.app/widgets/common/button) widget now includes convenient extension methods for setting contextual variants. Instead of using the verbose `variant: ButtonVariant.Success` parameter syntax, you can now use fluent methods that chain naturally with other button configuration.

```csharp
new Button("Success").Success()
new Button("Warning").Warning()
new Button("Info").Info()
```

These join the existing variant methods like `.Primary()`, `.Secondary()`, `.Destructive()`, `.Outline()`, `.Ghost()`, `.Link()`, and `.Ai()`, providing a complete and consistent API for button styling.

### Improved ResizablePanel API

The [ResizablePanelGroup](https://docs.ivy.app/widgets/layouts/resizeable-panel-group) component now uses a more structured and type-safe API for defining panel sizes. The old integer-based sizing has been replaced with a [Size](https://docs.ivy.app/api-reference/ivy-shared/size) API (e.g. `Size.Fraction()`, `.Min()`, `.Max()`) that provides better control and clarity.

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

GitHub authentication setup is now much simpler. You no longer need to manually register an HttpClient factory or call configuration methods:

```csharp
// Before (still works but deprecated)
server.Services.AddHttpClient("GitHubAuth", client =>
{
    client.DefaultRequestHeaders.Add("User-Agent", "Ivy-Framework");
    client.DefaultRequestHeaders.Add("Accept", "application/vnd.github+json");
    client.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");
});
server.Services.AddSingleton(server.Configuration);
server.UseAuth<GitHubAuthProvider>(c => c.UseGitHub());

// After
server.UseAuth<GitHubAuthProvider>();
```

The framework now handles HttpClient configuration automatically. Just configure your GitHub credentials in user secrets or environment variables and you're ready to go.

Similarly, Microsoft Entra (Azure AD) authentication no longer requires the `.UseMicrosoftEntra()` call:

```csharp
// Before (still works but deprecated)
server.UseAuth<MicrosoftEntraAuthProvider>(c => c.UseMicrosoftEntra());

// After
server.UseAuth<MicrosoftEntraAuthProvider>();
```

**Configurable User-Agent Headers**: All auth providers now support customizable User-Agent headers for HTTP requests.

If not specified, [auth providers](https://docs.ivy.app/onboarding/03_CLI/04_Authentication/01_Overview) default to `Ivy-Framework/{version}` where version is the Ivy assembly version.

### Customizable Authentication Cookie Settings

You now have full control over authentication cookie settings in your Ivy applications using the new `Server.ConfigureAuthCookieOptions` callback.

By default, Ivy authentication cookies are configured with secure defaults:

- **HttpOnly**: `true` (prevents JavaScript access)
- **Secure**: `true` in production (requires HTTPS)
- **SameSite**: `Lax` (provides CSRF protection)
- **Expires**: 1 year from creation

To customize these settings, add the following to your [Program.cs](https://docs.ivy.app/onboarding/concepts/program) before calling `server.RunAsync()`:

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

Other [ivy init](https://docs.ivy.app/onboarding/03_CLI/02_Init) options include `--hello` for demo apps, `--verbose` for detailed output, `--ignore-git` to skip Git operations, and `--prerelease` to include prerelease framework versions.

## Breaking Changes

### ResizeablePanel Renamed to ResizablePanel

`ResizeablePanelGroup` and `ResizeablePanel` have been renamed to `ResizablePanelGroup` and `ResizablePanel`. The `int?` size parameter now uses `Size.Fraction()`:

```csharp
// Before
new ResizeablePanel(25, content)

// After
new ResizablePanel(Size.Fraction(0.25f), content)
```

Panels now also support `.Min()` and `.Max()` constraints: `Size.Fraction(0.3f).Min(0.15f).Max(0.5f)`.

See the full [migration guide](./Refactors/1.2.15/ResizablePanel-Rename.md).

### OAuth Callback URL Changed

The OAuth callback URL has changed from `/ivy/webhook` to `/ivy/auth/callback`. If you use OAuth authentication (Auth0, GitHub, Microsoft Entra, Supabase), update the callback/redirect URL in your provider's dashboard settings.

See the full [migration guide](./Refactors/1.2.15/OAuth-Callback-URL.md).

### Button URL Default Target Changed

Buttons with `.Url()` now open in the **same tab** by default (previously new tab). Use `.OpenInNewTab()` to restore the old behavior:

```csharp
new Button("Docs").Url("https://docs.example.com").OpenInNewTab()
```

See the full [migration guide](./Refactors/1.2.15/Button-URL-Target.md).

## Bug Fixes

- **DataTable**: Fixed cell click events not firing for link cells and actions.
- **DataTable**: Resolved an issue where the last row could be cropped by the footer.
- **Textarea**: Improved resize handle appearance to match theme; added `.Rows()` for initial height control.
- **Code Block**: Fixed indentation inconsistency on the first line.
- **Avatar**: Fixed a crash when rendering avatars with empty user information.

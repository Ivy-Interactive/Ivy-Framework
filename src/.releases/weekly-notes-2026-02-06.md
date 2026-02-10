# Ivy Framework Weekly Notes - Week of 2026-02-06

> [!NOTE]
> We usually release on Fridays every week. Sign up on [https://ivy.app/](https://ivy.app/auth/sign-up) to get release notes directly to your inbox.

This release brings a new `IconInput` widget. It also includes a lot of bugfixes and improvements in existing UI of widgets, with a focus on theming and layout.

## LLM Framework Documentation

Added comprehensive framework documentation specifically designed for AI assistants and LLMs working with Ivy. The new `AGENTS.md` guide provides a complete introduction to the framework, including:

- Core concepts: Views, Widgets, Hooks, and Apps
- Common widgets with links to detailed documentation
- Layout system overview with code examples
- Text helpers and styling modifiers
- Event handling patterns
- Complete hooks reference (UseState, UseEffect, UseMemo, etc.)
- Input binding with state
- Best practices for building Ivy applications

This documentation serves as a quick reference for AI-assisted development and includes all essential patterns and APIs needed to work effectively with Ivy.

## New Features

### Configurable Authentication Cookie Options

You can now customize authentication cookie settings when configuring your Ivy server.

Configure authentication cookies in your `Program.cs`:

```csharp
// Configure authentication cookie options
Server.ConfigureAuthCookieOptions = options =>
{
    options.Domain = ".yourdomain.com";
    options.SameSite = SameSiteMode.Strict;
    options.Expires = DateTimeOffset.UtcNow.AddDays(30);
    options.IsEssential = true;
};

var server = new Server()
    .UseAuth<YourAuthProvider>();
```

## Widget Enhancements

All widgets now better support theming. We have also updated the theme customizer app. This also includes improvements to Scale API, default margins and paddings.

### ColorInput Swatch Variant

The `ColorInput` widget now includes a new **Swatch** variant that displays a grid of predefined theme-aware colors. Instead of entering hex codes or using a color picker, users can select from the full set of Ivy theme colors (`Colors` enum) presented in a clean, visual grid.

When you bind a `ColorInput` to a `Colors` enum state, the Swatch variant is automatically selected, making color selection more intuitive:

```csharp
// Automatic swatch for Colors enum
var colorState = UseState(Colors.Blue);
colorState.ToColorInput()  // Automatically uses Swatch variant

// Or explicitly set the variant
var colorState = UseState(Colors.Red);
colorState.ToColorInput().Variant(ColorInputs.Swatch)
```

<img width="391" height="342" alt="image" src="https://github.com/user-attachments/assets/e39424af-af6e-4353-83e5-c08221e8e26d" />

### Terminal Widget

The `Terminal` widget has been promoted from internal API to a public primitive widget. You can now use it to display terminal-like output in your applications.

```csharp
new Terminal()
    .Title("Installation")
    .AddCommand("dotnet tool install -g Ivy.Console")
    .AddOutput("You can use the following command to install Ivy globally.")
    .ShowCopyButton(true);
```

<img width="2052" height="216" alt="image" src="https://github.com/user-attachments/assets/5ecbc225-1765-436e-8a53-72cf1ced2ef2" />

### IconInput Widget

We added a new `IconInput` widget that allows users to select an icon from the Ivy icon set (Lucide icons). It provides a searchable dropdown of available icons.

```csharp
// Bind to an icon state
var iconState = UseState(Icons.Heart);
iconState.ToIconInput()
    .Placeholder("Select an icon");
```

## Breaking Changes

### Card Border Customization Removed

The `Card` widget no longer supports border customization methods. Previously, you could customize card borders with methods like `.BorderThickness()`, `.BorderStyle()`, `.BorderColor()`, and `.BorderRadius()`. These APIs have been removed to simplify the Card widget and maintain consistent styling.

### Table Default Width Behavior

Tables now default to `Size.Full()` width instead of a calculated smart width. Previously, the framework would automatically calculate table widths based on column content (between 100-400 units).

### Text.Literal() No Longer Supports Styling Methods

The `Text.Literal()` method has been streamlined and now returns a `TextBlock` directly instead of a `TextBuilder`. This means you can no longer chain styling methods like `.Bold()`, `.Italic()`, or `.Color()` on literal text.

## Bug Fixes

### MetricView API

The `MetricView` constructor signature requires the icon as the second parameter (can be null):

```csharp
// Without icon
new MetricView("Total Sales", null, ctx => UseStaticMetric(ctx, ...))

// With icon
new MetricView("Total Sales", Icons.DollarSign, ctx => UseStaticMetric(ctx, ...))
```

### GridLayout Cell Text Overflow

The `GridLayout` cell overflow behavior has been simplified. Previously, the grid applied complex hover behavior where truncated text would expand on hover using absolute positioning. This hover-to-reveal functionality has been removed in favor of simpler, more predictable cell rendering.

### Form Scaffolding Nullable Input Detection

Fixed an issue where the form scaffolder wasn't properly detecting nullable properties and fields when automatically generating form inputs. The scaffolder now correctly identifies nullable types and applies the appropriate behavior:

- **Nullable Detection**: The scaffolder now uses `NullabilityInfoContext` to accurately detect nullable reference types in addition to nullable value types
- **Clear Button Logic**: Input widgets only show the clear (X) button for nullable fields that are not marked as required
- **Consistent Behavior**: All input types (text, email, phone, URL, password, number, date/time, bool, color, select) now properly respect nullable annotations

## What's Changed
* (docs): implement Hooks Introduction page by @ArtemKhvorostianyi in https://github.com/Ivy-Interactive/Ivy-Framework/pull/2093
* [Docs}: add Page Title section to Apps documentation by @defymecobra in https://github.com/Ivy-Interactive/Ivy-Framework/pull/2092
* [Docs]: fix Hooks content missordering by @defymecobra in https://github.com/Ivy-Interactive/Ivy-Framework/pull/2091
* (code): improve visibility of code inside of sheets by @ArtemKhvorostianyi in https://github.com/Ivy-Interactive/Ivy-Framework/pull/2058
* Feat/move dbml by @rorychatt in https://github.com/Ivy-Interactive/Ivy-Framework/pull/2074
* [Docs]:  Source button in app widgets in samples has fixed links by @ArtemLazarchuk in https://github.com/Ivy-Interactive/Ivy-Framework/pull/2097
* [GridLayout]: handle text overflow with ellipsis and hover popout by @defymecobra in https://github.com/Ivy-Interactive/Ivy-Framework/pull/2095
* [Text]: introduce LiteralBuilder without styling methods by @defymecobra in https://github.com/Ivy-Interactive/Ivy-Framework/pull/2066
* Feat/md extractor mcp by @rorychatt in https://github.com/Ivy-Interactive/Ivy-Framework/pull/2107
* [Run]: refactor page by @defymecobra in https://github.com/Ivy-Interactive/Ivy-Framework/pull/2108
* [Kanban]: prevent premature selector compilation by @defymecobra in https://github.com/Ivy-Interactive/Ivy-Framework/pull/2103
* (docs): refactor hooks docs by @ArtemKhvorostianyi in https://github.com/Ivy-Interactive/Ivy-Framework/pull/2111
* [Docs]: add Show Code button to sample pages by migrating apps to SampleBase by @ArtemLazarchuk in https://github.com/Ivy-Interactive/Ivy-Framework/pull/2106
* [Docs]: Implement samples and docs page for Dialog by @ArtemLazarchuk in https://github.com/Ivy-Interactive/Ivy-Framework/pull/2104
* [Docs]: merge internal hook pages into widget documentation by @defymecobra in https://github.com/Ivy-Interactive/Ivy-Framework/pull/2120
* build(deps): bump lodash from 4.17.21 to 4.17.23 in /src/frontend by @dependabot[bot] in https://github.com/Ivy-Interactive/Ivy-Framework/pull/2119
* [Supabase]: remove user-controlled bypass security alert in OAuth callback by @defymecobra in https://github.com/Ivy-Interactive/Ivy-Framework/pull/2064
* [README]: refactor Features and Tools by @defymecobra in https://github.com/Ivy-Interactive/Ivy-Framework/pull/2114
* Dark theme input styling by @dcrjodle in https://github.com/Ivy-Interactive/Ivy-Framework/pull/2131
* (docs): populate with links by @ArtemKhvorostianyi in https://github.com/Ivy-Interactive/Ivy-Framework/pull/2126
* (feat): improve expandable disabled state by @ArtemKhvorostianyi in https://github.com/Ivy-Interactive/Ivy-Framework/pull/2128
* [ColorInput]: add Swatch variant by @defymecobra in https://github.com/Ivy-Interactive/Ivy-Framework/pull/2115
* (textblock): add metric variant by @dcrjodle in https://github.com/Ivy-Interactive/Ivy-Framework/pull/2073
* Revert "(textblock): add metric variant" by @rorychatt in https://github.com/Ivy-Interactive/Ivy-Framework/pull/2135
* (inputs): fix light theme no input /selectors borders by @dcrjodle in https://github.com/Ivy-Interactive/Ivy-Framework/pull/2134
* (MetricView): Add Textblock Variant and Fix MetricViews by @dcrjodle in https://github.com/Ivy-Interactive/Ivy-Framework/pull/2133
* [GridLayout]: fix cell overflow behavior on hover by @defymecobra in https://github.com/Ivy-Interactive/Ivy-Framework/pull/2149
* (hooks): preserve hook names without spaces in docs sidebar by @ArtemKhvorostianyi in https://github.com/Ivy-Interactive/Ivy-Framework/pull/2148
* (docs): fix broken GitHub source URLs in API section by @ArtemKhvorostianyi in https://github.com/Ivy-Interactive/Ivy-Framework/pull/2147
* (doc tools): improve md path recognizing algorithm to be case insensitive by @ArtemKhvorostianyi in https://github.com/Ivy-Interactive/Ivy-Framework/pull/2146
* [Theme]: enhance ThemeCustomizer with interactive preview, dashboard tab and export dialog by @ArtemLazarchuk in https://github.com/Ivy-Interactive/Ivy-Framework/pull/2141
* (text area): Implement scale for text area by @ArtemKhvorostianyi in https://github.com/Ivy-Interactive/Ivy-Framework/pull/2129
* [Calendar]: replace dropdowns with editable month/year input by @defymecobra in https://github.com/Ivy-Interactive/Ivy-Framework/pull/2117
* [Docs]: preserve links in markdown headings during conversion by @defymecobra in https://github.com/Ivy-Interactive/Ivy-Framework/pull/2113
* [UseMutation]: refactor docs by @defymecobra in https://github.com/Ivy-Interactive/Ivy-Framework/pull/2110
* [Navigation]: restore appArgs on reconnect and suppress intentional disconnect modal by @defymecobra in https://github.com/Ivy-Interactive/Ivy-Framework/pull/2102
* Feat/chat rebased by @rorychatt in https://github.com/Ivy-Interactive/Ivy-Framework/pull/2153
* (tabsLayout): fix api link by @ArtemKhvorostianyi in https://github.com/Ivy-Interactive/Ivy-Framework/pull/2157
* (doc tools): copy/download page without raw markdown links by @ArtemKhvorostianyi in https://github.com/Ivy-Interactive/Ivy-Framework/pull/2156
* (tooltip): resolve tooltip button-in-button hydration error by @ArtemKhvorostianyi in https://github.com/Ivy-Interactive/Ivy-Framework/pull/2155
* [Theme]: Add interactive Theme Customizer with live preview and typography controls by @ArtemLazarchuk in https://github.com/Ivy-Interactive/Ivy-Framework/pull/2161
* [GridLayout]: resolve labels overflow by @defymecobra in https://github.com/Ivy-Interactive/Ivy-Framework/pull/2163
* [DateRangeInput]: add text truncation with ellipsis on overflow by @defymecobra in https://github.com/Ivy-Interactive/Ivy-Framework/pull/2162
* [UseWebhook]: refactor docs by @defymecobra in https://github.com/Ivy-Interactive/Ivy-Framework/pull/2159
* (terminal): fix api link in docs by @ArtemKhvorostianyi in https://github.com/Ivy-Interactive/Ivy-Framework/pull/2142
* (upload): remove file size limit for uploads by @ArtemKhvorostianyi in https://github.com/Ivy-Interactive/Ivy-Framework/pull/2127
* (theme-editor): created main color expandable, dynamic gap implemented in fields, removed border from colorinput by @joshuauaua in https://github.com/Ivy-Interactive/Ivy-Framework/pull/2173
* (Icons): Add Ivy Corner Icon by @dcrjodle in https://github.com/Ivy-Interactive/Ivy-Framework/pull/2179
* (docs): Fix GitHub auth docs title by @zachwolfe in https://github.com/Ivy-Interactive/Ivy-Framework/pull/2175
* (tables): refactors across frontend code by @ArtemKhvorostianyi in https://github.com/Ivy-Interactive/Ivy-Framework/pull/2138
* [Chart]: improve Y-axis auto-scaling by @ArtemLazarchuk in https://github.com/Ivy-Interactive/Ivy-Framework/pull/2180
* [Tabs]: Fix dropdown overflow and improve width calculations by @ArtemLazarchuk in https://github.com/Ivy-Interactive/Ivy-Framework/pull/2172
* [Auth]: Make authentication cookie options configurable by @ArtemLazarchuk in https://github.com/Ivy-Interactive/Ivy-Framework/pull/2164
* (search): show document path in search results by @ArtemKhvorostianyi in https://github.com/Ivy-Interactive/Ivy-Framework/pull/2188
* (widgets): remove bad symbol from ingress text by @ArtemKhvorostianyi in https://github.com/Ivy-Interactive/Ivy-Framework/pull/2187
* [Forms]: fix nullable inputs in scaffolded forms by @defymecobra in https://github.com/Ivy-Interactive/Ivy-Framework/pull/2183
* [Card]: remove border styling options by @defymecobra in https://github.com/Ivy-Interactive/Ivy-Framework/pull/2181
* [Chrome]: enhance sidebar menu with active item highlighting and auto-scroll by @ArtemLazarchuk in https://github.com/Ivy-Interactive/Ivy-Framework/pull/2194
* [BoolInput]: top-align controls when description present by @defymecobra in https://github.com/Ivy-Interactive/Ivy-Framework/pull/2208
* [Datatable]: make links clickable, show pointer cursor and update docs by @defymecobra in https://github.com/Ivy-Interactive/Ivy-Framework/pull/2204
* [Datatable]: add tooltip support for row action buttons by @defymecobra in https://github.com/Ivy-Interactive/Ivy-Framework/pull/2203
* [Init]: add missing ivy init options including --script to docs page by @defymecobra in https://github.com/Ivy-Interactive/Ivy-Framework/pull/2202
* [Docs]: organize MarkdownApp into structured tabs with examples by @ArtemLazarchuk in https://github.com/Ivy-Interactive/Ivy-Framework/pull/2200
* [Docs]: Add authentication cookie configuration documentation by @ArtemLazarchuk in https://github.com/Ivy-Interactive/Ivy-Framework/pull/2199
* (AvatarWidget): Be more tolerant of empty fallback by @zachwolfe in https://github.com/Ivy-Interactive/Ivy-Framework/pull/2195
* [Multiselect]: fix badge sizing by reducing remove button height by @defymecobra in https://github.com/Ivy-Interactive/Ivy-Framework/pull/2158
* [OAuth]: implement same-tab OAuth flow by @defymecobra in https://github.com/Ivy-Interactive/Ivy-Framework/pull/2068
* Revert "[OAuth]: implement same-tab OAuth flow" by @rorychatt in https://github.com/Ivy-Interactive/Ivy-Framework/pull/2209
* (icons): implement icon picket widget by @ArtemKhvorostianyi in https://github.com/Ivy-Interactive/Ivy-Framework/pull/2196
* (chore): patch notes for upcoming release by @rorychatt in https://github.com/Ivy-Interactive/Ivy-Framework/pull/2191
* (sidebar): fix arrow navigation across sidebar by @ArtemKhvorostianyi in https://github.com/Ivy-Interactive/Ivy-Framework/pull/2197


**Full Changelog**: https://github.com/Ivy-Interactive/Ivy-Framework/compare/v1.2.13...v1.2.14

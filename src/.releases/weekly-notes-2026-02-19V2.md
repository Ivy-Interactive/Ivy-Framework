# Ivy Framework Weekly Notes - Week of 2026-02-19 (V2)

## UI/UX Polish & Refinements

### Enhanced Sidebar Navigation

The sidebar menu has received a significant upgrade for better usability and visual feedback:

- **Active Item Highlighting**: Clearly see your current location with highlighted active items.
- **Auto-scroll to Active Item**: The sidebar now automatically scrolls to bring the active item into partial view when navigating or loading a page.
- **Improved Keyboard Navigation**: Sidebar search results now support full keyboard navigation with arrow keys and automatic scrolling.
- **Forgiving Search**: Search is now smarter, ignoring spaces and special characters for easier matching (e.g., "datatables" finds "Data Tables").

### Boolean Input Alignment & Consistency

- **Description Alignment**: Boolean inputs (Checkbox, Switch, Radio) with descriptions now top-align with their labels, fixing previous centering issues with multi-line text.
- **Vertical Rhythm**: Boolean inputs now match height with text inputs across all sizes (Small, Medium, Large) for perfect alignment in mixed forms.
- **Checkbox Polish**: Checkboxes now use a refined border radius (half of the selector radius) to maintain their square shape even when higher border radii are used for other inputs.

### Button & Link enhancements

- **Configurable Link Targets**: Buttons with URLs can now easily open in new tabs using `.OpenInNewTab()` or be set explicitly with `.Target(LinkTarget.Blank)`.
- **Fluent Variant API**: New extension methods `.Success()`, `.Warning()`, and `.Info()` join the existing fluent API for button styling, making code more readable.
- **DataTable Links**: Cell links are now directly clickable without modifier keys, and the cursor properly indicates interactability.

### Switch & Icon Improvements

- **Icons in Switches**: Switches now support icons inside the thumb for clearer state indication (e.g., sun/moon for theme toggles).
- **Icon Input Widget**: A new `IconInput` widget provides a searchable dropdown with visual previews for selecting Lucide icons.
- **Icon Animations**: Bring your UI to life with built-in icon animations (Rotate, Pulse, Shake, Bounce) triggered automatically or on interaction.

## Theming & Customization

### Advanced Theme Color Picker

A completely redesigned `ThemeColorPicker` widget offers:

- **Dual Modes**: Switch between a visual Palette view (with automatic theme color detection) and a precise RGB Slider view.
- **Contextual Previews**: Foreground color selection now previews against the corresponding background color.
- **Clipboard Integration**: Easily copy color values with a single click.

### Granular Border Radius Control

Themes now support distinct border radius settings for different UI categories:

- **Boxes**: Cards, containers, tables.
- **Fields**: Input fields, textareas.
- **Selectors**: Dropdowns, comboboxes, buttons.
- **Visual Configurator**: The theme customizer includes a new visual selector for these settings.
- **Utility Classes**: New `.rounded-box`, `.rounded-field`, and `.rounded-selector` classes are available for custom components.

## Authentication & Security

### Streamlined Authentication Setup

- **Simplified Configuration**: GitHub and Microsoft Entra providers no longer require manual HttpClient or option configuration.
- **Same-Tab OAuth**: OAuth flows now complete within the same tab, preventing popup blocker issues and improving mobile experience. **Note**: This requires updating your OAuth callback URLs to end in `/ivy/auth/callback`.
- **Configurable Cookies**: Full control over authentication cookie settings (Expiration, SameSite, Secure) via `Server.ConfigureAuthCookieOptions`.

### Security Updates

- **Dependency Update**: Updated `lodash-es` to patch a prototype pollution vulnerability.

## Developer Experience

### External Widgets System

Complete documentation and tooling for creating **External Widgets** — allowing you to build and integrate custom React components into Ivy applications with ease. Includes CLI scaffolding (`ivy widget`) and comprehensive guides on state management and building.

### File-Based Applications

Run Ivy apps directly from a single `.cs` file using `ivy init --script` and `dotnet run`. Perfect for prototyping, reproduction scripts, and simple tools without full project overhead.

### CLI Enhancements

- **Enhanced `ivy init`**: New options for templates, IDE integration (Cursor/Claude), and unattended setup.
- **Robust `ivy describe`**: Automatically finds available ports if the default is busy.

## Bug Fixes

- **DataTable**: Fixed cell click events not firing for link cells and actions.
- **DataTable**: Resolved an issue where the last row could be cropped by the footer.
- **Textarea**: Improved resize handle appearance to match theme; added `.Rows()` for initial height control.
- **Code Block**: Fixed indentation inconsistency on the first line.
- **Avatar**: Fixed a crash when rendering avatars with empty user information.

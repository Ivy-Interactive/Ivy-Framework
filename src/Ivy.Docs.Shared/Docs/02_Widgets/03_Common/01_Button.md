---
prepare: |
  var client = UseService<IClientProvider>();
searchHints:
  - button
  - click
  - action
  - submit
  - cta
  - interactive
  - control
---

# Button

<Ingress>
Create interactive buttons with multiple variants, states, sizes, and styling options for triggering actions in your Ivy [applications](../../01_Onboarding/02_Concepts/10_Apps.md).
</Ingress>

The `Button` [widget](../../01_Onboarding/02_Concepts/03_Widgets.md) is one of the most fundamental interactive elements in Ivy. It allows users to [trigger actions](../../01_Onboarding/02_Concepts/07_EventHandlers.md) and [navigate](../../01_Onboarding/02_Concepts/09_Navigation.md) through your project.

## Basic Usage

Here's a simple example of a button that shows a [toast message](../../01_Onboarding/02_Concepts/13_Clients.md) when clicked:

```csharp
var client = UseService<IClientProvider>();
new Button("Click Me", onClick: _ => client.Toast("Hello!"));
```

```csharp demo
new Button("Click Me", onClick: _ => client.Toast("Hello!"))
```

## Semantic Variants

The Button widget includes three new contextual variants to help communicate different types of actions to users: [Success, Warning, and Info](../../01_Onboarding/02_Concepts/12_Theming.md). These variants complement the existing Primary, Secondary, Destructive, Outline, Ghost, and Link options.

```csharp demo-tabs
Layout.Horizontal()
    | new Button("Success").Success()
    | new Button("Warning").Warning()
    | new Button("Info").Info()
```

## Styling & Configuration

Buttons offer extensive styling options including standard variants, states, border radius, and icon integration. Use [Align](../../04_ApiReference/Ivy/Align.md) for icon position (e.g. `Align.Right`).

```csharp demo-tabs
Layout.Vertical().Gap(4)
    | Text.P("Standard Variants").Large()
    | (Layout.Horizontal().Gap(4)
        | new Button("Primary")
        | new Button("Destructive").Destructive()
        | new Button("Secondary").Secondary()
        | new Button("Outline").Outline()
        | new Button("Ghost").Ghost()
        | new Button("Link").Link())
    | Text.P("States").Large()
    | (Layout.Horizontal().Gap(4)
        | new Button("Disabled").Disabled()
        | new Button("Loading").Loading()
        | new Button("Secondary Disabled").Secondary().Disabled())
    | Text.P("Border Radius").Large()
    | (Layout.Horizontal().Gap(4)
        | new Button("None").BorderRadius(BorderRadius.None)
        | new Button("Rounded").BorderRadius(BorderRadius.Rounded)
        | new Button("Full").BorderRadius(BorderRadius.Full))
    | Text.P("Icons").Large()
    | (Layout.Horizontal().Gap(4)
        | new Button("Save").Icon(Icons.Save)
        | new Button("Next").Icon(Icons.ArrowRight, Align.Right)
        | new Button().Icon(Icons.Settings).Ghost())
```

## Buttons with URLs

Buttons can act as links by providing a [URL](../../01_Onboarding/02_Concepts/09_Navigation.md). When a button has a URL, clicking it will navigate to that URL in the same tab by default. Use `.OpenInNewTab()` to override this behavior.

<Callout Type="tip">
Buttons with URLs support [right-click actions](../../01_Onboarding/02_Concepts/09_Navigation.md) like "Copy Link" and "Open in New Tab", providing a better user experience than programmatic navigation.
</Callout>

```csharp demo-tabs
    Layout.Horizontal().Gap(4)
        | new Button("Visit Ivy Docs")
            .Url("https://github.com/Ivy-Interactive/Ivy-Framework")
        | new Button("External Link").Secondary()
            .Url("https://github.com/Ivy-Interactive/Ivy-Framework")
            .OpenInNewTab()
            .Icon(Icons.ExternalLink, Align.Right)
        | new Button("Link Style").Link()
            .Url("https://github.com/Ivy-Interactive/Ivy-Framework")
```


<WidgetDocs Type="Ivy.Button" ExtensionTypes="Ivy.ButtonExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/src/Ivy/Widgets/Button.cs"/>

## Examples

<Details>
<Summary>
What are the available ButtonVariant values in Ivy?
</Summary>
<Body>

`ButtonVariant` has these values: `Primary`, `Destructive`, `Outline`, `Secondary`, `Success`, `Warning`, `Info`, `Ghost`, `Link`, `Inline`, `Ai`.

Set via the `.Variant()` method or shortcut methods:

```csharp
new Button("Save", handler).Variant(ButtonVariant.Primary)
// or use shortcut:
new Button("Save", handler).Primary()

// Other shortcuts: .Secondary(), .Outline(), .Destructive(), .Ghost(), .Link(), .Inline(), .Ai()
```

**Important:** There is no `ButtonVariant.Default`. Use `ButtonVariant.Primary` instead.

</Body>
</Details>

<Details>
<Summary>
How do I run an async operation when a button is clicked?
</Summary>
<Body>

Button accepts `Func<ValueTask>` and `Func<Event<Button>, ValueTask>` handlers natively — just use `async`:

```csharp
var result = UseState<string?>(null);
var loading = UseState(false);

if (loading.Value) return new Text("Loading...");

new Button("Run", async () => {
    loading.Value = true;
    result.Value = await myService.DoWorkAsync();
    loading.Value = false;
});

if (result.Value != null)
    new Callout(result.Value).Success();
```

There is no `UseAsync` hook. For data fetching with automatic loading/error state, use `UseQuery()` instead. `UseMutation` is for cache invalidation, not general async operations.

</Body>
</Details>

<Details>
<Summary>
How do I associate keyboard shortcuts with a button?
</Summary>
<Body>

The `ShortcutKey` method allows you to associate a keyboard shortcut (like `Ctrl+K`, `Ctrl+S`, or `Ctrl+Enter`) with a button. The action will be triggered whenever the shortcut is pressed, regardless of whether the button is focused.

```csharp demo
Layout.Horizontal().Gap(8)
    | new Button("Search", _ => client.Toast("Searching...")).Primary().ShortcutKey("Ctrl+K")
    | new Button("Save", _ => client.Toast("Saved!")).Secondary().ShortcutKey("Ctrl+S")
    | new Button("GitHub").Url("https://github.com/Ivy-Interactive/Ivy-Framework").ShortcutKey("Ctrl+G")
```

The shortcut listener is registered globally on the window, so the button doesn't need to be focused to trigger the action.

</Body>
</Details>

---
prepare: |
  var client = UseService<IClientProvider>();
searchHints:
  - click
  - action
  - submit
  - cta
  - interactive
  - control
---

# Button

<Ingress>
Create interactive buttons with multiple variants, states, sizes, and styling options for triggering actions in your Ivy [applications](../../01_Onboarding/02_Concepts/15_Apps.md).
</Ingress>

The `Button` [widget](../../01_Onboarding/02_Concepts/03_Widgets.md) is one of the most fundamental interactive elements in Ivy. It allows users to [trigger actions](../../01_Onboarding/02_Concepts/07_EventHandlers.md) and [navigate](../../01_Onboarding/02_Concepts/14_Navigation.md) through your project.

## Basic Usage

Here's a simple example of a button that shows a [toast message](../../01_Onboarding/02_Concepts/19_Clients.md) when clicked:

```csharp
var client = UseService<IClientProvider>();
new Button("Click Me", onClick: _ => client.Toast("Hello!"));
```

```csharp demo
new Button("Click Me", onClick: _ => client.Toast("Hello!"))
```

## Semantic Variants

The Button widget includes three new contextual variants to help communicate different types of actions to users: [Success, Warning, and Info](../../01_Onboarding/02_Concepts/17_Theming.md). These variants complement the existing Primary, Secondary, Destructive, Outline, Ghost, and Link options.

```csharp demo-tabs
Layout.Horizontal()
    | new Button("Success", variant: ButtonVariant.Success)
    | new Button("Warning", variant: ButtonVariant.Warning)
    | new Button("Info", variant: ButtonVariant.Info)
```

## Styling & Configuration

Buttons offer extensive styling options including standard variants, states, border radius, and icon integration.

```csharp demo-tabs
Layout.Vertical().Gap(4)
    | Text.Large("Standard Variants")
    | (Layout.Horizontal().Gap(4)
        | new Button("Primary")
        | new Button("Destructive").Destructive()
        | new Button("Secondary").Secondary()
        | new Button("Outline").Outline()
        | new Button("Ghost").Ghost()
        | new Button("Link").Link())
    | Text.Large("States")
    | (Layout.Horizontal().Gap(4)
        | new Button("Disabled").Disabled()
        | new Button("Loading").Loading()
        | new Button("Secondary Disabled").Secondary().Disabled())
    | Text.Large("Border Radius")
    | (Layout.Horizontal().Gap(4)
        | new Button("None").BorderRadius(BorderRadius.None)
        | new Button("Rounded").BorderRadius(BorderRadius.Rounded)
        | new Button("Full").BorderRadius(BorderRadius.Full))
    | Text.Large("Icons")
    | (Layout.Horizontal().Gap(4)
        | new Button("Save").Icon(Icons.Save)
        | new Button("Next").Icon(Icons.ArrowRight, Align.Right)
        | new Button(null, icon: Icons.Settings, variant: ButtonVariant.Ghost))
```

## Buttons with URLs

Buttons can act as links by providing a [URL](../../01_Onboarding/02_Concepts/14_Navigation.md). When a button has a URL, clicking it will navigate to that URL in a new tab instead of triggering an `onClick` event.

<Callout Type="tip">
Buttons with URLs support [right-click actions](../../01_Onboarding/02_Concepts/14_Navigation.md) like "Copy Link" and "Open in New Tab", providing a better user experience than programmatic navigation.
</Callout>

```csharp demo-tabs
    Layout.Horizontal().Gap(4)
        | new Button("Visit Ivy Docs", variant: ButtonVariant.Primary)
            .Url("https://github.com/Ivy-Interactive/Ivy-Framework")
        | new Button("External Link", variant: ButtonVariant.Secondary)
            .Url("https://github.com/Ivy-Interactive/Ivy-Framework")
            .Icon(Icons.ExternalLink, Align.Right)
        | new Button("Link Style", variant: ButtonVariant.Link)
            .Url("https://github.com/Ivy-Interactive/Ivy-Framework")
```

<WidgetDocs Type="Ivy.Button" ExtensionTypes="Ivy.ButtonExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/Button.cs"/>

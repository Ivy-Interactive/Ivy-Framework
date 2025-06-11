---
prepare: |
  var client = this.UseService<IClientProvider>();
---

# Card

The Card widget is a versatile container used to group related content and actions in Ivy applications. It is ideal for displaying structured layouts, combining elements such as text, buttons, charts, or input fields within a unified visual block.

## Basic Usage

Here's a simple example of a Card with a button that triggers a toast message when clicked:

```csharp
new Card(
    "Welcome to our application!",
    new Button("Sign Me Up", _ => client.Toast("You have signed up!"))
)
.Title("Card Example")
.Description("This is a basic Card usage.")
.Width(Size.Units(100))
```

## Variants

You can display multiple Cards side by side or in custom arrangements to structure content.

```csharp demo-tabs
Layout.Horizontal()
    | new Card("Card A content").Title("Card A")
    | new Card("Card B content").Title("Card B").Description("This is Card B")
    | new Card("Card C content").Title("Card C").Width(Size.Pixels(250))
```

## Styling

Cards can be customized with padding, shadows, border radius, and background styles to match your design requirements.

```csharp demo-below
new Card("This Card has custom styles.")
    .Title("Styled Card")
    .Description("With padding, border radius, and shadow")
    .Padding(Padding.Large)
    .BorderRadius(BorderRadius.Full)
    .Shadow(Shadow.Large)
    .Background("linear-gradient(to right, #4facfe, #00f2fe)")
```

## States

Cards can be set to a disabled or loading state to reflect asynchronous operations or restricted access.

```csharp demo-tabs
Layout.Horizontal()
    | new Card("Loading data...").Title("Loading").Loading()
    | new Card("This card is not interactive").Title("Disabled").Disabled()
```

<WidgetDocs
Type="Ivy.Card"
ExtensionTypes="Ivy.CardExtensions"
SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/Card.cs"
/>
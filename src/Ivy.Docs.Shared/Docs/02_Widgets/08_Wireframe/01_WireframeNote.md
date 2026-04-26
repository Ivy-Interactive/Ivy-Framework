---
searchHints:
- wireframe
- sticky
- note
- sketch
- prototype
- balsamiq
- brainstorm
- postit
---

# WireframeNote

<Ingress>
A hand-drawn sticky note widget for wireframing, brainstorming, and low-fidelity prototyping with a Balsamiq-style aesthetic.
</Ingress>

The `WireframeNote` widget renders as a sticky note with a folded corner, drop shadow, slight rotation, and hand-drawn font. It is designed for sketching ideas, annotating layouts, and building low-fidelity prototypes.

## Basic Usage

```csharp demo
new WireframeNote("Remember to add validation")
```

## Colors

Six color variants are available to categorize and distinguish notes:

```csharp demo
Layout.Horizontal().Gap(4)
    | new WireframeNote("Yellow (default)")
    | new WireframeNote("Blue note", WireframeNoteColor.Blue)
    | new WireframeNote("Green note", WireframeNoteColor.Green)
    | new WireframeNote("Pink note", WireframeNoteColor.Pink)
    | new WireframeNote("Orange note", WireframeNoteColor.Orange)
    | new WireframeNote("Purple note", WireframeNoteColor.Purple)
```

| Color    | Typical Use                    |
|----------|--------------------------------|
| Yellow   | General notes, ideas           |
| Blue     | In-progress items, questions   |
| Green    | Completed items, approvals     |
| Pink     | Bugs, blockers, warnings       |
| Orange   | Deferred items, follow-ups     |
| Purple   | Design decisions, references   |

## Multi-line Text

Use newlines in the text string to create multi-line notes:

```csharp demo
Layout.Horizontal().Gap(4)
    | new WireframeNote("Step 1:\nUser signs up")
    | new WireframeNote("Step 2:\nVerify email", WireframeNoteColor.Blue)
    | new WireframeNote("Step 3:\nOnboarding flow", WireframeNoteColor.Green)
```

## Sizing

Control the note dimensions with `Width` and `Height`:

```csharp demo
Layout.Horizontal().Gap(4)
    | new WireframeNote("Small").Width(Size.Px(120))
    | new WireframeNote("Full width note").Width(Size.Full())
```

## On a Canvas

Combine with `CanvasLayout` for free-form placement:

```csharp demo
Layout.Canvas().Width(Size.Full()).Height(Size.Px(300))
    | new WireframeNote("Login page", WireframeNoteColor.Yellow)
        .CanvasLeft(Size.Px(20)).CanvasTop(Size.Px(20))
    | new WireframeNote("Auth service", WireframeNoteColor.Blue)
        .CanvasLeft(Size.Px(200)).CanvasTop(Size.Px(100))
    | new WireframeNote("Dashboard", WireframeNoteColor.Green)
        .CanvasLeft(Size.Px(380)).CanvasTop(Size.Px(30))
    | new WireframeNote("Error handling", WireframeNoteColor.Pink)
        .CanvasLeft(Size.Px(200)).CanvasTop(Size.Px(220))
```

## Project Board

Use with `StackLayout` for structured kanban-style boards:

```csharp demo
Layout.Vertical().Gap(4)
    | Text.H3("Sprint Board")
    | (Layout.Horizontal().Gap(4)
        | (Layout.Vertical().Gap(3).Width(Size.Units(40))
            | Text.Strong("To Do")
            | new WireframeNote("Design login page").Width(Size.Full())
            | new WireframeNote("Write API docs", WireframeNoteColor.Orange).Width(Size.Full()))
        | (Layout.Vertical().Gap(3).Width(Size.Units(40))
            | Text.Strong("In Progress")
            | new WireframeNote("Build widgets", WireframeNoteColor.Blue).Width(Size.Full()))
        | (Layout.Vertical().Gap(3).Width(Size.Units(40))
            | Text.Strong("Done")
            | new WireframeNote("DB schema", WireframeNoteColor.Green).Width(Size.Full())
            | new WireframeNote("Scaffolding", WireframeNoteColor.Green).Width(Size.Full())))
```

<WidgetDocs Type="Ivy.WireframeNote" ExtensionTypes="Ivy.WireframeNoteExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/src/Ivy/Widgets/Wireframe/WireframeNote.cs"/>

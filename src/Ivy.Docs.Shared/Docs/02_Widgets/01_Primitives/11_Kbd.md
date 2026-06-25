---
searchHints:
  - kbd
  - keyboard
  - shortcut
  - key
  - hotkey
  - command
  - keys
---

# Kbd

<Ingress>
Display keyboard shortcuts and key combinations with proper styling to help users identify commands and improve documentation.
</Ingress>

The `Kbd` [widget](../../01_Onboarding/02_Concepts/03_Widgets.md) displays keyboard shortcuts or key combinations with proper styling. It helps users identify key commands and improves documentation clarity.

Pass a full combination as a single string. Each key is rendered as its own standalone cap (so `"Ctrl+Enter"` shows two caps, not one), and modifier and navigation keys — `Ctrl`, `Cmd`/`Win`, `Shift`, `Alt`/`Option`, `Enter`, `Backspace`, and the arrow keys — are shown as platform-appropriate icons where available. Single-glyph caps are square; multi-character labels keep the same height and grow wider.

```csharp demo-below
Layout.Horizontal() | 
    new Kbd("Ctrl+C") | 
    new Kbd("Shift+Ctrl+C") |
    new Kbd("Cmd+Enter")
```

## Ghost

Use `.Ghost()` to drop the background and border, leaving just the key glyphs — useful for inline, low-emphasis shortcut hints.

```csharp demo-below
Layout.Horizontal() | 
    new Kbd("Cmd+Enter").Ghost() | 
    new Kbd("Esc").Ghost()
```

<WidgetDocs Type="Ivy.Kbd" ExtensionTypes="Ivy.KbdExtensions"  SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/src/Ivy/Widgets/Primitives/Kbd.cs"/>

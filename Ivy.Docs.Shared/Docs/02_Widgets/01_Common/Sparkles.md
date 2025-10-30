---
searchHints:
  - sparkle
  - badge
  - decoration
---

# Sparkles

<Ingress>
Decorative sparkles with optional text – a lightweight accent for titles and badges.
</Ingress>

## Basic Usage

```csharp demo-tabs
public class SparklesDemo : ViewBase
{
    public override object? Build()
    {
        return Layout.Horizontal().Gap(3)
            | new Sparkles()
            | new Sparkles().Text("Featured")
            | new Sparkles().Color(Colors.Yellow).Size(Sizes.Large);
    }
}
```

## Properties

- **Text(string?)**: Optional trailing text
- **Color(Colors?)**: Optional color
- **Size(Sizes)**: Small, Medium, Large

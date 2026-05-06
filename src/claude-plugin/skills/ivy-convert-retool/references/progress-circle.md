# Progress Circle

A content area to display a circular progress bar. Visualizes progress with a circular indicator and optional caption.

## Retool

```toolscript
// Properties
progressCircle1.value = 65
progressCircle1.hideOutput = false
progressCircle1.indeterminate = false
progressCircle1.horizontalAlign = "center"

// Methods
progressCircle1.clearValue();
progressCircle1.resetValue();
progressCircle1.setHidden(true);
progressCircle1.scrollIntoView({ behavior: 'smooth', block: 'center' });
```

## Ivy

Ivy does not have a dedicated circular progress widget. The `Progress` widget renders a horizontal bar. For a circular indicator, use a custom approach or compose with existing primitives.

```csharp
// Horizontal progress bar (closest equivalent)
new Progress(65)
    .Goal("65% Complete")
    .ColorVariant(ColorVariants.Success)
    .Width(Size.Px(200));

// Reactive binding via state
var state = new State<int>(0);
var progress = new Progress(state);
state.Value = 65; // updates the progress bar dynamically

// Alternative: use MetricView with goal progress for a visual indicator
new MetricView("Completion", Icons.CheckCircle, ctx =>
{
    return new QueryResult<MetricRecord>(
        new MetricRecord("65%", GoalAchieved: 0.65m, GoalFormatted: "100%")
    );
});
```

## Parameters

| Parameter              | Documentation                                         | Ivy                                               |
|------------------------|-------------------------------------------------------|----------------------------------------------------|
| `value`                | Progress value to display (number)                    | `Progress.Value` (horizontal bar only)             |
| `hideOutput`           | Hides the caption displayed with the value            | `.Goal(string)` controls displayed label           |
| `indeterminate`        | Shows loading state with unknown progress             | Not supported                                      |
| `horizontalAlign`      | Alignment of contents (left/center/right)             | Not supported (use layout)                         |
| `style`                | Custom styling options                                | `.ColorVariant(ColorVariants)`                     |
| `hidden`               | Visibility toggle                                     | `Visible` property                                 |
| `margin`               | External spacing                                      | Not supported (use layout spacing)                 |
| `isHiddenOnMobile`     | Hide on mobile layout                                 | Not supported                                      |
| `isHiddenOnDesktop`    | Hide on desktop layout                                | Not supported                                      |
| `maintainSpaceWhenHidden` | Reserve space when hidden                          | Not supported                                      |
| `clearValue()`         | Clears current values                                 | `state.Set(0)` via state                           |
| `resetValue()`         | Resets to default value                               | `state.Set(default)` via state                     |
| `setHidden(hidden)`    | Toggles visibility                                    | `Visible` property                                 |
| `scrollIntoView()`     | Scrolls component into view                           | Not supported                                      |
